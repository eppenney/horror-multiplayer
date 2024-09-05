using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class activateOwnerCamera : NetworkBehaviour
{
    [SerializeField]
    private GameObject cam;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cam.SetActive(true);
            // GameObject camera = Instantiate(cam);
            // cam.GetComponent<CinemachineVirtualCamera>().Follow = transform;
        }
    }
}
