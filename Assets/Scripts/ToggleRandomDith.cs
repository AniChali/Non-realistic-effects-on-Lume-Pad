/*
File: ToggleRandomDith.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable random dithering effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleRandomDith : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        RandomDither script = scripts.GetComponent<RandomDither>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedRandom(m_Toggle);
        });
        
    }

    void ToggleValueChangedRandom(Toggle change)
    {
        RandomDither script = scripts.GetComponent<RandomDither>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}