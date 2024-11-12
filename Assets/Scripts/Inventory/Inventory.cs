using UnityEngine;
using Unity.Netcode;

/*
Script for interacting with, picking up, storing, and using items. 
Should be placed on a player prefab
*/

public class Inventory : MonoBehaviour {
    public Item m_item1;
    public Item m_item2;
    public Item m_item3;
    public Item m_item4;
    public int m_heldItemIndex = 0;

    private void GetItem() {} // Get item within range 
    private void PickUp() {} // Pick up highlighted item 
    private void Drop() {} // Drop item currently held
    private void ChangeItem() {} // Inputs for changing item held 
    private void UseItems() {} // Inputs for using currently held item
}