using TMPro;
using UnityEngine;
using System.Collections;

public class SetJoinCodeText : MonoBehaviour
{
    public TextMeshProUGUI joinCodeText;  

    void Start()
    {
        StartCoroutine(WaitForJoinCode());
    }

    IEnumerator WaitForJoinCode()
    {
        while (BasicManager.Instance == null || string.IsNullOrEmpty(BasicManager.Instance.joinCode))
        {
            yield return null;  
        }

        joinCodeText.text = BasicManager.Instance.joinCode;
    }
}
