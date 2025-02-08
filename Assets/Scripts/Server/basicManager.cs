using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using TMPro;
 

public class BasicManager : MonoBehaviour
{
    public static BasicManager Instance { get; private set; }
    [SerializeField] private string m_sceneName; 
    public string joinCode {get; private set;}

    private async void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
        
        // Initialize Unity services
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in successfully.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize or sign in: {e.Message}");
        }
    }

    public async void HostGame()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            try {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log("Join Code:" + joinCode);

                var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartHost();
            }
            catch (RelayServiceException e) {
                Debug.LogError(e);
            }
        }
    }

    public async Task JoinGame(string p_joinCode) {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(p_joinCode);

                var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(joinAllocation, "dtls");

                NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    public async void JoinGame(TMP_InputField joinCodeInputField) {
        string joinCode = joinCodeInputField.text.Trim();
        await JoinGame(joinCode);
    }

    private async Task WaitUntilNetworkIsReady()
    {
        // Wait until the network is ready (host or client is connected)
        while (!NetworkManager.Singleton.IsListening)
        {
            await Task.Delay(100); // Poll every 100ms
        }
        Debug.Log("Network is ready.");
    }

    public async void LoadScene() {
        await WaitUntilNetworkIsReady();
        if (NetworkManager.Singleton.IsServer && !string.IsNullOrEmpty(m_sceneName))
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(m_sceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {m_sceneName} " +
                        $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
    }

    public void LeaveGame()
    {
        // Check if the NetworkManager is active
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            // Shut down the NetworkManager
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Network session ended.");
        }

        // Reset the join code (optional, for UI purposes)
        joinCode = null;

        // Load the main menu or default scene
        SceneManager.LoadScene("Menu"); // Replace "MainMenu" with your main menu scene name
    }

    public GameObject GetPlayerObject(ulong clientId)
    {
        foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (networkObject.OwnerClientId == clientId)
            {
                return networkObject.gameObject;
            }
        }
        Debug.LogWarning($"No player object found for client ID {clientId}.");
        return null;
    }

    public GameObject GetPlayerObject()
    {
        return GetPlayerObject(NetworkManager.Singleton.LocalClientId);
    }
}