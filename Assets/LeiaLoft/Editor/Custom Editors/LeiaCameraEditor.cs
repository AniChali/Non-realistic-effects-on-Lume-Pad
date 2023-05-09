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
    /// <summary>
    /// Extends LeiaCamera inspector with additional controls for calibration, emulation.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(LeiaCamera))]
    [CanEditMultipleObjects]
    public class LeiaCameraEditor : UnityEditor.Editor
    {
        private const string ConvergenceDistanceFieldLabel = "Convergence Distance";
        private const string BaselineScalingFieldLabel = "Baseline Scaling";
		private const string DrawCameraBoundsFieldLabel = "Always Show Camera Bounds in Scene Editor";
        private const string UpdateEffectsButtonLabel = "Update Effects";
        private const string UpdateEffectsHelpText = "Use to update all the effects manually (for example if you changed some parameter in an effect and want it to be applied to all the views). LeiaCamera code has relevant method: UpdateEffects.";
        private const string NoEffectsControllerHelpText = "If you want to use post effects with LeiaCamera, LeiaDisplay must have `Separate Tiles` On.";

        private LeiaCamera _controller;

        void Awake()
        {
            var displays = Resources.FindObjectsOfTypeAll<LeiaDisplay>();
            if (displays.Length == 0)
            {
                LogUtil.Trace("Creating new " + typeof(LeiaDisplay).Name + " gameObject");
                var go = new GameObject().AddComponent<LeiaDisplay>();
                if (go != null)
                {
                    go.name = go.ObjectName;
                }
            }
 
        }

 
        private void OnSceneGUI()
        {
            LeiaCamera t = target as LeiaCamera;
            int viewCount = t.GetViewCount();
            if ( Application.isPlaying && viewCount>1 &&  t.BaselineScaling >= 2.0e-5f && t.Camera.orthographic)
            {
                float aspect  = t.GetView(0).Camera.aspect ;
                float orthoSize = t.Camera.orthographicSize;
                float nearClip = t.Camera.nearClipPlane;
                float farClip = t.Camera.farClipPlane;
                Handles.color = new Color(1,1,1,0.6f);
    
                float left = -orthoSize * aspect;
                float right = -left;
                float _top = orthoSize;
                float _bottom = -orthoSize;
                Vector3 p0 = new Vector3(left, _bottom, nearClip);
                Vector3 p1 = new Vector3(left, _top, nearClip);
                Vector3 p2 = new Vector3(right, _top, nearClip);
                Vector3 p3 = new Vector3(right, _bottom, nearClip);
                Vector3 p4 = new Vector3(left, _bottom, farClip);
                Vector3 p5 = new Vector3(left, _top, farClip);
                Vector3 p6 = new Vector3(right, _top, farClip);
                Vector3 p7 = new Vector3(right, _bottom, farClip);

                for (int i = 0; i < viewCount; i++)
                {
                    LeiaView lv = t.GetView(i);
                    Transform tr = lv.Camera.transform;
                    Vector3 convergencePoint = t.transform.position + t.transform.forward * t.ConvergenceDistance;
                    Handles.matrix = Matrix4x4.identity;
                    Vector3 viewAxis =  Vector3.Normalize( convergencePoint - tr.position );
                    float d = Vector3.Dot(t.transform.forward, viewAxis);
                    viewAxis /= d;
                    Matrix4x4 tm = tr.localToWorldMatrix;
                    tm.SetColumn(2, viewAxis);
                    Handles.matrix = tm;
                    Handles.DrawLine(p0, p1);
                    Handles.DrawLine(p1, p2);
                    Handles.DrawLine(p2, p3);
                    Handles.DrawLine(p3, p0);
                    Handles.DrawLine(p4, p5);
                    Handles.DrawLine(p5, p6);
                    Handles.DrawLine(p6, p7);
                    Handles.DrawLine(p7, p4);
                    Handles.DrawLine(p0, p4);
                    Handles.DrawLine(p1, p5);
                    Handles.DrawLine(p2, p6);
                    Handles.DrawLine(p3, p7);
                }
             }
            ConvergencyPlaneHandlesDraw(t);
        }

        private static void ConvergencyPlaneHandlesDraw(LeiaCamera t)
        {
            Vector3 ConvergencyPlane = t.transform.position + t.transform.forward * t.ConvergenceDistance;
            ConvergencyPlane = Handles.Slider(ConvergencyPlane, t.transform.forward);
            t.ConvergenceDistance = Vector3.Distance(ConvergencyPlane - new Vector3(t.CameraShift.x, t.CameraShift.y, 0), t.transform.position);
        }

        public override void OnInspectorGUI()
        {
            if (_controller == null)
            {
                _controller = (LeiaCamera)target;
            }

            if (!_controller.enabled)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            // allow multi-object editing
            // display object properties in sequence when multiple objects selected
            for (int i = 0; i < targets.Length; i++)
            {
                _controller = (LeiaCamera)targets[i];

                if (targets.Length > 1)
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.ObjectField(_controller, typeof(LeiaCamera), true);
                }

                UndoableInputFieldUtils.ImmediateFloatField(() => _controller.ConvergenceDistance, v => _controller.ConvergenceDistance = v, ConvergenceDistanceFieldLabel, _controller);
                UndoableInputFieldUtils.ImmediateFloatField(() => _controller.BaselineScaling, v => _controller.BaselineScaling = v, BaselineScalingFieldLabel, _controller);
			    UndoableInputFieldUtils.BoolField(() => _controller.DrawCameraBounds, v => _controller.DrawCameraBounds = v, DrawCameraBoundsFieldLabel, _controller);

                // Only allow users to switch the fill status when outside of play mode and a non-default ViewportRect.
                // When rect values are approximately equal, the rects are equal
                EditorGUI.BeginDisabledGroup(_controller.Camera.rect == new Rect(Vector2.zero, Vector2.one));
                UndoableInputFieldUtils.EnumFieldWithTooltip(
                    () => { return _controller.ViewportRectFill; }, (fill) => { _controller.ViewportRectFill = (LeiaCamera.ViewportRectFillTechnique)fill; },
                    "Fill technique",
                    string.Format("If using a ViewportRect other than (x=0, y=0, w=1, h=1), changes whether pixels are stretched or cropped in final camera's viewport.\n\nFor simple rendering camera settings, try {0}.\n\nIf using anti-aliasing, HDR, Post-Processing, or Deferred rendering try {1}",
                        LeiaCamera.ViewportRectFillTechnique.TruncatedRectOfFullRenderTexture, LeiaCamera.ViewportRectFillTechnique.FullRectOfTruncatedRenderTexture),
                    _controller);

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Separator();

                if (EditorApplication.isPlaying)
                {
                    if (!LeiaDisplay.InstanceIsNull)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(UpdateEffectsButtonLabel))
                        {
                            _controller.UpdateEffects();
                        }

                        EditorGUILayout.HelpBox(UpdateEffectsHelpText, MessageType.Info);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(NoEffectsControllerHelpText, MessageType.Info);
                    }
                }

                if (targets.Length > 1)
                {
                    EditorGUILayout.Separator();
                }
            }

            if(EditorGUI.EndChangeCheck())
            {
                foreach (Object obj in targets)
                {
                    EditorUtility.SetDirty(obj);
                }
            }
            
        }

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0_0 || UNITY_5_0_1
        [DrawGizmo(GizmoType.Selected | GizmoType.NotSelected)]
#elif UNITY_5_0_2 || UNITY_5_0_3 || UNITY_5_0_4
        [DrawGizmo(GizmoType.Selected | GizmoType.NotInSelectionHierarchy)]
#else
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
#endif
        private static void OnDrawLeiaBounds(LeiaCamera controller, GizmoType gizmoType) 
        {
             LeiaCameraBounds.DrawCameraBounds(controller, gizmoType);
        }
    }
}