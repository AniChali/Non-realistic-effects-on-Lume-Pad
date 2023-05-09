using UnityEngine;
using System.Collections.Generic;

#if UNITY_2020_2_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace LeiaLoft
{
    public class LeiaRenderCamera : MonoBehaviour
    {
#pragma warning disable 414
        private Camera myCam;
#pragma warning restore 414

        LeiaCamera _leiaCamera;

#if UNITY_2020_2_OR_NEWER

        private readonly Dictionary<Camera, List<System.Action>> beginViewRenderingEvents = new Dictionary<Camera, List<System.Action>>();

        private readonly Dictionary<Camera, List<System.Action>> endViewRenderingEvents = new Dictionary<Camera, List<System.Action>>();

        // this has to be Start, not OnEnable, because in OnEnable the _leiaCamera is not set set
        void Start()
        {
            if (RenderPipelineUtils.IsUnityRenderPipeline())
            {
                RenderPipelineManager.endFrameRendering += EndFrameRenderHook;
                // this code calls the camera pre-render events events every frame
                RenderPipelineManager.beginCameraRendering += BeginCameraRenderingHook;
                // this code calls the camera post-render events every frame
                RenderPipelineManager.endCameraRendering += EndCameraRenderingHook;

                // this code sets some pre-Render events for each LeiaView each frame
                for (int i = 0; i < _leiaCamera.GetViewCount(); ++i)
                {
                    if (!beginViewRenderingEvents.ContainsKey(_leiaCamera.GetView(i).Camera))
                    {
                        beginViewRenderingEvents[_leiaCamera.GetView(i).Camera] = new List<System.Action>();
                    }

                    // have to not capture the value within the callback
                    float viewID = i * 1.0f;
                    // create the action event here here. will be called later
                    System.Action setViewID = () =>
                    {
                        Shader.SetGlobalFloat("_LeiaViewID", viewID);
                    };

                    beginViewRenderingEvents[_leiaCamera.GetView(i).Camera].Add( setViewID);
                }

                // this code sets some post-Render events for each LeiaView each frame
                for (int i = 0; i < _leiaCamera.GetViewCount(); ++i)
                {
                    if (!endViewRenderingEvents.ContainsKey(_leiaCamera.GetView(i).Camera))
                    {
                        endViewRenderingEvents[_leiaCamera.GetView(i).Camera] = new List<System.Action>();
                    }

                    System.Action unsetViewID = () =>
                    {
                        Shader.SetGlobalFloat("_LeiaViewID", -1);
                    };

                    endViewRenderingEvents[_leiaCamera.GetView(i).Camera].Add(unsetViewID);
                }
            }
        }

        void OnDisable()
        {
            if (RenderPipelineUtils.IsUnityRenderPipeline())
            {
                RenderPipelineManager.endFrameRendering -= EndFrameRenderHook;
                RenderPipelineManager.beginCameraRendering -= BeginCameraRenderingHook;
            }
        }

        void BeginCameraRenderingHook(ScriptableRenderContext context, Camera renderingCam)
        {
            // call events for each view. Generally these will be shader SetFloat events
            if (beginViewRenderingEvents.ContainsKey(renderingCam))
            {
                foreach (System.Action action in beginViewRenderingEvents[renderingCam])
                {
                    action.Invoke();
                }
            }
        }

        void EndCameraRenderingHook(ScriptableRenderContext context, Camera renderingCam)
        {
            if (endViewRenderingEvents.ContainsKey(renderingCam))
            {
                foreach (System.Action action in endViewRenderingEvents[renderingCam])
                {
                    action.Invoke();
                }
            }
        }

        void EndFrameRenderHook(ScriptableRenderContext context, Camera[] cams)
        {
            if (cams != null && myCam != null && cams.Length == 1 && cams[0] == myCam)
            {
                // only need to run EndFramRenderHook for LeiaRenderCamera's myCam
                Camera.SetupCurrent(myCam);
                OnPostRender();
            }
        }

#endif

        /// <summary>
        /// As soon as LeiaRenderCamera component is created / Added, it should have .setLeiaCamera called.
        ///
        /// This populates myCam.
        /// </summary>
        public Camera Camera
        {
            get
            {
                return myCam;
            }
        }

        public void setLeiaCamera(LeiaCamera leiaCamera)
        {
            // we may still end up populating a static collecton of Cameras here.
            // when users use ShareRenderTextures script, exactly one Camera (deepest Camera) needs to have clear flag = root cam's clear flag
            // other cameras need to have Clear flag = depth only
            myCam = leiaCamera.GetComponentInChildren<LeiaRenderCamera>().GetComponent<Camera>();
            _leiaCamera = leiaCamera;
        }


        void OnPostRender()
        {
            LeiaDisplay.Instance.RenderImage(_leiaCamera);
        }

    }
}
