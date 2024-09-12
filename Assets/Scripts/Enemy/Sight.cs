using UnityEngine;
using System.Collections.Generic;

public class Sight : MonoBehaviour {
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float detectionAngle = 90.0f;
    [SerializeField] private LayerMask detectionLayers;

    public List<Transform> GetVisibleTargets() {
        List<Transform> visibleTargets = new List<Transform>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayers);

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
}