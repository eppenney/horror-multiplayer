using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class basicManager : MonoBehaviour
{
    private NetworkManager m_NetworkManager;
    [SerializeField] private string m_sceneName; 

    void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
    }

    public void HostGame()
    {
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            m_NetworkManager.StartHost();
        }
    }

    public void JoinGame() {
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            m_NetworkManager.StartClient();
        }
    }

    public void LoadScene() {
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