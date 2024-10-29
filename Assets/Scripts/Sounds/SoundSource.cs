using UnityEngine;
using Unity.Netcode;

public class SoundSource : NetworkBehaviour {
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float volume = 1f;

    public void EmitSound(float p_radius = -1.0f, float p_volume = -1.0f) {
        if (p_radius == -1) { p_radius = detectionRadius; }
        if (p_volume == -1) { p_volume = volume; }
        
        if (!IsServer) return;
        Debug.Log("RPC hearb by server - Emitting Sound");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, p_radius);

        foreach (var hit in hitColliders) {
            SoundListener soundListener = hit.GetComponent<SoundListener>();

            if (soundListener != null)
            {
                soundListener.OnSoundHeard(transform, p_volume);
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
        // Debug.Log("Checking...");
        // Debug.Log("IsHost: " + IsHost);
        // Debug.Log("IsServer: " + IsServer);
        // Debug.Log("IsClient: " + IsClient);
        if (IsServer) {
            Debug.Log("Server emitting sound");
            EmitSound();
        } else {
            Debug.Log("Request Heard, Sending RPC");
            RequestEmitSoundServerRpc();  // Client sends the request to the server
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, detectionRadius); 
    }
}