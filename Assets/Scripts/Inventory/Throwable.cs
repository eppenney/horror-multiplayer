using UnityEngine;
using Unity.Netcode;


public class Throwable : Item {
    [SerializeField] private float throwForce = 10f;
    private Transform playerCam;
    private bool isAiming = false;

    private void Initialize() {
        if (playerCam == null) playerCam = Camera.main.transform;
    }

    void Start() {
        Initialize();
    }

    public override void SecondaryUseDown() {
        isAiming = true;
        Debug.Log("Aiming...");
        // Add visual or UI feedback for aiming here if needed
    }

    public override void SecondaryUseUp() {
        isAiming = false;
        Debug.Log("Stopped aiming.");
        // Remove visual or UI feedback for aiming here
    }

    public override void PrimaryUseDown() {
        if (isAiming) {
            Throw();
        }
    }

    private void Throw() {
        Debug.Log("Thrown!");
        transform.parent.parent.GetComponent<Inventory>().SpawnProjectileServerRpc(ID, throwForce, playerCam.forward);
    }

    
}
