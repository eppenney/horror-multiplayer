using UnityEngine;
using Unity.Netcode;

public class Door : NetworkBehaviour, IInteractable
{
    [SerializeField]
    private float doorAutoSpeed = 1.0f;
    [SerializeField]
    private float doorManualSpeed = 0.2f;
    [SerializeField]
    private float openLimitLeft = -90.0f;
    [SerializeField]
    private float openLimitRight = 90.0f; 
    [SerializeField]
    private bool facingRight = true; 
    private bool grabbed = false;
    private float position, currentPosition = 0.0f;
    private Quaternion closedRotation;
    private Quaternion openRotationLeft;
    private Quaternion openRotationRight;
    private Vector3 grabOffset;

    void Start() {
        closedRotation = transform.rotation;
        openRotationLeft = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openLimitLeft, 0));
        openRotationRight = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openLimitRight, 0));
    }
    public void PrimaryUseDown()
    {
        // Code to open the door
        Debug.Log("Door toggle.");
        if (transform.rotation != closedRotation) {
            transform.rotation = closedRotation;
        } else {
            /*Vector3 playerDirection = (transform.position - player.position).normalized;
            float dotProduct = Vector3.Dot(playerDirection, transform.forward);

            if (dotProduct > 0)
            {
                transform.rotation = openRotationLeft;
            }
            else
            {
                transform.rotation = openRotationRight;
            }
            */
        }
    }

    public void PrimaryUseUp()
    {
        Debug.Log("No Interaction");
    }

    public void SecondaryUseDown()
    {
        // Code to close the door
        grabbed = true;
        Debug.Log("Door Grabbed");
    }

    public void SecondaryUseUp()
    {
        // Code to close the door
        grabbed = false;
        Debug.Log("Door Released");
    }

    public void ToolTip() {
        Debug.Log("Press INTERACT to open the door!");
    }
}
