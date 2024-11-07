using UnityEngine;
using Unity.Netcode;

public class Navigation : NetworkBehaviour {
    public UnityEngine.AI.NavMeshAgent agent;

    public override void OnNetworkSpawn() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (!IsServer) {
            agent.enabled = false;
        }
        Debug.Log($"Enemy Navigation Initialized - IsServer: {IsServer}");
    }

    public void MoveToPosition(Vector3 targetPosition) {
        MoveToPositionServerRPC(targetPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveToPositionServerRPC(Vector3 targetPosition) {
        if (!IsServer) { return; }
        var path = new UnityEngine.AI.NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        switch (path.status)
        {
            case UnityEngine.AI.NavMeshPathStatus.PathComplete:
                // Debug.Log($"{agent.name} moving to target.");
                agent.SetPath(path);
                return;
            case UnityEngine.AI.NavMeshPathStatus.PathPartial:
                // Debug.LogWarning($"{agent.name} moving partway to target.");
                agent.SetPath(path);
                return;
            default:
                // Debug.LogError($"There is no valid path for {agent.name} to reach target.");
                return ;
        }
    }

    public int CheckPosition(Vector3 targetPosition) {
        var path = new UnityEngine.AI.NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        switch (path.status)
        {
            case UnityEngine.AI.NavMeshPathStatus.PathComplete:
                Debug.Log($"{agent.name} will be able to reach target.");
                return 1;
            case UnityEngine.AI.NavMeshPathStatus.PathPartial:
                Debug.LogWarning($"{agent.name} will only be able to move partway to target.");
                return 0;
            default:
                Debug.LogError($"There is no valid path for {agent.name} to reach target.");
                return -1;
        }
    }
}