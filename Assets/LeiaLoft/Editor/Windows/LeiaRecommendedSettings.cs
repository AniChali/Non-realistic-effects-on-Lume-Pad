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
using UnityEditor;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LeiaLoft
{

    public class LeiaRecommendedSettings : EditorWindow
    {
        static bool forceShow = true;
        delegate bool FailTest();
        /// <summary>
        /// Container for Unity Setting that may need adjustment based on Leia SDK requirements
        /// </summary>
        class Recommendation
        {
            /// <summary>
            /// Recommendation that requires manual action from the user
            /// </summary>
            /// <param name="title">GUI label title</param>
            /// <param name="unitySetting">Unity Setting state that may require adjustment</param>
            /// <param name="unitySettingPass">Unity Setting's passing state</param>
            /// <param name="failTest">Lamda expression. Check Unity Setting Pass state, return true when failure occurs </param>
            /// <param name="toolTip">Recommendation Tooltip</param>
            /// <param name="ignoreKey">Editor Prefs ignore key</param>
            public Recommendation(string title, object unitySetting, object unitySettingPass, FailTest failTest, string toolTip, string ignoreKey)
            {
                Title = title;
                UnitySetting = unitySetting;
                UnitySettingPass = unitySettingPass;
                ToolTip = toolTip;
                FailTest = failTest;
                IgnoreKey = ignoreKey;
            }
            /// <summary>
            /// Recommendation with auto-fix button option
            /// </summary>
            /// <param name="title">GUI label title</param>
            /// <param name="unitySetting">Unity Setting state that may require adjustment</param>
            /// <param name="unitySettingPass">Unity Setting's passing state</param>
            /// <param name="actionToPass">Lamda expression. Auto-fix action to bring Unity Setting to Pass State</param>
            /// <param name="failTest">Lamda expression. Check Unity Setting Pass state, return true when failure occurs </param>
            /// <param name="toolTip">Recommendation Tooltip</param>
            /// <param name="ignoreKey">Editor Prefs ignore key</param>
            /// <summary>
            public Recommendation(string title, object unitySetting, object unitySettingPass, UnityAction actionToPass, FailTest failTest, string toolTip, string ignoreKey)
            {
                Title = title;
                UnitySetting = unitySetting;
                UnitySettingPass = unitySettingPass;
                ActionToPass = actionToPass;
                ToolTip = toolTip;
                FailTest = failTest;
                IgnoreKey = ignoreKey;
            }
            public string Title { set; get; }
            public object UnitySetting { set; get; }
            public object UnitySettingPass { set; get; }
            public UnityAction ActionToPass { set; get; }
            public string ToolTip { set; get; }
            public FailTest FailTest;
            public string IgnoreKey { set; get; }
            public bool IsIgnored { set; get; }

            public void CheckRecommendation()
            {
                if (FailTest() && !EditorPrefs.HasKey(IgnoreKey))
                {
                    EditorWindowUtils.HorizontalLine();
                    EditorWindowUtils.BeginHorizontal();
                    EditorWindowUtils.Label(Title, ToolTip, true);
                    EditorWindowUtils.FlexibleSpace();
                    EditorWindowUtils.Label("(?)  ", ToolTip, false);
                    EditorWindowUtils.EndHorizontal();
                    EditorWindowUtils.Label(string.Format(currentValue, UnitySetting), ToolTip, false);
                    EditorWindowUtils.BeginHorizontal();
                    if (ActionToPass != null) //button solution
                    {
                        EditorWindowUtils.Button(ActionToPass, string.Format(useRecommended, UnitySettingPass));
                    }
                    else //manual solution
                    {
                        EditorWindowUtils.Label(string.Format(changeToRecommended, UnitySettingPass), ToolTip, false);
                    }
                    EditorWindowUtils.FlexibleSpace();
                    EditorWindowUtils.Button(() => { EditorPrefs.SetBool(IgnoreKey, true); IsIgnored = true; }, "Ignore");
                    EditorWindowUtils.EndHorizontal();
                }
                EditorWindowUtils.Space(5);
            }
        }
        static List<Recommendation> recommendations;
        /// <summary>
        /// Container for Game View Resolution / Aspect Ratio recommendation
        /// </summary>
        class DeviceGameViewResolution
        {
            /// <summary>
            /// Game View Resolution / Aspect Ratio recommendation
            /// </summary>
            /// <param name="res">Resolution</param>
            /// <param name="isRotatable">Does device auto-rotate?</param>
            public DeviceGameViewResolution(string name, int[] res, bool isRotatable)
            {
                this.Name = name;
                this.Res = res;
                this.IsRotatable = isRotatable;
            }
            public string Name { get; set; }
            public int[] Res { get; set; }
            public bool IsRotatable { get; set; }
        }

        /// <summary>
        /// For each device we suport on a platform
        /// </summary>
        class PlatformDeviceResolutions
        {
            /// <summary>
            /// Add a list of device resolutions suppoted on the current platform
            /// </summary>
            /// <param name="gameViewResolutions"> Platform supported device game view resolutions</param>
            public PlatformDeviceResolutions(List<DeviceGameViewResolution> gameViewResolutions)
            {
                GameViewResolutions = gameViewResolutions;
            }
            public List<DeviceGameViewResolution> GameViewResolutions { get; set; }
            public string DisplayGameViewResolutions()
            {
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < GameViewResolutions.Count; i++)
                {
                    s.AppendLine();
                    s.Append(GameViewResolutions[i].IsRotatable ?
                        string.Format("[{0}, {1}] or [{1}, {0}] for {2}", GameViewResolutions[i].Res[0], GameViewResolutions[i].Res[1], GameViewResolutions[i].Name) :
                        string.Format("[{0}, {1}] for {2}", GameViewResolutions[i].Res[0], GameViewResolutions[i].Res[1], GameViewResolutions[i].Name));
                }
                return s.ToString();
            }
            public bool FailMatchGameView()
            {
                GetMainGameViewSize();
                //If any resolutions match for a target platform, pass Fail Check
                for (int i = 0; i < GameViewResolutions.Count; i++)
                {
                   if ((gameViewResolution.x == GameViewResolutions[i].Res[0] && gameViewResolution.y == GameViewResolutions[i].Res[1]) ||
                   (GameViewResolutions[i].IsRotatable &&
                   (gameViewResolution.y == GameViewResolutions[i].Res[0] && gameViewResolution.x == GameViewResolutions[i].Res[1])))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        static PlatformDeviceResolutions platformResolutions;
        static Vector2 gameViewResolution;
        const string useRecommended = "Use recommended: {0}";
        const string changeToRecommended = "Change to: {0}";
        const string currentValue = "Current: {0}";
        const string editor_Recommendation_ForcePopUp = "LeiaLoft.Recommendation.ForcePopUp";
        const string editor_PrevIssueCount = "LeiaLoft.PreviousIssueCount";
        static LeiaRecommendedSettings window;
        const string BannerAssetFilename = "LeiaLoftSDK";
        private static Texture2D _bannerImage;
        private static Vector2 scrollPosition; 
        static int ignoreCount;
        static int issueCount;
        static int prevIssueCount;

        [MenuItem("LeiaLoft/Recommended Unity Settings &r")]
        public static void Init()
        {
            _bannerImage = Resources.Load<Texture2D>(BannerAssetFilename);
            window = GetWindow<LeiaRecommendedSettings>(true, "LeiaLoft SDK Recommended Settings");
            window.Show();
            window.minSize = EditorWindowUtils.WindowMinSize;
            InitRecommendations();
            UpdateIssuesIgnores();
        }

        static LeiaRecommendedSettings()
        {
            EditorApplication.update += Update;
        }

        public static void ForceRecommendationCompliance()
        {
            InitRecommendations();
            for (int i = 0; i < recommendations.Count; i++)
            {
                if(recommendations[i].ActionToPass != null)
                {
                    recommendations[i].ActionToPass.Invoke();
                }
            }
        }

        static void Update()
        {
            UpdateIssuesIgnores();
            if(ShouldForceWindowPopUp())
            {
                Init();
            }
        }
  
        static void UpdateIssuesIgnores()
        {
            ignoreCount = issueCount = 0;
            if (recommendations == null)
            {
                InitRecommendations();
            }
            for (int i = 0; i < recommendations.Count; i++)
            {
                if (recommendations[i].FailTest() && !recommendations[i].IsIgnored)
                {
                    issueCount++;
                }
                if (recommendations[i].IsIgnored)
                {
                    ignoreCount++;
                }
            }
        }
        static bool ShouldForceWindowPopUp()
        {
            if (!forceShow)
            {
                return false;
            }
            //Using editor prefs to store window variable otherwise reset when entering play mode
            prevIssueCount = EditorPrefs.GetInt(editor_PrevIssueCount, 0);
            if (issueCount != prevIssueCount)
            {
                int delta = issueCount - prevIssueCount;
                EditorPrefs.SetInt(editor_PrevIssueCount, issueCount);
                if (window == null && delta > 0)
                {
                    return true;
                }
            }
            return false;

        }
        public void OnGUI()
        {
            if(window == null)
            {
                Init();
            }
            EditorWindowUtils.TitleTexture(_bannerImage);
            
            if (issueCount == 0)
            {
                EditorWindowUtils.Space(2);
                var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                EditorWindowUtils.Label("Fantastic! You're good to go!", style);
            }
            else
            {
                EditorWindowUtils.HelpBox("Recommended Unity Editor settings for LeiaLoft SDK:", MessageType.Warning);
            }

            scrollPosition = EditorWindowUtils.BeginScrollView(scrollPosition);
            EditorWindowUtils.Space(5);

            if (recommendations != null)
            {
                for (int i = 0; i < recommendations.Count; i++)
                {
                    recommendations[i].CheckRecommendation();
                }
            }
            EditorWindowUtils.EndScrollView();
            EditorWindowUtils.BeginHorizontal();

            UndoableInputFieldUtils.BoolFieldWithTooltip(() => { forceShow = EditorPrefs.GetBool(editor_Recommendation_ForcePopUp, false); return forceShow; }, b => { forceShow = b; EditorPrefs.SetBool(editor_Recommendation_ForcePopUp, b); }, "  Automatically Pop-up", "Display this window when LeiaLoft detects unrecommended Unity Settings. Alternatively, this widow can be opened from LeiaLoft-> Recommended Unity Settings", window);

            if (ignoreCount > 0)
            {
                EditorWindowUtils.Button(() =>
                {
                    for (int i = 0; i < recommendations.Count; i++)
                    {
                        if (EditorPrefs.HasKey(recommendations[i].IgnoreKey))
                        {
                            EditorPrefs.DeleteKey(recommendations[i].IgnoreKey);
                            recommendations[i].IsIgnored = false;
                        }
                    }
                }, string.Format("Reset Ignores ({0})", ignoreCount));
            }
            EditorWindowUtils.EndHorizontal();
            EditorWindowUtils.Space(2);
        }

        static void InitRecommendations()
        {
            if (recommendations == null)
            {
                recommendations = new List<Recommendation>();
            }
            recommendations.Clear();
            recommendations.Add(new Recommendation(
                "Build Target",
                EditorUserBuildSettings.activeBuildTarget,
                "Supported Platforms: Android, Windows / OSX Standalone",
                () =>
                {
                    bool buildAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
                    bool buildWin = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                        EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64);
#if UNITY_2017_3_OR_NEWER
                bool buildOSX = EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX;
#else
                    bool buildOSX = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel ||
                        EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel64 ||
                        EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXUniversal);
#endif
                    return !(buildAndroid || buildOSX || buildWin);
                },
                "Supported Platforms: Android, Windows / OSX Standalone",
                "LeiaLoft.Ignore.BuildTarget"));

            recommendations.Add(new Recommendation(
                "Min Android SDK",
                PlayerSettings.Android.minSdkVersion,
                string.Format("{0} + ", AndroidSdkVersions.AndroidApiLevel23),
                () => PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23,
                () =>
                {
                    return (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android &&
                PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel23);
                },
                "LeiaLoft SDK relies on Android Library calls that are only available on or after API Level 23",
                "LeiaLoft.Ignore.AndroidMinSDK"));

            recommendations.Add(new Recommendation(
                "Anisotropic Textures",
                 QualitySettings.anisotropicFiltering,
                 "Per Texture",
                 () => QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable,
                 () =>
                 {
                     return QualitySettings.anisotropicFiltering != AnisotropicFiltering.Enable;
                 },
                 "Having Ansiotropic set to Forced On causes visual artifacts under certain scenarios.",
                 "LeiaLoft.Ignore.AnsiotropicFiltering"));

            //LeiaMedia Recorder hits a known Unity bug when recording specific file formats on specific Unity Editor versions. More information on this can be found here: https://issuetracker.unity3d.com/issues/crash-on-mediaencoder-custom-internal-addframe-injected-when-pushing-a-frame-into-mediaencoder-that-has-been-disposed

            string unityVersion = Application.unityVersion;

            string regexMajorPattern = @"\d+(?=\.)";
            string regexMinorPattern = @"(?<=\.)\d+(?=\.)";
            string regexPatchPattern = @"(?<=.)\d+(?=[a-z])";

            int currMajor = int.Parse(Regex.Match(unityVersion, regexMajorPattern).Value);
            int currMinor = int.Parse(Regex.Match(unityVersion, regexMinorPattern).Value);
            int currPatch = int.Parse(Regex.Match(unityVersion, regexPatchPattern).Value);

            int minReqMajor = 2019;
            int minReqMinor = 4;
            int minReqPatch = 36;

            if (currMajor == 2022)
            {
                minReqMajor = 2022;
                minReqMinor = 1;
                minReqPatch = 0;
            }
            else if (currMajor == 2021)
            {
                minReqMajor = 2021;
                minReqMinor = 2;
                minReqPatch = 6;
            }
            else if (currMajor == 2020)
            {
                minReqMajor = 2020;
                minReqMinor = 3;
                minReqPatch = 27;
            }
            recommendations.Add(new Recommendation(
                "Leia Media Recorder : Unsupported Unity Editor",
                Application.unityVersion,
                string.Format("{0}.{1}.{2}+.\nLeia Media Recorder may not function correctly in your Unity Editor.", minReqMajor, minReqMinor, minReqPatch),
                () =>
                {
                    return
                    currMajor < minReqMajor ||
                    (currMajor == minReqMajor && currMinor < minReqMinor) ||
                    (currMajor == minReqMajor && currMinor == minReqMinor && currPatch < minReqPatch);
                },
                string.Format("Leia Media Recorder : Upgrade Editor to {0}.{1}.{2}+", minReqMajor, minReqMinor, minReqPatch),
                "LeiaLoft.Ignore.LeiaMediaRecorderUnityEditorVersion"));

#if UNITY_EDITOR_OSX
#if !UNITY_2018_1_OR_NEWER
            recommendations.Add(new Recommendation(
                "LeiaDepthFocus : Mac Editor Version Pre-2018",
                Application.unityVersion,
                "2019 LTS+\n\nLeiaDepthFocus may not function correctly in Unity editor versions before 2018 when\n running in editor on Mac. Android builds are not affected and will still function correctly.\n\nFix:\n- Upgrade to Unity 2019 LTS+",
                () =>
                {
                    return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "AutoFocusMethods";
                },
                "Auto Focus : Upgrade Editor to 2019 LTS+",
                "LeiaLoft.Ignore.AutoFocusMacUnity2017"));
            #endif
            #endif
#if UNITY_ANDROID
#if !UNITY_2019_3_OR_NEWER
#if UNITY_EDITOR_WIN
            recommendations.Add(new Recommendation(
            "LeiaDepthFocus : Windows Editor Versions Pre-2019",
                Application.unityVersion,
                "2019 LTS+\n\nLeiaDepthFocus may not function correctly in Unity editor versions before 2019 when\nthe build target is set to Android. Android builds are not affected and will still function correctly.\n\nFix:\n- Upgrade to Unity 2019 LTS+ \n\nWorkaround:\n - Set build target (File>Build Settings>Platform) to \"PC, Mac, & Linux Standalone\" while testing in Editor,\nswitch back to Android for build.",
                () =>
                {
                    return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "AutoFocusMethods";
                },
                "Auto Focus : Upgrade Editor to 2019 LTS+",
                "LeiaLoft.Ignore.AutoFocusDepthMapBuildTarget"));
#endif
#endif
#if UNITY_2020_1_OR_NEWER
            recommendations.Add(new Recommendation(
                "Gradle Minification Release",
                PlayerSettings.Android.minifyRelease,
                 false,
                 () => PlayerSettings.Android.minifyRelease = false,
                 () => { return PlayerSettings.Android.minifyRelease; },
                 "Android Minification will cause backlight failure on Android device. See Player Settings -> Android -> Minify.",
                 "LeiaLoft.Ignore.AndroidMiniRelease2020Plus"));

            recommendations.Add(new Recommendation(
                "Gradle Minification Debug",
                PlayerSettings.Android.minifyDebug,
                 false,
                 () => PlayerSettings.Android.minifyDebug = false,
                 () => { return PlayerSettings.Android.minifyDebug; },
                 "Android Minification will cause backlight failure on Android device. See Player Settings -> Android -> Minify.",
                 "LeiaLoft.Ignore.AndroidMiniDebug2020Plus"));

            recommendations.Add(new Recommendation(
                "Gradle Minification R8",
                PlayerSettings.Android.minifyWithR8,
                 false,
                 () => PlayerSettings.Android.minifyWithR8 = false,
                 () => { return PlayerSettings.Android.minifyWithR8; },
                 "Android Minification will cause backlight failure on Android device. See Player Settings -> Android -> Minify.",
                 "LeiaLoft.Ignore.AndroidMiniR82020Plus"));

#elif UNITY_2017_1_OR_NEWER
            recommendations.Add(new Recommendation(
                "Gradle Minification Release",
                EditorUserBuildSettings.androidReleaseMinification,
                 AndroidMinification.None,
                 () => EditorUserBuildSettings.androidReleaseMinification = AndroidMinification.None,
                 () =>
                 {
                     return
#if UNITY_2019_1_OR_NEWER
            EditorUserBuildSettings.androidReleaseMinification != AndroidMinification.None;
#else
                 EditorUserBuildSettings.androidBuildSystem == AndroidBuildSystem.Gradle &&
                     EditorUserBuildSettings.androidReleaseMinification != AndroidMinification.None;
#endif
                 },
                 "Android Minification will cause backlight failure on Android device. See Player Settings -> Publishing Settings -> Minify.",
                 "LeiaLoft.Ignore.AndroidMiniRelease2017Plus"));

            recommendations.Add(new Recommendation(
                "Gradle Minification Debug",
                EditorUserBuildSettings.androidDebugMinification,
                AndroidMinification.None,
                () => EditorUserBuildSettings.androidDebugMinification = AndroidMinification.None,
                () =>
                {
                    return
#if UNITY_2019_1_OR_NEWER
            EditorUserBuildSettings.androidDebugMinification != AndroidMinification.None;
#else
                (EditorUserBuildSettings.androidBuildSystem == AndroidBuildSystem.Gradle &&
                    EditorUserBuildSettings.androidDebugMinification != AndroidMinification.None);
#endif
                },
                "Android Minification will cause backlight failure on Android device. See Player Settings -> Publishing Settings -> Minify.",
                "LeiaLoft.Ignore.AndroidMiniDebug2017Plus"));
#endif
#endif
#if UNITY_ANDROID
#if UNITY_2018_3_OR_NEWER
            recommendations.Add(new Recommendation(
            "Stripping Level",
            PlayerSettings.GetManagedStrippingLevel((BuildTargetGroup)EditorUserBuildSettings.activeBuildTarget),
            ManagedStrippingLevel.Disabled,
            () => PlayerSettings.SetManagedStrippingLevel((BuildTargetGroup)EditorUserBuildSettings.activeBuildTarget, ManagedStrippingLevel.Disabled),
            () => { return PlayerSettings.GetManagedStrippingLevel((BuildTargetGroup)EditorUserBuildSettings.activeBuildTarget) != ManagedStrippingLevel.Disabled; },
            "Stripping Level should be set to DISABLE to support Android builds",
            "LeiaLoft.Ignore.StrippingLevel"));
#else
            recommendations.Add(new Recommendation(
            "Stripping Level",
            PlayerSettings.strippingLevel,
            StrippingLevel.Disabled,
            () => PlayerSettings.strippingLevel = StrippingLevel.Disabled,
            () =>{ return PlayerSettings.strippingLevel != StrippingLevel.Disabled; },
            "Stripping Level should be set to DISABLE to support Android builds",
            "LeiaLoft.Ignore.StrippingLevel"));
#endif
            recommendations.Add(new Recommendation(
            "Android Internet Access",
            PlayerSettings.Android.forceInternetPermission ? "Require" : "Auto",
            "Require",
            () => PlayerSettings.Android.forceInternetPermission = true,
            () => { return PlayerSettings.Android.forceInternetPermission != true; },
            "Leia Media Player prefers required internet access to ensure url loading works without issue",
            "LeiaLoft.Ignore.AndroidInternetPermission")); ;
#endif
            for (int i = 0; i < recommendations.Count; i++)
            {
                recommendations[i].IsIgnored = EditorPrefs.HasKey(recommendations[i].IgnoreKey);
            }
        }
        static private PlatformDeviceResolutions GetPlatformResolutions()
        {
#if UNITY_STANDALONE
            return new PlatformDeviceResolutions(new List<DeviceGameViewResolution> {
                new DeviceGameViewResolution("A0", new[]{ 3840, 2160 }, false)
            });
#else
            return new PlatformDeviceResolutions(new List<DeviceGameViewResolution> {
                new DeviceGameViewResolution("Hydrogen", new[]{ 2560, 1440 }, true),
                new DeviceGameViewResolution("Lumepad", new[]{ 2560, 1600 }, true)
            });
#endif
        }
        public static void GetMainGameViewSize()
        {
            Vector2 res = Handles.GetMainGameViewSize();
            if (!Mathf.Approximately(res.magnitude, gameViewResolution.magnitude))
            {
                gameViewResolution = res;
            }
        }
    }
}
