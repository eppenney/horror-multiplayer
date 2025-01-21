using UnityEngine;
using Unity.Netcode;
using System;

public class FollowHead : NetworkBehaviour {
    [SerializeField] private CameraControl m_cameraControl; 
    [SerializeField] private Vector3 rotationMultiplier = new Vector3(0, 0, 1);
    private Transform cameraTransform; 
    private void Initialize()
    {
        if (cameraTransform == null) {
            try {
                cameraTransform = m_cameraControl.GetCamera().transform;
            } catch (NullReferenceException) {
                Debug.LogWarning("Error initializing followhead script");
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Initialize();
        }
    }

    void Start() {
        if (IsOwner) {
            Initialize();
        }
    }

    void Update() {
        if (cameraTransform != null) {
            Vector3 eulerRotation = cameraTransform.rotation.eulerAngles;
            eulerRotation = Vector3.Scale(eulerRotation, rotationMultiplier); // Multiply by the rotationMultiplier
            transform.localRotation = Quaternion.Euler(eulerRotation);
        } else {
            Initialize();
        }
    }
}