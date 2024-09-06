using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class MovementInput : NetworkBehaviour 
{
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float runSpeed = 15f;
    private bool isRunning = false;
    [SerializeField]
    private float jumpPower = 1f;
    [SerializeField]
    private float gravity = 0.01f;
    [SerializeField]
    float terminalVelocity = -2.0f;
    private float jumpVelocity = 0f;
    [SerializeField]
    private float crouchSpeed = 5f;
    private bool isCrouching = false;
    [SerializeField]
    private float turnSensitivity = 5.0f;
    private CharacterController controller;

    private float dX, dZ;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Player object spawned on client: " + OwnerClientId);
        if (IsOwner)
        {
            Debug.Log("This client owns the player object.");
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Debug.Log("This client does NOT own the player object.");
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
            direction = (transform.right * dX + transform.up * jumpVelocity + transform.forward * dZ)  * crouchSpeed * Time.deltaTime;
        } else if (isRunning) {
            direction = (transform.right * dX + transform.up * jumpVelocity + transform.forward * dZ)  * runSpeed * Time.deltaTime;
        } else {
            direction = (transform.right * dX + transform.up * jumpVelocity + transform.forward * dZ)  * speed * Time.deltaTime;
        }

        controller.Move(direction);
    }

    void Jump() {
        if (controller.isGrounded) {
            jumpVelocity = 0;
        } else if (jumpVelocity > terminalVelocity) {
            jumpVelocity -= gravity;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            jumpVelocity = jumpPower;
        }
    }

    void Turn() {
        // Rotation
        dX = Input.GetAxis("Mouse X");
        var rotationY = dX * turnSensitivity * Time.deltaTime * 100.0f;
        transform.Rotate(new Vector3(0.0f, rotationY, 0.0f));
    }
}
