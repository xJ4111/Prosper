using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Transform CamTransform;
    private Camera Cam;
    private Transform Pivot;

    private float MoveSpeed = 1f;
    private float RotSpeed = 1f;
    private float BoostAmount = 1.5f;

    private float ZoomSpeed = 1f;
    private float MinZoom, MaxZoom;

    // Start is called before the first frame update
    void Start()
    {
        Pivot = GetComponent<Transform>();
        Cam = GetComponentInChildren<Camera>();
        CamTransform = Cam.GetComponent<Transform>();

        MinZoom = Cam.fieldOfView * 0.5f;
        MaxZoom = Cam.fieldOfView * 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Rotate();
        CamZoom();
    }

    void Movement()
    {
        if(Input.GetKey(KeyCode.W))
        {
            Move(Pivot.forward);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Move(-Pivot.forward);
        }

        if (Input.GetKey(KeyCode.A))
        {
            Move(-Pivot.right);
        }

        if (Input.GetKey(KeyCode.D))
        {
            Move(Pivot.right);
        }
    }
    void Move(Vector3 dir)
    {
        if(!Input.GetKey(KeyCode.LeftShift))
        {
            Pivot.transform.localPosition += dir * (MoveSpeed);
        }
        else
        {
            Pivot.transform.localPosition += dir * (MoveSpeed * BoostAmount);
        }
    }

    void Rotate()
    {
        if(Input.GetMouseButton(1))
        {
            Vector3 rot = Pivot.localEulerAngles;
            rot.y += Input.GetAxis("Mouse X") * RotSpeed;
            Pivot.localRotation = Quaternion.Euler(rot);
        }
    }

    void CamZoom()
    {
        if (Input.GetMouseButton(1))
        {
            if (Cam.fieldOfView <= MaxZoom && Cam.fieldOfView >= MinZoom)
            {
                Cam.fieldOfView += Input.GetAxis("Mouse Y") * ZoomSpeed;

                if (Cam.fieldOfView > MaxZoom)
                    Cam.fieldOfView = MaxZoom;
                if (Cam.fieldOfView < MinZoom)
                    Cam.fieldOfView = MinZoom;
            }
        }
    }

}
