using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class Interactable : NetworkBehaviour {
    [SerializeField] private UnityEvent primaryEventsDown;
    [SerializeField] private UnityEvent primaryEventsUp;
    [SerializeField] private UnityEvent secondaryEventsDown;
    [SerializeField] private UnityEvent secondaryEventsUp;
    [SerializeField] private string tooltip;

    public void PrimaryUseDown() {
        InvokePrimaryEventsDownServerRPC();
    }

    public void PrimaryUseUp() {
        InvokePrimaryEventsUpServerRPC();
    }

    public void SecondaryUseDown() {
        InvokeSecondaryEventsDownServerRPC();
    }

    public void SecondaryUseUp() {
        InvokeSecondaryEventsUpServerRPC();
    }

    public void ToolTip() {
        Debug.Log(tooltip); // Eventually, this should display the tooltip 
    }

    [ServerRpc(RequireOwnership = false)]
    private void InvokePrimaryEventsDownServerRPC() {
        InvokeEvents(primaryEventsDown);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InvokePrimaryEventsUpServerRPC() {
        InvokeEvents(primaryEventsUp);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InvokeSecondaryEventsDownServerRPC() {
        InvokeEvents(secondaryEventsDown);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InvokeSecondaryEventsUpServerRPC() {
        InvokeEvents(secondaryEventsUp);
    }

    private void InvokeEvents(UnityEvent p_events) {
        try {
            p_events.Invoke();
        } catch (System.Exception ex) {
            Debug.LogError($"Error invoking event: {ex.Message}");
        }
    }
}
