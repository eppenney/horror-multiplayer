using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class cameraControl : NetworkBehaviour
{
    [SerializeField]
    private GameObject cam;
    [SerializeField]
    private Vector3 leanOffset;
    [SerializeField]
    private float leanSpeed;
    [SerializeField]
    private Vector3 turnAngle;
    [SerializeField]
    private float turnSpeed;


    private bool isLeaningLeft, isLeaningRight, isTurning;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        var camera = Instantiate(cam);
        camera.GetComponent<CinemachineVirtualCamera>().Follow = transform;
    }

    void Update() {
        if (!IsOwner) { return; }
        Inputs();
        Lean();
        Turn();
    }
    
    void Lean() {
        if ((isLeaningLeft && isLeaningRight || !(isLeaningLeft || isLeaningRight))) {
            if (Vector3.Distance(cam.transform.position, originalPosition) < 0.5f) {
                cam.transform.position = originalPosition;
            } else {
                cam.transform.position = Vector3.Lerp(cam.transform.position, originalPosition, leanSpeed * Time.deltaTime);
            }
        }else if (isLeaningLeft) {
            if (Vector3.Distance(cam.transform.position, new Vector3(-leanOffset.x, leanOffset.y, leanOffset.z)) < 0.5f) {
                cam.transform.position = new Vector3(-leanOffset.x, leanOffset.y, leanOffset.z);
            } else {
                cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(-leanOffset.x, leanOffset.y, leanOffset.z), leanSpeed * Time.deltaTime);
            }
        }else if (isLeaningRight) {
            if (Vector3.Distance(cam.transform.position, new Vector3(leanOffset.x, leanOffset.y, leanOffset.z)) < 0.5f) {
                cam.transform.position = new Vector3(leanOffset.x, leanOffset.y, leanOffset.z);
            } else {
                cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(leanOffset.x, leanOffset.y, leanOffset.z), leanSpeed * Time.deltaTime);
            }
        }
    }

    void Turn() {
        if (isTurning)
        {
            Quaternion targetRotation = Quaternion.Euler(turnAngle);

            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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

