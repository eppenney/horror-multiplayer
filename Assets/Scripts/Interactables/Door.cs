using UnityEngine;
using Unity.Netcode;

// This should still be switched to an animation controller eventually, but has been majorly paired down with new Interactable script - 24.11.05

public class Door : NetworkBehaviour
{

    [SerializeField] private float openAngle = -90.0f;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private NetworkVariable<bool> isDoorOpen = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Initialize()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
        isDoorOpen.OnValueChanged += OnDoorStateChanged;
    }

    public override void OnNetworkSpawn() {
        Initialize();
    }

    void Start() {
        Initialize();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleDoorServerRpc()
    {
        isDoorOpen.Value = !isDoorOpen.Value;
    }

    private void OnDoorStateChanged(bool previousValue, bool newValue)
    {
        transform.rotation = newValue ? openRotation : closedRotation;
    }
}
