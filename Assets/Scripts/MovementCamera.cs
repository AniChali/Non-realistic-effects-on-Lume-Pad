/*
File: MovementCamera.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to rotate camera on user input
*/

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class MovementCamera : MonoBehaviour
{

    public float mouseSensitivity = 5f;
    public Transform playerBody;
    private float xRotationCamera = 0f;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // RESOURCE: https://github.com/MasterKiller1239/Gitapp/blob/2017af443a145be8eee809347be505ccf91859ee/Assets/Scripts/MouseLook.cs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotationCamera -= mouseY;
        xRotationCamera = Mathf.Clamp(xRotationCamera, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotationCamera, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
        
    }

}