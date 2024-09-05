using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public void PrimaryUse()
    {
        // Code to open the door
        Debug.Log("Door opened.");
    }

    public void SecondaryUse()
    {
        // Code to close the door
        Debug.Log("Door closed.");
    }
}
