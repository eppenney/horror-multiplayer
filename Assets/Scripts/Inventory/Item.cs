using UnityEngine;
using Unity.Netcode;

public class Item : NetworkBehaviour {
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);
    public GameObject characterRepresentation;

    // Public getter for isPickedUp
    public bool IsPickedUp {
        get { return isPickedUp.Value; }
    }

    // Server RPC to be called by a client for picking up the item
    [ServerRpc(RequireOwnership = false)]
    public void PickUpServerRpc() {
        if (IsServer) {
            isPickedUp.Value = true;
            Debug.Log("Item picked up by a client.");
        }
    }

    // Server RPC to be called by a client for putting down the item
    [ServerRpc(RequireOwnership = false)]
    public void PutDownServerRpc() {
        if (IsServer) {
            isPickedUp.Value = false;
            Debug.Log("Item put down by a client.");
        }
    }

    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}
