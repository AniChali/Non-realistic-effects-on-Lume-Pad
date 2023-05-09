/*
File: SliderBaselineScript.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to control slider value
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LeiaLoft;

public class SliderBaselineScript : MonoBehaviour {
  public Slider sliderUI;
  public Text textSliderValue;
  public GameObject scripts;

  void Start (){
    // on start call slider value baseline function
    SliderValueBaseline();
  }

  void Update() {
    // per frame call slider value baseline function
    SliderValueBaseline();
  }

  public void SliderValueBaseline () {
    // set label text
    string sliderMessage = "Baseline Scaling = " + sliderUI.value;
    LeiaCamera script = scripts.GetComponent<LeiaCamera>(); 
    // set baseline scaling value
    script.BaselineScaling = sliderUI.value;
    textSliderValue.text = sliderMessage;
  }
}