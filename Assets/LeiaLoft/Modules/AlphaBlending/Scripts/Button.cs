using UnityEngine;
using UnityEngine.Events;
namespace LeiaLoft.Examples.Asteroids
{
    public class Button : MonoBehaviour
    {
        bool pressedPrev = false;
        int layerMask = 0;
        Vector3 startPos = Vector3.zero;
        [SerializeField] private Transform movingButton = null;
        [SerializeField] private Vector3 pressOffset = Vector3.zero;
        [SerializeField] private UnityEvent onClick = null;
        [SerializeField] private UnityEvent onHold = null;
        [SerializeField] private UnityEvent onRelease = null;

        void Start()
        {
            layerMask = LayerMask.GetMask("UI");
            startPos = movingButton.localPosition;
        }

        void Update()
        {
            bool pressed = false;

#if UNITY_EDITOR
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, layerMask))
            {
                if (hit.transform == transform)
                {
                    if (Input.GetMouseButton(0))
                    {
                        pressed = true;
                    }
                }
                else
                {
                    if (pressedPrev)
                    {
                        onRelease.Invoke();
                    }
                }
            }
#endif
            if (Input.touchCount > 0)
            {
                int count = Input.touchCount;

                for (int i = 0; i < count; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Ray touchRay = Camera.main.ScreenPointToRay(touch.position);
                    RaycastHit touchHit;
                    if (Physics.Raycast(touchRay, out touchHit, 100f, layerMask))
                    {
                        if (touchHit.transform == transform)
                        {
                            pressed = true;
                        }
                    }
                }
            }

            if (pressed && pressedPrev)
            {
                onHold.Invoke();
            }

            if (pressed)
            {
                movingButton.localPosition = startPos + pressOffset;
                if (!pressedPrev)
                {
                    onClick.Invoke();
                    pressedPrev = true;
                }
            }
            else
            {
                if (pressedPrev)
                {
                    onRelease.Invoke();
                }
                movingButton.localPosition = startPos;
            }

            pressedPrev = pressed;
        }
    }
}