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
    [System.Serializable]
    public struct LeiaCameraData
    {
        public float baseline { get; set; }
        public float screenHalfHeight { get; set; }
        public float screenHalfWidth { get; set; }
        public float baselineScaling { get; set; }
    }

    [System.Serializable]
    public struct LeiaBoundsData
    {
        public Vector3[] screen { get; set; }
        public Vector3[] north { get; set; }
        public Vector3[] south { get; set; }
        public Vector3[] top { get; set; }
        public Vector3[] bottom { get; set; }
        public Vector3[] east { get; set; }
        public Vector3[] west { get; set; }
    }

    public static class LeiaCameraUtils
    {
        [System.Obsolete("Deprecated in 0.6.21. Scheduled for removal in 0.6.23. Superseded by editor-time LeiaCameraBounds.ComputeLeiaCameraData")]
        public static LeiaCameraData ComputeLeiaCamera(Camera camera, float convergenceDistance, float baselineScaling, DisplayConfig displayConfig)
        {
		      return new LeiaCameraData();
        }

        [System.Obsolete("Deprecated in 0.6.21. Scheduled for removal in 0.6.23. Superseded by editor-time LeiaCameraBounds.ComputeLeiaBounds")]
        public static LeiaBoundsData ComputeLeiaBounds(Camera camera, LeiaCameraData leiaCamera, float convergenceDistance, Vector2 cameraShift, DisplayConfig displayConfig)
        {
            return new LeiaBoundsData();
        }

        /// <summary>
        /// Performs a raycast from the given LeiaCamera
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="position">A screenPosition</param>
        /// <returns>A ray from the camera's world position, that passes through the screenPosition</returns>
        public static Ray ScreenPointToRay(this LeiaCamera leiaCam, Vector3 screenPosition)
        {
            Camera cam = leiaCam.Camera;
            bool prev_state = cam.enabled;
            cam.enabled = true;
            Ray r = cam.ScreenPointToRay(screenPosition);
            cam.enabled = prev_state;
            return (r);
        }

        /// <summary>
        /// Transforms a point from screen space to world space
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="position">A screenPosition</param>
        /// <returns>A Vector3 representing screenPosition in world space coordinates</returns>
        public static Vector3 ScreenToWorldPoint(this LeiaCamera leiaCam, Vector3 screenPosition)
        {
            Camera cam = leiaCam.Camera;
            bool prev_state = cam.enabled;
            cam.enabled = true;
            Vector3 r = cam.ScreenToWorldPoint(screenPosition);
            cam.enabled = prev_state;
            return (r);
        }

        /// <summary>
        /// Returns a baseline scaling value for a LeiaCamera based on a desired 
        /// convergence distance and leia frustum near plane distance.
        /// Useful for automatic baseline calculation scripts.
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="farPlaneDistance">The distance of the desired far plane of the Leia frustum. 
        /// Ideally should be set to the distance from the camera to the furthest currently visible point in the scene.</param>
        /// <returns>A float representing a baseline scaling value that satisfies the specified 
        /// convergence distance and Leia frustum far plane distance.</returns>
        public static float GetRecommendedBaselineBasedOnFarPlane(LeiaCamera leiaCamera, float farPlaneDistance, float convergenceDistance)
        {
            float recommendedBaseline;

            if (leiaCamera.Camera.orthographic)
            {
                recommendedBaseline = 1f / (farPlaneDistance - convergenceDistance);
            }
            else //if its a perspective camera
            {
                recommendedBaseline = farPlaneDistance / Mathf.Max(convergenceDistance - farPlaneDistance, .01f);
            }

            return recommendedBaseline;
        }

        /// <summary>
        /// Returns a baseline scaling value for a LeiaCamera based on a desired 
        /// convergence distance and leia frustum near plane distance.
        /// Useful for automatic baseline calculation scripts.
        /// </summary>
        /// <param name="leiaCam">A LeiaCamera with a Camera component and Transform</param>
        /// <param name="nearPlaneDistance">The distance of the desired near plane of the Leia frustum. 
        /// Ideally should be set to the distance from the camera to the closest currently visible point in the scene.</param>
        /// <returns>A float representing a baseline scaling value that satisfies the specified 
        /// convergence distance and Leia frustum near plane distance.</returns>
        public static float GetRecommendedBaselineBasedOnNearPlane(LeiaCamera leiaCamera, float nearPlaneDistance, float convergenceDistance)
        {
            float recommendedBaseline;

            if (leiaCamera.Camera.orthographic)
            {
                recommendedBaseline = -1f / (nearPlaneDistance - convergenceDistance);
            }
            else //if its a perspective camera
            {
                recommendedBaseline = nearPlaneDistance / Mathf.Max(convergenceDistance - nearPlaneDistance, .01f);
            }

            return recommendedBaseline;
        }
    }
}
