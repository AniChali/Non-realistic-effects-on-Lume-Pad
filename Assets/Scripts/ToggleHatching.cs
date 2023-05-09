/*
File: ToggleHatching.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable hatching effect
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ToggleHatching : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        PostProcessing script = scripts.GetComponent<PostProcessing>(); 
        script.enabled = false;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChangedHatching(m_Toggle);
        });
        
    }

    void ToggleValueChangedHatching(Toggle change)
    {
        PostProcessing script = scripts.GetComponent<PostProcessing>(); 
        // enable effect
        if (m_Toggle.isOn) {
            script.enabled = true;
        // disable effect
        } else {
            script.enabled = false;
        }
        
    }
}