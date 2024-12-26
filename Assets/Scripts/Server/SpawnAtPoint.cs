using Unity.Netcode;
using UnityEngine;

public class SpawnAtPoint : NetworkBehaviour
{
    public static int spawnIndex = 0;  // Static index for cycling through spawn points
    public Vector3[] spawnPoints;    // Array of spawn points


    private void OnNetworkSpawn()
    {
        if (IsLocalPlayer)  // Only spawn for the local player
        {
            Vector3 spawnPosition = GetSpawnPosition();
            transform.position = spawnPosition;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Get the spawn position based on the static spawn index
        Vector3 spawnPosition = spawnPoints[spawnIndex];

        // Increment the spawnIndex and reset it if it exceeds the number of spawn points
        spawnIndex = (spawnIndex + 1) % spawnPoints.Length;

        return spawnPosition;
    }
}
