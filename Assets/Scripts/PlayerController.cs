using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0f, 500f)] public float sensitivity;
    public float speed;
    public Transform orientation;

    private Rigidbody _rigidbody;

    private float _xRotation;
    private float _yRotation;
    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // If tested in unity editor the cursor remains visible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        #if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        #endif
    }

    void FixedUpdate()
    {
        
        // Simple moving script the rotates camera and set velocity depending on camera angle
        
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * sensitivity;

        float horizontal = Input.GetAxisRaw("Horizontal") * Time.fixedDeltaTime * speed;
        float vertical = Input.GetAxisRaw("Vertical") * Time.fixedDeltaTime * speed;

        _yRotation += mouseX;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -65f, 65);
        
        orientation.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

        Vector3 moveForce = orientation.forward * vertical + orientation.right * horizontal;
        moveForce.y = 0;
        
        _rigidbody.velocity = moveForce;
        
        
            
    }

    
}
