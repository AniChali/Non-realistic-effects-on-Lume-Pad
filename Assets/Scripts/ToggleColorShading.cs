/*
File: ToggleColorShading.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable color shading effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleColorShading : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        ColorShading script = scripts.GetComponent<ColorShading>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate { 
            ToggleValueChangedColorShading(m_Toggle);
        });
        
    }

    void ToggleValueChangedColorShading(Toggle change)
    {
        ColorShading script = scripts.GetComponent<ColorShading>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}