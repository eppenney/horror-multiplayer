using UnityEngine;
using UnityEngine.Events;

/*
This is fine for now, but eventually secondary up/down should toggle a flag
that causes regular raycasts for a target. 
*/

[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject> { }

public class Consumable : Item {
    [SerializeField] private GameObjectEvent onConsume; // UnityEvent for consuming the item
    [SerializeField] private LayerMask targetLayer;     // LayerMask to filter targets during raycast
    [SerializeField] private float raycastDistance = 10f; // Max distance for raycast
    private Transform playerCam;
    private GameObject targetPlayer;

    private void Initialize() {
        if (playerCam == null) playerCam = Camera.main.transform;
    }

    void Start() {
        Initialize();
    }

    public override void SecondaryUseDown() {
        RaycastHit hit;
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        if (Physics.Raycast(ray, out hit, raycastDistance, targetLayer)) {
            targetPlayer = hit.collider.gameObject;
            Debug.Log($"Target acquired: {targetPlayer.name}");
        } else {
            targetPlayer = null;
            Debug.Log("No target found.");
        }
    }

    public override void SecondaryUseUp() {
        // Stop targeting
        targetPlayer = null;
        Debug.Log("Stopped targeting.");
    }

    public override void PrimaryUseDown() {
        // Invoke the event, either targeting self or another player
        if (targetPlayer != null) {
            Debug.Log($"Using consumable on target: {targetPlayer.name}");
            onConsume.Invoke(targetPlayer);
        } else {
            Debug.Log("Using consumable on self.");
            onConsume.Invoke(gameObject); // Use on self
        }
    }
}