using UnityEngine;

public class Gun : Item {
    [SerializeField] private LayerMask targetLayer; 
    [SerializeField] private float maxBullets = 6.0f;
    [SerializeField] private float currentBullets = 6.0f; 
    [SerializeField] private float bulletDistance = 100.0f; 
    [SerializeField] private Vector2 damageRange = new Vector2(10, 40);
    private Transform playerCam;

    private void Initialize() {
        if (playerCam == null) playerCam = Camera.main.transform;
    }

    void Start() {
        Initialize();
    }

    public override void PrimaryUseDown() {
        RaycastHit hit;
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        Debug.Log("Ray Sent");
        if (Physics.Raycast(ray, out hit, bulletDistance, targetLayer)) {
            Debug.Log("Target hit");
            Health hp = hit.transform.gameObject.GetComponent<Health>();
            if (hp != null) {
                hp.AdjustHP((int) Random.Range(damageRange.x, damageRange.y));
            }
        }
    }

    public override void PrimaryUseUp() {
        // Stop Aiming
    }

    public override void SecondaryUseDown() {
        // Aim
    }

    public override void SecondaryUseUp() {
        // Stop Aiming
    }
}