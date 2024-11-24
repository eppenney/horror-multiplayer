using UnityEngine;

public class Item : MonoBehaviour {
    [SerializeField] private string defaultID = "DefaultID"; 
    public string ID { get; private set; }

    void OnEnable()
    {
        ID = defaultID;
    }

    public virtual void PrimaryUseUp() {}
    public virtual void PrimaryUseDown() {}
    public virtual void SecondaryUseUp() {}
    public virtual void SecondaryUseDown() {}
}
