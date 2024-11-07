using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;


// Simple script, manages health, and runs events on death. Only server can manage health 
// death effects are assumed to be managaing their own client-server logic 

public class Health : NetworkBehaviour {
    [SerializeField] private int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    [SerializeField] private UnityEvent[] onDeathEvents;

    public int GetHealth() { return currentHealth.Value; }

    private void Awake() {
        if (IsServer) {
            currentHealth.Value = maxHealth;
        }
    }

    public void AdjustHP(int p_change) {
        if (!IsServer) { return; }

        currentHealth.Value += p_change;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);

        if (currentHealth.Value <= 0) {
            Death();
        }
    }

    private void Death()
    {
        foreach (UnityEvent deathEvent in onDeathEvents)
        {
            deathEvent.Invoke();
        }
    }
}