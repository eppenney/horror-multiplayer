using UnityEngine;
using System.Collections.Generic;
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

public class StateControl : MonoBehaviour {

    public EnemyState state = EnemyState.Wandering;

    public List<TargetInfo> targetList;
    TargetInfo target;

    public float agroTime = 5.0f;
    private float agroTimer = 0.0f;

    private Navigation nav;
    private Sight sight;


    void Start() {
        nav = GetComponent<Navigation>();
        sight = GetComponent<Sight>();

        targetList = new List<TargetInfo>();
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
        switch (p_state)
        {
            case EnemyState.Wandering:
                Debug.Log("Enemy is Wandering");
                break;
            case EnemyState.Investigating:
                Debug.Log("Enemy is Investigating");
                nav.MoveToPosition(target.position);
                break;
            case EnemyState.Searching:
                Debug.Log("Enemy is Searching");
                break;
            case EnemyState.Hunting:
                Debug.Log("Enemy is Hunting");
                nav.MoveToPosition(target.position);
                break;
            case EnemyState.Fleeing:
                Debug.Log("Enemy is Fleeing");
                break;
            case EnemyState.Waiting:
                Debug.Log("Enemy is Waiting");
                break;
            case EnemyState.Following:
                Debug.Log("Enemy is Following");
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
        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
    }

    private void InvestigatingState()
    {
        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
    }

    private void SearchingState()
    {
        TargetInfo newTarget = SeePlayer();
        if (newTarget != null) {
            target = newTarget;
            ChangeState(EnemyState.Hunting);    
        }
    }

    private void HuntingState()
    {
        agroTimer += Time.deltaTime; 
        if (SeePlayer() == target) {
            agroTimer = 0.0f;
        }
        if (agroTimer > agroTime) {
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
            Debug.Log($"Closest visible target: {closestTarget.name} at distance: {closestDistance}");
            seenTarget = new TargetInfo(closestTarget, closestTarget.position, 0.0f, true);
            ChangeState(EnemyState.Hunting);
            return seenTarget;
        }
        return seenTarget;
    }
}