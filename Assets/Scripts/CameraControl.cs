using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public float moveSpeed;
    public float lookSpeed;

    Camera cam;

    Vector2 rotation = Vector3.zero;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {

        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            movement += cam.transform.forward * moveSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement -= cam.transform.forward * moveSpeed;
        }

        if(Input.GetKey(KeyCode.D))
        {
            movement += cam.transform.right * moveSpeed;
        }
        else if (Input.GetKey(KeyCode.A)) 
        {
            movement -= cam.transform.right * moveSpeed;
        }

        if (Input.GetKey(KeyCode.E))
        {
            movement += Vector3.up * moveSpeed;
        }
        else if (Input.GetKey(KeyCode.Q)) 
        {
            movement -= Vector3.up * moveSpeed;
        }

        cam.transform.Translate(movement * Time.deltaTime, Space.World);

        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");

        transform.eulerAngles = rotation * lookSpeed;

    }
}
