using UnityEngine;

public class SoundSource : MonoBehaviour {
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float volume = 1f;
    public void EmitSound() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (var hit in hitColliders) {
            SoundListener soundListener = hit.GetComponent<SoundListener>();

            if (soundListener != null)
            {
                soundListener.OnSoundHeard(transform.position, volume);
            }
        }
    }
}