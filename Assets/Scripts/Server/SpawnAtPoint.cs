using Unity.Netcode;
using UnityEngine;
using System.Collections;
/*
This script is currently working but it is getting around a problem in a messy way.
I suspect the issue is that the player prefab is not fully spawned when the script runs. 
As such, we wait with a coroutine. 
*/

public class SpawnAtPoint : NetworkBehaviour
{
    public static int spawnIndex = 0;  // Static index for cycling through spawn points
    public Vector3[] spawnPoints;    // Array of spawn points

    private bool initialized = false;
    private Vector3 spawnPosition;

    int attempts = 0;

    void Initialize() {
        if (IsOwner)  // Only spawn for the local player
        {
            AssignSpawnPositionServerRpc();
        }
    }

    IEnumerator WaitForSpawn()
    {
        while (!IsSpawned) 
        {
            yield return null;  // Wait one frame
        }
        yield return new WaitForSeconds(1.5f);  // Small delay for network sync
        Initialize();
    }

    void Start()
    {
        StartCoroutine(WaitForSpawn());
    }

    [ServerRpc]
    private void AssignSpawnPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        spawnPosition = GetSpawnPosition();
        SetSpawnPositionClientRpc(spawnPosition);
    }

    [ClientRpc]
    private void SetSpawnPositionClientRpc(Vector3 position)
    {
        if (!IsOwner) {return;}
        Debug.Log($"[ClientRpc] Setting spawn position to: {position} for player {OwnerClientId}");
        transform.position = position;
        if (TryGetComponent(out NetworkObject netObj))
        {
            netObj.transform.position = position; // Ensure position is applied
        }
        initialized = true;
    }

    private Vector3 GetSpawnPosition()
    {
        // Get the spawn position based on the static spawn index
        spawnPosition = spawnPoints[spawnIndex];

        Debug.Log($"Using spawn point {spawnIndex}");

        return spawnPosition;
    }

    private bool IsAtSpawn() {
        return Vector3Int.RoundToInt(spawnPosition) == Vector3Int.RoundToInt(transform.position);
    }

    void Update() {
        if (attempts++ < 100) {
            Initialize();
        }      
    }
}
