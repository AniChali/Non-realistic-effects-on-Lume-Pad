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
using System.Collections.Generic;

namespace LeiaLoft
{
    /// <summary>
    /// Default LeiaDevice that makes no real connection to any device
    /// </summary>
    public class OfflineEmulationLeiaDevice : AbstractLeiaDevice
    {
        // most of these string tokens should be moved to LeiaPreferenceUtil
        // and LeiaPreferenceUtil should be accessible in builds
        private const string emulatedDisplayConfigFilename = "emulatedDisplayConfigFilename";
        private const string emulatedDisplayConfigDefaultFilename = "DisplayConfiguration_Hydrogen.json";
        public static readonly string updateGameViewResOnDisplayProfileChange = "updateGameViewResOnDisplayProfileChange";

        public static string EmulatedDisplayConfigFilename
        {
            get
            {
#if UNITY_EDITOR
                string buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
                return LeiaPreferenceUtil.GetUserPreference(emulatedDisplayConfigDefaultFilename, emulatedDisplayConfigFilename,
                    Application.dataPath, buildTarget);
#else
                return "";
#endif
            }
            set
            {
                // Allow users to set display profile per {project} & {build target}.
                // Do not vary the display profile per scene.

#if UNITY_EDITOR
                string buildTargetKey = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
                LeiaPreferenceUtil.SetUserPreference(emulatedDisplayConfigFilename, value,
                    Application.dataPath, buildTargetKey);
#if UNITY_2017_1_OR_NEWER
                if (LeiaPreferenceUtil.GetUserPreferenceBool(true, updateGameViewResOnDisplayProfileChange, new string[] { Application.dataPath }))
                {
                    JsonParamCollection keys;
                    if (StringAssetUtil.TryGetJsonObjectFromDeviceAwareFilename(value, out keys))
                    {
                        int[] res = (int[])keys["PanelResolution"];

                        // set new game view resolution
                        GameViewUtils.SetGameViewSize(res[0], res[1]);
                    }
                }
#endif

#else
                LogUtil.Log(LogLevel.Error, "Do not set OfflineEmulationLeiaDevice :: EmulatedDisplayConfigFilename in builds. Arg was {0}", value);
#endif
            }
        }

        public OfflineEmulationLeiaDevice(string stubName)
        {
            this.Debug("ctor()");
            _profileStubName = stubName;
        }

		public override void SetBacklightMode(int modeId)
		{
			// this method was left blank intentionally
		}

		public override void SetBacklightMode(int modeId, int delay)
		{
			// this method was left blank intentionally
		}

		public override void RequestBacklightMode(int modeId)
		{
			// this method was left blank intentionally
		}

		public override void RequestBacklightMode(int modeId, int delay)
		{
			// this method was left blank intentionally
		}

		public override int GetBacklightMode()
		{
			return 3;
		}


		public override DisplayConfig GetDisplayConfig()
        {
            if (_displayConfig != null)
            {
                return _displayConfig;
            }

            // call DisplayConfig's constructor. default is a DisplayConfig with square = true, slanted = false
            _displayConfig = new DisplayConfig();

            // in Unity editor, read in displayConfig data from JsonParamCollection
            // this _displayConfig will have its isSquare + isSlanted flags set in this step.
            // consider setting in-editor game view resolution as well at this step
            // also consider setting build target to platform which the profile is built for
#if UNITY_EDITOR
            base.ApplyDisplayConfigUpdate(EmulatedDisplayConfigFilename);
#elif UNITY_ANDROID
            //Need to set the display config in offline if non-leia device otherwise view count initialized to 0 until views updated
            AndroidJavaClass leiaBacklightClass = new AndroidJavaClass("android.leia.LeiaBacklight");
            bool isLeiaDevice = leiaBacklightClass.CallStatic<bool>("isLeiaDevice");
            if (!isLeiaDevice)
            {
				base.ApplyOfflineConfigValues();
            }
#endif

            // then overpopulate _displayConfig from json with developer-tuned values
            base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);

            return _displayConfig;
		}

        public override RuntimePlatform GetRuntimePlatform()
        {
            this.Debug("Offline platform - emulating Android");
            return RuntimePlatform.Android;
            // throw new System.NotImplementedException();
        }
    }
}