using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class cameraControl : NetworkBehaviour
{
    [SerializeField]
    private GameObject cam;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        var camera = Instantiate(cam);
        camera.GetComponent<CinemachineVirtualCamera>().Follow = transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
