using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // Importing Netcode namespace

public class CanvasInteraction : NetworkBehaviour
{
    [SerializeField] private Canvas targetCanvas; // Reference to the attached canvas
    [SerializeField] private GameObject virtCam;
    private bool active = false;

    void Update()
    {
        // Ensure this logic only runs on the client
        if (!IsClient) return;

        // Check for mouse press
        if (!active && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            EmitRaycast();
        }
        if (active && Input.GetKeyDown(KeyCode.Escape)) {
            active = false;
            virtCam.SetActive(false);
            EnablePlayerMovement();
            SetCursorToLocked();
        }
    }

    private void EmitRaycast()
    {
        // Get the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found.");
            return;
        }

        // Emit a raycast from the center of the screen
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform the raycast
        // Perform the raycast
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object has the same canvas attached
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                SetCursorToWindowed();
                DisablePlayerMovement();
                virtCam.SetActive(true);
                active = true;
            }
            else
            {
                Debug.LogWarning("Raycast hit an object that is not the target canvas.");
            }
        }
        else
        {
            Debug.LogWarning("Raycast did not hit any object.");
        }
    }

    private void SetCursorToWindowed()
    {
        // Change the cursor mode to windowed
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true; // Show the cursor
        Debug.Log("Cursor set to windowed mode due to canvas interaction.");
    }

     private void SetCursorToLocked()
    {
        // Change the cursor mode to windowed
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Show the cursor
        Debug.Log("Cursor set to locked mode due to canvas interaction.");
    }

    private void DisablePlayerMovement() {
        GameObject t_player = BasicManager.Instance.GetPlayerObject();
        t_player.GetComponent<MovementInput>().enabled = false;
        t_player.GetComponent<CameraControl>().enabled = false;
    }

    private void EnablePlayerMovement() {
        GameObject t_player = BasicManager.Instance.GetPlayerObject();
        t_player.GetComponent<MovementInput>().enabled = true;
        t_player.GetComponent<CameraControl>().enabled = true;
    }
}
