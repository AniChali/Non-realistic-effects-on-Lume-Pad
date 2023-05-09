/*
File: ToggleOutlineFullEffect.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable outline full effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleOutlineFullEffect : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        OutlineFull script = scripts.GetComponent<OutlineFull>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedOutlineFull(m_Toggle);
        });
        
    }

    void ToggleValueChangedOutlineFull(Toggle change)
    {
        OutlineFull script = scripts.GetComponent<OutlineFull>(); 
        
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}