using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

// This should still be switched to an animation controller eventually, but has been majorly paired down with new Interactable script - 24.11.05

public class Generator : NetworkBehaviour
{
    [SerializeField] private float maxCapacity = 100.0f;
    [SerializeField] private NetworkVariable<float> currentCapacity = new NetworkVariable<float>(100.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float drainRate = 0.1f;
    [SerializeField] private UnityEvent onEmpty;    

    private void Initialize()
    {

    }

    public override void OnNetworkSpawn() {
        Initialize();
    }

    void Start() {
        Initialize();
    }

    public void Update() {
        if (!IsServer) { return; }
        if (currentCapacity.Value > -1.0f) {
            currentCapacity.Value -= drainRate * Time.deltaTime;
            if (currentCapacity.Value <= 0.0f) {
                OnEmpty();
                currentCapacity.Value = -1.0f;
            }
        }
    }

    public void Fill(float p_amount) {
        FillServerRPC(p_amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FillServerRPC(float p_amount)
    {
        if (!IsServer) { return; }
        currentCapacity.Value += p_amount;
    }

    public void OnEmpty() {
        onEmpty.Invoke();
    }
}
