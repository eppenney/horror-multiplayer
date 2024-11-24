using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set;}
    private Dictionary<string, ItemPrefab> itemDictionary; 
    [SerializeField] private List<ItemPrefab> allItemPrefabs;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        itemDictionary = new Dictionary<string, ItemPrefab>();

        foreach (ItemPrefab itemPrefab in allItemPrefabs) {
            if (!itemDictionary.ContainsKey(itemPrefab.itemID))
            {
                itemDictionary.Add(itemPrefab.itemID, itemPrefab);
            }
            else
            {
                Debug.LogWarning($"Duplicate itemID found: {itemPrefab.itemID}");
            }
        }
    }

    public ItemPrefab GetItemPrefabByID(string itemID)
    {
        if (itemDictionary.ContainsKey(itemID))
        {
            return itemDictionary[itemID];
        }
        else
        {
            Debug.LogError($"Item with ID {itemID} not found!");
            return null;
        }
    }
}
