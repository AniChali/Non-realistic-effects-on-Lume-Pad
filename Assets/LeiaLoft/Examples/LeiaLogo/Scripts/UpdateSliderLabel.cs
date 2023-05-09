using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft.Examples
{
    public class UpdateSliderLabel : MonoBehaviour
    {
        [SerializeField] private Text label = null;
        [SerializeField] private Slider slider = null;
        [SerializeField] private string valueName = "";

        // Start is called before the first frame update
        void Start()
        {
            slider.onValueChanged.AddListener(UpdateLabel);
            UpdateLabel(slider.value);
        }

        public void UpdateLabel(float value)
        {
            label.text = string.Format(
                "{0}: {1}",
                valueName,
                slider.value.ToString("F1")
                );
        }
    }
}
