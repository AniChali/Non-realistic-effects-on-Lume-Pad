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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeiaLoft
{
    /// <summary>
    /// Presents single display object, has state and settings (decorators) that determine rendering mode.
    /// </summary>
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/unity-sdk-components#leiadisplay-component")]
    [ExecuteInEditMode]
    public partial class LeiaDisplay : Singleton<LeiaDisplay>, IFaceTrackingDisplay
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private static string VersionFileName { get { return "VERSION"; } }

        private LeiaSettings _settings = null;
        [SerializeField] private bool _isDirty = false;
        private bool detachCallbackForSceneChange;
        // Serialize m_LightfieldMode directly on LeiaDisplay. Future-compatible
        [SerializeField] private LightfieldMode m_DesiredLightfieldMode = LightfieldMode.On;
        [SerializeField] private LightfieldMode m_ActualLightfieldMode = LightfieldMode.On;
        private LightfieldMode m_PreviousLightfieldMode = LightfieldMode.On;

        private ILeiaState _leiaState;
        private LeiaStateFactory _stateFactory = new LeiaStateFactory();
        private LeiaDeviceFactory _deviceFactory = new LeiaDeviceFactory();
        private ILeiaDevice _leiaDevice;

        private const int CalibratingOffsetMin = -16;
        private const int CalibratingOffsetMax = 16;

#if UNITY_ANDROID && !UNITY_EDITOR
        private float _disparityBackup;
        private float _disparityAnimTime = 0;
        private float _disparityAnimDirection = 0;
        private const float BASELINE_ANIM_PEAK_TIME = 0.5f;
#endif

        /// <summary>
        /// This enum defines the LeiaDevice's backlight on/off state,
        /// which determines whether parallax pixel content gives depth cues to the viewer.
        /// </summary>
        public enum LightfieldMode : int
        {
            Off = 0,
            On = 1

            // do not assume that only values will ever be 0 and 1
        };

        /// <summary>
        /// This enum defines the LeiaDevice's render technique,
        /// which determines rendering order of Leia Views.
        /// </summary>
        public enum RenderTechnique
        {
            Default = 0,
            Stereo = 1
        };

        public static string HPO { get { return "HPO"; } }
        public static string TWO_D { get { return "2D"; } }
        public static string THREE_D { get { return "3D"; } }

        /// <summary>
        /// Occurs when LeiaDisplay has leiaState or decorators changed.
        /// </summary>
        public event System.Action StateChanged = delegate { };

        /// <summary>
        /// Occurs when LeiaDisplay has leiaState or decorators changed.
        /// Accepts parameters LightfieldMode previousState, LightfieldMode currentState
        // Used to subscribe to specific backlight mode changes instead of explicitly checking LeiaDisplay values.
        /// </summary>
        public event System.Action<LeiaDisplay.LightfieldMode, LeiaDisplay.LightfieldMode> BacklightStateChanged = delegate { };

        /// <summary>
        /// Gets current leia device.
        /// </summary>
        public ILeiaDevice LeiaDevice
        {
            get
            {
                return _leiaDevice;
            }
        }

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                // in edit mode when not playing, do not modify _isDirty. When user sets RenderMode from LeiaDisplayEditor :: ShowRenderMode, do not set _isDirty flag
                // setter can only make IsDirty true. In Update(), after state regen _isDirty is assigned false
                _isDirty = Application.isPlaying && (value || _isDirty);
            }
        }

        /// <summary>
        /// Gets current leiaState factory.
        /// </summary>
        public LeiaStateFactory StateFactory
        {
            get
            {
                return _stateFactory;
            }
        }

        /// <summary>
        /// Gets current leiaDevice factory
        /// </summary>
        public LeiaDeviceFactory DeviceFactory
        {
            get
            {
                return _deviceFactory;
            }
        }

        /// <summary>
        /// Gets settings object (where all LeiaDisplay settings aggregated)
        /// </summary>
        public LeiaSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = FindObjectOfType<LeiaSettings>();

                    if (_settings == null)
                    {
                        _settings = new GameObject(LeiaSettings.GameObjectName).AddComponent<LeiaSettings>();
                    }

                    _settings.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }

                return _settings;
            }
        }

        /// <summary>
        /// Allows user to enable/disable Parallax Auto Rotation animation on LeiaDisplay at runtime.
        ///
        /// To set the LeiaDisplay gameObject's initial value on scene load, use the LeiaDisplay gameObject's
        /// Inspector to set the component's Parallax Auto Rotation.
        ///
        /// True: parallax rotation animations can be processed at runtime on device.
        /// False: new parallax rotation animations will not be processed. (Active animation will still complete.)
        /// </summary>
        [System.Obsolete("Parallax auto-rotation not supported in recent Unity SDKs. This feature will be removed in a future Unity SDK release")]
        public bool ParallaxAutoRotationAnimation
        {
            get
            {
                return Decorators.ParallaxAutoRotation;
            }
            set
            {
                LeiaStateDecorators decos = Decorators;
                decos.ParallaxAutoRotation = value;
                Settings.Decorators = decos;
            }
        }

        /// <summary>
        /// Gives access to AntiaAliasing setting, sets dirty flag when changed
        /// </summary>
        [System.Obsolete("Deprecated in LeiaLoft Unity SDK 0.6.20. Will be removed in 0.6.23. LeiaDisplay.AntiAliasing does not control LeiaView AA anymore. See IntermediateRenderTextureProperties.json")]
        public int AntiAliasing
        {
            get
            {
                return Settings.AntiAliasing;
            }
            set
            {
                // this method intentionally left blank. Deprecated
                ;
            }
        }

        /// <summary>
        /// Gives access to ProfileStubName setting, creates new factroy (based on new profile), sets dirty flag when changed
        /// </summary>
        public string ProfileStubName
        {
            get
            {
                return Settings.ProfileStubName;
            }
            set
            {
                Settings.ProfileStubName = value;
                _isDirty = true;

                if (Application.isPlaying)
                {
                    _leiaDevice.SetProfileStubName(value);
                    _stateFactory.SetDisplayConfig(GetDisplayConfig());
                }
            }
        }

        /// <summary>
        /// A getter method for checking if the LeiaDisplay desires the LightfieldMode to be on
        /// </summary>
        /// <returns>True if internal state machine thinks LeiaDisplay wants ActualLightfieldMode == LightfieldMode.On</returns>
        public bool IsLightfieldModeActualOn()
        {
            return m_ActualLightfieldMode == LightfieldMode.On;
        }

        /// <summary>
        /// This property provides a getter/setter for changing the LeiaDisplay's actual pixels and backlight from unlit traditional (Off) to lit parallax (On).
        ///
        /// Setting this property without modifying DesiredLightfieldMode causes the display's status to change, but does not save the user's desired value.
        ///
        /// This property should be expected to replace LeiaStateID.
        /// </summary>
        public LightfieldMode ActualLightfieldMode
        {
            get
            {
                return m_ActualLightfieldMode;
            }
            set
            {
                // this setter replaces the functionality of LeiaStateId
                if (value == LightfieldMode.Off)
                {
                    Settings.LeiaStateId = TWO_D;
                    m_ActualLightfieldMode = LightfieldMode.Off;
                }
                else if (value == LightfieldMode.On)
                {
                    Settings.LeiaStateId = HPO;
                    m_ActualLightfieldMode = LightfieldMode.On;
                }
                RequestLeiaStateUpdate();
            }
        }

        /// <summary>
        /// Get the ActualLightfieldMode as if it were an int. 0 = off, 1 = on
        /// </summary>
        public int ActualLightfieldValue
        {
            get
            {
                return (int)m_ActualLightfieldMode;
            }
            set
            {
                ActualLightfieldMode = (LightfieldMode)value;
            }
        }

        /// <summary>
        /// Gets or sets the actual LeiaState of the display. Leia devices can be forced into 2D mode by
        /// backlight shutoff (thermal), or
        /// Android onscreen text.
        /// </summary>
        [Obsolete("Deprecated in 0.6.18. Use ActualLightfieldMode instead. Scheduled for removal in 0.6.20.")]
        public string LeiaStateId
        {
            get
            {
                return Settings.LeiaStateId;
            }
            set
            {
                // LeiaStateID is now merely an adapter for LeiaDisplayActualLightfieldMode
                if (value == TWO_D)
                    ActualLightfieldMode = LightfieldMode.Off;
                else if (value == THREE_D || value == HPO)
                    ActualLightfieldMode = LightfieldMode.On;
            }
        }

        /// <summary>
        /// A getter method for checking if the LeiaDisplay desires the LightfieldMode to be on
        /// </summary>
        /// <returns>True if internal state machine thinks LeiaDisplay wants DesiredLightfieldMode == LightfieldMode.On</returns>
        public bool IsLightfieldModeDesiredOn()
        {
            return m_DesiredLightfieldMode == LightfieldMode.On;
        }

        /// <summary>
        /// This property provides a getter/setter for changing the LeiaDisplay's onscreen pixels and backlight to 2D / 3D.
        ///
        /// This property should be expected to replace DesiredLeiaStateID.
        /// </summary>
        public LightfieldMode DesiredLightfieldMode
        {
            get
            {
                return m_DesiredLightfieldMode;
            }
            set
            {
                // this setter replaces the functionality of DesiredLeiaStateID
                m_PreviousLightfieldMode = m_DesiredLightfieldMode;
                m_DesiredLightfieldMode = value;
                if (value == LightfieldMode.Off)
                {
                    Settings.DesiredLeiaStateID = TWO_D;
                }
                else if (value == LightfieldMode.On)
                {
                    Settings.DesiredLeiaStateID = HPO;
                }
                else
                {
                    LogUtil.Log(LogLevel.Error, "Unsupported LightfieldMode {0} passed to DesiredDisplayHoloMode", value);
                }

                ActualLightfieldMode = value;
            }
        }

        /// <summary>
        /// Get the DesiredLightFieldMode as if it were an int. 0 = off, 1 = on
        /// </summary>
        public int DesiredLightfieldValue
        {
            get
            {
                return (int)DesiredLightfieldMode;
            }
            set
            {
                DesiredLightfieldMode = (LightfieldMode)value;
            }
        }

        /// <summary>
        /// Specifies a desired LeiaState - "2D", or "HPO". Switches both screen content and device backlight status (if device has a backlight)
        /// </summary>
        [Obsolete("Deprecated in 0.6.18. Use DesiredLightfieldMode instead. Scheduled for removal in 0.6.20.")]
        public string DesiredLeiaStateID
        {
            get
            {
                return Settings.DesiredLeiaStateID;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.Warning("Provide a value when setting DesiredLeiaStateID");
                    return;
                }

                // map lowercase to uppercase
                // map {"3d", "3D"} to "HPO"
                value = value.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                if (value.Equals(THREE_D))
                {
                    value = HPO;
                }

                // DesiredLeiaStateID is now merely an adapter for DesiredLightfieldMode
                if (value == TWO_D)
                    DesiredLightfieldMode = LightfieldMode.Off;
                else if (value == HPO)
                    DesiredLightfieldMode = LightfieldMode.On;
            }
        }
        public RenderTechnique DesiredRenderTechnique
        {
            get { return Decorators.RenderTechnique; }
            set
            {
                LeiaStateDecorators decos = Decorators;
                decos.RenderTechnique = value;
                Decorators = decos;
                RequestLeiaStateUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the decorators, when changed - recreates LeiaStateFactory and applies leiaState to views
        /// </summary>
        /// <value>The decorators.</value>
        public LeiaStateDecorators Decorators
        {
            get
            {
                return Settings.Decorators;
            }
            set
            {
                if (Settings.Decorators.Equals(value)) { return; }
                Settings.Decorators = value;
#if UNITY_EDITOR
                // when a property on LeiaDisplay.LeiaSettings.Decorators is updated, force the UI for LeiaDisplay to update
                UnityEditor.EditorUtility.SetDirty(this);
#endif

                if (Application.isPlaying)
                {
                    _stateFactory.SetDisplayConfig(GetDisplayConfig());
                    UpdateLeiaState();
                }
            }
        }

        private void LogVersionAndGeneralInfo()
        {
            var version = Resources.Load<TextAsset>(VersionFileName);

            if (version == null)
            {
                return;
            }

            string logData = string.Format(
                "LeiaLoft Unity SDK Version: {0}\nUnity version: {1}\nCurrent platform: {2}\nIs editor? {3}\n",
                version.text, Application.unityVersion, Application.platform, Application.isEditor);

            // log using LogUtil with Debug priority
            this.Debug(logData);

#if !UNITY_EDITOR
            // in builds which have Debug.unityLogger.logEnabled and didn't strip out log commands, also log through Unity's built-in logger
            Debug.Log(logData);
#endif
        }

        public int[] CalibrationOffset
        {
            get
            {
                return _leiaDevice.CalibrationOffset;
            }
            set
            {
                if (Application.isPlaying)
                {
                    value[1] = Mathf.Clamp(value[1], CalibratingOffsetMin, CalibratingOffsetMax);
                    value[0] = Mathf.Clamp(value[0], CalibratingOffsetMin, CalibratingOffsetMax);
                    _leiaDevice.GetDisplayConfig().AlignmentOffset = new XyPair<float>(value[0], value[1]);
                    _isDirty = true;
                }
            }
        }

        private void OnResume()
        {
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() == 3)
            {
                // on resume, if the LeiaLights DisplaySDK state machine thinks this app was in 3D mode on last frame
                // then _getBacklightMode should be 3. Store in the latest3DFrame flag.
                // In BacklightModeChanged we will discard BacklightModeChanged(2D) callbacks which occur recently after
                // OnResume {GetBacklightMode == 3}
                latest3DFrame = Time.frameCount;
            }

            // on return to app, if desired state is HPO and forced state is not HPO,
            // we are allowed to return to HPO
            if (DesiredLeiaStateID.Equals(HPO) && !LeiaStateId.Equals(HPO))
            {
                DesiredLeiaStateID = HPO;
            }
            else
            {
                _isDirty = true;
            }
        }

        private void OnPause()
        {
            // catch a case with DisplaySDK 7.1 where Hydrogen apps which were minimized using moveTaskToBack would not disengage backlight
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() != 2)
            {
                _leiaDevice.SetBacklightMode(2);
            }
#endif
        }

        /// <summary>
        /// Called when a standalone platform changes focus ("active window") to this application
        /// </summary>
        /// <param name="focus">True if focusing on app, false if dropping focus on app</param>
        private void OnApplicationFocus(bool focus)
        {
            // check flag, only set if not already set; save double work in case where OnApplicationFocus and OnApplicationPause both trigger
            if (focus && !_isDirty)
            {
                OnResume();
            }
            else
            {
                OnPause();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                OnResume();
            }
            else
            {
                OnPause();
            }
        }

        protected override void OnEnableSingleton()
        {
            LogVersionAndGeneralInfo();
            this.Debug("OnEnable");
            AbstractLeiaDevice device;
#if LEIALOFT_CONFIG_OVERRIDE
            device = new OverrideLeiaDevice(OverrideLeiaDevice.DefaultOverrideConfigFilename);
#elif UNITY_ANDROID && !UNITY_EDITOR
            device = new AndroidLeiaDevice(ProfileStubName);
#else
            device = new OfflineEmulationLeiaDevice(ProfileStubName);
#endif
            _deviceFactory.RegisterLeiaDevice(device);
            UpdateDevice();
            SceneManager.activeSceneChanged += onSceneChange;
        }

        /// <summary>
        /// Gets new device from deviceFactory (providing profile stub name in case if device not available).
        /// Gets profile from new device, sends it to leiaStateFactory.
        /// Gets default LeiaStateId if LeiaStateId is empty.
        /// Applies lState.
        /// </summary>
        public void UpdateDevice()
        {
            this.Debug("UpdateDevice");
            _leiaDevice = _deviceFactory.GetDevice(ProfileStubName);
            _stateFactory.SetDisplayConfig(GetDisplayConfig());

            RequestLeiaStateUpdate();
        }

        private void OnDisable()
        {
            this.Debug("OnDisable");
            if (_leiaState != null)
            {
                _leiaState.Release();
            }

            // set flag for OnSceneChange callback to be detached after OnSceneChange is run
            detachCallbackForSceneChange = true;
        }

        /// <summary>
        /// On App quit, sets backlight off.
        /// Handles case where this scene is being deloaded, and no new scene is being loaded.
        /// </summary>
        private void OnApplicationQuit()
        {
            _leiaDevice.SetBacklightMode(2);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// If device is rotated, animates disparity scaling to 0, sets proper parallax orientation
        /// and then animates disparity scaling back
        /// </summary>
        private void ProcessParallaxRotation()
        {
            if (Decorators.ParallaxAutoRotation &&
                (Decorators.ShouldParallaxBePortrait !=
                 (Input.deviceOrientation == DeviceOrientation.Portrait ||
                  Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)) &&
                _disparityAnimDirection == 0)
            {
                _disparityAnimDirection = 1;
                _disparityBackup = LeiaCamera.Instance.BaselineScaling;
                _disparityAnimTime = Time.realtimeSinceStartup;
            }

            float timeDifference = Time.realtimeSinceStartup - _disparityAnimTime + 0.001f;

            if (_disparityAnimDirection == 1)
            {
                if (timeDifference < BASELINE_ANIM_PEAK_TIME)
                {
                    LeiaCamera.Instance.BaselineScaling = _disparityBackup * (1.0f - timeDifference/BASELINE_ANIM_PEAK_TIME);
                }
                else
                {
                    _disparityAnimTime = Time.realtimeSinceStartup;
                    _disparityAnimDirection = -1;
                    var tmpDecorator = Decorators;
                    tmpDecorator.ShouldParallaxBePortrait =
                    (Input.deviceOrientation == DeviceOrientation.Portrait ||
                    Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown);
                    Decorators = tmpDecorator;
                    _isDirty = true;
                }
            }
            else if (_disparityAnimDirection == -1)
            {
                if (timeDifference < BASELINE_ANIM_PEAK_TIME)
                {
                    LeiaCamera.Instance.BaselineScaling = _disparityBackup * (timeDifference/BASELINE_ANIM_PEAK_TIME);
                }
                else
                {
                    LeiaCamera.Instance.BaselineScaling = _disparityBackup;
                    _disparityAnimDirection = 0;
                }
            }
        }
#endif

        /// <summary>
        /// Render in Play mode, check isDirty flag and (re)Set leiaState
        /// </summary>
        private void Update()
        {
            if (_leiaDevice != null && _leiaDevice.HasDeviceOrientationChangedSinceLastQuery())
            {
                _isDirty = true;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            ProcessParallaxRotation();
#else
#endif

            if (_isDirty)
            {
                _isDirty = false;
                DisplayConfig displayConfig = GetDisplayConfig();
                // inside GetDisplayConfig, calculate UserOrientationIsLandscape
                _stateFactory.SetDisplayConfig(displayConfig);
                RequestLeiaStateUpdate();
            }
        }

        /// <summary>
        /// Use leiaState to render final picture
        /// </summary>
        public void RenderImage(LeiaCamera camera)
        {
            if (enabled)
            {
                _leiaState.DrawImage(camera, Decorators);
            }
        }

        /// <summary>
        /// When in 2D, forces HPO then back to 2D
        /// When in HPO, forces 2D then back to HPO
        ///
        /// Skips backlight switching.
        ///
        /// This method resolves a specific issue with orthographic / perspective cameras. External users should use
        /// DesiredLeiaStateId = ...
        /// </summary>
        public void ForceLeiaStateUpdate()
        {
            string init = LeiaStateId;
            string conj = (LeiaStateId == HPO ? TWO_D : HPO);


            Settings.LeiaStateId = conj;
            _leiaState = _stateFactory.GetState(LeiaStateId);
            UpdateLeiaState();

            Settings.LeiaStateId = init;
            _leiaState = _stateFactory.GetState(LeiaStateId);
            UpdateLeiaState();
        }

        /// <summary>
        /// Requests new state from current LeiaStateFactory, switches backlight,
        /// updates texture. UpdateLeiaState triggers StateChanged() actions.
        /// </summary>
        private void RequestLeiaStateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (_leiaState != null)
            {
                _leiaState.Release();
            }

            // set interlacing state based on LeiaStateId
            _leiaState = _stateFactory.GetState(LeiaStateId);

            // Set backlight state based on LeiaStateId
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() == 3 && !LeiaStateId.Equals(HPO))
            {
                _leiaDevice.SetBacklightMode(_leiaState.GetBacklightMode());
            }
            if (_leiaDevice != null && _leiaDevice.GetBacklightMode() != 3 && LeiaStateId.Equals(HPO))
            {
                _leiaDevice.SetBacklightMode(_leiaState.GetBacklightMode());
            }

            if (_leiaDevice.GetBacklightMode() == 3)
            {
                latest3DFrame = Time.frameCount;
            }

            // private member RequestLeiaStateUpdate() calls public member UpdateLeiaState
            // UpdateLeiaState is called by LeiaConfigSettingsUI
            UpdateLeiaState();
        }

        public void UpdateLeiaState()
        {
            _leiaState.UpdateState(Decorators, LeiaDevice);

            // if not dirty, and state matches backlight, then we are ready to trigger StateChanged callback
            bool matchedState = !_isDirty &&
                ((LeiaStateId == HPO && _leiaState.GetBacklightMode() == 3) ||
                (LeiaStateId != HPO && _leiaState.GetBacklightMode() != 3));

            if (matchedState && StateChanged != null)
            {
                StateChanged();
                BacklightStateChanged(m_PreviousLightfieldMode, ActualLightfieldMode);
            }
        }

        /// <summary>
        /// This method is called by the LeiaLights Display SDK when the device's backlight engages or disengages. Do not call this method.
        ///
        /// Examples of reasons for LeiaLights DisplaySDK to call this method:
        ///     thermal callback caused backlight to turn off
        ///     app reopened, and LeiaLights DisplaySDK set backlight back to last known state that it thought the app was in
        ///     user dragged down Android status bar on their device
        /// </summary>
        /// <param name="mode">2D if firmware sets backlight off, 3D if firmware sets backlight on</param>
        private int latest3DFrame = -1;
        private void BacklightModeChanged(string mode)
        {
            if (string.IsNullOrEmpty(mode) || !(mode == THREE_D || mode == TWO_D))
            {
                LogUtil.Log(LogLevel.Warning, "Symbol {0} is not a valid backlight state", mode);
                return;
            }
            mode = mode.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

            if (mode == THREE_D && DesiredLeiaStateID != HPO)
            {
                DesiredLeiaStateID = HPO;
            }
            else if (mode == TWO_D && LeiaStateId == HPO && latest3DFrame <= Time.frameCount - 1)
            {
                // suppress if we recently set to 3D.

                // if not recently set to 3D, then we can switch content to 2D to match device's unlit state
                LeiaStateId = TWO_D;
            }
        }

        /// <summary>
        /// Newer versions of LeiaLights SDK / DisplaySDK AAR 7.x call
        /// LeiaDisplay :: onBacklightModeChanged instead of
        /// LeiaDisplay :: BacklightModeChanged. These calls can be forwarded to BacklightModeChanged
        /// </summary>
        /// <param name="mode">2D if firmware has turned backlight off, 3D if firmware has turned backlight on</param>
        void onBacklightModeChanged(string mode)
        {
            BacklightModeChanged(mode);
        }

        public DisplayConfig GetDisplayConfig()
        {
            DisplayConfig displayConfig;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                displayConfig = _deviceFactory.GetDevice(ProfileStubName).GetDisplayConfig();
                displayConfig.UserOrientationIsLandscape = _leiaDevice == null ? true : _leiaDevice.IsScreenOrientationLandscape();
                return displayConfig;
            }
#endif
            displayConfig = _leiaDevice.GetDisplayConfig();
            displayConfig.UserOrientationIsLandscape = _leiaDevice == null ? true : _leiaDevice.IsScreenOrientationLandscape();
            return displayConfig;
        }

        public bool IsConnected()
        {
            return LeiaDevice.IsConnected();
        }

        /// <summary>
        /// Updates the views by applying leiaState UpdateViews method and sets renderTextures from texturePool.
        /// </summary>
        public void UpdateViews(LeiaCamera camera)
        {
            if (enabled)
            {
                this.Debug("UpdateViews");
                _leiaState.UpdateViews(camera);
            }
        }

        private void onSceneChange(Scene scene, Scene scene2)
        {
            bool containsLeia;

            GameObject[] gameObjects = scene2.GetRootGameObjects();
            LeiaDisplay leiaDisp = null;
            for (int i = 0; i < gameObjects.Length; i++)
            {
                leiaDisp = gameObjects[i].GetComponentInChildren<LeiaDisplay>();
                if (leiaDisp != null) { break; }
            }
            containsLeia = (leiaDisp != null);

            if (!containsLeia)
            {
                LeiaDevice.SetBacklightMode(2, 1);
            }
            if (detachCallbackForSceneChange)
            {
                SceneManager.activeSceneChanged -= onSceneChange;
            }

            if (detachCallbackForSceneChange)
            {
                SceneManager.activeSceneChanged -= onSceneChange;
            }

        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
