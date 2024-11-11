using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class LightSwitchController : NetworkBehaviour
{
    [SerializeField] private List<Light> lights;

    public void ToggleLights()
    {
        if (IsServer)
        {
            ToggleLightsOnServer();
        }
        else
        {
            ToggleLightsServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleLightsServerRpc()
    {
        ToggleLightsOnServer();
    }

    private void ToggleLightsOnServer()
    {
        bool anyLightOn = lights.Exists(light => light.enabled);
        foreach (Light light in lights)
        {
            light.enabled = !anyLightOn;
        }

        ToggleLightsClientRpc(!anyLightOn);
    }

    [ClientRpc]
    private void ToggleLightsClientRpc(bool state)
    {
        foreach (Light light in lights)
        {
            light.enabled = state;
        }
    }
}
