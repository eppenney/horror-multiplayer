using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Inventory : NetworkBehaviour {
    [SerializeField] private List<GameObject> m_items = new List<GameObject>(4);
    [SerializeField] private int m_heldItemIndex = 0;
    [SerializeField] private float interactDistance = 1.0f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private Transform heldPosition;
    [SerializeField] private float defaultThrowForce = 5.0f;
    private Transform playerCam;

    void Start() {
        if (playerCam == null) playerCam = Camera.main.transform;
        for (int i = 0; i < 4; i++) {
            m_items.Add(null);
        }
    }

    void Update() {
        if (playerCam == null) { playerCam = Camera.main.transform; }
        if (!IsOwner) { return; }

        Inputs();
        SetItemsToHeldPosition();
    }

    private void Inputs() {
        if (m_items[m_heldItemIndex] != null) {
            UseItems();
            if (Input.GetKeyDown(KeyCode.F)) Drop();
        } else {
            if (Input.GetButtonDown("Fire1")) PickUp();
        }
        ChangeItem();
    }

    private GameObject GetItem() {
        GameObject target = null;
        RaycastHit hit;
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        if (Physics.Raycast(ray, out hit, interactDistance, itemLayer)) {
            target = hit.transform.gameObject;
        }
        return target;
    }

    private void PickUp() {
        GameObject worldItem = GetItem();
        if (worldItem != null) {
            PickUp p_itemComponent = worldItem.GetComponent<PickUp>();
            if (p_itemComponent == null) { return; }

            string item_id = p_itemComponent.ID;
            GameObject newItem = SpawnPlayerItem(item_id);

            if (newItem == null) { return;}

            Destroy(worldItem);
            m_items[m_heldItemIndex] = newItem;
        }
    }

    private void Drop() {
        GameObject itemToDrop = m_items[m_heldItemIndex];
        if (itemToDrop == null) { return; }
    
        Item p_itemComponent = itemToDrop.GetComponent<Item>();
        if (p_itemComponent == null) { return; }

        string item_id = p_itemComponent.ID;
        ItemPrefab itemPrefab = ItemManager.Instance.GetItemPrefabByID(item_id);
        GameObject newItem = Instantiate(itemPrefab.worldPrefab, transform.position + transform.forward, Quaternion.identity);

        NetworkObject netObj = newItem.GetComponent<NetworkObject>();
        netObj.Spawn();

        Destroy(itemToDrop);
        m_items[m_heldItemIndex] = null;
    }

    private void ChangeItem() {
        int start_index = m_heldItemIndex;
        if (m_items == null) { return; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) m_heldItemIndex = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) m_heldItemIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) m_heldItemIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) m_heldItemIndex = 3;

        if (start_index == m_heldItemIndex) { return;}
        for (int i = 0; i < m_items.Count; i++) {
            if (m_items[i] == null) { continue; }
            m_items[i].SetActive(i == m_heldItemIndex);
        }
    }

    private void UseItems() {
        GameObject currentItem = m_items[m_heldItemIndex];
        if (currentItem == null) return;
        Item p_item = currentItem.GetComponent<Item>();
        if (p_item != null) {
            if (Input.GetMouseButtonDown(0)) p_item.PrimaryUseDown();
            if (Input.GetMouseButtonUp(0)) p_item.PrimaryUseUp();
            if (Input.GetMouseButtonDown(1)) p_item.SecondaryUseDown();
            if (Input.GetMouseButtonUp(1)) p_item.SecondaryUseUp();
        }
    }

    private void SetItemsToHeldPosition() {
        foreach (GameObject item in m_items) {
            if (item != null) {
                item.transform.position = heldPosition.position;
                item.transform.rotation = heldPosition.rotation;
            }
        }
    }

    private GameObject SpawnPlayerItem(string p_id) {
        ItemPrefab itemPrefab = ItemManager.Instance.GetItemPrefabByID(p_id);

        if (itemPrefab == null) { return null; }

        // Spawn or instantiate the characterRepresentation at the held position on all clients
        GameObject p_characterRep = Instantiate(itemPrefab.playerPrefab);
        p_characterRep.transform.SetParent(heldPosition);
        p_characterRep.transform.localPosition = Vector3.zero;
        p_characterRep.transform.localRotation = Quaternion.identity;

        return p_characterRep;
    }
}
