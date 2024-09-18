using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.Netcode;

[System.Serializable]
public class SoundEvent : UnityEvent<GameObject, float> { }

public class SoundListener : NetworkBehaviour  {
    [SerializeField] private List<SoundEvent> soundEvents = new List<UnityEvent>();
    public void OnSoundHeard(Transform source, float volume = 1.0f)
    {
        if (!IsServer) return;
        ReactToSound(source, volume);
    }

    private void ReactToSound(Transform source, float volume)
    {
        Debug.Log("Sound heard! Reacting...");
        foreach (var soundEvent in soundEvents)
        {
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
} 