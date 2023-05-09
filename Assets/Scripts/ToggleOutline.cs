/*
File: ToggleOutline.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable outline effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleOutline : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        OutlineOnly script = scripts.GetComponent<OutlineOnly>(); 
        script.enabled = false;
        
        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedOutline(m_Toggle);
        });
        
    }

    void ToggleValueChangedOutline(Toggle change)
    {
        OutlineOnly script = scripts.GetComponent<OutlineOnly>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
    }
}