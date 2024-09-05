using UnityEngine;

public interface IInteractable
{
    // Method to be called for primary interaction (e.g., opening a door)
    void PrimaryUse();

    // Method to be called for secondary interaction (e.g., grabbing a door)
    void SecondaryUse();
}
