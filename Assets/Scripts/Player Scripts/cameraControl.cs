using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class cameraControl : NetworkBehaviour
{
    [SerializeField]
    private GameObject cam;
    private GameObject camera;
    [SerializeField]
    private Vector3 leanOffset;
    [SerializeField]
    private float leanThreshold = 0.05f;
    private Vector3 originalPosition;
    [SerializeField]
    private float leanSpeed = 5.0f;
    [SerializeField]
    private Vector3 turnAngle;
    [SerializeField]
    private float turnSpeed = 5.0f;
    [SerializeField]
    private float turnThreshold = 0.05f;
    private CinemachineCameraOffset cinemachineCameraOffset;
    private bool isLeaningLeft, isLeaningRight, isTurning;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        camera = Instantiate(cam);
        camera.GetComponent<CinemachineVirtualCamera>().Follow = transform;
        cinemachineCameraOffset = camera.GetComponent<CinemachineCameraOffset>();
        originalPosition = camera.transform.position;
    }

    void Update() {
        if (!IsOwner) { return; }
        Inputs();
        Lean();
        Turn();
    }
    
    void Lean() {
        if (((isLeaningLeft && isLeaningRight) || !(isLeaningLeft || isLeaningRight))) {
            if (Vector3.Distance(cinemachineCameraOffset.m_Offset, originalPosition) < leanThreshold) {
                cinemachineCameraOffset.m_Offset = originalPosition;
            } else {
                cinemachineCameraOffset.m_Offset = Vector3.Lerp(cinemachineCameraOffset.m_Offset, originalPosition, leanSpeed * Time.deltaTime);
            }
        }else if (isLeaningLeft) {
            cinemachineCameraOffset.m_Offset = Vector3.Lerp(cinemachineCameraOffset.m_Offset, new Vector3(-leanOffset.x, cinemachineCameraOffset.m_Offset.y, cinemachineCameraOffset.m_Offset.z), leanSpeed * Time.deltaTime);
        }else if (isLeaningRight) {
            cinemachineCameraOffset.m_Offset = Vector3.Lerp(cinemachineCameraOffset.m_Offset, new Vector3(leanOffset.x, cinemachineCameraOffset.m_Offset.y, cinemachineCameraOffset.m_Offset.z), leanSpeed * Time.deltaTime);
        }
    }

    void Turn() {
        if (isTurning)
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(camera.transform.rotation.x, turnAngle.y, camera.transform.rotation.x));
            camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        } else {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(camera.transform.rotation.x, 0.0f, camera.transform.rotation.x));
            camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void Inputs() {
        // Leaning Input
        if (Input.GetButtonDown("Lean Left")) {
            isLeaningLeft = true;
        }
        if (Input.GetButtonUp("Lean Left")) {
            isLeaningLeft = false;
        }
        if (Input.GetButtonDown("Lean Right")) {
            isLeaningRight = true;
        }
        if (Input.GetButtonUp("Lean Right")) {
            isLeaningRight = false;
        }

        // Turn Input 
        if (Input.GetButtonDown("Turn")) {
            isTurning = true;
        }
        if (Input.GetButtonUp("Turn")) {
            isTurning = false;
        }
    }
}

