using UnityEngine;

namespace LeiaLoft.Examples
{
    public class CameraRigMovementMouse : MonoBehaviour
    {
        [SerializeField] private float sensitivity = .01f;
        private Vector3 startMousePosition = Vector3.zero;
        private Vector3 startPosition = Vector3.zero;
        private Transform childCamera = null;
        private bool multiTouching = false;

        void Start()
        {
            childCamera = GetComponentInChildren<Camera>().transform;
        }

        void LateUpdate()
        {
            if (Input.touchCount > 1)
            {
                multiTouching = true;
                return;
            }
            else
            {
                if (multiTouching)
                {
                    if (!Input.GetMouseButton(0))
                    {
                        multiTouching = false;
                    }
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
            {
                startMousePosition = Input.mousePosition;
                startPosition = transform.position;
            }

            float zoomLevel = childCamera.localPosition.z;

            if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
            {
                Quaternion rotateBy = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);

                Vector3 deltaMousePosition =
                    new Vector3(
                        Input.mousePosition.x - startMousePosition.x,
                        0,
                        Input.mousePosition.y - startMousePosition.y
                        );

                transform.position = startPosition + (rotateBy * (deltaMousePosition * zoomLevel * sensitivity));
            }
        }
    }
}