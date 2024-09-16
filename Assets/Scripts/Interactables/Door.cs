using UnityEngine;
using Unity.Netcode;

// Alright, throwing some GPT code in and figuring it out post-mordem. (Mortem?)

public class Door : NetworkBehaviour, IInteractable
{

    [SerializeField]
    private float openLimitLeft = -90.0f;
    [SerializeField]
    private float openLimitRight = 90.0f; 
    private Quaternion closedRotation;
    private Quaternion openRotationLeft;
    private Quaternion openRotationRight;
    private NetworkVariable<bool> isDoorOpen = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start() {
        closedRotation = transform.rotation;
        openRotationLeft = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openLimitLeft, 0));
        openRotationRight = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openLimitRight, 0));
        isDoorOpen.OnValueChanged += OnDoorStateChanged; // This is cool. When the value of isDoorOpen changes, it automatically runs OnDoorStateChanged. Hell yeah. 
    }

    [ServerRpc(RequireOwnership = false)] // A signal sent to the server - I think requireOwnership=false means either client or host can send these requests. 
    private void ToggleDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        isDoorOpen.Value = !isDoorOpen.Value;
        UpdateDoorStateClientRpc(isDoorOpen.Value); // ClientRPC sent from ServerRPC? 
    }

    [ClientRpc] // A signal sent from client to server? Unsure about this one 
    private void UpdateDoorStateClientRpc(bool openState)
    {
        if (openState) // Dead simple
        {
            transform.rotation = openRotationLeft; 
        }
        else
        {
            transform.rotation = closedRotation;
        }
    }

    private void OnDoorStateChanged(bool previousValue, bool newValue) // Assuming this needs 2 params because it's being automatically sent OnValueChanged
    {
        if (newValue)
        {
            transform.rotation = openRotationLeft; 
        }
        else
        {
            transform.rotation = closedRotation;
        }
    }
    public void PrimaryUseDown()
    {
        ToggleDoorServerRpc();
    }

    public void PrimaryUseUp()
    {
        Debug.Log("No Interaction");
    }

    public void SecondaryUseDown()
    {
        // Code to close the door
        Debug.Log("Door Grabbed");
    }

    public void SecondaryUseUp()
    {
        // Code to close the door
        Debug.Log("Door Released");
    }

    public void ToolTip() {
        Debug.Log("Press INTERACT to open the door!");
    }
}
