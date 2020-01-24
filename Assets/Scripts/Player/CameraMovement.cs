using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement M;
    private void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(this);
        }
    }

    public Camera Cam;
    private Transform CamTransform;
    private Transform Pivot;

    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float RotSpeed = 1f;
    [SerializeField] private float BoostAmount = 1.5f;
    [SerializeField] private bool Smooth;
    [SerializeField] private float LerpSpeed = 10f;
    private Vector3 TargetPos;

    [SerializeField] private float ZoomSpeed = 1f;
    private float MinZoom, MaxZoom;

    // Start is called before the first frame update
    void Start()
    {
        Pivot = GetComponent<Transform>();
        Cam = GetComponentInChildren<Camera>();
        CamTransform = Cam.GetComponent<Transform>();

        TargetPos = Pivot.position;

        MinZoom = Cam.fieldOfView * 0.5f;
        MaxZoom = Cam.fieldOfView * 1.25f;
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

        if(Smooth)
            Pivot.position = Vector3.Lerp(Pivot.position, TargetPos, Time.deltaTime * LerpSpeed);
    }
    void Move(Vector3 dir)
    {
        if(!Input.GetKey(KeyCode.LeftShift))
        {
            if (Smooth)
                TargetPos += dir * (MoveSpeed);
            else
                Pivot.position += dir * (MoveSpeed);
        }
        else
        {
            if (Smooth)
                TargetPos += dir * (MoveSpeed * BoostAmount);
            else
                Pivot.position += dir * (MoveSpeed * BoostAmount);
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
