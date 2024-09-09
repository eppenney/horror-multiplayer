using Unity.Netcode;
using UnityEngine;
/*
I think this should be changed to hold space / Interact to do secondary, press it once to do primary 
*/
public class EnvironmentInteraction : NetworkBehaviour
{
    [SerializeField]
    private float interactDistance = 1.0f;
    [SerializeField]
    private Transform playerCam;
    [SerializeField]
    private LayerMask interactableLayer;
    [SerializeField]
    private IInteractable target;
    public override void OnNetworkSpawn()
    {
        
    }

    void Update() {
        if (!IsOwner) { return; }
        GetTarget();
        Inputs();
    }

    void Primary() {
        if (target != null) {
            target.PrimaryUseDown();
        }
    }

    void Secondary() {
        if (target != null) {
            target.SecondaryUseDown();
        }
    }

    void GetTarget() {
        RaycastHit hit;
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer)) {
            target = hit.transform.gameObject.GetComponent<IInteractable>();
        } else {
            target = null;
        }
    }

    void Inputs() {
        if (Input.GetButtonDown("Interact")) {
            Primary();
        }
        if (Input.GetButtonDown("Fire2")) {
            Secondary();
        }
        if (Input.GetButtonUp("Fire1") && target != null) {
            target.PrimaryUseUp();
            target = null;
        }
        if (Input.GetButtonUp("Fire2") && target != null) {
            target.SecondaryUseUp();
            target = null;
        }
    }
}

