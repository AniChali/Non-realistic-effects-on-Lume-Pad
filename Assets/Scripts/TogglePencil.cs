/*
File: TogglePencil.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable pencil effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class TogglePencil : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        Pencil script = scripts.GetComponent<Pencil>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedPencil(m_Toggle);
        });
        
    }

    void ToggleValueChangedPencil(Toggle change)
    {
        Pencil script = scripts.GetComponent<Pencil>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}