using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Toggle : Item
{
    [SerializeField] private bool toggle;
    [SerializeField] private UnityEvent toggleOn;
    [SerializeField] private UnityEvent toggleOff;
    
    private void Initialize() {

    }

    void Start() {
        Initialize();
    }

    public override void PrimaryUseDown() {

    }

    public override void PrimaryUseUp() {
        // Stop Aiming
    }

    public override void SecondaryUseDown() {
        toggle = !toggle;
        if (toggle) {
            toggleOn.Invoke();
        } else {
            toggleOff.Invoke();
        }
    }

    public override void SecondaryUseUp() {
        // Stop Aiming
    }
}
