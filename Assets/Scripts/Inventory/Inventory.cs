using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/*
Script for interacting with, picking up, storing, and using items. 
Should be placed on a player prefab
*/

public class Inventory : NetworkBehaviour {
    [SerializeField] private List<GameObject> m_items = new List<GameObject>(4);
    [SerializeField] private int m_heldItemIndex = 0;
    [SerializeField] private float interactDistance = 1.0f;
    [SerializeField] private LayerMask itemLayer; 
    [SerializeField] private GameObject itemContainer; 
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
        Debug.Log("Item Ray Sent");
        if (Physics.Raycast(ray, out hit, interactDistance, itemLayer)) {
            Debug.Log("Item Target hit");
            target = hit.transform.gameObject;
        }
        return target;
    }

    private void PickUp() {
        GameObject p_item = GetItem();
        
        if (p_item != null) {
             // Parent the item to the player
            NetworkObject netObj = p_item.GetComponent<NetworkObject>();
            if (netObj != null) {
                if (!netObj.TrySetParent(heldPosition.parent)) {
                    Debug.LogWarning("Failed to reparent NetworkObject item.");
                    return;
                }
            } else {
                return;
            }

            // Get the item component and set picked up value
            Item p_itemComponent = p_item.GetComponent<Item>();
            if (p_itemComponent.IsPickedUp) { return; }
            p_itemComponent.PickUpServerRpc();

            

            Rigidbody rb = p_item.GetComponent<Rigidbody>();
            rb.isKinematic  = true;

            m_items[m_heldItemIndex] = p_item;
        }
    }

    private void Drop() {
        GameObject itemToDrop = m_items[m_heldItemIndex];
        if (itemToDrop != null) {
            // Detach from the player
            NetworkObject netObj = itemToDrop.GetComponent<NetworkObject>();
            if (netObj != null) {
                if (!netObj.TrySetParent((GameObject) null, true)) {
                    Debug.LogWarning("Failed to reparent NetworkObject item.");
                    return;
                }
            } else {
                return;
            }

            Item p_itemComponent = itemToDrop.GetComponent<Item>();
            p_itemComponent.PutDownServerRpc();

            Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
            rb.isKinematic  = false;
            rb.velocity = Vector3.zero;
            rb.AddForce(playerCam.forward * defaultThrowForce);

            m_items[m_heldItemIndex] = null;
        }
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
                item.transform.localPosition = heldPosition.localPosition;
                item.transform.localRotation = heldPosition.localRotation;
            }
        }
    }
}
