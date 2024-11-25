using UnityEngine;

public class Item : MonoBehaviour {
    [SerializeField] private string id_value = "DefaultID"; 
    public string ID { get; private set; }

    void OnEnable()
    {
        ID = id_value;
    }

    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}
