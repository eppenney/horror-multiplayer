using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SoundListener : MonoBehaviour {
    [SerializeField] private SoundListener<UnityEvent> soundEvents = new List<UnityEvent>();
    public void OnSoundHeard(Vector3 soundPosition)
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