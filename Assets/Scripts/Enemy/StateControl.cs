using UnityEngine;

public enum EnemyState {
    Wandering, // Just wandering 
    Investigating, // Checking out a stimuli 
    Searching, // Knows something is nearby and is actively searching for it 
    Hunting, // In pursuit of a target 
    Fleeing, // Attempting to escape a target 
    Waiting, // Knows something is nearby and is lying in wait to hunt it 
    Following // Sees a target and is staying out of sight
    
}

public class StateControl : MonoBehaviour {

    public EnemyState state = EnemyState.Waiting;

    [[RequireComponent(typeof(Navigation))]] 
    private Navigation nav;

    [[RequireComponent(typeof(Sight))]] 
    private Sight sight;


    void Start() {
        nav = GetComponent<Navigation>();
        sight = GetComponent<sight>();
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
        state = p_state;
        Debug.Log("State Changed to " + p_state);
    }

    private void WanderingState()
    {
        // Dummy function for Wandering state
        Debug.Log("Enemy is Wandering");
        // Add logic for wandering behavior here
    }

    private void InvestigatingState()
    {
        // Dummy function for Investigating state
        Debug.Log("Enemy is Investigating");
        // Add logic for investigating behavior here
    }

    private void SearchingState()
    {
        // Dummy function for Searching state
        Debug.Log("Enemy is Searching");
        // Add logic for searching behavior here
    }

    private void HuntingState()
    {
        // Dummy function for Hunting state
        Debug.Log("Enemy is Hunting");
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
}