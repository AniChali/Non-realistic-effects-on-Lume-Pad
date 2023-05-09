/*
File: ResetButtonScript.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to reset convergence distance and baseline scaling slider values
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LeiaLoft;

public class ResetButtonScript : MonoBehaviour
{
    public GameObject scripts;
    public Slider sliderUI1;
    public Slider sliderUI2;
    public Text textSliderValue1;
    public Text textSliderValue2;
    public Button button;
   
    void Start()
    {
        // add listener to button value change
        button.onClick.AddListener(ResetValues);
    }

    public void ResetValues () {
        LeiaCamera script = scripts.GetComponent<LeiaCamera>(); 
        // reset values
        script.BaselineScaling = 1.0f;
        script.ConvergenceDistance = 5.5f;
        sliderUI1.value = 1.0f;
        sliderUI2.value = 5.5f;
        // updaate label text
        string sliderMessage1 = "Baseline Scaling = " + sliderUI1.value;
        string sliderMessage2 = "Convergence Distance = " + sliderUI2.value;
        textSliderValue1.text = sliderMessage1;
        textSliderValue2.text = sliderMessage2;
    }
}
