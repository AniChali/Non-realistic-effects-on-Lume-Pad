using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft.Examples
{
    public class BaselineSliderInitializer : MonoBehaviour
    {
#pragma warning disable 649 // Suppress warning that var is never assigned to and will always be null
        [SerializeField] private LeiaCamera leiaCamera;
#pragma warning restore 649

        // Use this for initialization
        void Start()
        {
            Slider slider = GetComponent<Slider>();
            slider.value = leiaCamera.BaselineScaling;
        }
    }
}
