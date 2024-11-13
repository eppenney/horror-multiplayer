using UnityEngine;
using Unity.Netcode;

public class Item : NetworkBehaviour {
    public GameObject m_worldModel;
    public GameObject m_playerModel;

    public void PickUp(GameObject p_player) {
        if (IsServer) { DestroyItemServerRpc(); }
        else {
            NetworkObject netObj = p_player.GetComponent<NetworkObject>();
            if (netObj != null) {
                RequestPickUpServerRpc(new NetworkObjectReference(netObj));
            }
        }
    }

    public void PutDown(GameObject p_player) {
        if (IsServer) {
            Instantiate(m_worldModel, p_player.transform.position + p_player.transform.forward, Quaternion.identity);
            DestroyItemServerRpc();
        } else {
            NetworkObject netObj = p_player.GetComponent<NetworkObject>();
            if (netObj != null) {
                RequestPutDownServerRpc(new NetworkObjectReference(netObj));
            }
        }
    }

    [ServerRpc]
    private void RequestPickUpServerRpc(NetworkObjectReference p_playerRef) {
        if (p_playerRef.TryGet(out NetworkObject p_playerNetObj)) {
            PickUp(p_playerNetObj.gameObject);
        }
    }

    [ServerRpc]
    private void RequestPutDownServerRpc(NetworkObjectReference p_playerRef) {
        if (p_playerRef.TryGet(out NetworkObject p_playerNetObj)) {
            PutDown(p_playerNetObj.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyItemServerRpc() {
        if (IsServer) {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned) {
                Destroy(networkObject.gameObject);
            }
        }
    }

    public GameObject GetWorldModel() {
        return m_worldModel;
    }

    public GameObject GetPlayerModel() {
        return m_playerModel;
    }

    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}
