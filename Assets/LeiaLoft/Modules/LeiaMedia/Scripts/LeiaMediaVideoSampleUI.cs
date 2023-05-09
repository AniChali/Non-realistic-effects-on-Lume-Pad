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

using System;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft
{
    /// <summary>
    /// This class demonstrates uses of LeiaMediaVideoPlayer, and allows separation of UI from implemetation for modularity. We encourage developers to use this sample UI as a starting point.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class LeiaMediaVideoSampleUI : MonoBehaviour
    {
#pragma warning disable 0649 // Suppress warning that var is never assigned to and will always be null

        [SerializeField] private LeiaMediaVideoPlayer _leiaMediaVideo;
        [SerializeField] private Slider _volumeSlider, _videoProgressSlider;
        [SerializeField] private Button _playButton, _pauseButton;
        [SerializeField] private Text _currentTimeText, _durationText;
        [SerializeField] private GameObject _volumeControlObject;
        [SerializeField] private Button _soundOnButton, _soundOffButton;
        [SerializeField] private Sprite[] _volumeSprites;
        [SerializeField] private float _idleTime = 3f;

        private Vector3 _lastInputPosition = Vector3.zero;
        private float _internalTimer = 0f;
        private Animator _animator;
        private long _scrubFrame = -1;
        private const string STATE_UI_DISABLED = "disabled_state";
        private const string STATE_UI_ENABLED = "enabled_state";
        private const string enableTrigger = "enable_trigger";
        private const string disableTrigger = "disable_trigger";

#pragma warning restore 0649

        #region Unity Functions

        private void OnEnable()
        {
            if (!_leiaMediaVideo)
            {
                return;
            }
            _leiaMediaVideo.OnClipInitialize += ClipInitialized;
            _leiaMediaVideo.OnClipSwap += ClipSwapped;
            _leiaMediaVideo.OnClipDisconnect += ClipDisconnected;
        }
        private void OnDisable()
        {
            if (!_leiaMediaVideo)
            {
                return;
            }
            _leiaMediaVideo.OnClipInitialize -= ClipInitialized;
            _leiaMediaVideo.OnClipSwap -= ClipSwapped;
            _leiaMediaVideo.OnClipDisconnect -= ClipDisconnected;

        }
        void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        void Start()
        {
            Debug.AssertFormat(_leiaMediaVideo != null, "{0}._leiaMediaVideo property is assigned", this.GetType());

            _volumeSlider.value = _leiaMediaVideo.GetVolume();
            SetVolume();
        }
        void Update()
        {
            if (_leiaMediaVideo == null)
            {
                return;
            }
            UpdateProgressVisual();
            UpdateUIDisplayState();
        }

        #endregion
        #region Public Functions

        public void PlayVideo()
        {
            _leiaMediaVideo.PlayVideo();
            ToggleButtons(_pauseButton, _playButton, true);
        }
        public void PauseVideo()
        {
            _leiaMediaVideo.PauseVideo();
            ToggleButtons(_playButton, _pauseButton, true);
        }
        public void Reset()
        {
            _leiaMediaVideo.ResetVideo();
        }
        public void ToggleVolumeControl()
        {
            _volumeControlObject.SetActive(!_volumeControlObject.activeSelf);
        }
        public void Mute()
        {
            _leiaMediaVideo.MuteVideo();
        }
        public void Unmute()
        {
            _leiaMediaVideo.UnmuteVideo();
        }
        public void SetVolume()
        {
            _leiaMediaVideo.SetVolume(_volumeSlider.value);
            float volume = _volumeSlider.value;
            float divisor = 1f / (_volumeSprites.Length - 1);
            int volumeSpriteIndex = Mathf.CeilToInt(volume / divisor);
            _soundOnButton.image.sprite = _volumeSprites[volumeSpriteIndex];
        }
        public void ToggleVolume(bool toggle)
        {
            _soundOnButton.gameObject.SetActive(toggle);
            _soundOffButton.image.sprite = _soundOnButton.image.sprite;
            _soundOffButton.gameObject.SetActive(!toggle);
            _volumeControlObject.SetActive(toggle);
        }

        public void UpdateProgressVisual()
        {
            //Current frame does not update immediately once video resumes. Wait for the frame to update before resuming slider.
            if (!_leiaMediaVideo.IsPlaying() || (_leiaMediaVideo.GetVideoFrame() == _scrubFrame)) { return; }
            _scrubFrame = -1;

            float videoProgress = _leiaMediaVideo.GetVideoProgress();
            _videoProgressSlider.value = videoProgress;

            float durationSeconds = _leiaMediaVideo.GetVideoDuration();
            if (durationSeconds > 0.0f)
            {
                TimeSpan durationTimespan = TimeSpan.FromSeconds(durationSeconds);
                TimeSpan progressTimespan = TimeSpan.FromSeconds(durationSeconds * videoProgress);
                _durationText.text = string.Format("{0}:{1:D2}", durationTimespan.Minutes, durationTimespan.Seconds);
                _currentTimeText.text = string.Format("{0}:{1:D2}", progressTimespan.Minutes, progressTimespan.Seconds);
            }
            else
            {
                _durationText.text = "0:00";
                _currentTimeText.text = "0:00";
            }
        }
        public void ScrubVideo()
        {
            _scrubFrame = _leiaMediaVideo.GetVideoFrame();
            _leiaMediaVideo.ScrubVideo(_videoProgressSlider.value);
        }

        public void RewindVideo(float seconds)
        {
            float rewindPercent = Mathf.Clamp01(seconds * (1.0f / _leiaMediaVideo.GetVideoDuration()));
            _videoProgressSlider.value -= rewindPercent;
            _leiaMediaVideo.ScrubVideo(_videoProgressSlider.value);
        }
        public void FastForwardVideo(float seconds)
        {
            float forwardPercent = Mathf.Clamp01(seconds * (1.0f / _leiaMediaVideo.GetVideoDuration()));
            _videoProgressSlider.value += forwardPercent;
            _leiaMediaVideo.ScrubVideo(_videoProgressSlider.value);
        }
        public void HideVideoUI()
        {
            _volumeControlObject.SetActive(false);
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_UI_ENABLED))
            {
                _animator.SetTrigger(disableTrigger);
            }
            _internalTimer = _idleTime;
        }
        public void ShowVideoUI()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_UI_DISABLED))
            {
                _animator.SetTrigger(enableTrigger);
            }
            _internalTimer = 0.0f;
        }
        public void ToggleVideoUI()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_UI_DISABLED))
            {
                ShowVideoUI();
            }
            else
            {
                HideVideoUI();
            }
        }
        #endregion
        #region Private Functions

        private void UpdateUIDisplayState()
        {
            // mouse has moved this frame or show if the volume control is showing
            if (!IsInputStationary() || Input.GetMouseButton(0))
            {
                ShowVideoUI();
            }
            // hide ui
            else
            {
                _internalTimer += Time.deltaTime;
                if (_internalTimer >= _idleTime)
                {
                    HideVideoUI();
                }
                _internalTimer = Mathf.Min(_idleTime, _internalTimer);
            }
        }

        private bool IsInputStationary()
        {
            float distanceTreshhold = Screen.width / (Screen.dpi); //~4 landscape, ~2 portrait
            if (Vector3.Distance(Input.mousePosition, _lastInputPosition) < distanceTreshhold)
            {
                _lastInputPosition = Input.mousePosition;
                return true;
            }

            _lastInputPosition = Input.mousePosition;
            return false;
        }

        static void ToggleButtons(Button option1, Button option2, bool toggleCase)
        {
            option1.gameObject.SetActive(toggleCase);
            option2.gameObject.SetActive(!toggleCase);
        }

        void InitializeVideoUI()
        {
            ToggleButtons(_pauseButton, _playButton, _leiaMediaVideo.IsPlayOnAwake());
            _volumeSlider.value = _leiaMediaVideo.GetVolume();
            SetVolume();
        }

        void ResetVideoUI()
        {
            ToggleButtons(_playButton, _pauseButton, true);

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(STATE_UI_ENABLED))
            {
                _animator.SetTrigger(disableTrigger);
                _internalTimer = _idleTime;
            }
        }

        #endregion
        #region Event Listeners

        void ClipInitialized()
        {
            InitializeVideoUI();
        }
        void ClipSwapped()
        {
            InitializeVideoUI();
        }
        void ClipDisconnected()
        {
            ResetVideoUI();
        }

        #endregion
    }
}
