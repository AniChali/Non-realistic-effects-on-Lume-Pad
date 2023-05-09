using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    [DefaultExecutionOrder(4), RequireComponent(typeof(LeiaCamera))]
    public class ConvergenceZoomCompensator : MonoBehaviour
    {
        Vector3 localPositionPrev;
        LeiaCamera leiaCamera;
        LeiaFocus[] leiaFocus;

        void Start()
        {
            localPositionPrev = transform.localPosition;
            leiaCamera = GetComponent<LeiaCamera>();
            leiaFocus = GetComponents<LeiaFocus>();
        }

        void LateUpdate()
        {
            Vector3 movement = transform.localPosition - localPositionPrev;

            leiaCamera.ConvergenceDistance -= movement.z;

            localPositionPrev = transform.localPosition;

            int count = leiaFocus.Length;

            for (int i = 0; i < count; i++)
            {
                if (leiaFocus[i].enabled)
                {
                    leiaFocus[i].AddOffset(-movement.z);
                }
            }
        }
    }
}