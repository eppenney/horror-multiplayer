using System.Collections;
using UnityEngine;

public class FootstepManager : MonoBehaviour {
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private float runInterval = 0.25f;
    [SerializeField] private float walkVolume = 0.5f;
    [SerializeField] private float walkSoundRange = 2.0f;
    [SerializeField] private float runVolume = 0.7f;
    [SerializeField] private float runSoundRange = 4.0f;
    
    private CharacterController characterController;
    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private SoundSource soundSource;

    private void Start() {
        characterController = GetComponent<CharacterController>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        soundSource = GetComponent<SoundSource>();
        StartCoroutine(PlayFootstepSounds());
    }

    private IEnumerator PlayFootstepSounds() {
        while (true) {
            if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f) {
                bool running = false;
                if (characterController != null) {
                    running = characterController.velocity.magnitude > 5f;
                }
                else if (navMeshAgent != null) {
                    running = navMeshAgent.velocity.magnitude > 5f;
                }

                float currentInterval = running ? runInterval : footstepInterval;
                float volume = running? runVolume : walkVolume;

                AudioClip clip = clips[Random.Range(0, clips.Length)];
                audioSource.PlayOneShot(clip, volume);

                soundSource.EmitSound(running ? runSoundRange : walkSoundRange, running ? runVolume : walkVolume);

                yield return new WaitForSeconds(currentInterval);
            }
            else
            {
                yield return null; // Wait until the next frame to check again
            }
        }
    }
}