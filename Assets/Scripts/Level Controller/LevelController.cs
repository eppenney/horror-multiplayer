using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Events;

/* 
One of these exists, and manages all spawn events on spawn.
It also keeps track of any proximity events, and timed events.
The level controller determines which of these are created,
and spreads out times as per the scriptable difficulty object. 
Should have a list of connected level events. 
*/
public class LevelController : NetworkBehaviour {
    public List<LevelEvent> connectedLevelEvents = new List<LevelEvent>();
    public ScriptableObject difficultyObject; // Placeholder for difficulty settings
    
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        // Initialize or activate level events
        foreach (var levelEvent in connectedLevelEvents) {
            levelEvent.InitializeEvent();
        }
    }

    public void RegisterEvent(LevelEvent levelEvent) {
        connectedLevelEvents.Add(levelEvent);
    }
}