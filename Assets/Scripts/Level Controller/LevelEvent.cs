using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class LevelEvent : NetworkBehaviour {
    public UnityEvent eventTrigger = new UnityEvent();

    protected LevelController levelController;

    public virtual void InitializeEvent() {
        // Logic to initialize the event (e.g., set timers, connect to LevelController)
        levelController = FindObjectOfType<LevelController>();
        if (levelController != null) {
            levelController.RegisterEvent(this);
        }
    }

    protected void TriggerEventServer() {
        if (IsServer) {
            TriggerEventClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerEventClientRpc() {
        eventTrigger?.Invoke();
    }
}