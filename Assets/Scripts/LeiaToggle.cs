/*
File: LeiaToggle.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to enable/disable 3D mode of Lume Pad with a toggle
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LeiaLoft;

public class LeiaToggle : MonoBehaviour
{
    Toggle m_Toggle; // 3D mode toggle

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        
        // enable light field mode
        LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;

        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChangedLeia(m_Toggle);
        });
        
    }

    void ToggleValueChangedLeia(Toggle change)
    {
        // enable
        if (m_Toggle.isOn) {
            LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
        // disable
        } else {
            LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
        }
        
    }
}