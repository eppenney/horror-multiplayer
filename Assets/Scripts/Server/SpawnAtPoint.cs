using Unity.Netcode;
using UnityEngine;

public class SpawnAtPoint : NetworkBehaviour
{
    public static int spawnIndex = 0;  // Static index for cycling through spawn points
    public Vector3[] spawnPoints;    // Array of spawn points

    private bool initialized = false;

    void Initialize() {
        if (IsOwner)  // Only spawn for the local player
        {
            Vector3 spawnPosition = GetSpawnPosition();
            transform.position = spawnPosition;
            initialized = true;
        }
    }

    private void OnNetworkSpawn()
    {
        Initialize();
    }

    private Vector3 GetSpawnPosition()
    {
        // Get the spawn position based on the static spawn index
        Vector3 spawnPosition = spawnPoints[spawnIndex];

        // Increment the spawnIndex and reset it if it exceeds the number of spawn points
        spawnIndex = (spawnIndex + 1) % spawnPoints.Length;

        return spawnPosition;
    }

    void Update() {
        if (!initialized) {
            Initialize();
        }
    }
}
