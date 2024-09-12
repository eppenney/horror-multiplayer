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
        playerCam = Camera.main.transform;// GetComponent<cameraControl>().GetCamera().transform;
    }

    void Update() {
        if (!IsOwner) { return; }
        Inputs();
    }

    void Primary() {
        if (target != null) {
            Debug.Log("Primary Use Activated");
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
        Debug.Log("Ray Sent");
        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer)) {
            Debug.Log("Target hit");
            target = hit.transform.gameObject.GetComponent<IInteractable>();
        } else {
            target = null;
        }
    }

    void Inputs() {
        if (Input.GetButtonDown("Interact")) {
            GetTarget();
            Primary();
        }
        if (Input.GetButtonDown("Fire2")) {
            GetTarget();
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

