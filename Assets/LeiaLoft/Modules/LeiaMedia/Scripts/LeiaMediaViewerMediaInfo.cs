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

        [System.Serializable]
        abstract class MediaInfo
        {

            public abstract LeiaMediaType type { get; }

            public Vector2Int parsedLayout = new Vector2Int(2, 1);
            private Vector2Int _parsedLayout;

            public bool useCustomLayout = false;
            private bool _useCustomLayout;

            public Vector2Int customLayout = new Vector2Int(2, 1);
            private Vector2Int _customLayout;

            public abstract string mediaName { get; }

            /// <summary>
            /// Tracks whether media property has changed
            /// </summary>
            /// <returns>True if the MediaInfo's Texture/VideoClip/string videoURL has changed</returns>
            protected abstract bool GetMediaSourceChanged();

            public abstract Texture GetTexture(LeiaMediaViewer lmv);

            public abstract void OnMediaInfoSourceChanged(LeiaMediaViewer lmv);

            public void OnUpdate(LeiaMediaViewer lmv)
            {
                lmv.d_mediaSource = GetMediaSourceChanged() || lmv.d_mediaSource;
                if (lmv.d_mediaSource)
                {
                    parsedLayout = new Vector2Int(-1, -1);
                    string _mediaName = mediaName;
                    if (!string.IsNullOrEmpty(_mediaName))
                    {
                        int cols = -1, rows = -1;
                        if (StringExtensions.TryParseColsRowsFromFilename(_mediaName, out cols, out rows))
                        {
                            parsedLayout = new Vector2Int(cols, rows);
                        }
                    }
                }


                if (useCustomLayout != _useCustomLayout)
                {
                    lmv.d_layout = true;
                }

                if (useCustomLayout)
                {
                    if (customLayout != _customLayout)
                    {
                        lmv.d_layout = true;
                    }
                }
                else
                {
                    if (parsedLayout != _parsedLayout)
                    {
                        lmv.d_layout = true;
                    }
                }

                _parsedLayout = parsedLayout;
                _useCustomLayout = useCustomLayout;
                _customLayout = customLayout;

            }

            public Vector2Int filteredLayout {
                get {
                    Vector2Int result = useCustomLayout ? customLayout : parsedLayout;
                    result.x = Mathf.Max(1, result.x);
                    result.y = Mathf.Max(1, result.y);
                    return result;
                }
            }

            public virtual void OnMediaInfoActive(LeiaMediaViewer lmv)
            {
                lmv.d_mediaSource = true;
                lmv.d_layout = true;
            }

            public virtual void OnMediaInfoInactive(LeiaMediaViewer lmv)
            {

            }
        }

        [System.Serializable]
        class MediaInfoTextue : MediaInfo
        {

            public Texture texture;
            private Texture _texture;

            public override LeiaMediaType type
            {
                get
                {
                    return LeiaMediaType.Texture;
                }
            }

            protected override bool GetMediaSourceChanged()
            {
                if (texture != _texture)
                {
                    _texture = texture;
                    return true;
                }
                return false;
            }

            public override string mediaName
            {
                get
                {
                    return texture == null ? "" : texture.name;
                }
            }

            public override void OnMediaInfoActive(LeiaMediaViewer lmv)
            {
                base.OnMediaInfoActive(lmv);
                lmv.asource.hideFlags = HideFlags.HideInInspector;
                lmv.vp.hideFlags = HideFlags.HideInInspector;
                lmv.lmvp.hideFlags = HideFlags.HideInInspector;
            }

            public override Texture GetTexture(LeiaMediaViewer lmv)
            {
                return texture;
            }

            public override void OnMediaInfoSourceChanged(LeiaMediaViewer lmv)
            {

            }
        }

        [System.Serializable]
        class MediaInfoVideoClip : MediaInfo
        {

            public override LeiaMediaType type
            {
                get
                {
                    return LeiaMediaType.Video;
                }
            }

            public VideoClip clip;
            private VideoClip _clip;

            protected override bool GetMediaSourceChanged()
            {
                if (clip != _clip)
                {
                    _clip = clip;
                    return true;
                }
                return false;
            }

            public override string mediaName
            {
                get
                {
                    return clip == null ? "" : clip.name;
                }
            }

            public override void OnMediaInfoActive(LeiaMediaViewer lmv)
            {
                base.OnMediaInfoActive(lmv);
                lmv.asource.hideFlags = HideFlags.None;
                lmv.vp.hideFlags = HideFlags.None;
                lmv.lmvp.hideFlags = HideFlags.None;
                lmv.vp.enabled = true;
            }

            public override Texture GetTexture(LeiaMediaViewer lmv)
            {
                return clip == null ? null : lmv.vp.texture;
            }

            public override void OnMediaInfoSourceChanged(LeiaMediaViewer lmv)
            {
                lmv.vp.clip = clip;
                lmv.vp.source = VideoSource.VideoClip;
                lmv.SetVideoChangedResponsesDirty();
            }
        }

        [System.Serializable]
        class MediaInfoVideoURL : MediaInfoVideoClip
        {

            public override LeiaMediaType type
            {
                get
                {
                    return LeiaMediaType.VideoURL;
                }
            }

            public string url;
            private string _url;

            protected override bool GetMediaSourceChanged()
            {
                if (url != _url)
                {
                    _url = url;
                    return true;
                }
                return false;
            }

            public override string mediaName
            {
                get
                {
                    return url == null ? "" : url;
                }
            }

            public override void OnMediaInfoActive(LeiaMediaViewer lmv)
            {
                base.OnMediaInfoActive(lmv);
                lmv.asource.hideFlags = HideFlags.None;
                lmv.vp.hideFlags = HideFlags.None;
                lmv.lmvp.hideFlags = HideFlags.None;
                lmv.vp.enabled = true;
            }

            public override void OnMediaInfoSourceChanged(LeiaMediaViewer lmv)
            {
                if (Application.isPlaying)
                {
                    lmv.vp.url = url;
                    lmv.vp.source = VideoSource.Url;
                }
                lmv.SetVideoChangedResponsesDirty();
            }

            public override Texture GetTexture(LeiaMediaViewer lmv)
            {
                return url == null ? null : lmv.vp.texture;
            }
        }
 
    }

}
