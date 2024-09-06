using UnityEngine;

public interface IInteractable
{
    // Method to be called for primary interaction (e.g., opening a door)
    void PrimaryUseDown();

    void PrimaryUseUp();

    // Method to be called for secondary interaction (e.g., grabbing a door)
    void SecondaryUseDown();

    void SecondaryUseUp();

    void ToolTip();
}
