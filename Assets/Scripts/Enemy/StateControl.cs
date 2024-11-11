using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Unity.Netcode;

/* 
I'm thinking maintain 4 "targets"
Wandering -> Hear/See Target -> Investigate -> Searching / Hunting / Wandering
Searching -> Hunting with target found 
Hunting -> Searching with target lost
Searching -> Wandering with target not found 

If the creature would go to wandering and there is still a target in the list, 
it will instead switch to investigating the closest target
*/
public enum EnemyState {
    Wandering, // Just wandering 
    Investigating, // Checking out a stimuli 
    Searching, // Knows something is nearby and is actively searching for it 
    Hunting, // In pursuit of a target 
    Fleeing, // Attempting to escape a target 
    Waiting, // Knows something is nearby and is lying in wait to hunt it 
    Following // Sees a target and is staying out of sight
    
}

[System.Serializable]
public class TargetInfo
{
    public Transform transform; // To store the transform
    public Vector3 position;     // To store the position
    public float volume;         // To store the volume
    public bool seen;

    // Constructor
    public TargetInfo(Transform transform, Vector3 position, float volume, bool seen = false)
    {
        this.transform = transform;
        this.position = position;
        this.volume = volume;
        this.seen = seen;
    }
}

public class StateControl : NetworkBehaviour {

    public EnemyState state = EnemyState.Wandering;
    public List<TargetInfo> targetList;
    TargetInfo target;
    private bool lockedAnimation = false;

    // Component References 
    private Navigation nav;
    private Sight sight;
    private Attack attack;


    [Header("Hunt Settings")]
    [SerializeField] private float huntSpeed = 3.5f;
    [SerializeField] private float distanceThreshold = 0.25f;


    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderSpeed = 1.5f;

    [Header("Search Settings")]
    [SerializeField] private float searchTimer = 0.0f;
    [SerializeField] private  float searchTime = 20.0f;
    [SerializeField] private float searchRadius = 10f;
    [SerializeField] private float searchSpeed = 2.5f;

    public override void OnNetworkSpawn() {
        nav = GetComponent<Navigation>();
        sight = GetComponent<Sight>();
        attack = GetComponent<Attack>();
        targetList = new List<TargetInfo>();
        Debug.Log($"Creature Intialized - IsServer: {IsServer}");
    }

    void Update() {
        if (!IsServer) { return; }
        if (lockedAnimation) { return; } // If locked in an animation, do not act
        switch (state)
        {
            case EnemyState.Wandering:
                WanderingState();
                break;
            case EnemyState.Investigating:
                InvestigatingState();
                break;
            case EnemyState.Searching:
                SearchingState();
                break;
            case EnemyState.Hunting:
                HuntingState();
                break;
            case EnemyState.Fleeing:
                FleeingState();
                break;
            case EnemyState.Waiting:
                WaitingState();
                break;
            case EnemyState.Following:
                FollowingState();
                break;
            default:
                Debug.LogWarning("Unhandled state: " + state);
                break;
        }
    }

    public void ChangeState(EnemyState p_state) {
        if (lockedAnimation) { return; } // If locked in an animation, do not change state 

        EnemyState temp_state = state;
        state = p_state;
        UpdateStateClientRpc(p_state);
        Debug.Log("State changed from " + temp_state + " to " + p_state);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(EnemyState p_state)
    {
        state = p_state; // Update the state on all clients
    }

    private void WanderingState()
    {
        nav.agent.speed = wanderSpeed;
        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);
            return;
        }

        if (nav.agent.remainingDistance < distanceThreshold) {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;  // Add to current position to stay nearby

            NavMeshHit hit;
            // Sample the NavMesh to find a valid point within the radius
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                nav.MoveToPosition(hit.position);  // Move to the valid point
            }
        }
    }

    private void InvestigatingState()
    {
        nav.agent.speed = wanderSpeed;
        nav.MoveToPosition(target.position);

        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
        if (nav.agent.remainingDistance < distanceThreshold) {
            ChangeState(EnemyState.Searching); // When you find the source of sound, search the area
        }
    }

    private void SearchingState()
    {
        nav.agent.speed = searchSpeed;

        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }

        if (nav.agent.remainingDistance < distanceThreshold && Random.Range(0, 10) < 1) 
        {
            // Choose a random point around the last known target position within a smaller search radius
            Vector3 searchPoint = target.position + Random.insideUnitSphere * (searchRadius);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                nav.MoveToPosition(hit.position); // Move to the new search point
            }
        }
        
        // Transition to wandering state if search time exceeds a threshold and no target is found
        searchTimer += Time.deltaTime;
        if (searchTimer > searchTime)
        {
            searchTimer = 0.0f;
            ChangeState(EnemyState.Wandering);
        }
    }
    
    private void HuntingState()
    {
        nav.agent.speed = huntSpeed;

        if (sight.CanSee(target.transform)){
            target = new TargetInfo(target.transform, target.transform.position, target.volume);
            nav.MoveToPosition(target.transform.position);
            return;
        }

        // Move towards target 
        nav.MoveToPosition(target.position);

        // If close enough to target, attack them. This may need to be adjusted or changed to an attacking state  
        if (Vector3.Distance(transform.position, target.transform.position) < attack.Range) {
            Debug.Log($"Attacking");
            attack.AttackServerRpc();
        }

        // If  we have reached the last known position, search for target
        if (nav.agent.remainingDistance < distanceThreshold) {
            Debug.Log("Target Gone, Searching");
            searchTimer = 0.0f;
            ChangeState(EnemyState.Searching);
        }
    }

    private void FleeingState()
    {
        // Dummy function for Fleeing state
    }

    private void WaitingState()
    {
        // Dummy function for Waiting state
    }

    private void FollowingState()
    {
        // Dummy function for Following state
    }

    public void HearSound(Transform source, float volume) {
        Debug.Log("Enemy in state " + state + " heard sound of volume " + volume);
        switch (state)
        {
            case EnemyState.Wandering: // Investigate sound
                target = new TargetInfo(source, source.position, volume);
                ChangeState(EnemyState.Investigating);
                break;
            case EnemyState.Investigating: // Investigate if louder, or update target position
                if (volume >= target.volume) {
                    target = new TargetInfo(source, source.position, volume);
                }
                if (source == target.transform) {
                    target = new TargetInfo(source, source.position, volume);
                }
                ChangeState(EnemyState.Investigating);
                break;
            case EnemyState.Searching: // Investigate sound 
                target = new TargetInfo(source, source.position, volume);
                ChangeState(EnemyState.Investigating);
                break;
            case EnemyState.Hunting:
                break;
            case EnemyState.Fleeing:
                break;
            case EnemyState.Waiting:
                break;
            case EnemyState.Following:
                break;
            default:
                Debug.LogWarning("Unhandled state: " + state);
                break;
        }
    }

    public TargetInfo SeePlayer() {
        List<Transform> visible = sight.GetVisibleTargets();
        float closestDistance = Mathf.Infinity; 
        Transform closestTarget = null;

        foreach (Transform target in visible) {
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance < closestDistance) {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        TargetInfo seenTarget = null;

        if (closestTarget != null) {
            // Debug.Log($"Closest visible target: {closestTarget.name}");
            seenTarget = new TargetInfo(closestTarget, closestTarget.position, 0.0f, true);
        }
        return seenTarget;
    }

    void LockAnimation() { lockedAnimation = true; }
    void UnlockAnimation() { lockedAnimation = false; }

    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Set the color of the gizmo
            Gizmos.color = Color.red;
            
            // Draw a sphere at the target's position
            Gizmos.DrawSphere(target.position, 0.5f); // Adjust the size as needed
            
            if (state == EnemyState.Searching) {
                Gizmos.DrawWireSphere(target.position, searchRadius);
            }
        }
    }

}