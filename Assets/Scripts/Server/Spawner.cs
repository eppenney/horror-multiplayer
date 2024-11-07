using UnityEngine;
using Unity.Netcode;

public class Spawner : MonoBehaviour 
{
    [SerializeField] private GameObject enemyPrefab;

    public void SpawnEnemy() {
        GameObject enemyInstance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        enemyInstance.GetComponent<NetworkObject>().Spawn(); // Spawns the enemy across all clients
    }
}
