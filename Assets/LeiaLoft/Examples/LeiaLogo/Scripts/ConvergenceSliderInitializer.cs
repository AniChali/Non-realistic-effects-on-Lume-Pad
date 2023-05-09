using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

//namespace LeiaLoft.Examples
//{
    public class ConvergenceSliderInitializer : MonoBehaviour
    {
#pragma warning disable 0649 // Suppress warning that var is never assigned to and will always be null
        [SerializeField] private LeiaCamera leiaCamera;
#pragma warning restore 0649

        // Use this for initialization
        void Start()
        {
            Slider slider = GetComponent<Slider>();
            slider.value = leiaCamera.ConvergenceDistance;
        }

        void Update() {
            Slider slider = GetComponent<Slider>();
            leiaCamera.ConvergenceDistance = slider.value;
        }
    }
//}
