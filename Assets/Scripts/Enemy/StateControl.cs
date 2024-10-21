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

    public float agroTime = 5.0f;
   [SerializeField] private float agroTimer = 0.0f;

    private Navigation nav;
    private Sight sight;

    [Header("Wander Settings")]
    public float wanderRadius = 10f;

    [SerializeField] private float distanceThreshold = 0.25f;
    private bool lockedAnimation = false;

    void Start() {
        nav = GetComponent<Navigation>();
        sight = GetComponent<Sight>();

        targetList = new List<TargetInfo>();
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
        UpdateStateClientRpc(newState);
        Debug.Log("State changed from " + temp_state + " to " + p_state);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(EnemyState newState)
    {
        state = newState; // Update the state on all clients
    }

    private void WanderingState()
    {
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
        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }

        if (nav.agent.remainingDistance < distanceThreshold) 
        {
            // Choose a random point around the last known target position within a smaller search radius
            Vector3 searchPoint = target.position + Random.insideUnitSphere * (wanderRadius / 2);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPoint, out hit, wanderRadius / 2, NavMesh.AllAreas))
            {
                nav.MoveToPosition(hit.position); // Move to the new search point
            }
        }
        
        // Transition to wandering state if search time exceeds a threshold and no target is found
        agroTimer += Time.deltaTime;
        if (agroTimer > agroTime)
        {
            agroTimer = 0.0f;
            ChangeState(EnemyState.Wandering);
        }
    }
    
    private void HuntingState()
    {
        // Move towards target 
        nav.MoveToPosition(target.position);

        // If the target is seen, update position data
        TargetInfo new_target = SeePlayer();
        if (new_target != null) {
            if (new_target.transform == target.transform) {
                target = new_target;
                return;
            }
        }

        // If close enough to target, attack them 
        // if (close) {attack}
        
        // If  we have reached the last known position, search for target
        if (nav.agent.remainingDistance < distanceThreshold) {
            agroTimer = 0.0f;
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
            return seenTarget;
        }
        return seenTarget;
    }

    void LockAnimation() { lockedAnimation = true; }
    void UnlockAnimation() { lockedAnimation = false; }
}