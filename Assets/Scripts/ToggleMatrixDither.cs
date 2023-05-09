/*
File: ToggleMatrixDither.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable matrix dithering effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleMatrixDither : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>(); 
        
        MatrixDith script = scripts.GetComponent<MatrixDith>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(m_Toggle);
        });
        
    }

    void ToggleValueChanged(Toggle change)
    {
        MatrixDith script = scripts.GetComponent<MatrixDith>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}