using UnityEngine;

/*
An event that happens on spawn.
Ex] Item population, door locked/unlocked status 
*/
public class SpawnEvent : LevelEvent {
    private void Start() {
        if (IsServer) {
            TriggerEventServer();
        }
    }

    public override void InitializeEvent() {
        base.InitializeEvent();
        // Additional initialization logic if needed
    }
}