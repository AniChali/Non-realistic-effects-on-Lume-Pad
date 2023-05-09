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

using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace LeiaLoft
{

    /// <summary>
    /// LeiaMediaVideoPlayer wraps a VideoPlayer componenet and offers controlled access to video / audio funtions.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(LeiaMediaViewer))]
    public class LeiaMediaVideoPlayer : UnityEngine.MonoBehaviour
    {
        private VideoPlayer mVideoPlayer;
        private VideoPlayer _video
        {
            get
            {
                if (mVideoPlayer == null)
                {
                    mVideoPlayer = GetComponent<VideoPlayer>();
                }
                return mVideoPlayer;
            }
        }

        private AudioSource mAudioSource;
        private AudioSource _audio
        {
            get
            {
                if (mAudioSource == null)
                {
                    mAudioSource = GetComponent<AudioSource>();
                }
                return mAudioSource;
            }
        }

        private LeiaMediaViewer mLeiaMediaViewer;
        private LeiaMediaViewer _leiaMediaViewer
        {
            get
            {
                if (mLeiaMediaViewer == null)
                {
                    mLeiaMediaViewer = GetComponent<LeiaMediaViewer>();
                }
                return mLeiaMediaViewer;
            }
        }

        enum VideoSourceType { Clip, URL }
        VideoSourceType video_source = VideoSourceType.Clip;

        VideoClip _lastKnownClip;

        #region Delegate Events

        public delegate void ClipInitializeAction();
        public event ClipInitializeAction OnClipInitialize;

        public delegate void ClipCompletAction();
        public event ClipCompletAction OnClipComplete;

        public delegate void ClipLoopAction();
        public event ClipLoopAction OnClipLoop;

        public delegate void ClipDisconnectAction();
        public event ClipDisconnectAction OnClipDisconnect;

        public delegate void ClipSwapAction();
        public event ClipSwapAction OnClipSwap;

        public delegate void ClipPlayActon();
        public event ClipPlayActon OnClipPlay;

        public delegate void ClipPauseAction();
        public event ClipPauseAction OnClipPause;

        #endregion
        #region Unity Functions

        private void Start()
        {
            _video.loopPointReached += EndReached;
            _leiaMediaViewer.VideoChangedResponses += InitializeVideo;
            InitializeVideo();
        }

        private void Update()
        {
            CheckClipAdjustment(false);
        }

        private void OnDisable()
        {
            _video.loopPointReached -= EndReached;
            _leiaMediaViewer.VideoChangedResponses -= InitializeVideo;
        }

        #endregion
        #region Public Functions

        public void LoadVideo(VideoClip videoClip)
        {
            _leiaMediaViewer.SetVideoClip(videoClip);
            LoadVideoClip();
        }
        public void LoadVideo(VideoClip videoClip, int rows, int columns)
        {
            _leiaMediaViewer.SetVideoClip(videoClip, rows, columns);
            LoadVideoClip();
        }
        public void LoadVideo(string videoPathOrURL)
        {
            _leiaMediaViewer.SetVideoURL(videoPathOrURL);
            LoadVideoURL();
        }
        public void LoadVideo(string videoPathOrURL, int rows, int columns)
        {
            _leiaMediaViewer.SetVideoURL(videoPathOrURL, rows, columns);
            LoadVideoURL();
        }

        public void PlayVideo()
        {
            _video.Play();
            _audio.Play();
            if (OnClipPlay != null)
            {
                OnClipPlay();
            }
        }

        public void PauseVideo()
        {
            if (!_video.isPlaying) return;
            _video.Pause();
            _audio.Pause();
            if (OnClipPause != null)
            {
                OnClipPause();
            }
        }

        public void LoopVideo(bool loopValue)
        {
            _video.isLooping = loopValue;
        }

        public void ResetVideo()
        {
            PauseVideo();
            ScrubVideo(0f);
            PlayVideo();
        }

        public void ScrubVideo(float percent)
        {
            if (_video == null || !_video.isPrepared)
            {
                return;
            }
            percent = Mathf.Clamp(percent, 0, 1);
            float timeLocation = percent * Duration();
            _video.time = (double)timeLocation;
            _audio.time = timeLocation;
        }

        public void MuteVideo()
        {
            _audio.mute = true;
        }

        public void UnmuteVideo()
        {
            _audio.mute = false;
        }

        public void SetVolume(float percent)
        {
            _audio.volume = percent;
        }

        public bool IsPlaying()
        {
            return _video.isPlaying;
        }

        public bool IsPlayOnAwake()
        {
            return _video.playOnAwake;
        }

        public bool IsLooping()
        {
            return _video.isLooping;
        }

        public bool IsMuted()
        {
            return _audio.mute;
        }

        public float GetVolume()
        {
            return _audio.volume;
        }

        public float GetVideoProgress()
        {
            if (_video == null)
            {
                return 0;
            }
            return (float)_video.frame / (float)_video.frameCount;
        }
        public float GetVideoDuration()
        {
            return (float)_video.frameCount / _video.frameRate;
        }

        public long GetVideoFrame()
        {
            return _video.frame;
        }

        #endregion
        #region Private Functions

        private void LoadVideoClip()
        {
            video_source = VideoSourceType.Clip;
            InitializeVideo();
        }

        private void LoadVideoURL()
        {
            video_source = VideoSourceType.URL;
            _video.audioOutputMode = VideoAudioOutputMode.Direct;
            InitializeVideo();
        }

        IEnumerator EnsureVideoIsPreped()
        {
            _video.Stop();
            yield return new WaitForEndOfFrame();
            if (_video.playOnAwake)
            {
                ResetVideo();
            }
            _video.controlledAudioTrackCount = _video.audioTrackCount;
            if (OnClipInitialize != null)
            {
                OnClipInitialize();
            }
        }
        private ulong Duration()
        {
            if (_video == null || _video.frameRate < Mathf.Epsilon)
            {
                return 0;
            }
            return _video.frameCount / (ulong)_video.frameRate;
        }

        private void CheckClipAdjustment(bool initializeCheck)
        {
            if (_lastKnownClip == null)
            {
                if (_video.clip == null)
                {
                    //no clips loaded, do nothing
                }
                else if (_lastKnownClip != _video.clip)
                {
                    //first clip is being loaded
                    _lastKnownClip = _video.clip;
                    if (!initializeCheck)
                        InitializeVideo();
                }
                else
                {
                    //unknown
                    LogUtil.Log(LogLevel.Info, "Unkown clip situation between last clip : " + _lastKnownClip.name + " and current : " + _video.clip.name);
                }
            }
            else
            {
                if (_video.clip == null)
                {
                    //disconnected
                    _lastKnownClip = null;
                    _audio.Pause();
                    if (OnClipDisconnect != null)
                    {
                        OnClipDisconnect();
                    }
                }
                else if (_lastKnownClip != _video.clip)
                {
                    //video swap
                    _lastKnownClip = _video.clip;

                    if (OnClipSwap != null)
                    {
                        OnClipSwap();
                    }

                    if (!initializeCheck)
                        InitializeVideo();
                }
                else
                {
                    //the clip remains the same, do nothing
                }
            }
        }


        #endregion
        #region EventListeners

        private void InitializeVideo()
        {

            CheckClipAdjustment(true);
            if (_video.clip == null && video_source == VideoSourceType.Clip)
            {
                return;
            }
            _leiaMediaViewer.SetRendererActive(true);
            _video.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _video.EnableAudioTrack(0, true);
            _video.SetTargetAudioSource(0, _audio);
            _video.controlledAudioTrackCount = 1;
            LoopVideo(_video.isLooping);

            StartCoroutine(EnsureVideoIsPreped());
        }

        void EndReached(VideoPlayer vp)
        {
            if (IsLooping())
            {
                if (OnClipLoop != null)
                {
                    OnClipLoop();
                }
            }
            else
            {
                if (OnClipComplete != null)
                {
                    OnClipComplete();
                }
            }
        }

        #endregion
    }
}
