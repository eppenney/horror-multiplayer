using UnityEngine;
using Unity.Netcode;

/*
Item class, meant to be inherited and override use functions. 
When picked up, it should dissapear from game world 
When put down, the item form of it should be spawned
*/
public class Item : NetworkBehaviour {
    // Both prefabs should contain the item class with reference to itself
    [SerializeField] private GameObject m_worldModel { get; };
    [SerializeField] private GameObject m_playerModel { get; };
    public void PickUp(GameObject p_player)
    {
        if (IsServer) { DestroyItemServerRpc(); } // Ensure the object is destroyed by the server
        else { RequestPickUpServerRpc(p_player); }
    }
    public void PutDown(GameObject p_player) {
        if (IsServer) {
            Instantiate(w_worldModel, p_player.transform.position + p_player.transform.forward);
            DestroyItemServerRpc();
        }   
        else
        {
            RequestPutDownServerRpc(p_player);
        }
    }

    // Called to request picking up an item (client to server)
    [ServerRpc]
    private void RequestPickUpServerRpc(GameObject p_player)
    {
        PickUp(p_player);
    }

    // Called to request putting down an item (client to server)
    [ServerRpc]
    private void RequestPutDownServerRpc(GameObject p_player)
    {
        PutDown(p_player);
    }

    // Server RPC to destroy this object
    [ServerRpc(RequireOwnership = false)]
    private void DestroyItemServerRpc()
    {
        if (IsServer)
        {
            // Destroy both the network object and the GameObject itself
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                Destroy(networkObject.gameObject);
            }
        }
    }
    
    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}