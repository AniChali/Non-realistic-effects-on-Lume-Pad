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
using UnityEngine.Rendering;

#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
using UnityEngine.Rendering.Universal;
#endif

namespace LeiaLoft
{
    /// <summary>
    /// Wrapper around child LeiaCamera view camera.
    /// </summary>
    public class LeiaView: IReleasable
    {
        public static readonly CameraEvent[] LeiaMediaEventTimes = new[] { CameraEvent.BeforeGBuffer, CameraEvent.BeforeForwardOpaque, CameraEvent.AfterEverything };
        private readonly CommandBuffer[] leiaMediaCommandBuffers = new CommandBuffer[3];

        public static string ENABLED_NAME { get { return "LeiaView"; } }
        public static string DISABLED_NAME { get { return "Disabled_LeiaView"; } }

        private int _viewIndexX = -1;
        private int _viewIndexY = -1;
        private int _viewIndex = -1;

        private readonly System.Collections.Generic.Dictionary<System.Type, Behaviour> _trackedBehaviours = new System.Collections.Generic.Dictionary<System.Type, Behaviour>();

        /// <summary>
        /// Absolute index of this view. The ith LeiaView will have ViewIndex i regardless of its position in a camera grid.
        /// </summary>
        public int ViewIndex
        {
            get
            {
                return (IsCameraNull || !Enabled) ? -1 : _viewIndex;
            }
            set
            {
                _viewIndex = value;
            }
        }

        /// <summary>
        /// First dimension of position in a n x m grid of cameras
        /// </summary>
        public int ViewIndexX
        {
            get
            {
                return (IsCameraNull || !Enabled) ? -1 : _viewIndexX;
            }
            set
            {
                _viewIndexX = value;
            }
        }

        /// <summary>
        /// Second dimension of position in a n x m grid of cameras
        /// </summary>
        public int ViewIndexY
        {
            get
            {
                return (IsCameraNull || !Enabled) ? -1 : _viewIndexY;
            }
            set
            {
                _viewIndexY = value;
            }
        }

        private Camera _camera;

        // maintain same style as LeiaCamera :: Camera
        public Camera Camera
        {
            get
            {
                return _camera;
            }
        }

        public bool IsCameraNull
        {
            get { return _camera ? false : true; }
        }

        /// <summary>
        /// Pass-through property so users can refer to LeiaView's gameObject as LeiaView.Object or LeiaView.gameObject
        /// </summary>
        public GameObject gameObject
        {
            get
            {
                return this.Object;
            }
        }

        public GameObject Object
        {
            get { return _camera ? _camera.gameObject : default(GameObject); }
        }

        public Vector3 Position
        {
            get { return _camera.transform.localPosition; }
            set { _camera.transform.localPosition = value; }
        }

        public Matrix4x4 Matrix
        {
            get { return _camera.projectionMatrix; }
            set { _camera.projectionMatrix = value; }
        }

        public float FarClipPlane
        {
            get { return _camera.farClipPlane; }
            set { _camera.farClipPlane = value; }
        }

        public float NearClipPlane
        {
            get { return _camera.nearClipPlane; }
            set { _camera.nearClipPlane = value; }
        }

        public Rect ViewRect
        {
            get { return _camera.rect; }
            set { _camera.rect = value; }
        }

        public RenderTexture TargetTexture
        {
            get { return !_camera ? null : _camera.targetTexture; }
            set { if (_camera) { _camera.targetTexture = value; } }
        }

        public bool Enabled
        {
            get { return !_camera ? false : _camera.enabled; }
            set { if (_camera) { _camera.enabled = value; } }
        }

        /// <summary>
        /// Creates a renderTexture with a specific width and height, but no name.
        /// 
        /// Use cases - user has old code, wants to continue compiling with old code.
        /// Or user intentionally wants to not specify a name for RenderTexture.
        /// </summary>
        /// <param name="width">Width of renderTexture</param>
        /// <param name="height">Height of renderTexture</param>
        public void SetTextureParams(int width, int height)
        {
            SetTextureParams(width, height, "");
        }

        /// <summary>
        /// Creates a renderTexture.
        /// </summary>
        /// <param name="width">Width of renderTexture in pixels</param>
        /// <param name="height">Height of renderTexture in pixels</param>
        /// <param name="viewName">Name of renderTexture</param>
        public void SetTextureParams(int width, int height, string viewName)
        {
            if (IsCameraNull)
            {
                return;
            }

            if (_camera.targetTexture == null)
            {
                TargetTexture = CreateRenderTexture(width, height, viewName);
            }
            else
            {
                if (TargetTexture.width != width ||
                    TargetTexture.height != height)
                {
                    Release();
                    TargetTexture = CreateRenderTexture(width, height, viewName);
                }
            }
        }

        private static RenderTexture CreateRenderTexture(int width, int height, string rtName)
        {
            var leiaViewSubTexture = new RenderTexture(width, height, 0)
            {
                name = rtName,
            };
            leiaViewSubTexture.ApplyIntermediateTextureRecommendedProperties();
            leiaViewSubTexture.ApplyLeiaViewRecommendedProperties();
            leiaViewSubTexture.Create();

            return leiaViewSubTexture;
        }

        /// <summary>
        /// Gets parameters from root camera
        /// </summary>
        public void RefreshParameters(UnityCameraParams cameraParams)
        {
            if (IsCameraNull)
            {
                return;
            }

            _camera.clearFlags = cameraParams.ClearFlags;
            _camera.cullingMask = cameraParams.CullingMask;
            _camera.depth = cameraParams.Depth;
            _camera.backgroundColor = cameraParams.BackgroundColor;
            _camera.orthographic = cameraParams.Orthographic;
            _camera.orthographicSize = cameraParams.OrthographicSize;
            _camera.fieldOfView = cameraParams.FieldOfView;
            ViewRect = cameraParams.ViewportRect;
#if UNITY_5_6_OR_NEWER
            _camera.allowHDR = cameraParams.AllowHDR;
#else
			_camera.hdr = cameraParams.AllowHDR;
#endif
            _camera.renderingPath = cameraParams.RenderingPath;
            _camera.useOcclusionCulling = cameraParams.UseOcclusionCulling;
#if UNITY_2020_2_OR_NEWER && LEIA_URP_DETECTED
            //In case URP is installed but not assigned in Graphics Settings, do not look for AdditionalCameraData (would be null)
            if (!RenderPipelineUtils.IsUnityRenderPipeline())
            {
                return;
            }
            _camera.GetUniversalAdditionalCameraData().SetRenderer(cameraParams.ScriptableRendererIndex);
            _camera.GetUniversalAdditionalCameraData().antialiasing = cameraParams.AntialiasingMode;
            _camera.GetUniversalAdditionalCameraData().antialiasingQuality = cameraParams.AntialiasingQuality;
            _camera.GetUniversalAdditionalCameraData().dithering = cameraParams.Dithering;
            _camera.GetUniversalAdditionalCameraData().renderPostProcessing = cameraParams.RenderPostProcessing;
            _camera.GetUniversalAdditionalCameraData().renderShadows = cameraParams.RenderShadows;
            _camera.GetUniversalAdditionalCameraData().requiresColorOption = cameraParams.RequiresColorOption;
            _camera.GetUniversalAdditionalCameraData().requiresColorTexture = cameraParams.RequiresColorTexture;
            _camera.GetUniversalAdditionalCameraData().requiresDepthOption = cameraParams.RequiresDepthOption;
            _camera.GetUniversalAdditionalCameraData().requiresDepthTexture = cameraParams.RequiresDepthTexture;
            _camera.GetUniversalAdditionalCameraData().stopNaN = cameraParams.StopNaN;
            _camera.GetUniversalAdditionalCameraData().volumeLayerMask = cameraParams.VolumeLayerMask;
            _camera.GetUniversalAdditionalCameraData().volumeTrigger = cameraParams.VolumeTrigger;
#endif
        }

        /// <summary>
        /// Detaches a Behaviour of a given type
        /// </summary>
        /// <param name="behaviourType">A type of Behaviour effect which was previously added using AttachBehaviourToView, which we wish to now detach</param>
        /// <returns>True if a Behaviour of Type was previously attached using AttachBehaviourToView and has now been destroyed</returns>
        public bool DetachBehaviourFromView(System.Type behaviourType)
        {
            if (_trackedBehaviours.ContainsKey(behaviourType))
            {
                // create a process for destroying objects
                System.Action<Object> destroyScript = (o) =>
                {
                    if (!Application.isPlaying) { UnityEngine.Object.DestroyImmediate(o); }
                    else { UnityEngine.Object.Destroy(o); }
                };

                destroyScript(_trackedBehaviours[behaviourType]);
                // drop reference
                _trackedBehaviours.Remove(behaviourType);

                return true;
            }
            else
            {
                // no known component; give feedback tha we didn't succeed in detaching script
                return false;
            }
        }

        /// <summary>
        /// Clones a MonoBehaviour using Reflection and attaches the cloned MonoBehaviour to the LeiaView's gameObject. If user attempts to attach multiple MonoBehaviours of same type to this LeiaView, process will only apply the latest provided Behaviour
        /// </summary>
        /// <param name="original">A Behaviour to copy</param>
        /// <returns>The attached Behaviour</returns>
        public Behaviour AttachBehaviourToView(Behaviour original)
        {
            if (original == null) { return null; }
            System.Type type = original.GetType();

            Behaviour copy = null;

            if (_trackedBehaviours.ContainsKey(type))
            {
                copy = _trackedBehaviours[type];
            }
            if (copy == null)
            {
                // sometimes the Behaviour already exists but is not tracked, due to user modifying object states
                copy = (Behaviour) this.gameObject.GetComponent(original.GetType());
            }

            if (copy == null)
            {
                // else add the component
                // this is a point at which Behaviour.OnEnable will be triggered
                copy = (Behaviour) gameObject.AddComponent(type);
            }

            if (copy != null)
            {
                _trackedBehaviours[type] = copy;
            }

            if (copy != null && original != null)
            {
                // "copy" is now non-null. now fill in every property
                copy.CopyFieldsFrom(original, original.GetComponent<Camera>(), this);

                copy.enabled = false;
                copy.enabled = original.enabled;
            }

            return copy;
        }

        /// <summary>
        /// A method for attaching a CommandBuffer to a LeiaView.
        /// Dispose of CommandBuffer in calling function.
        /// </summary>
        /// <param name="cb">The CommandBuffer to attach</param>
        /// <param name="eventTime">The CameraEvent which should trigger the CommandBuffer</param>
        public void AttachCommandBufferToView(CommandBuffer cb, CameraEvent eventTime)
        {
            if (_camera != null)
            {
                _camera.AddCommandBuffer(eventTime, cb);
            }
        }

        public void AttachLeiaMediaCommandBuffersForIndex(int index)
        {
            for (int i = 0; i < leiaMediaCommandBuffers.Length; i++)
            {
                if (leiaMediaCommandBuffers[i] == null)
                {
                    // for the "AfterEverything" CommandBuffer, we wish to have it be -1, regardless of what the LeiaView's index is
                    int indexFlag = index;
                    if (LeiaMediaEventTimes[i] == CameraEvent.AfterEverything)
                    {
                        indexFlag = -1;
                    }

                    // attach a CommandBuffer which sets _LeiaViewID early in deferred and forward rendering paths
                    leiaMediaCommandBuffers[i] = new CommandBuffer { name = "_LeiaViewID = " + indexFlag.ToString() };
                    leiaMediaCommandBuffers[i].SetGlobalFloat("_LeiaViewID", indexFlag);
                    // deferred: beforegbuffer, forward: beforeforwardopaque
                    AttachCommandBufferToView(leiaMediaCommandBuffers[i], LeiaMediaEventTimes[i]);
                }
            }
        }

        public LeiaView(GameObject root, UnityCameraParams cameraParams)
        {
            this.Debug("ctor()");
            var rootCamera = root.GetComponent<Camera>();

            for (int i = 0; i < rootCamera.transform.childCount; i++)
            {
                var child = rootCamera.transform.GetChild(i);

                if (child.name == DISABLED_NAME)
                {
                    child.name = ENABLED_NAME;
                    child.hideFlags = HideFlags.None;
                    _camera = child.GetComponent<Camera>();
                    _camera.enabled = true;

#if UNITY_5_6_OR_NEWER
                    _camera.allowHDR = cameraParams.AllowHDR;
#else
					_camera.hdr = cameraParams.AllowHDR;
#endif
                    break;
                }
            }

            if (_camera == null)
            {
                _camera = new GameObject(ENABLED_NAME).AddComponent<Camera>();
            }

            _camera.transform.parent = root.transform;
            _camera.transform.localPosition = Vector3.zero;
            _camera.transform.localRotation = Quaternion.identity;
            _camera.clearFlags = cameraParams.ClearFlags;
            _camera.cullingMask = cameraParams.CullingMask;
            _camera.depth = cameraParams.Depth;
            _camera.backgroundColor = cameraParams.BackgroundColor;
            _camera.fieldOfView = cameraParams.FieldOfView;
            _camera.depthTextureMode = DepthTextureMode.None;
            _camera.hideFlags = HideFlags.None;
            _camera.orthographic = cameraParams.Orthographic;
            _camera.orthographicSize = cameraParams.OrthographicSize;
            _camera.renderingPath = cameraParams.RenderingPath;
            ViewRect = rootCamera.rect;
#if UNITY_5_6_OR_NEWER
            _camera.allowHDR = cameraParams.AllowHDR;
#else
			_camera.hdr = cameraParams.AllowHDR;
#endif

        }

        public void Release()
        {
            // targetTexture can be null at this point in execution
            if (TargetTexture != null)
            {
                if (Application.isPlaying)
                {
                    TargetTexture.Release();
                    GameObject.Destroy(TargetTexture);
                }
                else
                {
                    TargetTexture.Release();
                    GameObject.DestroyImmediate(TargetTexture);
                }

                TargetTexture = null;
            }

            // internal LeiaMedia CommandBuffers are released at same time as all other Disposable / Releasable resources
            for (int i = 0; i < leiaMediaCommandBuffers.Length; i++)
            {
                if (leiaMediaCommandBuffers[i] != null)
                {
                    if (_camera != null)
                    {
                        _camera.RemoveCommandBuffer(LeiaMediaEventTimes[i], leiaMediaCommandBuffers[i]);
                    }
                    leiaMediaCommandBuffers[i].Dispose();
                    leiaMediaCommandBuffers[i] = null;
                }
            }
        }
    }
}
