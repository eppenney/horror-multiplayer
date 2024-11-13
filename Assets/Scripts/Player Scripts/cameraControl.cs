using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class cameraControl : NetworkBehaviour
{
    [SerializeField]
    private GameObject cam;
    private GameObject m_cam;
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
    private void Initialize()
    {
        if (!IsOwner) { return; }
        if (m_cam == null) {
            m_cam = Instantiate(cam);
            m_cam.transform.SetParent(transform);
            m_cam = m_cam.transform.GetChild(0).gameObject;
            m_cam.GetComponent<CinemachineVirtualCamera>().Follow = transform;
            cinemachineCameraOffset = m_cam.GetComponent<CinemachineCameraOffset>();
            originalPosition = m_cam.transform.position;
        }
    }

    public override void OnNetworkSpawn() {
        Initialize();
    }

    void Start() {
        Initialize();
    }

    public GameObject GetCamera() {
        return m_cam; 
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
        float targetYRotation = isTurning ? turnAngle.y : 0.0f;
        float rotationDifference = Mathf.Abs(currentYRotation - targetYRotation);

        if (rotationDifference > turnThreshold)
        {
            // Smoothly interpolate the current Y rotation towards the target using Lerp
            currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, turnSpeed * Time.deltaTime);
        }

        // Apply the new Y rotation while keeping the X and Z rotations unchanged
        Quaternion targetRotation = Quaternion.Euler(m_cam.transform.localRotation.eulerAngles.x, currentYRotation, m_cam.transform.localRotation.eulerAngles.z);
        m_cam.transform.localRotation = targetRotation;
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

