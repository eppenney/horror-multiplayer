using UnityEngine;

/*
An event that occurs after a random or predetermined time. 
Ex] Power failing, alarm going off, etc. 
*/
public class TimedEvent : LevelEvent {
    public float timeToTrigger = 10f;
    private float timer;

    private void Start() {
        timer = timeToTrigger;
    }

    private void Update() {
        if (IsServer && timer > 0) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                TriggerEventServer();
            }
        }
    }

    public override void InitializeEvent() {
        base.InitializeEvent();
        // Additional initialization logic if needed
    }
}