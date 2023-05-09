/*
File: RotationOnOffScript.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to start/stop rotating objects
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LeiaLoft;

public class RotationOnOffScript : MonoBehaviour
{
    public GameObject panelBase;
    public GameObject panel;
    public GameObject minifig1;
    public GameObject minifig2;
    public Button button;

    void Start()
    {
        // add listener to button value change
        button.onClick.AddListener(ResetValuesRotation);
    }

    public void ResetValuesRotation () {
        // get rotation scripts from objects
        Rotation scriptPanelBase = panelBase.GetComponent<Rotation>(); 
        Rotation scriptPanel = panel.GetComponent<Rotation>(); 
        Rotation scriptMinifig1 = minifig1.GetComponent<Rotation>(); 
        Rotation scriptMinifig2 = minifig2.GetComponent<Rotation>(); 
        // enable or disable rotation scripts of objects
        if (scriptPanelBase.enabled) {
            scriptPanelBase.enabled = false;
            scriptPanel.enabled = false;
            scriptMinifig1.enabled = false;
            scriptMinifig2.enabled = false;
        } else {
            scriptPanelBase.enabled = true;
            scriptPanel.enabled = true;
            scriptMinifig1.enabled = true;
            scriptMinifig2.enabled = true;
        }
        
    }
}
