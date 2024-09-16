using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.Netcode;


public class SoundListener : NetworkBehaviour  {
    [SerializeField] private List<UnityEvent> soundEvents = new List<UnityEvent>();
    public void OnSoundHeard(Vector3 soundPosition, float volume = 1.0f)
    {
        if (!IsServer) return;
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