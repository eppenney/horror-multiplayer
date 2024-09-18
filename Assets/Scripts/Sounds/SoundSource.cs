using UnityEngine;
using Unity.Netcode;

public class SoundSource : NetworkBehaviour {
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float volume = 1f;
    public void EmitSound() {
        Debug.Log("RPC  Heard");
        if (!IsServer) return;
        Debug.Log("Emitting Sound");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (var hit in hitColliders) {
            SoundListener soundListener = hit.GetComponent<SoundListener>();

            if (soundListener != null)
            {
                soundListener.OnSoundHeard(transform, volume);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]  // Allow clients to request the server to emit sound
    public void RequestEmitSoundServerRpc() {
        Debug.Log("RPC Sent");
        EmitSound();  // Server processes the sound emission
    }

    // Call this method from a client to request the server to emit sound
    public void RequestSoundFromClient() {
        Debug.Log("Checking...");
        Debug.Log("IsHost: " + IsHost);
        Debug.Log("IsServer: " + IsServer);
        Debug.Log("IsClient: " + IsClient);
        if (IsClient || IsHost) {
            Debug.Log("Request Heard, Sending RPC");
            RequestEmitSoundServerRpc();  // Client sends the request to the server
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, detectionRadius); 
    }
}