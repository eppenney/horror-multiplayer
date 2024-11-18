using UnityEngine;

/*
An event that happens when a player or optionally, monster comes within range.
Can be repeatable, and may be certain or random.
Ex] Pipe bursting, bridge breaking, spooky sound, etc. 
*/
public class ProximityEvent : LevelEvent {
    public float detectionRadius = 5f;
    public LayerMask detectionLayer;
    public bool isRepeatable = false;
    private bool hasTriggered = false;

    private void Update() {
        if (IsServer && (!hasTriggered || isRepeatable)) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);
            if (hitColliders.Length > 0) {
                TriggerEventServer();
                hasTriggered = true;
            }
        }
    }

    public override void InitializeEvent() {
        base.InitializeEvent();
        // Additional initialization logic if needed
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}