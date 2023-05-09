using UnityEngine;

namespace LeiaLoft.Examples
{
    public class CameraRigTouchRotation : MonoBehaviour
    {
        private Vector3 averageStartPoint = Vector3.zero;
        private Vector3 startRotation = Vector3.zero;

        void Update()
        {
            if (Input.touchCount > 1)
            {
                if (Input.GetTouch(1).phase == TouchPhase.Began)
                {
                    Vector3 touch1StartPos = Input.GetTouch(0).position;
                    Vector3 touch2StartPos = Input.GetTouch(1).position;
                    averageStartPoint = (touch1StartPos + touch2StartPos) / 2f;
                    startRotation = transform.eulerAngles;
                }
                else
                {
                    Vector3 averagePoint = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2f;
                    Vector3 delta = averagePoint - averageStartPoint;
                    Vector3 targetEulerAngles = startRotation - new Vector3(
                        delta.y / 20f,
                        -delta.x / 20f,
                        0
                    );

                    transform.eulerAngles = new Vector3(
                        Mathf.Clamp(targetEulerAngles.x, 1, 89),
                        targetEulerAngles.y,
                        targetEulerAngles.z
                    );
                }
            }
        }
    }
}