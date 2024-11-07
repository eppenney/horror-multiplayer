using Unity.Netcode;
using UnityEngine;

public class basicManager : MonoBehaviour
{
    private NetworkManager m_NetworkManager;

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
}
