/*
ObjectiveUI should be a component added to a UI gameobject. 
It should have public functions to set itself as not started, completed, or in-progress
and each of these methods should call the appropriate function in it's world counterpart 
and invoke a unityEvent. 
*/
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private ObjectiveWorld objectiveWorld;

    [SerializeField] private UnityEvent onNotStarted;
    [SerializeField] private UnityEvent onInProgress;
    [SerializeField] private UnityEvent onCompleted;

    public void SetNotStarted()
    {
        objectiveWorld?.MarkNotStarted();
        onNotStarted?.Invoke();
    }

    public void SetInProgress()
    {
        objectiveWorld?.MarkInProgress();
        onInProgress?.Invoke();
    }

    public void SetCompleted()
    {
        objectiveWorld?.MarkCompleted();
        onCompleted?.Invoke();
    }

    public void OnWorldObjectInteracted()
    {
        // Logic to handle interaction from the world object.
    }
}