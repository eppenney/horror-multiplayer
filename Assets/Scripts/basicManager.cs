using Unity.Netcode;

using UnityEngine;

public class basicManager : MonoBehaviour
{
    private NetworkManager m_NetworkManager;

    void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            StartButtons();
        }
        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
        if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
        if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
    }

    // Update is called once per frame
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
}
