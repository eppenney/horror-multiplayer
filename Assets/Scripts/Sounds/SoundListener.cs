using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SoundListener : MonoBehaviour {
    [SerializeField] private List<UnityEvent> soundEvents = new List<UnityEvent>();
    public void OnSoundHeard(Vector3 soundPosition, float volume = 1.0f)
    {
        ReactToSound();
    }

    private void ReactToSound()
    {
        Debug.Log("Sound heard! Reacting...");
        foreach (var soundEvent in soundEvents)
        {
            soundEvent.Invoke();
        }
    }
} 