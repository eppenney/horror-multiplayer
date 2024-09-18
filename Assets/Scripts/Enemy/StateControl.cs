using UnityEngine;

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
    public float agroTime = 5.0f;
    private float agroTimer = 0.0f;

    // Constructor
    public TargetInfo(Transform transform, Vector3 position, float volume, bool seen = false)
    {
        this.transform = transform;
        this.position = position;
        this.volume = volume;
        this.seen = seen;
    }
}

public class StateControl : MonoBehaviour {

    public EnemyState state = EnemyState.Waiting;

    public List<TargetInfo> targetList;
    TargetInfo target;

    private Navigation nav;
    private Sight sight;


    void Start() {
        nav = GetComponent<Navigation>();
        sight = GetComponent<Sight>();

        targetList = new List<transform>();
    }

    void Update() {
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
        switch (state)
        {
            case EnemyState.Wandering:
                break;
            case EnemyState.Investigating:
                nav.MoveToPosition(target.position);
                break;
            case EnemyState.Searching:
                break;
            case EnemyState.Hunting:
                nav.MoveToPosition(target.position);
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
        state = p_state;
        Debug.Log("State Changed to " + p_state);
    }

    private void WanderingState()
    {
        // Dummy function for Wandering state
        Debug.Log("Enemy is Wandering");
        TargetInfo newTarget = SeePlayer();
        if (newTarget !== null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
        // Add logic for wandering behavior here
    }

    private void InvestigatingState()
    {
        // Dummy function for Investigating state
        Debug.Log("Enemy is Investigating");
        TargetInfo newTarget = SeePlayer();
        if (newTarget !== null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
        // Add logic for investigating behavior here
    }

    private void SearchingState()
    {
        // Dummy function for Searching state
        Debug.Log("Enemy is Searching");
        TargetInfo newTarget = SeePlayer();
        if (newTarget !== null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
        // Add logic for searching behavior here
    }

    private void HuntingState()
    {
        // Dummy function for Hunting state
        Debug.Log("Enemy is Hunting");
        agroTimer += Time.deltaTime; 
        if (SeePlayer() == target) {
            agroTimer = 0.0f;
        }
        if (agroTimer > agroTime) {
            ChangeState(EnemyState.Searching);
        }
        // Add logic for hunting behavior here
        // If not in GetVisibleTargets for x time, switch to Searching 
    }

    private void FleeingState()
    {
        // Dummy function for Fleeing state
        Debug.Log("Enemy is Fleeing");
        // Add logic for fleeing behavior here
    }

    private void WaitingState()
    {
        // Dummy function for Waiting state
        Debug.Log("Enemy is Waiting");
        // Add logic for waiting behavior here
    }

    private void FollowingState()
    {
        // Dummy function for Following state
        Debug.Log("Enemy is Following");
        // Add logic for following behavior here
    }

    public void HearSound(Transform source, float volume) {
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
        targetList<Transform> visible = sight.GetVisibleTargets();
        float closestDistance = Mathf.Infinity; 

        foreach (Transform target in visible) {
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance < closestDistance) {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        TargetInfo seenTarget = null;

        if (closestTarget != null) {
            Debug.Log($"Closest visible target: {closestTarget.name} at distance: {closestDistance}");
            seenTarget = new TargetInfo(closestTarget, closestTarget.position, 0.0f, true);
            ChangeState(EnemyState.Hunting);
            return seenTarget;
        }
        return seenTarget;
    }
}