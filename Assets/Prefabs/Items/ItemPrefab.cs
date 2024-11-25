using UnityEngine;

[CreateAssetMenu(fileName = "New ItemPrefab", menuName = "Inventory/ItemPrefab", order = 1)]
public class ItemPrefab : ScriptableObject
{
    public string itemID;                  // Unique ID for the item
    public GameObject worldPrefab;        // Prefab for the item in the world
    public GameObject playerPrefab;           // Prefab for the item held in the player's hand
    public GameObject projectilePrefab;     // Optional prefab for the item as a projectile
}
