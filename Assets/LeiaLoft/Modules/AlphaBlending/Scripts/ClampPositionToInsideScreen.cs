using UnityEngine;

namespace LeiaLoft.Examples.Asteroids
{
    [DefaultExecutionOrder(100)]
    public class ClampPositionToInsideScreen : MonoBehaviour
    {
        [SerializeField] private Transform screenLeft = null;
        [SerializeField] private Transform screenRight = null;

        void LateUpdate()
        {
            ClampPositionToScreen();
        }

        void ClampPositionToScreen()
        {
            transform.position =
                new Vector3(
                Mathf.Clamp(transform.position.x, screenLeft.position.x, screenRight.position.x),
                transform.position.y,
                transform.position.z
                );
        }
    }
}