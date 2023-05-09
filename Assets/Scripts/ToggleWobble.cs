/*
File: ToggleWobble.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable wobble effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleWobble : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        Wobble script = scripts.GetComponent<Wobble>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedWobble(m_Toggle);
        });
        
    }

    void ToggleValueChangedWobble(Toggle change)
    {
        Wobble script = scripts.GetComponent<Wobble>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}