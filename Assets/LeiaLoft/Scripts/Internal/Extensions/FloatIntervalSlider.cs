namespace LeiaLoft
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [Serializable]
    public class FloatIntervalSlider : Slider
    {
        [SerializeField] private float _snapInterval = 0;
        protected override void Set(float input, bool sendCallback)
        {
            input = Mathf.Round(input * (1 / _snapInterval)) * _snapInterval;
            base.Set(input, sendCallback);
        }
    }
}
