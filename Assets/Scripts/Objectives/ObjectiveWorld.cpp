/*
This should be a component added to an in-world object.
This needs a function such that when it is first seen or interacted with (toggle or enum) it 
activates a function on its UI counterpart (a serialized, private ObjectiveUI object)
This should have public methods to mark itself as not started, completed, or in-progress. 
Each of these methods should call the same function in it's UI counterpart, 
and invoke a private serialized UnityEvent for each one. 
*/
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveWorld : MonoBehaviour
{
    [SerializeField] private ObjectiveUI objectiveUI;

    [SerializeField] private UnityEvent onNotStarted;
    [SerializeField] private UnityEvent onInProgress;
    [SerializeField] private UnityEvent onCompleted;

    private bool hasBeenInteractedWith = false;

    public void MarkNotStarted()
    {
        objectiveUI?.SetNotStarted();
        onNotStarted?.Invoke();
    }

    public void MarkInProgress()
    {
        objectiveUI?.SetInProgress();
        onInProgress?.Invoke();
    }

    public void MarkCompleted()
    {
        objectiveUI?.SetCompleted();
        onCompleted?.Invoke();
    }

    public void OnFirstSeenOrInteracted()
    {
        if (!hasBeenInteractedWith)
        {
            hasBeenInteractedWith = true;
            objectiveUI?.OnWorldObjectInteracted();
        }
    }
}