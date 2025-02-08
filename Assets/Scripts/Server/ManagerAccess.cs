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

    public void JoinGame(TMP_InputField joinCodeInputField) {
        BasicManager.Instance.JoinGame(joinCodeInputField);
    }

    public void LoadScene() {
        BasicManager.Instance.LoadScene();
    }
}
