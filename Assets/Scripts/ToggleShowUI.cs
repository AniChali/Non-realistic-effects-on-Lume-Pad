/*
File: ToggleShowUI.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to show/hide UI
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LeiaLoft;

public class ToggleShowUI : MonoBehaviour
{
    Toggle m_Toggle;
    public GameObject scripts;
    public GameObject canvas;
    public GameObject resetButton;
    

    void Start()
    {
        // fetch toggle gameobject
        m_Toggle = GetComponent<Toggle>();
        // fetch and enable movement camera script
        MovementCamera script = scripts.GetComponent<MovementCamera>();
        script.enabled = true; 

        // enable light field mode
        LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;

        // fetch panel with UI
        RectTransform m_Panel = canvas.GetComponent<RectTransform>();
        m_Panel.gameObject.SetActive(!m_Panel.gameObject.activeSelf);


        // add listener to toggle value change
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChangedUI(m_Toggle);
        });
        
    }

    void ToggleValueChangedUI(Toggle change)
    {
        RectTransform m_Panel = canvas.GetComponent<RectTransform>();
        m_Panel.gameObject.SetActive(!m_Panel.gameObject.activeSelf);

        MovementCamera script = scripts.GetComponent<MovementCamera>(); 
        // disable movement and hide UI 
        if (m_Toggle.isOn) {
            script.enabled = false;
            resetButton.SetActive(false);
        // enable movement and hide UI 
        } else {
            script.enabled = true;
            resetButton.SetActive(true);
        }
        
    }
}