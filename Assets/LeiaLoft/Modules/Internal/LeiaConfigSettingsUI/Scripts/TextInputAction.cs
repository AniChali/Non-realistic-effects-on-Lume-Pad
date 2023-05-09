using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft
{
	/// <summary>
    /// Class for receiving user input as a string, parsing it as a float. On success, updates user-specified text, and performs an action.
    /// </summary>
	public class TextInputAction : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField] private float min = -1f;
		[SerializeField] private float max = 16f;
		[SerializeField] private string formattedLabel = "Label: {0:F2}";
		[SerializeField] Text label;
#pragma warning restore 649

		Action actionOnChange;

		/// <summary>
        /// Dev should attach an action to this TextInputAction script.
        /// </summary>
        /// <param name="action">An action to perform after user triggers OnEndEdit</param>
		public void SetActionOnChange(Action action)
        {
			actionOnChange = action;
        }

        private void Awake()
        {
			SetData("");
        }

        [SerializeField] float mValue = float.NegativeInfinity;
		public float value
        {
			get
            {
				return mValue;
            }
        }

		/// <summary>
        /// Convenience method for getting an int. All floats can be converted to ints
        /// </summary>
		public int valueAsInt
		{
			get
			{
				return (int)value;
			}
		}

		/// <summary>
        /// Sets the data in this text input bundle. Setting string $data triggers a parse of the string as a float, updates a user-facing label, and triggers any actionOnChange
		/// </summary>
		public void SetData(string data)
		{
			float parseTarget;
			bool parsed = float.TryParse(data, out parseTarget);
			if (parsed)
            {
				mValue = Mathf.Clamp(parseTarget, min, max);
				if (label != null && !string.IsNullOrEmpty(formattedLabel))
				{
					// update text to reflect new value
					label.text = string.Format(formattedLabel, mValue);
                }

				if (actionOnChange != null)
                {
					// execute user-specified actions
					actionOnChange();
                }
            }
			else
			{
				if (label != null && !string.IsNullOrEmpty(formattedLabel))
				{
					label.text = string.Format(formattedLabel, "?");
				}
			}
		}
	}
}
