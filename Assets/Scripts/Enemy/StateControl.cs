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
    private NetworkVariable<bool> lockedAnimation = new NetworkVariable<bool>(false);

    // Component References 
    private Navigation nav;
    private Sight sight;
    private Attack attack;
    private Animator anim;

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

    [Header("Fleeing Settings")]
    [SerializeField] private float fleeSpeed = 4.0f;
    [SerializeField] private float fleeDistance = 25.0f;
    [SerializeField] private float fleeRadius = 10.0f;

    void Initialize() {
        if (nav == null) nav = GetComponent<Navigation>();
        if (sight == null) sight = GetComponent<Sight>();
        if (attack == null) attack = GetComponent<Attack>();
        if (targetList == null) targetList = new List<TargetInfo>();
        if (anim == null) anim = transform.GetChild(0).GetComponent<Animator>();

        Debug.Log($"Creature Intialized - IsServer: {IsServer}");
    }

    public override void OnNetworkSpawn() {
        Initialize();
    }

    public void Start() {
        Initialize();
    }

    void Update() {
        if (!IsServer) { return; }
        if (lockedAnimation.Value) {
            // Debug.Log("Halting until unlocked"); 
            return; 
        } // If locked in an animation, do not act
        anim.SetFloat("Speed", nav.agent.velocity.magnitude);
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
        if (lockedAnimation.Value) { return; } // If locked in an animation, do not change state 

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
        // Move towards target 
        nav.MoveToPosition(target.position);

        // If close enough to target, attack them. This may need to be adjusted or changed to an attacking state  
        if (attack.TargetInRange(target.transform)) {
            Debug.Log($"Attacking");
            attack.AttackServerRpc();
            // LockAnimation();
            return;
        }

        if (sight.CanSee(target.transform)){
            target = new TargetInfo(target.transform, target.transform.position, target.volume);
            nav.MoveToPosition(target.transform.position);
            return;
        }

        // If  we have reached the last known position, search for target
        if (nav.agent.remainingDistance < distanceThreshold) {
            Debug.Log("Target Gone, Searching");
            searchTimer = 0.0f;
            ChangeState(EnemyState.Searching);
        }
    }

    // Simple flee function - may need to be updated to account for traps (whether or not traps can be considered as targets, really)
    // Will be linked with when the enemy takes damage. Target should be set to damage source, then either transition to flee or hunt
    private void FleeingState()
    {
        nav.agent.speed = fleeSpeed;

        TargetInfo newTarget = SeePlayer();
        if (nav.agent.remainingDistance < distanceThreshold && newTarget != null) {
            // Get the direction away from target
            Vector3 direction = (transform.position - target.position).normalized * fleeDistance;

            // Pick a random point within a sphere of fleeRadius, fleeDistance units away
            Vector3 fleePoint = target.position + direction + Random.insideUnitSphere * (fleeRadius);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePoint, out hit, fleeRadius, NavMesh.AllAreas))
            {
                nav.MoveToPosition(hit.position); // Move to the new point
            }
        }
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

    public void TakeDamage(Transform p_source) {
        // here, consult adaption. For now, random 
        target = new TargetInfo(p_source, p_source.position, 10.0f);
        if (Random.Range(0, 10) < 2) {
            ChangeState(EnemyState.Hunting);
        } else {
            ChangeState(EnemyState.Fleeing);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockAnimationServerRpc()
    {
        lockedAnimation.Value = true;
        LockAnimationClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UnlockAnimationServerRpc()
    {
        lockedAnimation.Value = false;
        UnlockAnimationClientRpc();
    }

    [ClientRpc]
    private void LockAnimationClientRpc()
    {
        // Debug.Log("Locking");
        lockedAnimation.Value = true;
    }

    [ClientRpc]
    private void UnlockAnimationClientRpc()
    {
        // Debug.Log("Unlocking");
        lockedAnimation.Value = false;
    }

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