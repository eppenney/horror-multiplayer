using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Logout : MonoBehaviour
{
    [SerializeField] private string lobbyScene;
    // Method to logout the player and return to the lobby scene
    public void LogoutPlayer()
    {
        // Find the NetworkManager object in the scene
        NetworkManager networkManager = FindObjectOfType<NetworkManager>();

        // Ensure the NetworkManager is found
        if (networkManager != null)
        {
            // Check if this instance is the server or a client and shut down accordingly
            if (networkManager.IsServer || networkManager.IsClient)
            {
                networkManager.Shutdown();
                Debug.Log("Network session shut down successfully.");
            }
            else
            {
                Debug.LogWarning("NetworkManager is found, but not running as a server or client.");
            }
        }
        else
        {
            Debug.LogError("NetworkManager not found in the scene.");
           
        }

        // Load the lobby scene (ensure the scene name is correct)
        SceneManager.LoadScene(lobbyScene);
        Cursor.lockState = CursorLockMode.Confined;
    }
}
