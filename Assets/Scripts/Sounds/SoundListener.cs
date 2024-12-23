using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.Netcode;

[System.Serializable]
public class SoundEvent : UnityEvent<Transform, float> { }

public class SoundListener : NetworkBehaviour, ListenerInterface  {
    [SerializeField] private SoundEvent soundEvent;
    public void OnSoundHeard(Transform source, float volume = 1.0f)
    {
        if (!IsServer) return;
        ReactToSound(source, volume);
    }

    private void ReactToSound(Transform source, float volume)
    {
        // Debug.Log("Sound heard! Reacting...");
        try
        {
            soundEvent.Invoke(source, volume);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error invoking sound event: {ex.Message}");
        }
    }
} 