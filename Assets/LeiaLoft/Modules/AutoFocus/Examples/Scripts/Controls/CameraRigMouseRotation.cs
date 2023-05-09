using UnityEngine;

namespace LeiaLoft.Examples
{
    public class CameraRigMouseRotation : MonoBehaviour
    {
#pragma warning disable 414
        [SerializeField, Range(0.01f, 1f)] private float sensitivity = 0.1f;
#pragma warning restore 414
#if UNITY_EDITOR || UNITY_STANDALONE
    private Vector3 startMousePosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            startMousePosition = Input.mousePosition;
            startRotation = transform.rotation;
        }

        if (Input.GetMouseButton(1))
        {
            float deltaMousePositionX = Input.mousePosition.x - startMousePosition.x;
            float deltaMousePositionY = Input.mousePosition.y - startMousePosition.y;

            transform.rotation = Quaternion.Euler(
                startRotation.eulerAngles.x - deltaMousePositionY * sensitivity,
                startRotation.eulerAngles.y + deltaMousePositionX * sensitivity,
                0);
        }
    }
#endif
    }
}
