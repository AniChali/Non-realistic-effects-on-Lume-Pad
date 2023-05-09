/*
File: SliderScript.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to control slider value
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using LeiaLoft;

public class SliderScript : MonoBehaviour {
  public Slider sliderUI;
  //private Text textSliderValue;
  public Text textSliderValue;
  public GameObject scripts;

  void Start (){
    // on start call slider value function
    SliderValue();
  }

  void Update() {
    // per frame call slider value function
    SliderValue();
  }

  public void SliderValue () {
    // set label text
    string sliderMessage = "Convergence distance = " + sliderUI.value;
    LeiaCamera script = scripts.GetComponent<LeiaCamera>(); 
    // set convergence scaling value
    script.ConvergenceDistance = sliderUI.value;
    textSliderValue.text = sliderMessage;
  }
}