using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour {
    [SerializeField] private string defaultID = "DefaultID"; 
    public string ID { get; private set; }

    [SerializeField] private float throwForce;

    float GetThrowForce() {
        return throwForce;
    }

    void OnEnable()
    {
        ID = defaultID;
    }
}


