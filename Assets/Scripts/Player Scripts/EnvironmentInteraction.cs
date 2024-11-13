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
    private Interactable target;

    private void Initialize() {
        if (playerCam == null) playerCam = Camera.main.transform;
    }
    public override void OnNetworkSpawn()
    {
        Initialize();
    }

    void Start() {
        Initialize();
    }
    void Update() {
        if (!IsOwner) { return; }
        Initialize();
        Inputs();
    }

    private void Primary() {
        if (target != null) {
            Debug.Log("Primary Use Activated");
            target.PrimaryUseDown();
        }
    }

    private void Secondary() {
        if (target != null) {
            target.SecondaryUseDown();
        }
    }

    private void PrimaryUp() {
        if (target != null) {
            target.PrimaryUseUp();
            target = null;
        }
    }

    private void SecondaryUp() {
        if (target != null) {
            target.SecondaryUseUp();
            target = null;
        }
    }

    private void GetTarget() {
        RaycastHit hit;
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        Debug.Log("Ray Sent");
        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer)) {
            Debug.Log("Target hit");
            target = hit.transform.gameObject.GetComponent<Interactable>();
        } else {
            target = null;
        }
    }

    private void Inputs() {
        if (Input.GetButtonDown("Interact")) {
            GetTarget();
            Primary();
        }
        if (Input.GetButtonDown("Fire2")) {
            GetTarget();
            Secondary();
        }
        if (Input.GetButtonUp("Fire1")) {
            PrimaryUp();
        }
        if (Input.GetButtonUp("Fire2")) {
            SecondaryUp();
        }
    }

    private void OnDrawGizmos() {
        if (playerCam == null) return; // Ensure playerCam is assigned

        Ray ray = new Ray(playerCam.position, playerCam.forward);

        Gizmos.color = Color.green;
        // Draw the ray as a line to visualize the interaction distance
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * interactDistance);
    }
}

