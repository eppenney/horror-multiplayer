using UnityEngine 

public class Navigation : MonoBehaviour {
    [RequireComponent(typeof(NavMeshAgent))]
    private NavMeshAgent agent;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    public int MoveToPosition(Vector3 targetPosition) {
        var path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                Debug.Log($"{agent.name} moving to target.");
                agent.SetPath(path);
                return 1;
            case NavMeshPathStatus.PathPartial:
                Debug.LogWarning($"{agent.name} moving partway to target.");
                agent.SetPath(path);
                return 0;
            default:
                Debug.LogError($"There is no valid path for {agent.name} to reach target.");
                return -1;
        }
    }

    public int CheckPosition(Vector3 targetPosition) {
        var path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                Debug.Log($"{agent.name} will be able to reach target.");
                return 1;
            case NavMeshPathStatus.PathPartial:
                Debug.LogWarning($"{agent.name} will only be able to move partway to target.");
                return 0;
            default:
                Debug.LogError($"There is no valid path for {agent.name} to reach target.");
                return -1;
        }
    }
}