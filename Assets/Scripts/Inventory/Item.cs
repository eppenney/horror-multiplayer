using UnityEngine;
using Unity.Netcode;

/*
Item class, meant to be inherited and override use functions. 
When picked up, it should dissapear from game world 
When put down, the item form of it should be spawned
*/
public class Item : NetworkBehaviour {
    [SerializeField] private GameObject m_worldModel;
    [SerializeField] private GameObject m_playerModel;
    public void PickUp(GameObject p_player) {}
    public void PutDown(GameObject p_player) {}
    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}