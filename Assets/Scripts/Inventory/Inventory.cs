using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public enum ItemPrefabType {
    Player,
    World,
    Projectile
}

public class Inventory : NetworkBehaviour {
    [SerializeField] private List<GameObject> m_items = new List<GameObject>(4);
    // [SerializeField] private NetworkVariable<int> m_heldItemIndex = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<int> m_heldItemIndex = new NetworkVariable<int>(
    default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
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
    }

    private void Inputs() {
        if (m_items[m_heldItemIndex.Value] != null) {
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
        if (worldItem == null) return;

        WorldItem p_itemComponent = worldItem.GetComponent<WorldItem>();
        if (p_itemComponent == null) return; 

        // Spawn player item
        string item_id = p_itemComponent.ID;
        SpawnPlayerItemServerRpc(item_id);

        // Delete world Item
        NetworkObject worldItemNetObj = worldItem.GetComponent<NetworkObject>();
        if (worldItemNetObj != null) {
            ulong networkId = worldItemNetObj.NetworkObjectId;
            ItemManager.Instance.DeleteItemServerRpc(networkId);
        } else {
            Destroy(worldItem);
        }

        ChangeItem(true);
    }

    private void Drop() {
        GameObject itemToDrop = m_items[m_heldItemIndex.Value];
        if (itemToDrop == null) { return; }
    
        Item p_itemComponent = itemToDrop.GetComponent<Item>();
        if (p_itemComponent == null) { return; }
        string item_id = p_itemComponent.ID;

        SpawnWorldItemServerRpc(item_id);
        DestroyPlayerItemServerRpc();
    }

    private void ChangeItem(bool forceUpdate = false) {
        int start_index = m_heldItemIndex.Value;
        if (m_items == null) { return; }
        if (Input.GetKeyDown(KeyCode.Alpha1)) m_heldItemIndex.Value = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) m_heldItemIndex.Value = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) m_heldItemIndex.Value = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) m_heldItemIndex.Value = 3;

        if (start_index == m_heldItemIndex.Value && !forceUpdate) { return;}
        for (int i = 0; i < m_items.Count; i++) {
            if (m_items[i] == null) { continue; }
            m_items[i].SetActive(i == m_heldItemIndex.Value);
        }
    }

    private void UseItems() {
        GameObject currentItem = m_items[m_heldItemIndex.Value];
        if (currentItem == null) return;
        Item p_item = currentItem.GetComponent<Item>();
        if (p_item != null) {
            if (Input.GetMouseButtonDown(0)) p_item.PrimaryUseDown();
            if (Input.GetMouseButtonUp(0)) p_item.PrimaryUseUp();
            if (Input.GetMouseButtonDown(1)) p_item.SecondaryUseDown();
            if (Input.GetMouseButtonUp(1)) p_item.SecondaryUseUp();
        }
    }

    [ServerRpc]
    private void SpawnPlayerItemServerRpc(string p_id) {
        ItemPrefab itemPrefab = ItemManager.Instance.GetItemPrefabByID(p_id);
        if (itemPrefab == null) { return; }

        SpawnPlayerItemClientRpc(p_id);
    }

    [ClientRpc]
    private void SpawnPlayerItemClientRpc(string p_id) {
        ItemPrefab itemPrefab = ItemManager.Instance.GetItemPrefabByID(p_id);
        if (itemPrefab == null) { return; }

        // Spawn the item on the server
        GameObject p_characterRep = Instantiate(itemPrefab.playerPrefab);     

        p_characterRep.transform.SetParent(heldPosition);
        p_characterRep.transform.localPosition = Vector3.zero;
        p_characterRep.transform.localRotation = itemPrefab.playerPrefab.transform.rotation;

        m_items[m_heldItemIndex.Value] = p_characterRep;   
    }

    [ServerRpc]
    private void SpawnWorldItemServerRpc(string p_id) {
        ItemPrefab itemPrefab = ItemManager.Instance.GetItemPrefabByID(p_id);
        if (itemPrefab == null) { return; }

        GameObject p_worldRep = Instantiate(itemPrefab.worldPrefab);
        p_worldRep.transform.position = transform.position + transform.forward;
        p_worldRep.transform.rotation = transform.rotation;

        NetworkObject netObj = p_worldRep.GetComponent<NetworkObject>();
        if (netObj != null) netObj.Spawn();
    }

    [ServerRpc]
    public void DestroyPlayerItemServerRpc() {
        DestroyPlayerItemClientRpc();
    }

    [ClientRpc]
    private void DestroyPlayerItemClientRpc() {
        GameObject toDestroy = m_items[m_heldItemIndex.Value];

        // Delete player Item
        NetworkObject playerItemNetObj = toDestroy.GetComponent<NetworkObject>();
        if (playerItemNetObj != null) {
            ulong networkId = playerItemNetObj.NetworkObjectId;
            ItemManager.Instance.DeleteItemServerRpc(networkId);
        } else {
            Destroy(toDestroy);
        }

        m_items[m_heldItemIndex.Value] = null;
    }

    [ServerRpc]
    public void SpawnProjectileServerRpc(string p_id, float p_force, Vector3 p_direction)
    {
        ItemPrefab itemPrefab = ItemManager.Instance.GetItemPrefabByID(p_id);
        if (itemPrefab == null) { return; }

        GameObject p_projectile = Instantiate(itemPrefab.projectilePrefab, heldPosition.position, heldPosition.rotation);

        NetworkObject netObj = p_projectile.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            netObj.Spawn();

            Rigidbody rb = p_projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.AddForce(p_direction * p_force, ForceMode.VelocityChange);
            }
        }
    }
}
