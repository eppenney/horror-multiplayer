using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class ManagerAccess : MonoBehaviour
{    
    public void LogoutPlayer()
    {
        BasicManager.Instance.LeaveGame();
    }
    public void HostGame() {
        BasicManager.Instance.HostGame();
    }

    public void LoadScene() {
        BasicManager.Instance.LoadScene();
    }
}
