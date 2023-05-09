/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// Script that adds a vetical parallax effect using the device's gyroscope
    /// Requires Unity version 2017.1 or newer
    /// </summary>
    [RequireComponent(typeof(LeiaCamera))]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/modules/parallax-effect")]
    public class ParallaxEffect : MonoBehaviour
    {
        /// <summary>
        /// Controls the intensity of the parallax effect
        /// </summary>
        [SerializeField] private float Scale = 6f;

        private const float neutralHandPositionGravityOffset = 0.5f;
        private LeiaCamera leiaCam;
        private Vector2 shift;
        private bool isAppInFocus;

        void Start()
        {
            leiaCam = GetComponent<LeiaCamera>();
            shift = Vector2.zero;

#if !UNITY_2017_1_OR_NEWER
            RemoveParallaxCondition(true, LogLevel.Error, "Removing ParallaxEffect.cs component: Requires Unity version 2017.1 or newer");
#endif
            RemoveParallaxCondition(leiaCam == null, LogLevel.Error, string.Format("{0} {1}", "Removing ParallaxEffect.cs component: could not find LeiaCamera attached to gameObject named ", gameObject.name));
#if !UNITY_EDITOR
            RemoveParallaxCondition(Application.platform != RuntimePlatform.Android, LogLevel.Warning, "Removing ParallaxEffect.cs component: Runtime platform is not Android");
            RemoveParallaxCondition(!SystemInfo.supportsGyroscope, LogLevel.Warning, "Removing ParallaxEffect.cs component: Device does not support Gyroscope");
#endif
        }

        void Update()
        {
            if(!isAppInFocus)
            {
                return;
            }
            shift.y = (Input.gyro.gravity.z + neutralHandPositionGravityOffset) * Scale;
            leiaCam.CameraShift += shift - leiaCam.CameraShift;
        }

        private void OnEnable()
        {
            ToggleGyro(true);
        }

        private void OnDisable()
        {
            ToggleGyro(false);
        }

        private void OnApplicationFocus(bool inFocus)
        {
            ToggleGyro(inFocus);
        }
        private void OnApplicationPause(bool isPaused)
        {
            ToggleGyro(!isPaused);
        }
        private void ToggleGyro(bool toggle)
        {
            isAppInFocus = toggle;
            Input.gyro.enabled = toggle;
        }
        private void RemoveParallaxCondition(bool destroyCondition, LogLevel logLevel, string msg)
        {
            if (destroyCondition)
            {
                LogUtil.Log(logLevel, msg);
                Destroy(this);
            }
        }
    }
}