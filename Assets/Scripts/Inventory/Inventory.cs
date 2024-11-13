using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/*
Script for interacting with, picking up, storing, and using items. 
Should be placed on a player prefab
*/

public class Inventory : MonoBehaviour {
    [SerializeField] private List<Item> m_items = new List<Item>(4);
    [SerializeField] private int m_heldItemIndex = 0;
    [SerializeField] private float interactDistance = 1.0f;
    [SerializeField] private LayerMask itemLayer; 
    [SerializeField] private GameObject itemContainer; 
    private Transform playerCam;

    void Start() {
        if (playerCam == null) { playerCam = Camera.main.transform; }

        for (int i = 0; i < 4; i++) {
            m_items.Add(null);
        }
    }

    void Update() {
        if (playerCam == null) { playerCam = Camera.main.transform; }
    }

    private Item GetItem() {
        Item target = null; 

        RaycastHit hit;
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        Debug.Log("Ray Sent");
        if (Physics.Raycast(ray, out hit, interactDistance, itemLayer)) {
            Debug.Log("Target hit");
            target = hit.transform.gameObject.GetComponent<Item>();
        }
        return target;
    }

    private void PickUp(Item item) {
        if (Input.GetButtonDown("Fire1")) {
            Item p_item = GetItem();
            if (p_item != null) {
                // Create the correct item, and add it to the players hand
                GameObjext handItem = Instantiate(p_item.m_playerModel);
                p_item.transform.SetParent(itemContainer.transform);

                m_items[m_heldItemIndex] = p_item;
                p_item.PickUp(gameObject);
            }
        }
    }

    private void Drop() {
        Item itemToDrop = m_items[m_heldItemIndex];
        if (itemToDrop != null) {
            itemToDrop.PutDown(gameObject);
            m_items[m_heldItemIndex] = null;
        }
    }

    private void ChangeItem() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) m_heldItemIndex = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) m_heldItemIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) m_heldItemIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) m_heldItemIndex = 3;

        Debug.Log("Switched to item slot: " + m_heldItemIndex);
    }

    private void UseItems() {
        Item currentItem = m_items[m_heldItemIndex];
        if (currentItem != null) {
            if (Input.GetMouseButtonDown(0)) currentItem.PrimaryUseDown();
            if (Input.GetMouseButtonUp(0)) currentItem.PrimaryUseUp();
            if (Input.GetMouseButtonDown(1)) currentItem.SecondaryUseDown();
            if (Input.GetMouseButtonUp(1)) currentItem.SecondaryUseUp();
        }
    } 
}
