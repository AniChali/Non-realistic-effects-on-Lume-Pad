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
    /// Wrapper for repeating routine of getting parameters from Settings and doing calculations
    /// </summary>
    class CameraCalculatedParams
    {
        public float ScreenHalfHeight { get; private set; }
        public float ScreenHalfWidth { get; private set; }
        public float EmissionRescalingFactor { get; private set; }

        /// <summary>
        /// Gets aspect ratio of the viewport of a Unity Camera
        /// </summary>
        /// <param name="renderingCamera">A Unity Camera</param>
        /// <returns>Aspect ratio of the Unity Camera</returns>
        public static float GetViewportAspectFor(Camera renderingCamera)
        {
            return renderingCamera.pixelRect.width * 1.0f / renderingCamera.pixelRect.height;
        }

        /// <summary>
        /// Gets the appropriate aspect ratio for this camera setup
        /// </summary>
        /// <param name="renderingCamera">A LeiaCamera which has a root Unity Camera</param>
        /// <param name="config">A configuration file which may give more info about which aspect ratio to retrieve</param>
        /// <returns></returns>
        public static float GetViewportAspectFor(LeiaCamera renderingCamera, DisplayConfig config)
        {
            // the actual Camera.aspect is sometimes mismatched after rotation. always want width / height though
            // for now, in all cases, the root camera's aspect ratio as modified by ViewportRect is the desired aspect ratio in our calculations
            return GetViewportAspectFor(renderingCamera.Camera);
        }

        /// <summary>
        /// Calculates the projection matrix that would push the given camera to look at the convergencePoint.
        /// Cameras which are same distance on Camera.transform.forward from convergencePoint, and have same FOV and same aspect ratio, will pass through same convergence plane
        /// </summary>
        /// <param name="Camera">A Camera to calculate a convergence matrix for</param>
        /// <param name="convergencePoint">A point (in world coordinates) to converge one or more camera views onto</param>
        /// <returns>The projection matrix which would cause the Camera's view to pass through the convergencePoint</returns>
        public static Matrix4x4 GetConvergedProjectionMatrixForPosition(Camera Camera, Vector3 convergencePoint)
        {
            Matrix4x4 m = Matrix4x4.zero;

            Vector3 cameraToConvergencePoint = convergencePoint - Camera.transform.position;
            
            float far = Camera.farClipPlane;
            float near = Camera.nearClipPlane;

            // posX and posY are the camera-axis-aligned translations off of "root camera" position
            float posX = -1 * Vector3.Dot(cameraToConvergencePoint, Camera.transform.right);
            float posY = -1 * Vector3.Dot(cameraToConvergencePoint, Camera.transform.up);

            // this is really posZ. it is better if posZ is positive-signed
            float ConvergenceDistance = Mathf.Max(Vector3.Dot(cameraToConvergencePoint, Camera.transform.forward), 1E-5f);

            if (Camera.orthographic)
            {
                // calculate the halfSizeX and halfSizeY values that we need for orthographic cameras

                float halfSizeX = Camera.orthographicSize * GetViewportAspectFor(Camera);
                float halfSizeY = Camera.orthographicSize;

                // orthographic

                // row 0
                m[0, 0] = 1.0f / halfSizeX;
                m[0, 1] = 0.0f;
                m[0, 2] = -posX / (halfSizeX * ConvergenceDistance);
                m[0, 3] = 0.0f;

                // row 1
                m[1, 0] = 0.0f;
                m[1, 1] = 1.0f / halfSizeY;
                m[1, 2] = -posY / (halfSizeY * ConvergenceDistance);
                m[1, 3] = 0.0f;

                // row 2
                m[2, 0] = 0.0f;
                m[2, 1] = 0.0f;
                m[2, 2] = -2.0f / (far - near);
                m[2, 3] = -(far + near) / (far - near);

                // row 3
                m[3, 0] = 0.0f;
                m[3, 1] = 0.0f;
                m[3, 2] = 0.0f;
                m[3, 3] = 1.0f;
            }
            else
            {
                // calculate the halfSizeX and halfSizeY values for perspective cam that we would have gotten if we had used new CameraCalculatedParams.
                // we don't need "f" (disparity per camera vertical pixel count) or EmissionRescalingFactor
                const float minAspect = 1E-5f;
                float aspect = Mathf.Max(GetViewportAspectFor(Camera), minAspect);
                float halfSizeY = ConvergenceDistance * Mathf.Tan(Camera.fieldOfView * Mathf.PI / 360.0f);
                float halfSizeX = aspect * halfSizeY;

                // perspective

                // row 0
                m[0, 0] = ConvergenceDistance / halfSizeX;
                m[0, 1] = 0.0f;
                m[0, 2] = -posX / halfSizeX;
                m[0, 3] = 0.0f;

                // row 1
                m[1, 0] = 0.0f;
                m[1, 1] = ConvergenceDistance / halfSizeY;
                m[1, 2] = -posY / halfSizeY;
                m[1, 3] = 0.0f;

                // row 2
                m[2, 0] = 0.0f;
                m[2, 1] = 0.0f;
                m[2, 2] = -(far + near) / (far - near);
                m[2, 3] = -2.0f * far * near / (far - near);

                // row 3
                m[3, 0] = 0.0f;
                m[3, 1] = 0.0f;
                m[3, 2] = -1.0f;
                m[3, 3] = 0.0f;
            }
            return m;
        }

        /// <summary>
        /// Calculates ScreenHalfHeight, ScreenHalfWidth, and EmissionRescalingFactor given these params
        /// </summary>
        /// <param name="properties">A Unity Camera</param>
        /// <param name="BaselineScaling">User-provided baseline</param>
        /// <param name="ConvergenceDistance">User-provided convergence distance</param>
        /// <param name="ViewResolution">Pixel view resolution</param>
        /// <param name="ResolutionScale">Resolution upscale</param>
        /// <param name="SystemDisparityPixels">Disparity per ($ViewResolution.y) pixels</param>
        public CameraCalculatedParams(Camera properties, float BaselineScaling, float ConvergenceDistance, XyPair<int> ViewResolution, float ResolutionScale, float SystemDisparityPixels)
        {
            // When user selects ViewportRect values which push content off the screen and create an aspect ratio of 0, ensure that we do not suffer a divide-by-zero issue
            const float minAspect = 1E-5f;

            float aspect = Mathf.Max(GetViewportAspectFor(properties), minAspect);

            ScreenHalfHeight = ConvergenceDistance * Mathf.Tan(properties.fieldOfView * Mathf.PI / 360.0f);
            ScreenHalfWidth = aspect * ScreenHalfHeight;
            float f = (ViewResolution.y / ResolutionScale) / 2f / Mathf.Tan(properties.fieldOfView * Mathf.PI / 360f);
            EmissionRescalingFactor = SystemDisparityPixels * BaselineScaling * ConvergenceDistance / f;
        }

        public CameraCalculatedParams(LeiaCamera properties, DisplayConfig displayConfig)
		{
            // When user selects ViewportRect values which push content off the screen and create an aspect ratio of 0, ensure that we do not suffer a divide-by-zero issue
            const float minAspect = 1E-5f;

            float aspect = Mathf.Max(GetViewportAspectFor(properties, displayConfig), minAspect);

			ScreenHalfHeight = properties.ConvergenceDistance * Mathf.Tan(properties.FieldOfView * Mathf.PI / 360.0f);
			ScreenHalfWidth = aspect * ScreenHalfHeight;
			float f = (displayConfig.ViewResolution.y / displayConfig.ResolutionScale) / 2f / Mathf.Tan(properties.FieldOfView * Mathf.PI / 360f);
            EmissionRescalingFactor = displayConfig.SystemDisparityPixels * properties.BaselineScaling * properties.ConvergenceDistance / f;
        }
    }
}
