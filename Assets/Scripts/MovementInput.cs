using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class MovementInput : NetworkBehaviour 
{
    [SerializeField]
    private float speed = 0.1f;
    private CharacterController controller;
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
        }
        else
        {
            Debug.Log("This client does NOT own the player object.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move() {
        if (!IsOwner) return;
        // Debug.Log("Moving");
        if (Input.GetButtonDown("Jump")) {
            Debug.Log("Test"); 
        }

        float dX = Input.GetAxis("Horizontal");
        float dZ = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(dX, 0.0f, dZ) * speed * Time.deltaTime;
        controller.Move(direction);
    }
}
