using UnityEngine;

public class SoundListener : MonoBehaviour {
    public void OnSoundHeard(Vector3 soundPosition)
    {
        ReactToSound();
    }

    private void ReactToSound()
    {
        Debug.Log("Sound heard! Reacting...");
    }
} 