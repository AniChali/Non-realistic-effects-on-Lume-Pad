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
using System.Collections.Generic;
namespace LeiaLoft
{
    /// <summary>
    /// Basic abstract implementation of ILeiaDevice with profile loading methods implemented
    /// and calibration saved inside unity editor/app(for builds) preferences.
    /// </summary>
    public abstract class AbstractLeiaDevice : ILeiaDevice
    {
        public string GetProfileStubName()
        {
			return _profileStubName;
        }

        public static string PrefOffsetX { get { return "LeiaLoft_UserOffsetX"; } }
        public static string PrefOffsetY { get { return "LeiaLoft_UserOffsetY"; } }

		private bool _hasProfile;
		private float _systemScalingPercent;
		protected string _profileStubName;
		protected string _cachedProfileName;
		protected DisplayConfig _displayConfig;

		protected AbstractLeiaDevice()
		{
			if (CalibrationOffset == null)
			{
				CalibrationOffset = new int[2];
			}
		}

		public void SetProfileStubName(string name)
		{
			_profileStubName = name;
		}

		public abstract void SetBacklightMode(int modeId);

		public abstract void SetBacklightMode(int modeId, int delay);

		public abstract void RequestBacklightMode(int modeId);

		public abstract void RequestBacklightMode(int modeId, int delay);

		public abstract int GetBacklightMode();

		/// <summary>
        /// Starting from a _displayConfig with base data already populated by firmware / json profile, applies sparse update parameters.
        /// 
        /// Selects between DisplayConfigUpdateSquare.json and DisplayConfigUpdateSlanted.json based on flags already on config.
        /// </summary>
        /// <param name="sparseUpdates">A collection of string-data pairs which provide sparse update information</param>
        /// <param name="accessLevel">Permission level which the update is applied with</param>
		protected virtual void ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level accessLevel)
        {
			string stateUpdateFilename = string.Format("DisplayConfigUpdate{0}.json", _displayConfig.isSlanted ? "Slanted" : "Square");
			JsonParamCollection sparseUpdates;
			if (StringAssetUtil.TryGetJsonObjectFromDeviceAwareFilename(stateUpdateFilename, out sparseUpdates))
            {
				foreach (KeyValuePair<string, Array> pair in sparseUpdates)
				{
					_displayConfig.SetPropertyByReflection(pair.Key, pair.Value, accessLevel);
				}
			}
        }

		/// <summary>
		/// Starting from a new _displayConfig from constructor but with no settings from json yet,
		///
		/// reads in data from the given filename as a JsonParamCollection and applies sparsely defined properties in
		/// the JsonParamCollection to the _displayConfig.
		/// </summary>
		/// <param name="deviceSimulationFilePath">A file name in Application.dataPath/Assets/LeiaLoft/Resources/</param>
		protected virtual void ApplyDisplayConfigUpdate(string deviceSimulationFilePath)
		{
			JsonParamCollection sparseUpdates;
			if (StringAssetUtil.TryGetJsonObjectFromDeviceAwareFilename(deviceSimulationFilePath, out sparseUpdates))
			{
				foreach (KeyValuePair<string, Array> pair in sparseUpdates)
				{
					_displayConfig.SetPropertyByReflection(pair.Key, pair.Value, DisplayConfigModifyPermission.Level.DeviceSimulation);
				}
			}
			else
            {
				LogUtil.Log(LogLevel.Error, "Could not load simulated device profile {0}. Please re-set the emulated device profile on LeiaDisplay!", deviceSimulationFilePath);
            }
		}

		public virtual DisplayConfig GetDisplayConfig(bool forceReload)
		{
			if (forceReload)
            {
				_displayConfig = null;
            }

			// calls most specific type's GetDisplayConfig
			return GetDisplayConfig();
		}

		public virtual DisplayConfig GetDisplayConfig()
		{
			// This DisplayConfig contains params that used to need to be non-null when DisplayConfig :: set_UserOrientationIsLandscape was called.
            // This stub DC is acquired
            //		in Unity Editor at runtime,
			// 		in limited cases at edit time
            // in builds at start time; after a *LeiaDeviceBehaviour runs Start / RegisterDevice, this stub DC will be overwritten with data
            // from firmware.

			// since DisplayConfig now has a constructor, this code is ready to be transitioned to abstract
			_displayConfig = new DisplayConfig
			{
				isSquare = true,
				ResolutionScale = 1,
				AlignmentOffset = new XyPair<float>(0, 0),
				DisplaySizeInMm = new XyPair<int>(0, 0),
				DotPitchInMm = new XyPair<float>(0, 0),
				NumViews = new XyPair<int>(4, 4),
				PanelResolution = new XyPair<int>(1440, 2560),
				SystemDisparityPercent = 0.0125f,
				SystemDisparityPixels = 8f,
				ViewResolution = new XyPair<int>(360, 640),
				ActCoefficients = new XyPair<List<float>>(new List<float> { .06f, .025f }, new List<float> { .04f, .02f })
			};

			return _displayConfig;
		}

        public virtual DisplayConfig ApplyOfflineConfigValues()
        {
            if (_displayConfig == null)
            {
                _displayConfig = new DisplayConfig();
			}

            _displayConfig.PanelResolution = new XyPair<int>(Screen.width, Screen.height);
            _displayConfig.NumViews = new XyPair<int>(1, 1);

            return _displayConfig;
        }

        public virtual int GetDisplayWidth()
        {
            return 0;
        }

		public virtual int GetDisplayHeight()
		{
			return 0;
		}

        public virtual int GetDisplayViewcount()
        {
            return 4;
        }

        public abstract RuntimePlatform GetRuntimePlatform();

		public virtual string GetSensors()
		{
			return null;
		}

		public virtual int[] CalibrationOffset
		{
			get
			{
				return new [] {
					_displayConfig == null ? 0 : (int)_displayConfig.AlignmentOffset.x,
					_displayConfig == null ? 0 : (int)_displayConfig.AlignmentOffset.y
					};
			}
			set
			{
				//Deprecated: should use DisplayConfig.AlignmentOffset 
#if UNITY_EDITOR
				UnityEditor.EditorPrefs.SetInt(PrefOffsetX, value[0]);
				UnityEditor.EditorPrefs.SetInt(PrefOffsetY, value[1]);
#else
				UnityEngine.PlayerPrefs.SetInt(PrefOffsetX, value[0]);
				UnityEngine.PlayerPrefs.SetInt(PrefOffsetY, value[1]);
#endif
			}
		}

		public virtual bool IsSensorsAvailable()
		{
			return false;
		}

		public virtual void CalibrateSensors()
		{
		}

		public virtual bool IsConnected()
		{
			return false;
		}

		/// <summary>
        /// DisplayConfig needs to know what orientation the LeiaDevice is in.
        ///
        /// Some LeiaDevices may rotate screen between portrait/landscape, some may not.
        ///
        /// OfflineEmulationLeiaDevice needs to be able to set its screen orientation to simulate portrait mode.
        /// </summary>
        /// <returns>True if not overridden by a more specific type</returns>
		public virtual bool IsScreenOrientationLandscape()
        {
			// in the editor, Screen.orientation/Screen.width/Screen.height are nonsense values. Use GameViewUtil instead
#if UNITY_EDITOR
			return GameViewUtils.GetGameViewAspectRatio() > 1.0f;
#else
			return true;
#endif
		}

		private ScreenOrientation mPrevOrientation;
		private float mPrevAspectRatio;
		// collection of runtime device orientations which may trigger a state recalculation
		private readonly HashSet<ScreenOrientation> responsiveScreenOrientations = new HashSet<ScreenOrientation>(
			new[] { ScreenOrientation.LandscapeLeft, ScreenOrientation.LandscapeRight, ScreenOrientation.Portrait, ScreenOrientation.PortraitUpsideDown});

		/// <summary>
		/// Implement orientation-state-tracking in AbstractLeiaDevice. Every frame, we must check whether the aspect ratio and/or device orientation changed.
		///
		/// On some LitByLeia devices, DeviceOrientation.LandscapeLeft and DeviceOrientation.LandscapeRight may have the same aspect ratio but may require different config information.
		/// </summary>
		/// <returns>true if device orientation or aspect ratio has changed since the last time the method was called</returns>
		public virtual bool HasDeviceOrientationChangedSinceLastQuery()
        {
			float aspectRatio = 
#if UNITY_EDITOR
			// In UnityEditor, Screen width/height are nonsense and vary per Unity version
			GameViewUtils.GetGameViewAspectRatio();
#else
			Screen.width * 1.0f / Mathf.Max(1.0f, Screen.height);
#endif

			ScreenOrientation orientation =
#if UNITY_ANDROID && !UNITY_EDITOR
			Screen.orientation;
#else
			// In UnityEditor, Screen.orientation always reports portrait... not useful
			aspectRatio > 1.0f ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
#endif

			// when aspect ratio changes due to landscape/portrait change, or when screen changes orientation into a notable orientation
			// return true. typically this will be queried in LeiaDisplay :: Update and will trigger an IsDirty event
			if (!Mathf.Approximately(aspectRatio, mPrevAspectRatio) ||
				(responsiveScreenOrientations.Contains(orientation) && orientation != mPrevOrientation))
            {
				mPrevAspectRatio = aspectRatio;
				mPrevOrientation = orientation;
				return true;
			}
			return false;
        }

		/// <summary>
		/// Returns last screen orientation that was in tracked screen orientations (responsiveScreenOrientations).
        ///
        /// This allows us to distinguish whether the LitByLeia device was last in RGB orientation or BGR orientation.
		/// </summary>
		/// <returns>Most recent device orienation - LandscapeLeft/LandscapeRight/Portrait/PortraitUpsideDown</returns>
		public virtual ScreenOrientation GetScreenOrientationRGB()
        {
			// return latest known orientation. Unity SDK should run HasDeviceOrientationChangedSinceLastQuery every frame
			// so that mPrevOrientation is kept up-to-date
			return mPrevOrientation;
        }
    }
}
