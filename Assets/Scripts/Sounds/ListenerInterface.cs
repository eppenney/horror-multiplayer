using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ListenerInterface
{
    public void OnSoundHeard(Transform source, float volume);
}
