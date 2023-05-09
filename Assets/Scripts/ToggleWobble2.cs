/*
File: ToggleWobble2.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable wobble 2 effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleWobble2 : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        Wobble2 script = scripts.GetComponent<Wobble2>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedWobble2(m_Toggle);
        });
        
    }

    void ToggleValueChangedWobble2(Toggle change)
    {
        Wobble2 script = scripts.GetComponent<Wobble2>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}