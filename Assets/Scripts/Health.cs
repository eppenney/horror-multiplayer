using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;


// Simple script, manages health, and runs events on death. Only server can manage health 
// death effects are assumed to be managaing their own client-server logic 

public class Health : NetworkBehaviour {
    [SerializeField] private int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    [SerializeField] private UnityEvent deathEvents;

    public int GetHealth() { return currentHealth.Value; }

    public override void OnNetworkSpawn() {
        currentHealth.Value = maxHealth;
    }

    public void AdjustHP(int p_change) {
        // if (!IsServer) { return; }

        // Debug.Log($"Health - {currentHealth.Value}");
        currentHealth.Value += p_change;
        // Debug.Log($"Health - {currentHealth.Value}");
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
        Debug.Log($"Health - {currentHealth.Value}");

        if (currentHealth.Value <= 0) {
            Death();
        }
        Debug.Log($"Health - {currentHealth.Value}");
    }

    // [ServerRpc(RequireOwnership = false)]
    // public void RequestAdjustHPServerRpc(int p_change)
    // {
    //     // The server processes the health adjustment.
    //     AdjustHP(p_change);
    // }

    private void Death()
    {
        Debug.Log("Dead");
        deathEvents.Invoke();
    }
}