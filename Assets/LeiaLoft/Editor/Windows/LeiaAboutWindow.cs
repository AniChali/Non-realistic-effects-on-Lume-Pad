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
using UnityEditor;
using UnityEngine;

namespace LeiaLoft.Editor
{
    [InitializeOnLoad]
    public class LeiaAboutWindow : UnityEditor.EditorWindow
    {

        const string BannerAssetFilename = "LeiaLoftSDK";
        const string editor_About_ForcePopUp = "LeiaLoft.About.ForcePopUp";

        static LeiaWelcomeWindow welcomeWindow;
        static LeiaReleaseNotesWindow releaseNotesWindow;
        static LeiaLogSettingsWindow logSettingsWindow;
        static LeiaVersionUpdateWindow versionUpdateWindow;
        static LeiaAboutWindow window;
        private enum Page { Welcome, Release_Notes, Log_Settings };
        private static Page _page = Page.Welcome;

        static string[] pageNames;
        private static bool _isInitialized = false, forceShow = true;

        private static Texture2D mBannerImage;
        private static bool bannerImageRequested;
        private static Texture2D BannerImage
        {
            get
            {
                if (mBannerImage == null)
                {
                    // cannot call T2D black texture (or general Unity things) from pre-constructor code in LeiaAboutWindow properties. have to set here
                    mBannerImage = Texture2D.blackTexture;
                }
                if (!bannerImageRequested)
                {
                    // if we have never requested banner image, begin request
                    bannerImageRequested = true;
                    ResourceRequest bannerImageRequest = Resources.LoadAsync<Texture2D>(BannerAssetFilename);
                    bannerImageRequest.completed += (AsyncOperation requestResult) =>
                    {
                        mBannerImage = (Texture2D)((ResourceRequest)requestResult).asset;
                    };
                }

                // return a T2D black texture or the loaded banner image
                return mBannerImage;
            }
        }
        private static Vector2 scrollPosition;
        static GUIStyle centeredStyle;
        static LeiaAboutWindow()
        {
            EditorApplication.update += Update;
            EditorApplication.update += GetCurrentVersion;
        }

        static void Update()
        {


            if (ShouldForceWindowPopUp())
            {
 
                Open();
            }
        }
        static void GetCurrentVersion()
        {

            UpdateChecker.CurrentSDKVersion = LeiaLoft.Diagnostics.SDKStringData.SDKVersionLine;

            if (!string.IsNullOrEmpty(UpdateChecker.CurrentSDKVersion))
            {
                EditorApplication.update -= GetCurrentVersion;
            }
        }
        static bool ShouldForceWindowPopUp()
        {
            forceShow = EditorPrefs.GetBool(editor_About_ForcePopUp, true);

            if (!forceShow)
            {
                return false;
            }
            if (_isInitialized)
            {
                return false;
            }
           

            if (String.IsNullOrEmpty(UpdateChecker.CurrentSDKVersion) || !UpdateChecker.UpdateChecked || UpdateChecker.CheckUpToDate())
            {
                return false;
            }
            return true;
        }

        private static void CreatePopupToggle()
        {
            UndoableInputFieldUtils.BoolFieldWithTooltip(() => { forceShow = EditorPrefs.GetBool(editor_About_ForcePopUp, true); return forceShow; }, b => { forceShow = b; EditorPrefs.SetBool(editor_About_ForcePopUp, b); }, "  Automatically Pop-up", "Display this window when opening Unity. Alternatively, this widow can be opened from LeiaLoft-> About", window);
        }
        private void OnDestroy()
        {
            _page = Page.Welcome;
        }

        [MenuItem("LeiaLoft/About &l")]
        public static void Open()
        {

            pageNames = Enum.GetNames(typeof(Page));
            for (int i = 0; i < pageNames.Length; i++)
            {
                pageNames[i] = pageNames[i].Replace('_', ' ');
            }
            // re-update the actual SDK version each time that the About window is opened
            UpdateChecker.CurrentSDKVersion = LeiaLoft.Diagnostics.SDKStringData.SDKVersionLine;
            window = GetWindow<LeiaAboutWindow>(true, "About LeiaLoft SDK");
            window.minSize = EditorWindowUtils.WindowMinSize;
            _isInitialized = true;
        }

        private static void InitilizeWindowTabs()
        {
      
            if (releaseNotesWindow == null) {
                releaseNotesWindow = new LeiaReleaseNotesWindow();
            }


            if (logSettingsWindow == null) {
                logSettingsWindow = new LeiaLogSettingsWindow();
            }
    

            if (welcomeWindow == null) {
                welcomeWindow = new LeiaWelcomeWindow();
            }

            if (versionUpdateWindow == null) {
                versionUpdateWindow = new LeiaVersionUpdateWindow(); 
            }
 
        }
 

        private void Title()
        {
            EditorWindowUtils.TitleTexture(BannerImage);

            if (String.IsNullOrEmpty(UpdateChecker.CurrentSDKVersion))
            {
                return;
            }
            if (centeredStyle == null)
            {
                centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            }
            EditorWindowUtils.Space(5);
            EditorWindowUtils.Label(String.Format("Version: {0}", UpdateChecker.CurrentSDKVersion), centeredStyle);

            if (pageNames == null) //use case: About Window is open while entering Play
            {
                Open();
            }
            EditorWindowUtils.BeginHorizontalCenter();
            EditorWindowUtils.Button(() => { GetWindow<LeiaRecommendedSettings>(true); }, "Leia Recommended Settings");
            EditorWindowUtils.EndHorizontalCenter();
            EditorWindowUtils.Space(10);
            _page = (Page)GUILayout.Toolbar((int)_page, pageNames);
            EditorWindowUtils.HorizontalLine();
            EditorWindowUtils.Space(5);
        }

        private void OnGUI()
        {
            Title();
            EditorWindowUtils.BeginVertical();
            scrollPosition = EditorWindowUtils.BeginScrollView(scrollPosition);

            InitilizeWindowTabs();

            switch (_page)
            {
                case Page.Welcome:
                    versionUpdateWindow.DrawGUI();
                    welcomeWindow.DrawGUI();
                    break;
                case Page.Release_Notes:
                    releaseNotesWindow.DrawGUI();
                    break;
                case Page.Log_Settings:
                    logSettingsWindow.DrawGUI();
                    break;
                default:
                    welcomeWindow.DrawGUI();
                    break;
            }
 

            EditorWindowUtils.EndScrollView();
            EditorWindowUtils.EndVertical();
            EditorWindowUtils.Space(10);
            CreatePopupToggle();
            EditorWindowUtils.Space(10);
        }
    }
}
