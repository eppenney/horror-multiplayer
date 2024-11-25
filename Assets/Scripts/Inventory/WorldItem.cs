using UnityEngine;
using Unity.Netcode;

public class WorldItem : NetworkBehaviour
{
    [SerializeField] private string id_value = "DefaultID"; 
    public string ID { get; private set; }

    void OnEnable()
    {
        ID = id_value;
    }

    void Initialize() {
        
    }
}
