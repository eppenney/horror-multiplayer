using UnityEngine;
using System.Collections.Generic;

public class Sight : MonoBehaviour {
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float detectionAngle = 135.0f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private Color gizmoColor = Color.green;

    public List<Transform> GetVisibleTargets() {
        List<Transform> visibleTargets = new List<Transform>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        foreach (var hit in hitColliders) {
            Vector3 direction = (hit.transform.position - transform.position).normalized;

            float dotProduct = Vector3.Dot(transform.forward, direction);
            float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

            if (angle <= detectionAngle) {
                RaycastHit hitInfo;
                if (Physics.Raycast(transform.position, direction, out hitInfo, detectionRadius)) {
                    if (hitInfo.transform == hit.transform) {
                        visibleTargets.Add(hit.transform);
                    }
                }
            }
        }
        return visibleTargets;
    }

    public bool CanSee(Transform p_target) {
        RaycastHit hit;
        Vector3 directionToTarget = p_target.position - transform.position;

        if (Physics.Raycast(transform.position, directionToTarget, out hit, directionToTarget.magnitude, detectionLayer))
        {
            // If we hit something, check if it's not the target itself
            if (hit.transform == p_target)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = gizmoColor;

        // Draw the detection radius as a wire sphere
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw the detection angle as two lines indicating the edges of the cone
        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRadius;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        int segmentCount = 20; // Number of segments to draw the cone
        float angleIncrement = detectionAngle / segmentCount;
        
        Vector3 previousDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward;
        for (int i = 1; i <= segmentCount; i++) {
            float currentAngle = -detectionAngle / 2 + i * angleIncrement;
            Vector3 currentDirection = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            // Draw lines between each segment to form a cone
            Gizmos.DrawLine(transform.position, transform.position + currentDirection * detectionRadius);
            Gizmos.DrawLine(transform.position + previousDirection * detectionRadius, transform.position + currentDirection * detectionRadius);

            previousDirection = currentDirection;
        }
    }
}