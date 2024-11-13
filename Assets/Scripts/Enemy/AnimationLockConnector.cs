using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLockConnector : MonoBehaviour
{
    public StateControl stateController;
    // Start is called before the first frame update
    public void Lock()
    {
        stateController.LockAnimationServerRpc();
    }

    // Update is called once per frame
    public void Unlock()
    {
        stateController.UnlockAnimationServerRpc();
    }
}
