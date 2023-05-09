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
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace LeiaLoft
{
    public class AndroidLeiaDevice : AbstractLeiaDevice
    {
        private AndroidJavaClass _leiaBacklightClass;
        private AndroidJavaObject _leiaBacklightInstance;
        private bool _isLeiaDevice = false;

        const int supportedRatioCount = 6;

        // maps backlight mode to initial light intensities
        private readonly Dictionary<int, float[]> initialLightRatioCachePerMode = new Dictionary<int, float[]>();

        // maps backlight mode to most recently set light intensities
        private readonly Dictionary<int, float[]> lightRatioCachePerMode = new Dictionary<int, float[]>();

        public override int[] CalibrationOffset
        {
            get
            {
                InitializeBacklightModule();
                if (_leiaBacklightInstance == null)
                {
                    return base.CalibrationOffset;
                }

                int[] temp = _leiaBacklightInstance.Call<int[]> ("getXYCalibration");
                return new [] { temp[1], temp[2] };
            }
            set
            {
                this.Warning("Setting calibration from Unity Plugin is not supported anymore - use relevant app instead.");
            }
        }

        private void InitializeBacklightModule()
        {
            if ((_leiaBacklightClass == null ||
                _leiaBacklightInstance == null))
            {
                try
                {
                    if (_leiaBacklightClass == null)
                    {
                        _leiaBacklightClass = new AndroidJavaClass("android.leia.LeiaBacklight");
                        _isLeiaDevice = _leiaBacklightClass.CallStatic<bool>("isLeiaDevice");

                        if (_isLeiaDevice)
                        {
                            _leiaBacklightClass.CallStatic("start", "LeiaDisplay", false);
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (_isLeiaDevice)
                    {
                        _leiaBacklightInstance = _leiaBacklightClass.GetStatic<AndroidJavaObject>("instance");

                        // populate the backlight ratio cache. each supported backlight mode has an associated 2D/3D light ratio
                        for (int i = 0; i < supportedRatioCount; ++i)
                        {
                            // get the result of object.method
                            AndroidJavaObject resultObj = _leiaBacklightInstance.Call<AndroidJavaObject>("getRatio", i);

                            // get a C# pointer to the result. should be an array
                            System.IntPtr rawPtr = resultObj.GetRawObject();

                            // recruit ConvertFromJNIArray to move data from Java to C#
                            float[] result = AndroidJNIHelper.ConvertFromJNIArray<float[]>(rawPtr);

                            // for 2D and 3D light ratios, store in initial reference as well
                            initialLightRatioCachePerMode[i] = result;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    this.Error("Unable to get response from backlight service. Using default profile stub:" + _profileStubName);
                    this.Error(e.ToString());

                }

            }
        }

        public AndroidLeiaDevice(string stubName)
        {
            this.Debug("ctor");
            string displayType = stubName;

            if (!string.IsNullOrEmpty(displayType))
            {
                _profileStubName = displayType;
                this.Trace("displayType " + displayType);
            }
            else
            {
                this.Debug("No displayType received, using stub: " + stubName);
            }
            InitializeBacklightModule();
        }

        /// <summary>
        /// See LeiaDisplaySDK / androidsdk-display/src/main/java/com/leia/android/lights/LeiaLightsManagerV8.java#L8 for values.
        /// ... 2 = 2D, 3 = 3D, ... 5 = immersive
        /// </summary>
        /// <param name="modeId">The backlight mode to take on. In current version of the displaySDK, supports 2D, 3D, 5 (immersive)</param>
        public override void SetBacklightMode(int modeId)
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null)
            {
                this.Trace("SetBacklightMode" + modeId);
                _leiaBacklightInstance.Call ("setBacklightMode", modeId);

                // mode 5 is ImmersiveMode in Java DisplaySDK. this mode will never support backlight transition because it needs
                // to have 3D compensation disabled and re-setting light balance triggers 3D compensation
                const int transitionID = 5;
                if (lightRatioCachePerMode.ContainsKey(modeId) && modeId != transitionID)
                {
                    SetBacklightTransition(lightRatioCachePerMode[modeId][0], lightRatioCachePerMode[modeId][1], true);
                }
            }
        }

        public float[] GetLightRatio()
        {
            try
            {
                // get the result of object.method
                AndroidJavaObject resultObj = _leiaBacklightInstance.Call<AndroidJavaObject>("getRatio");

                // get a C# pointer to the result. should be an array
                System.IntPtr rawPtr = resultObj.GetRawObject();

                // recruit ConvertFromJNIArray to move data from Java to C#
                float[] result = AndroidJNIHelper.ConvertFromJNIArray<float[]>(rawPtr);

                return result;
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "When trying to retrieve {0} got error {1}", "getRatio", e);
                return default(float[]);
            }
        }

        /// <summary>
        /// Sets the current Lightfield mode (on / 2D OR off / 3D) to be equivalent to an intermediate mode between 2D and 3D. When the device is set to this Lightfield mode again, the last intermediate mode is restored.
        /// </summary>
        /// <param name="alpha">The 3D backlight intensity. 0 is very weak, 1 is fully active</param>
        public void SetDisplayLightBalance(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);

            // calculate an intensity for the 2D light that is on the spectrum from 0.95f to 0.0f
            float ratio2D = Mathf.Lerp(initialLightRatioCachePerMode[2][0], initialLightRatioCachePerMode[3][0], alpha);

            // calculate an intensity for the 3D light that is on the spectrum from 0.0f to 2.0f
            float ratio3D = Mathf.Lerp(initialLightRatioCachePerMode[2][1], initialLightRatioCachePerMode[3][1], alpha);

            SetBacklightTransition(ratio2D, ratio3D, true);
        }

        /// <summary>
        /// Calls display-sdk API to set backlight ratios.
        ///
        /// If user sets their backlight ratios without updating the light ratio cache, then calls SetBacklightMode(mode), the backlight ratios will use the last known light ratio values, which may be default values.
        ///
        /// Not ignoring the cache is useful if users want to transition from 3D to 2D, but on re-entering 3D, want to snap back to full 3D immediately.
        /// </summary>
        /// <param name="ratio2D">Intensity of 2D light</param>
        /// <param name="ratio3D">Intensity of 3D backlight</param>
        /// <param name="updateCache">Whether to update cache's record of this mode's latest ratios</param>
        public void SetBacklightTransition(float ratio2D, float ratio3D, bool updateCache)
        {
            int mode = GetBacklightMode();

            if (updateCache)
            {
                // update the light cache
                lightRatioCachePerMode[mode] = new[] { ratio2D, ratio3D };
            }

            // set 2D / standard display light's intensity to the value represented by $ratio2D,
            // set 3D / backlight intensity to the value represented by $ratio3D
            _leiaBacklightInstance.Call("setBacklightTransition", ratio2D, ratio3D);
        }

        /// <summary>
        /// Updates light balance and light balance cache to the SDK's known default light balance for current mode.
        /// </summary>
        public void ResetLightBalance()
        {
            int mode = GetBacklightMode();

            float light0 = initialLightRatioCachePerMode[mode][0];
            float light1 = initialLightRatioCachePerMode[mode][1];
            
            SetBacklightTransition(light0, light1, true);
        }

        /// <summary>
        /// See LeiaDisplaySDK / androidsdk-display/src/main/java/com/leia/android/lights/LeiaLightsManagerV8.java#L8 for values.
        /// ... 2 = 2D, 3 = 3D, ... 5 = immersive
        /// </summary>
        /// <param name="modeId">The backlight mode to take on. In current version of the displaySDK, supports 2D, 3D, 5 (immersive)</param>
        /// <param name="delay">Passes a setBacklightMode request with delay to displaySDK</param>
        public override void SetBacklightMode(int modeId, int delay)
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null)
            {
                this.Trace("SetBacklightMode" + modeId);
                _leiaBacklightInstance.Call ("setBacklightMode", modeId, delay);

                // mode 5 is ImmersiveMode in Java DisplaySDK. this mode will never support backlight transition because it needs
                // to have 3D compensation disabled and re-setting light balance triggers 3D compensation
                const int transitionID = 5;
                if (lightRatioCachePerMode.ContainsKey(modeId) && modeId != transitionID)
                {
                    SetBacklightTransition(lightRatioCachePerMode[modeId][0], lightRatioCachePerMode[modeId][1], true);
                }
            }
        }

        public override void RequestBacklightMode(int modeId)
        {
        }

        public override void RequestBacklightMode(int modeId, int delay)
        {
        }
        public override int GetBacklightMode()
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null)
            {
                int mode =_leiaBacklightInstance.Call<int>("getBacklightMode");
                return mode;
            }

            return 2;
        }

        public override DisplayConfig GetDisplayConfig()
        {

            if (_displayConfig != null)
            {
                return _displayConfig;
            }

            _displayConfig = new DisplayConfig();

            if (!_isLeiaDevice)
            {
                base.ApplyOfflineConfigValues();

                base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);
                Debug.Log("Non supported device");
                return _displayConfig;
            }

            try
            {
                AndroidJavaObject javaConfig = _leiaBacklightInstance.Call<AndroidJavaObject> ("getDisplayConfig");
                _displayConfig = new DisplayConfig();

                _displayConfig.Gamma = javaConfig.Call<float>("getGamma");
                _displayConfig.Beta = javaConfig.Call<float>("getBeta");
                
                // The following params
                // DotPitchInMm, PanelResolution, NumViews, AlignmentOffset, DisplaySizeInMm, and ViewResolution
                // are reported as
                // (value for device's portrait mode width, value for device's portrait mode height) rather than
                // (value for device's wide side, value for device's long side) as we would expect.
                // See DisplayConfig :: UserOrientationIsLandscape

                _displayConfig.DotPitchInMm = GetXyPairFromAndroid<float>(javaConfig, "getDotPitchInMm", "floatValue");
                _displayConfig.PanelResolution = GetXyPairFromAndroid<int>(javaConfig, "getPanelResolution", "intValue");
                _displayConfig.NumViews = GetXyPairFromAndroid<int>(javaConfig, "getNumViews", "intValue");
                _displayConfig.AlignmentOffset = GetXyPairFromAndroid<float>(javaConfig, "getAlignmentOffset", "floatValue");
                _displayConfig.DisplaySizeInMm = GetXyPairFromAndroid<int>(javaConfig, "getDisplaySizeInMm", "intValue");
                _displayConfig.ViewResolution = GetXyPairFromAndroid<int>(javaConfig, "getViewResolution", "intValue");

                _displayConfig.ActCoefficients = GetXyPairOfListsOfTFromAndroid<float>(javaConfig, "getViewSharpeningCoefficients", "floatValue");

                // since we now use slanted pathway to render all content, we no longer need to retrieve these flags
                _displayConfig.Slant = false;
                _displayConfig.isSlanted = true;
                _displayConfig.isSquare = false;

                _displayConfig.InterlacingMatrixLandscape = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixLandscape");
                _displayConfig.InterlacingMatrixLandscape180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixLandscape180");
                _displayConfig.InterlacingMatrixPortrait = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixPortrait");
                _displayConfig.InterlacingMatrixPortrait180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingMatrixPortrait180");

                _displayConfig.InterlacingVectorLandscape = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorLandscape");
                _displayConfig.InterlacingVectorLandscape180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorLandscape180");
                _displayConfig.InterlacingVectorPortrait = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorPortrait");
                _displayConfig.InterlacingVectorPortrait180 = GetArrayFromAndroid<float>(javaConfig, "getInterlacingVectorPortrait180");

                _displayConfig.SystemDisparityPercent = _leiaBacklightInstance.Call<float>("getSystemDisparityPercent");
                _displayConfig.SystemDisparityPixels = _leiaBacklightInstance.Call<float>("getSystemDisparityPixels");
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "While loading data from Android DisplayConfig from firmware, encountered error {0}", e);
            }

            // populate _displayConfig from FW with developer-tuned values
            base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);

            return _displayConfig;
        }

        public override bool IsConnected()
        {
            InitializeBacklightModule();
            if (_leiaBacklightInstance != null && _isLeiaDevice)
            {
                return _leiaBacklightInstance.Call<bool>("isConnected");
            }
            return false;
        }

        public override RuntimePlatform GetRuntimePlatform()
        {
            return RuntimePlatform.Android;
        }

        /// <summary>
        /// Android devices may have screen or height greater at any moment.
        ///
        /// Due to an issue on Lumepad where Screen.Orientation can be Portrait or Landscape in wrong cases,
        /// have to use Screen.width/height to determine if device is landscape or not.
        /// </summary>
        /// <returns>True if device screen is wider than it is tall</returns>
        public override bool IsScreenOrientationLandscape()
        {
            return Screen.width > Screen.height;
        }

        /// <summary>
        /// Helper function for moving XyPair<List<T>> from Java into C#"
        /// </summary>
        /// <typeparam name="T">Type parameter of XyPair<List<T>></typeparam>
        /// <param name="javaConfig">A reference to the Java-side DisplayConfig in the LeiaLights DisplaySDK</param>
        /// <param name="methodName">A method on the Java-side DisplayConfig to call to get an XyPair<List<T>></param>
        /// <param name="javaTypeName">The name of the method of the Java XyPair to call to convert a T into a primitive type which can be moved from Java to C#</param>
        /// <returns>an XyPair of Lists from Java</returns>
        private static XyPair<List<T>> GetXyPairOfListsOfTFromAndroid<T>(AndroidJavaObject javaConfig, string methodName, string javaTypeName)
        {
            const string sizeMethodName = "size";
            const string getMethodName = "get";
            AndroidJavaObject javaPairObj = javaConfig.Call<AndroidJavaObject>(methodName);
            // assume that this returned a java-side XyPair<List<T>> as an AndroidJavaObject

            List<List<T>> items = new List<List<T>>();

            // iterate through x and y List child objects of the XyPair
            foreach (string xy in new [] { "x", "y"})
            {
                items.Add(new List<T>());
                // iterate through elements in the XyPair.x or XyPair.y
                for (int i = 0; i < javaPairObj.Get<AndroidJavaObject>(xy).Call<int>(sizeMethodName); ++i)
                {
                    // call List.Get(i), convert into a C# object
                    T element = javaPairObj.Get<AndroidJavaObject>(xy).Call<AndroidJavaObject>(getMethodName, i).Call<T>(javaTypeName);
                    items[items.Count - 1].Add(element);
                }
            }

            XyPair<List<T>> pair = new XyPair<List<T>>(items[0], items[1]);
            return pair;
        }

        /// <summary>
        /// Helper function for moving XyPair from Java to C#
        /// </summary>
        /// <typeparam name="T">Type parameter of XyPair</typeparam>
        /// <param name="javaConfig">A reference to the Java-side DisplayConfig in the LeiaLights DisplaySDK</param>
        /// <param name="methodName">A method on the Java-side DisplayConfig to call to get an XyPairOfT</param>
        /// <param name="javaTypeName">The name of the method of the Java XyPair to call to convert a T into a primitive type which can be moved from Java to C#</param>
        /// <returns>An XyPair from Java</returns>
        private static XyPair<T> GetXyPairFromAndroid<T>(AndroidJavaObject javaConfig, string methodName, string javaTypeName)
        {
            AndroidJavaObject javaPairObj = javaConfig.Call<AndroidJavaObject>(methodName);
            T item1 = javaPairObj.Get<AndroidJavaObject>("x").Call<T>(javaTypeName);
            T item2 = javaPairObj.Get<AndroidJavaObject>("y").Call<T>(javaTypeName);
            XyPair<T> pair = new XyPair<T>(item1, item2);

            return pair;
        }

        /// <summary>
        /// Helper function for moving array from Java to C#
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="javaObj">A Java object which supports javaObj.methodName()</param>
        /// <param name="methodName">A method to call</param>
        /// <returns>Returns the T[] on the Java object into C#</returns>
        private static T[] GetArrayFromAndroid<T>(AndroidJavaObject javaObj, string methodName)
        {
            try
            {
                // get the result of object.method
                AndroidJavaObject resultObj = javaObj.Call<AndroidJavaObject>(methodName);

                // get a C# pointer to the result. should be an array
                System.IntPtr rawPtr = resultObj.GetRawObject();

                // recruit ConvertFromJNIArray to move data from Java to C#
                T[] result = AndroidJNIHelper.ConvertFromJNIArray<T[]>(rawPtr);

                return result;
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "When trying to retrieve {0} got error {1}", methodName, e);
                return default(T[]);
            }
        }
    }
}
