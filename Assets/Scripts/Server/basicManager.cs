using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Core;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using TMPro;
 

public class basicManager : MonoBehaviour
{
    private NetworkManager m_NetworkManager;
    [SerializeField] private string m_sceneName; 

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();

        // Initialize Unity services
        // await UnityServices.InitializeAsync();
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
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            try {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log("Join Code:" + joinCode);

                var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(allocation, "dtls");
                m_NetworkManager.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(relayServerData);

                m_NetworkManager.StartHost();
            }
            catch (RelayServiceException e) {
                Debug.LogError(e);
            }
        }
    }

    public async void JoinGame(string joinCode) {
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(joinAllocation, "dtls");

                m_NetworkManager.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(relayServerData);

                m_NetworkManager.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    public async void JoinGame(TMP_InputField joinCodeInputField) {
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            string joinCode = joinCodeInputField.text.Trim();

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogWarning("Join code is empty. Please enter a valid join code.");
                return;
            }
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var relayServerData = new Unity.Networking.Transport.Relay.RelayServerData(joinAllocation, "dtls");

                m_NetworkManager.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetRelayServerData(relayServerData);

                m_NetworkManager.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    private async Task WaitUntilNetworkIsReady()
    {
        // Wait until the network is ready (host or client is connected)
        while (!m_NetworkManager.IsListening)
        {
            await Task.Delay(100); // Poll every 100ms
        }
        Debug.Log("Network is ready.");
    }

    public async void LoadScene() {
        await WaitUntilNetworkIsReady();
        if (m_NetworkManager.IsServer && !string.IsNullOrEmpty(m_sceneName))
      {
          var status = m_NetworkManager.SceneManager.LoadScene(m_sceneName, LoadSceneMode.Single);
          if (status != SceneEventProgressStatus.Started)
          {
              Debug.LogWarning($"Failed to load {m_sceneName} " +
                    $"with a {nameof(SceneEventProgressStatus)}: {status}");
          }
      }
    }
}