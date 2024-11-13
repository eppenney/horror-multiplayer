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
    public void PickUp(GameObject p_player) {
        Destroy(this);
    }
    public void PutDown(GameObject p_player) {
        Instantiate(w_worldModel, p_player.transform.position + p_player.transform.forward);
        Destroy(this);
    }
    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}