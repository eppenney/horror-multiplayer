using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour {
    [SerializeField] private string defaultID = "DefaultID"; 
    public string ID { get; private set; }

    void OnEnable()
    {
        ID = defaultID;
    }
}


