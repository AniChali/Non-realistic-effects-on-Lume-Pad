using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft
{
	/// <summary>
    /// Class for receiving user input to a Slider, triggering callback(s). Supports a label update when Text to update is supplied; and also supports user-specified additional callback.
    /// </summary>
	public class SliderInputAction : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField] private string formattedLabel = "Label: {0:F2}";
		[SerializeField] Text label;
        [SerializeField] private Slider mSlider;
#pragma warning restore 649

        /// <summary>
        /// Lazy initializer for slider
        /// </summary>
        public Slider slider
        {
            get
            {
                if (mSlider == null)
                {
                    mSlider = GetComponent<Slider>();
                }
                return mSlider;
            }
        }

        UnityEngine.Events.UnityAction<float> sliderCallback;

		/// <summary>
        /// Dev should attach an action to this SliderInputAction script. Only one externally-specified Action is supported at this time
        /// </summary>
        /// <param name="action">An action to perform after user triggers OnValueChanged</param>
		public void SetActionOnChange(UnityEngine.Events.UnityAction<float> callback)
        {
            if (slider != null && sliderCallback != null)
            {
                // if slider was previously attached, remove it
                slider.onValueChanged.RemoveListener(sliderCallback);
            }
            sliderCallback = callback;

            if (slider != null)
            {
                slider.onValueChanged.AddListener(sliderCallback);
            }
        }

        private void OnEnable()
        {
            // always attach callback for setting text. this occurs independently of SetActionOnChange
            if (slider != null && label != null)
            {
                slider.onValueChanged.AddListener((_) =>
                {
                    label.text = string.Format(formattedLabel, slider.value);
                });
            }
        }

		public float value
        {
            set
            {
                if (slider != null)
                {
                    slider.value = value;
                }
            }
			get
            {
                if (slider != null)
                {
                    return slider.value;
                }
                return default(float);
            }
        }

		/// <summary>
        /// Convenience method for getting an int. All floats can be converted to ints
        /// </summary>
		public int valueAsInt
		{
			get
			{
                if (slider != null)
                {
                    return (int) slider.value;
                }
                return default(int);
			}
		}
	}
}
