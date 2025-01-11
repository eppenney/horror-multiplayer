using Unity.Netcode;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]

public class MovementInput : NetworkBehaviour 
{
    // Base Movement 
    [SerializeField] private float speed = 10f;
    [SerializeField] private float runSpeed = 15f;
    private bool isRunning = false;

    // Jumping 
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private float gravity = 0.01f;
    [SerializeField] float terminalVelocity = -2.0f;
    private float yVelocity = 0f;

    // Crouching 
    [SerializeField] private float crouchSpeed = 5f;
    private bool isCrouching = false;

    // Movements 
    [SerializeField] private Transform cameraTransform; 
    [SerializeField] private float maxUpAngle = 60f; 
    [SerializeField] private float maxDownAngle = 60f;
    [SerializeField] private float horizontalSensitivity = 2f;     
    [SerializeField] private float verticalSensitivity = 2f; 
    private float verticalRotation = 0f;
    private CharacterController controller;

    private float dX, dZ;
    // Start is called before the first frame update
    private void Initialize()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (cameraTransform == null) {
            try {
                cameraTransform = GetComponent<cameraControl>().GetCamera().transform;
            } catch (NullReferenceException) {

            }
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Player object spawned on client: " + OwnerClientId);
        if (IsOwner)
        {
            Debug.Log("This client owns the player object.");
            Initialize();
        }
        else
        {
            Debug.Log("This client does NOT own the player object.");
        }
    }

    void Start() {
        if (IsOwner) {
            Initialize();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        Inputs();
        Jump();
        Move();
        Turn();
    }

    void Inputs() {
        // Movement Input
        dX = Input.GetAxis("Horizontal");
        dZ = Input.GetAxis("Vertical");

        // Sprint Input
        if (Input.GetButtonDown("Run")) {
            isRunning = true;
        } 
        if (Input.GetButtonUp("Run")) {
            isRunning = false;
        }

        // Crouch Input
        if (Input.GetButtonDown("Crouch")) {
            isCrouching = true;
        } 
        if (Input.GetButtonUp("Crouch")) {
            isCrouching = false;
        }
    }

    void Move() {
        // Move, modified by crouching or running 
        Vector3 direction; 
        if (isCrouching) {
            direction = (transform.right * dX + transform.up * yVelocity + transform.forward * dZ)  * crouchSpeed * Time.deltaTime;
        } else if (isRunning) {
            direction = (transform.right * dX + transform.up * yVelocity + transform.forward * dZ)  * runSpeed * Time.deltaTime;
        } else {
            direction = (transform.right * dX + transform.up * yVelocity + transform.forward * dZ)  * speed * Time.deltaTime;
        }

        controller.Move(direction);
    }

    void Jump() {
        if (controller.isGrounded) {
            yVelocity = -0.1f * Time.deltaTime; // Small amount of gravity to correct isGrounded? Can also try setting min move distance -> 0
        } else if (yVelocity > terminalVelocity) {
            yVelocity -= gravity;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            yVelocity = jumpPower;
        }
    }

    void Turn() {
        // Rotation
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate the character left and right
        transform.Rotate(Vector3.up * mouseX * horizontalSensitivity);

        // Calculate vertical rotation and clamp it
        verticalRotation -= mouseY * verticalSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxDownAngle, maxUpAngle);

        // Apply vertical rotation to the cameraTransform
        cameraTransform.localRotation = Quaternion.Euler(new Vector3(verticalRotation, cameraTransform.localRotation.eulerAngles.y, cameraTransform.localRotation.eulerAngles.z));
    }
}
