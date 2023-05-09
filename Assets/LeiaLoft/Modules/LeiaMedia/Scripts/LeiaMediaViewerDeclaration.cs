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
    public partial class LeiaMediaViewer : UnityEngine.MonoBehaviour, ILeiaMediaMaterialHandler
    {

        public enum LeiaMediaType
        {
            Texture,
            Video,
            VideoURL
        }

        public enum MediaScaleMode
        {
            WorldXYZScale,
            OnscreenPercent
        }

 
        public delegate void OnMediaChanged();
        public event OnMediaChanged VideoChangedResponses;

        private Mesh _meshPlane;
        private Mesh meshPlane
        {
            get
            {
                if (_meshPlane == null)
                {
                    _meshPlane = new Mesh();
                    _meshPlane.name = "LeiaMediaViewer plane";
                    _meshPlane.hideFlags = HideFlags.HideAndDontSave;
                    _meshPlane.vertices = new[] { new Vector3(-0.5f, -0.5f, 0), new Vector3(-0.5f, 0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(0.5f, -0.5f, 0) };
                    _meshPlane.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
                    _meshPlane.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                    _meshPlane.RecalculateBounds();
                }
                return _meshPlane;
            }
        }

        MeshRenderer _mr;
        private MeshRenderer mr {
            get {
                if (_mr == null) {
                    _mr = GetComponent<MeshRenderer>();
                }
                return _mr;
            }
        }

        private MeshFilter _mf;
        private MeshFilter mf {
            get {
                if (_mf == null) {
                    _mf = GetComponent<MeshFilter>();
                }
                return _mf;
            }
        }

        private VideoPlayer _vp;
        private VideoPlayer vp
        {
            get
            {
                if (_vp == null)
                {
                    _vp = GetComponent<VideoPlayer>();
                }
                return _vp;
            }
        }

        private AudioSource _asource;
        private AudioSource asource
        {
            get
            {
                if (_asource == null)
                {
                    _asource = GetComponent<AudioSource>();
                }
                return _asource;
            }
        }

        private LeiaMediaVideoPlayer _lmvp;
        private LeiaMediaVideoPlayer lmvp
        {
            get
            {
                if (_lmvp == null)
                {
                    _lmvp = GetComponent<LeiaMediaVideoPlayer>();
                }
                return _lmvp;
            }
        }

        private MaterialPropertyBlock _mpb;
        private MaterialPropertyBlock mpb {
            get {
                if (_mpb == null) {
                    _mpb = new MaterialPropertyBlock();
                }
                return _mpb;
            }
        }

        public bool automaticAspectRatio = true;

        Material _LeiaMaterial;
        private Material LeiaMaterial
        {
            get {
                if (_LeiaMaterial == null)
                {
                    Material lmsource = Resources.Load(LeiaMaterial_id) as Material;
                    if (lmsource == null)
                    {
                        Debug.LogAssertionFormat("Loading {0} failed. Please reinstall LeiaLoft SDK", LeiaMaterial_id);
                        lmsource = new Material(Shader.Find("Unlit/Texture"));
                    }
                    _LeiaMaterial = Instantiate(lmsource) as Material;
                    _LeiaMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return _LeiaMaterial;
            }
        }

        private readonly string LeiaMaterial_id = "Materials/LeiaMediaMaterial";

        // previus frame state of activeMediaType
        private int p_activeMediaType = -1;

        [SerializeField]
        private LeiaMediaType m_activeMediaType = LeiaMediaType.Texture;

        public LeiaMediaType activeMediaType {
            get
            {
                return m_activeMediaType;
            }

            set
            {
                m_activeMediaType = value;
            }
        }

        [SerializeField]
        private MediaScaleMode m_mediaScaleMode = MediaScaleMode.WorldXYZScale;

        public MediaScaleMode mediaScaleMode
        {
            get
            {
                return m_mediaScaleMode;
            }

            set
            {
                m_mediaScaleMode = value;
            }
        }

        [SerializeField]
        private Rect m_onscreenPercent = new Rect(0, 0, 1, 1);

        public Rect OnscreenPercent
        {
            get {
                return m_onscreenPercent;
            }

            set {
                m_onscreenPercent = value;
            }
        }

        [SerializeField]
        private MediaInfoTextue mitexture = new MediaInfoTextue();
 
        [SerializeField]
        private MediaInfoVideoClip miclip = new MediaInfoVideoClip();

        [SerializeField]
        private MediaInfoVideoURL miurl = new MediaInfoVideoURL();

        Action<string> _onMediaSourceChanged;
        public Action<string> OnMediaSourceChanged {
            get
            {
                return _onMediaSourceChanged;
            }

            set
            {
                _onMediaSourceChanged = value;

            }
        }

        Action<int, int> _onMediaLayoutChanged;
        public Action<int, int> OnMediaLayoutChanged {
            get {

                return _onMediaLayoutChanged;
            }

            set {
                _onMediaLayoutChanged = value;
            }
        }

        Action<LeiaMediaType> _onActiveMediaTypeChanged;
        public Action<LeiaMediaType> OnActiveMediaTypeChanged {
            get {
                return _onActiveMediaTypeChanged;
            }

            set {
                _onActiveMediaTypeChanged = value;
            }
        }

        private MediaInfo this [ int i ]
        {
            get {
                 switch (i)
                 {
                     case 0: return mitexture;
                     case 1: return miclip;
                     case 2: return miurl;
                     default: return mitexture;
                 }
            }
        }

        private readonly int id_main_tex = Shader.PropertyToID("_MainTex");
        private readonly int id_col_count = Shader.PropertyToID("_ColCount");  
        private readonly int id_row_count = Shader.PropertyToID("_RowCount"); 
        private readonly int id_OnscreenPercent = Shader.PropertyToID("_OnscreenPercent"); 
        private readonly int id_EnableOnscreenPercent = Shader.PropertyToID("_EnableOnscreenPercent");
        private readonly string id_user_view_count = "_UserViewCount";
        private readonly string id_device_view_count = "_DeviceViewCount";
        //Flagged to true every time the layout is changed for any reason, reset to false in LateUpdate () after exporting columns / rows to the shader
        private bool d_layout = true;
        //Flagged to true  when active media source is changed,  reset to false after invoke this[activeMediaTypeInt].OnMediaSourceChanged(this); 
        private bool d_mediaSource = true;
        // The counter of repetition of VideoChangedResponses over   frames. Rude temporary fix of not relaible video starting.  
        private bool d_videoChangedResponses ; 
    }

}
