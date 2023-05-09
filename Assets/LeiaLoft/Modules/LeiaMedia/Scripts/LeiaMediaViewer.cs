/****************************************************************
*
* Copyright 2019 Â© Leia Inc.  All rights reserved.
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

using System;
using UnityEngine;
using UnityEngine.Video;

namespace LeiaLoft
{
    /// <summary>
    /// Class for spawning an object with a LeiaMaterial in front of the LeiaCamera.
    /// The LeiaMaterial gives different views to different LeiaCameras.
    /// Media types which can be rendered using a LeiaMaterial include images (textures) and video.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(LeiaMediaVideoPlayer))]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/modules/leia-media")]
    [ExecuteInEditMode]
    public partial class LeiaMediaViewer : UnityEngine.MonoBehaviour, ILeiaMediaMaterialHandler  
    {
 
        void OnEnable()
        {
            p_activeMediaType = -1;
            d_layout = true;
            d_mediaSource = true;

            if (!LeiaDisplay.InstanceIsNull)
            {
                LeiaDisplay.Instance.StateChanged += OnLeiaDisplayStateChanged;
            }
            SetVideoChangedResponsesDirty();
        }

        void OnLeiaDisplayStateChanged()
        {
 
            if (!LeiaDisplay.InstanceIsNull)
            {
                DisplayConfig dc = LeiaDisplay.Instance.GetDisplayConfig();
                if (LeiaDisplay.Instance.DesiredLightfieldMode == LeiaDisplay.LightfieldMode.On)
                {
                    mpb.SetFloat(id_user_view_count, dc.UserNumViews.x);
                }
                else
                {
                    mpb.SetFloat(id_user_view_count, 1);
                }
                mpb.SetFloat(id_device_view_count, dc.NumViews.x);
                SetVideoChangedResponsesDirty();
            }
        }

        void LateUpdate()
        {
            if ((int)m_activeMediaType != p_activeMediaType)
            {
                this[p_activeMediaType].OnMediaInfoInactive(this);
                this[(int)m_activeMediaType].OnMediaInfoActive(this);
                p_activeMediaType = (int)m_activeMediaType;
                d_mediaSource = true;
                if (OnActiveMediaTypeChanged != null) {
                    OnActiveMediaTypeChanged(m_activeMediaType);
                }
            }

            this[activeMediaTypeInt].OnUpdate( this );
 
            if (d_mediaSource)
            {
                this[activeMediaTypeInt].OnMediaInfoSourceChanged(this);
                if (OnMediaSourceChanged != null) {
                    OnMediaSourceChanged( this[activeMediaTypeInt].mediaName );
                }
                d_mediaSource = false;
            }

            if (d_layout)
            {
                Vector2Int layout = this[activeMediaTypeInt].filteredLayout;
                mpb.SetFloat(id_col_count, layout.x);
                mpb.SetFloat(id_row_count, layout.y);

                if (OnMediaLayoutChanged != null) {
                    OnMediaLayoutChanged(layout.x, layout.y);
                }

                d_layout = false;

            }

            Texture t = this[activeMediaTypeInt].GetTexture(this);
            if (t == null)
            {
                t = Texture2D.blackTexture;
            }

            mpb.SetTexture(id_main_tex, t);
            mpb.SetVector(id_OnscreenPercent, new Vector4(m_onscreenPercent.x, m_onscreenPercent.y, m_onscreenPercent.width, m_onscreenPercent.height));
            mpb.SetFloat(id_EnableOnscreenPercent, m_mediaScaleMode == MediaScaleMode.OnscreenPercent ? 1 : 0);
            mr.SetPropertyBlock(mpb);
            mf.sharedMesh = meshPlane;
            mr.sharedMaterial = LeiaMaterial;

            if (d_videoChangedResponses )
            {
                if ( VideoChangedResponses != null)
                {
                    VideoChangedResponses();
                }
                d_videoChangedResponses = false;
            }
        }

        private void SetVideoChangedResponsesDirty() {
            d_videoChangedResponses = true;
        }

        private void Dispose()
        {
            if (Application.isPlaying)
            {
                Destroy(LeiaMaterial);
            }
            else
            {
                DestroyImmediate(LeiaMaterial);
            }
            _LeiaMaterial = null;
            if (!LeiaDisplay.InstanceIsNull)
            {
                LeiaDisplay.Instance.StateChanged -= OnLeiaDisplayStateChanged;
            }
        }

        void OnDestroy()
        {
            Dispose();
        }
 
        public int GetLeiaMediaColsFor(LeiaMediaType type)
        {
            return this[(int)type].filteredLayout.x;
        }

        public void SetLeiaMediaColsFor(LeiaMediaType type, int value)
        {
            this[(int)type].useCustomLayout = true;
            this[(int)type].customLayout.x = value;
        }
 
        public int GetLeiaMediaRowsFor(LeiaMediaType type)
        {
            return this[(int)type].filteredLayout.y;
        }

        public void SetLeiaMediaRowsFor(LeiaMediaType type, int value)
        {
            this[(int)type].useCustomLayout = true;
            this[(int)type].customLayout.x = value;           
        }
 
 
        /// <summary>
        /// Gets the video URL of this LeiaMediaViewer
        /// </summary>
        /// <returns></returns>
        public string GetLeiaMediaVideoURL()
        {
            return miurl.url;
        }

        /// <summary>
        /// Sets the video URL of this LeiaMediaViewer
        /// </summary>
        /// <param name="absolute_path">Absolute path to a video clip outside the Unity build</param>
        public void SetVideoURL(string absolute_path)
        {
            miurl.url = absolute_path;
            m_activeMediaType = LeiaMediaType.VideoURL;
        }

        /// <summary>
        /// Sets the video URL of this LeiaMediaViewer
        /// </summary>
        /// <param name="absolute_path">Absolute path to a video clip outside the Unity build</param>
        /// <param name="rows">Leia Media rows</param>
        /// <param name="columns">Leia Media columns</param>
        public void SetVideoURL(string absolute_path, int columns, int rows)
        {
            this[(int)LeiaMediaType.VideoURL].customLayout.x = columns;
            this[(int)LeiaMediaType.VideoURL].customLayout.y = rows;
            m_activeMediaType = LeiaMediaType.VideoURL;
            miurl.url = absolute_path;
        }

        /// <summary>
        /// Gets state of Renderer component on this object
        /// </summary>
        /// <returns>true if Renderer attached and enabled, false otherwise</returns>
        public bool GetRendererActive()
        {
            return (mr.enabled);
        }

        /// <summary>
        /// Sets renderer enabled state
        /// </summary>
        /// <param name="status">true if enabled, false otherwise</param>
        public void SetRendererActive(bool status)
        {
            mr.enabled = status;
        }

        /// <summary>
        /// Toggle MeshRenderer on/off. Most useful for toggling one Leia Media off while toggling another on,
        /// so that movie can seamlessly move from one renderer to another.
        /// </summary>
        public void ToggleRenderer()
        {
            mr.enabled = !mr.enabled;
        }

        /// <summary>
        /// Sets video clip on Leia Media
        /// </summary>
        /// <param name="video_clip">A video clip which is routed through MaterialPropertyBlock</param>
        public void SetVideoClip(VideoClip video_clip)
        {
            miclip.clip = video_clip;
            m_activeMediaType = LeiaMediaType.Video;
        }

        /// <summary>
        ///  Sets video clip on Leia Media 
        /// <param name="video_clip">A video clip which is routed through MaterialPropertyBlock</param>
        /// <param name="rows">Leia Media rows</param>
        /// <param name="columns">Leia Media columns</param>
        public void SetVideoClip(VideoClip video_clip, int columns, int rows)
        {
            miclip.useCustomLayout = true;
            miclip.customLayout.x = columns;
            miclip.customLayout.y = rows;
            miclip.clip = video_clip;
            m_activeMediaType = LeiaMediaType.Video;
        }


        /// <summary>
        /// Sets texture on Leia Media
        /// </summary>
        /// <param name="texture"></param>
        public void SetTexture(Texture texture)
        {
            mitexture.texture = texture;
            m_activeMediaType = LeiaMediaType.Texture;
        }

        /// <summary>
        /// Sets texture on Leia Media
        /// </summary>
        /// <param name="texture">texture to apply to Leia Media</param>
        /// <param name="rows">Leia Media rows</param>
        /// <param name="columns">Leia Media columns</param>
        public void SetTexture(Texture texture, int columns, int rows)
        {
            mitexture.texture = texture;
            mitexture.useCustomLayout = true;
            mitexture.customLayout.x = columns;
            mitexture.customLayout.y = rows;
            m_activeMediaType = LeiaMediaType.Texture;
        }

        /// <summary>
        /// Switches aspect ratio regulation. By default, aspect ratio is regulated by dimensions of Leia Media
        /// </summary>
        public void ToggleAspectRatioRegulation()
        {
            automaticAspectRatio = !automaticAspectRatio;
        }

        /// <summary>
        /// Sets state of aspect ratio regulation.
        /// </summary>
        /// <param name="status">true: Leia Media's local xy scale are changed to fit the media playing on it. false: Leia Media's local xy scale are not changed</param>
        public void SetAspectRatioRegulation(bool status)
        {
            automaticAspectRatio = status;  
        }

        /// <summary>
        /// Retrieves aspect ratio regulation state
        /// </summary>
        /// <returns>true if aspect ratio is corrected by Leia Media, false if aspect ratio is not corrected by Leia Media</returns>
        public bool GetAspectRatioRegulation()
        {
            return (automaticAspectRatio);
        }

        /// <summary>
        /// Forces dimensions/localScale of Leia Media to be forcedx, forcedy, same z
        /// </summary>
        /// <param name="forced_aspect_ratio">(width, height)</param>
        public void ForceAspectRatio(Vector2 forced_aspect_ratio){
            this.Warning("not implemented");
        }

        public void ProjectOntoZDP()
        {
            Transform t = null;
            LeiaCamera ideal_lc = null;

            if (Camera.main != null && Camera.main.GetComponent<LeiaCamera>() != null)
            {
                t = Camera.main.transform;
                ideal_lc = Camera.main.GetComponent<LeiaCamera>();
            }
            else if (FindObjectOfType<LeiaCamera>() != null)
            {
                t = FindObjectOfType<LeiaCamera>().transform;
                ideal_lc = t.GetComponent<LeiaCamera>();
            }
            else if (FindObjectOfType<Camera>() != null)
            {
                t = FindObjectOfType<Camera>().transform;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("No LeiaCamera or Camera in scene");
#else
                LogUtil.Debug("No LeiaCamera or Camera in scene.");
#endif

            }

            if (ideal_lc != null)
            {
                Vector3 error = ideal_lc.transform.position + ideal_lc.ConvergenceDistance * ideal_lc.transform.forward - transform.position;
                Vector3 BF = Vector3.Project(error, ideal_lc.transform.forward);
                transform.position = transform.position + BF;
                transform.rotation = ideal_lc.transform.rotation;
            }
            else if (t != null)
            {
                Vector3 error = t.position + 10.0f * t.forward - transform.position;
                Vector3 BF = Vector3.Project(error, t.forward);
                transform.position = t.position + BF;
                transform.rotation = t.rotation;
            }
        }

        /// <summary>
        /// Property that will always retrieve the string-ish equivalent of our active LeiaMedia
        /// </summary>
        public string ActiveLeiaMediaName
        {
            get
            {
                return this[(int)m_activeMediaType].mediaName;
            }
        }

        /// <summary>
        /// Getter/setter for activeMediaType as integer. Useful for controlling media from Unity drop-down
        /// </summary>
        public int activeMediaTypeInt
        {
            get
            {
                return (int)m_activeMediaType;
            }
            set
            {
                m_activeMediaType = (LeiaMediaType)value;
            }
        }
    }

}
