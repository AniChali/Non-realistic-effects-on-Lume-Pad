using UnityEngine;

namespace LeiaLoft.Examples
{
    public class CameraRigMovementArrowKeys : MonoBehaviour
    {
        [SerializeField] private float speed = 5;
        [SerializeField] private float drag = 5;
        private Rigidbody rb = null;
        private Transform childCamera = null;

        void Start()
        {
            rb = transform.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.drag = drag;
            }

            childCamera = GetComponentInChildren<Camera>().transform;
        }

        void LateUpdate()
        {
            MoveCamera(
                -Input.GetAxis("Horizontal"),
                -Input.GetAxis("Vertical")
                );
        }

        public void MoveCamera(float horizontal, float vertical)
        {
            Vector3 controlsMoveVector;
            Quaternion forwardsDirection;

            controlsMoveVector = new Vector3(
                    horizontal * speed * (childCamera.localPosition.z + 10),
                    0,
                    vertical * speed * (childCamera.localPosition.z + 10)
                    );

            forwardsDirection = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);

            Vector3 moveVector = forwardsDirection * controlsMoveVector;

            rb.AddForce(moveVector);
        }
    }
}