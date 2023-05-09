/*
File: Rotation.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to rotate an object
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public Quaternion originalRotation;

    void Start () { 
        // save default rotation
        originalRotation = transform.rotation; 
    }

    void Update()
    {
        // rotate object
        transform.Rotate(new Vector3(0f, 10f, 0f) * Time.deltaTime);
        
    }

    void OnDisable () {
        // on disable reset rotation to original rotation
        transform.rotation = originalRotation;
    }
}
