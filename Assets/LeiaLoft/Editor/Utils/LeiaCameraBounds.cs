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
using UnityEditor;

namespace LeiaLoft
{
    public static class LeiaCameraBounds
    {
        /// <summary>
        /// At Edit-time Computes LeiaCameraData when given convergence distance, baseline scaling, DisplayConfig
        /// </summary>
        /// <param name="camera">A Unity Camera to read properties from</param>
        /// <param name="convergenceDistance">A distance along local z-axis</param>
        /// <param name="baselineScaling">A scale factor by which to multiply interocular distance / LeiaView positions / eye separations</param>
        /// <param name="displayConfig">A collection of name-value associations that describe our LeiaDisplay</param>
        /// <returns>A LeiaCameraData object</returns>
        public static LeiaCameraData ComputeLeiaCameraData(Camera camera, float convergenceDistance, float baselineScaling, DisplayConfig displayConfig)
        {
            LeiaCameraData leiaCameraData = new LeiaCameraData();
            displayConfig.UserOrientationIsLandscape = camera.pixelWidth > camera.pixelHeight;
            float f = displayConfig.UserViewResolution.y / 2f / Mathf.Tan(camera.fieldOfView * Mathf.PI / 360f);
            leiaCameraData.baseline = displayConfig.SystemDisparityPixels * baselineScaling * convergenceDistance / f;
            leiaCameraData.screenHalfHeight = convergenceDistance * Mathf.Tan(camera.fieldOfView * Mathf.PI / 360.0f);
            leiaCameraData.screenHalfWidth = GameViewUtils.GetGameViewAspectRatio() * leiaCameraData.screenHalfHeight;
            leiaCameraData.baselineScaling = baselineScaling;
            return leiaCameraData;
        }


        public static LeiaBoundsData ComputeLeiaBoundsData(Camera camera, LeiaCameraData leiaCamera, float convergenceDistance, Vector2 cameraShift, DisplayConfig displayConfig)
        {
            LeiaBoundsData leiaBounds = new LeiaBoundsData();
            var localToWorldMatrix = camera.transform.localToWorldMatrix;

            localToWorldMatrix.SetColumn(0, localToWorldMatrix.GetColumn(0).normalized);
            localToWorldMatrix.SetColumn(1, localToWorldMatrix.GetColumn(1).normalized);
            localToWorldMatrix.SetColumn(2, localToWorldMatrix.GetColumn(2).normalized);
            Rect cameraRect = camera.rect;
            cameraRect.yMax = Mathf.Min(cameraRect.yMax, 1);
            cameraRect.yMin = Mathf.Max(cameraRect.yMin, 0);
            cameraRect.xMax = Mathf.Min(cameraRect.xMax, 1);
            cameraRect.xMin = Mathf.Max(cameraRect.xMin, 0);
            float rh =  cameraRect.height   ;
            float rw = cameraRect.width / rh;
 

            if (camera.orthographic)
            {
                // assumes baseline = (baseline scaling) * (width of view in world units) * (system disparity in pixels) * (convergence distance) / (view width in pixels)
                float halfSizeY = camera.orthographicSize;
                float halfSizeX = halfSizeY * GameViewUtils.GetGameViewAspectRatio();
 
                float left = -halfSizeX * rw;
                float right = -left;  

                Vector3 screenTopLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, halfSizeY, convergenceDistance));
                Vector3 screenTopRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, halfSizeY, convergenceDistance));
                Vector3 screenBottomLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, -halfSizeY, convergenceDistance));
                Vector3 screenBottomRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, -halfSizeY, convergenceDistance));
                float negativeSystemDisparityZ = convergenceDistance - 1.0f / leiaCamera.baselineScaling;
                Vector3 nearTopLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, halfSizeY, negativeSystemDisparityZ));
                Vector3 nearTopRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, halfSizeY, negativeSystemDisparityZ));
                Vector3 nearBottomLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, -halfSizeY, negativeSystemDisparityZ));
                Vector3 nearBottomRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, -halfSizeY, negativeSystemDisparityZ));
                float positiveSystemDisparityZ = convergenceDistance + 1.0f / leiaCamera.baselineScaling;
                Vector3 farTopLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, halfSizeY, positiveSystemDisparityZ));
                Vector3 farTopRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, halfSizeY, positiveSystemDisparityZ));
                Vector3 farBottomLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, -halfSizeY, positiveSystemDisparityZ));
                Vector3 farBottomRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, -halfSizeY, positiveSystemDisparityZ));
                leiaBounds.screen = new[] { screenTopLeft, screenTopRight, screenBottomRight, screenBottomLeft };
                leiaBounds.south = new[] { nearTopLeft, nearTopRight, nearBottomRight, nearBottomLeft };
                leiaBounds.north = new[] { farTopLeft, farTopRight, farBottomRight, farBottomLeft };
                leiaBounds.top = new[] { nearTopLeft, nearTopRight, farTopRight, farTopLeft };
                leiaBounds.bottom = new[] { nearBottomLeft, nearBottomRight, farBottomRight, farBottomLeft };
                leiaBounds.east = new[] { nearTopRight, nearBottomRight, farBottomRight, farTopRight };
                leiaBounds.west = new[] { nearTopLeft, nearBottomLeft, farBottomLeft, farTopLeft };
            }
            else
            {
                float halfSizeX = leiaCamera.screenHalfHeight;
                float left = -leiaCamera.screenHalfWidth * rw;
                float right = -left;

                cameraShift = leiaCamera.baseline * cameraShift;
                Vector3 screenTopLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, halfSizeX, convergenceDistance));
                Vector3 screenTopRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, halfSizeX, convergenceDistance));
                Vector3 screenBottomLeft = localToWorldMatrix.MultiplyPoint(new Vector3(left, -halfSizeX, convergenceDistance));
                Vector3 screenBottomRight = localToWorldMatrix.MultiplyPoint(new Vector3(right, -halfSizeX, convergenceDistance));
                float nearPlaneZ = (leiaCamera.baselineScaling * convergenceDistance) / (leiaCamera.baselineScaling + 1f);
                float nearRatio = nearPlaneZ / convergenceDistance;
                float nearShiftRatio = 1f - nearRatio;
                Bounds localNearPlaneBounds = new Bounds(
                  new Vector3(nearShiftRatio * cameraShift.x, nearShiftRatio * cameraShift.y, nearPlaneZ),
                  new Vector3(right * nearRatio * 2, leiaCamera.screenHalfHeight * nearRatio * 2, 0));
                Vector3 nearTopLeft = localToWorldMatrix.MultiplyPoint(new Vector3(localNearPlaneBounds.min.x, localNearPlaneBounds.max.y, localNearPlaneBounds.center.z));
                Vector3 nearTopRight = localToWorldMatrix.MultiplyPoint(new Vector3(localNearPlaneBounds.max.x, localNearPlaneBounds.max.y, localNearPlaneBounds.center.z));
                Vector3 nearBottomLeft = localToWorldMatrix.MultiplyPoint(new Vector3(localNearPlaneBounds.min.x, localNearPlaneBounds.min.y, localNearPlaneBounds.center.z));
                Vector3 nearBottomRight = localToWorldMatrix.MultiplyPoint(new Vector3(localNearPlaneBounds.max.x, localNearPlaneBounds.min.y, localNearPlaneBounds.center.z));
                float farPlaneZ = (leiaCamera.baselineScaling * convergenceDistance) / (leiaCamera.baselineScaling - 1f);
                farPlaneZ = 1f / Mathf.Max(1f / farPlaneZ, 1e-5f);
                float farRatio = farPlaneZ / convergenceDistance;
                float farShiftRatio = 1f - farRatio;
                Bounds localFarPlaneBounds = new Bounds(
                  new Vector3(farShiftRatio * cameraShift.x, farShiftRatio * cameraShift.y, farPlaneZ),
                  new Vector3(right * farRatio * 2, halfSizeX * farRatio * 2, 0));

                Vector3 farTopLeft = localToWorldMatrix.MultiplyPoint(new Vector3(localFarPlaneBounds.min.x, localFarPlaneBounds.max.y, localFarPlaneBounds.center.z));
                Vector3 farTopRight = localToWorldMatrix.MultiplyPoint(new Vector3(localFarPlaneBounds.max.x, localFarPlaneBounds.max.y, localFarPlaneBounds.center.z));
                Vector3 farBottomLeft = localToWorldMatrix.MultiplyPoint(new Vector3(localFarPlaneBounds.min.x, localFarPlaneBounds.min.y, localFarPlaneBounds.center.z));
                Vector3 farBottomRight = localToWorldMatrix.MultiplyPoint(new Vector3(localFarPlaneBounds.max.x, localFarPlaneBounds.min.y, localFarPlaneBounds.center.z));

                leiaBounds.screen = new[] { screenTopLeft, screenTopRight, screenBottomRight, screenBottomLeft };
                leiaBounds.south = new[] { nearTopLeft, nearTopRight, nearBottomRight, nearBottomLeft };
                leiaBounds.north = new[] { farTopLeft, farTopRight, farBottomRight, farBottomLeft };
                leiaBounds.top = new[] { nearTopLeft, nearTopRight, farTopRight, farTopLeft };
                leiaBounds.bottom = new[] { nearBottomLeft, nearBottomRight, farBottomRight, farBottomLeft };
                leiaBounds.east = new[] { nearTopRight, nearBottomRight, farBottomRight, farTopRight };
                leiaBounds.west = new[] { nearTopLeft, nearBottomLeft, farBottomLeft, farTopLeft };
            }
            return leiaBounds;
        }

        private static readonly Color _leiaScreenPlaneColor = new Color(20 / 255.0f, 100 / 255.0f, 160 / 255.0f, 0.2f);
        private static readonly Color _leiaScreenWireColor = new Color(35 / 255.0f, 200 / 255.0f, 1.0f, 0.6f);
        private static readonly Color _leiaBoundsPlaneColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);
        private static readonly Color _leiaBoundsWireColor = new Color(1.0f, 1.0f, 1.0f, 0.2f);

        public static void DrawCameraBounds(LeiaCamera controller, GizmoType gizmoType)
        {

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0_0 || UNITY_5_0_1
            GizmoType notSelected = GizmoType.NotSelected;
#elif UNITY_5_0_2 || UNITY_5_0_3 || UNITY_5_0_4
            GizmoType notSelected = GizmoType.NotInSelectionHierarchy;
#else
            GizmoType notSelected = GizmoType.NonSelected;
#endif
            if ((gizmoType & notSelected) != 0 && controller.DrawCameraBounds == false)
            {
                return;
            }

            var camera = controller.GetComponent<Camera>();

            if (camera == null)
            {
                return;
            }

			DisplayConfig displayConfig;

            if (Application.isPlaying)
            {
                displayConfig = LeiaDisplay.Instance.GetDisplayConfig();
            }
            else
            {
                displayConfig = Object.FindObjectOfType<LeiaDisplay>().GetDisplayConfig();
            }

            LeiaCameraData leiaCameraData = ComputeLeiaCameraData(
                camera,
                controller.ConvergenceDistance,
                controller.BaselineScaling,
				displayConfig);

            LeiaBoundsData leiaBoundsData = ComputeLeiaBoundsData(camera, leiaCameraData, controller.ConvergenceDistance, controller.CameraShift, displayConfig);

            if (((gizmoType & notSelected) != 0 && controller.DrawCameraBounds) || (gizmoType & GizmoType.Selected) != 0)
            {
                // draw convergence plane in editor play mode
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.screen, _leiaScreenPlaneColor, _leiaScreenWireColor);
            }

            if (((gizmoType & notSelected) != 0 && controller.DrawCameraBounds) || (gizmoType & GizmoType.Selected) != 0)
            {
                // draw frustum outline in white
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.north, _leiaBoundsPlaneColor, _leiaBoundsWireColor);
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.south, _leiaBoundsPlaneColor, _leiaBoundsWireColor);
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.east, _leiaBoundsPlaneColor, _leiaBoundsWireColor);
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.west, _leiaBoundsPlaneColor, _leiaBoundsWireColor);
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.top, _leiaBoundsPlaneColor, _leiaBoundsWireColor);
                Handles.DrawSolidRectangleWithOutline(leiaBoundsData.bottom, _leiaBoundsPlaneColor, _leiaBoundsWireColor);
            }
        }

    }
}