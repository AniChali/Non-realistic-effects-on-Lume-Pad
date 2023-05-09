/*
File: ToggleTreshold.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable tresholding effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleTreshold : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
    
        Tresholding script = scripts.GetComponent<Tresholding>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedTreshold(m_Toggle);
        });
        
    }

    void ToggleValueChangedTreshold(Toggle change)
    {
        Tresholding script = scripts.GetComponent<Tresholding>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}