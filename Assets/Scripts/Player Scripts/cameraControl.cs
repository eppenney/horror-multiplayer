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
    float currentYRotation = 0.0f;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        camera = Instantiate(cam);
        camera.transform.SetParent(transform);
        camera = camera.transform.GetChild(0).gameObject;
        camera.GetComponent<CinemachineVirtualCamera>().Follow = transform;
        cinemachineCameraOffset = camera.GetComponent<CinemachineCameraOffset>();
        originalPosition = camera.transform.position;
    }

    public GameObject GetCamera() {
        return camera; 
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
        /*
        if (isTurning)
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(camera.transform.localRotation.x, turnAngle.y, camera.transform.localRotation.x));
            if (camera.transform.localRotation.y < turnAngle.y){
                // camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, targetRotation, turnSpeed * Time.deltaTime);
                camera.transform.localRotation = Quaternion.Euler(new Vector3(camera.transform.localRotation.x, camera.transform.localRotation.y + turnSpeed, camera.transform.localRotation.z));
            }
        } else {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(camera.transform.localRotation.x, 0.0f, camera.transform.localRotation.x));
            if (Quaternion.Angle(camera.transform.rotation, targetRotation) > turnThreshold)
            {
                //camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, targetRotation, turnSpeed * Time.deltaTime);
                camera.transform.localRotation = Quaternion.Euler(new Vector3(camera.transform.localRotation.x, camera.transform.localRotation.y - turnSpeed * Time.deltaTime, camera.transform.localRotation.z));
            }
        }
        */
        float targetYRotation = isTurning ? turnAngle.y : 0.0f;
        float rotationDifference = Mathf.Abs(currentYRotation - targetYRotation);

        if (rotationDifference > turnThreshold)
        {
            // Smoothly interpolate the current Y rotation towards the target using Lerp
            currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, turnSpeed * Time.deltaTime);
        }

        // Apply the new Y rotation while keeping the X and Z rotations unchanged
        Quaternion targetRotation = Quaternion.Euler(camera.transform.localRotation.eulerAngles.x, currentYRotation, camera.transform.localRotation.eulerAngles.z);
        camera.transform.localRotation = targetRotation;
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

