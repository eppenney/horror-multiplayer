using UnityEngine;

/*
Okay, so theoretically the idea here is that if an object is interacted with, 
the client sends a request to the server. On the server side, the events are invoked
and theoretically, each of those should handle their own client-server relations 
so the events don't need to be called on client side. If a component isn't properly 
set up with client-server logic, this won't work. And running the logic on the clients
would only cause issues, as multiple clients would suddenly send requests to open a door. 
*/

public class Interactable : NetworkBehaviour {
    [SerializeField] private UnityEvent[] primaryEventsDown;
    [SerializeField] private UnityEvent[] primaryEventsUp;
    [SerializeField] private UnityEvent[] secondaryEventsDown;
    [SerializeField] private UnityEvent[] secondaryEventsUp;
    [SerializeField] private string tooltip;

    public void PrimaryUseDown() {
        InvokeEventsServerRPC(primaryEventsDown);
    };

    public void PrimaryUseUp() {
        InvokeEventsServerRPC(primaryEventsUp);
    };

    public void SecondaryUseDown() {
        InvokeEventsServerRPC(secondaryEventsDown);
    };

    public void SecondaryUseUp() {
        InvokeEventsServerRPC(secondaryEventsUp);
    };

    public void ToolTip() {
        Debug.Log(tooltip); // Eventually, this should display the tooltip 
    };

    [ServerRpc(RequireOwnership = false)] // This allows clients to call this on the server
    private void InvokeEventsServerRPC(UnityEvent[] p_events)
    {
        InvokeEvents(p_events);
    }

    private void InvokeEvents(UnityEvent[] p_events) {
        foreach (UnityEvent t_event in p_events) {
            try
            {
                t_event.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error invoking event: {ex.Message}");
            }
        }
    }
}