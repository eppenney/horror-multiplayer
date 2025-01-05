using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;
    private float yRotation = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        yRotation += speed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0.0f, yRotation, 0.0f);
    }
}
