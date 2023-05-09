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

using UnityEditor;
using UnityEngine;

namespace LeiaLoft.Editor
{
    [InitializeOnLoad]
    public class LeiaVersionUpdateWindow  
    {
        private static readonly float PatchNotesWidth = 500;
        private static readonly float PatchNotesMinHeight = 250.0f;

        private static Vector2 scrollPositionPatchNotes;
        private static GUIStyle _centeredStyle = GUIStyle.none;
        private static GUIStyle _versionStyle = GUIStyle.none;
        private static GUIStyle _patchNotesStyle = GUIStyle.none;
        private static bool _isInitialized = false;
        private static bool _isExpanded;

        public static GUIStyle CenteredStyle
        {
            get
            {
                if (_centeredStyle == GUIStyle.none)
                {
                    _centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
                }
                return _centeredStyle;
            }
        }
        public static GUIStyle VersionStyle
        {
            get
            {
                if (_versionStyle == GUIStyle.none)
                {
                    _versionStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 14,
                        margin = new RectOffset(20, 20, 0, 0)
                    };
                }
                return _versionStyle;
            }
        }
        public static GUIStyle PatchNotesStyle
        {
            get
            {
                if (_patchNotesStyle == GUIStyle.none)
                {
                    _patchNotesStyle = new GUIStyle(EditorStyles.textArea)
                    {
                        richText = true,
                        margin = new RectOffset(10, 10, 10, 10),
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                }
                return _patchNotesStyle;
            }
        }

        static LeiaVersionUpdateWindow()
        {
            UpdateChecker.CheckForUpdates();
            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (!_isInitialized && UpdateChecker.UpdateChecked && !string.IsNullOrEmpty(UpdateChecker.CurrentSDKVersion))
            {
                _isInitialized = true;
                _isExpanded = !UpdateChecker.CheckUpToDate();
                EditorApplication.update -= Update;
            }
        }

        private void Title()
        {
            EditorWindowUtils.Space(20);
            string updateText;
            if (!UpdateChecker.UpdateChecked)
            {
                updateText = "Checking for updates...";
            }
            else
            {
                if (!UpdateChecker.CheckUpToDate())
                {
                    updateText = "A new version of the LeiaLoft Unity SDK is available!";
                }
                else
                {
                    updateText = "Your LeiaLoft Unity SDK is up to date!";
                }
            }
            EditorWindowUtils.Label(updateText, CenteredStyle);
            EditorWindowUtils.Space(20);
            EditorWindowUtils.Label("Currently installed version: " + UpdateChecker.CurrentSDKVersion, VersionStyle);
            EditorWindowUtils.Space(5);
            EditorWindowUtils.Label("Latest version: " + UpdateChecker.LatestSDKVersion, VersionStyle);
            EditorWindowUtils.Space(10);
            EditorWindowUtils.HorizontalLine();
            EditorWindowUtils.Space(10);
        }

        private static void Changes()
        {
            EditorWindowUtils.Label("Changes for " + UpdateChecker.LatestSDKVersion + ":", VersionStyle);
            EditorWindowUtils.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorWindowUtils.Space(5);
                using (new EditorGUILayout.VerticalScope())
                {
                    scrollPositionPatchNotes = EditorWindowUtils.DrawScrollableSelectableLabel(
                        scrollPositionPatchNotes,
                        PatchNotesWidth,
                        // this is the string that is displayed in welcome panel. it is set in UpdateChecker.cs
                        UpdateChecker.Patchnotes,
                        PatchNotesStyle,
                        20.0f,
                        20.0f,
                        PatchNotesMinHeight
                    );
                }
            }
        }

        private static void UpdateFoldout()
        {
            EditorWindowUtils.BeginHorizontal();
            _isExpanded = EditorGUILayout.Foldout(_isExpanded,string.Format("Updates [ {0}! ]", UpdateChecker.CheckUpToDate() ? "Up To Date" : "Update Available"), true);
            EditorWindowUtils.EndHorizontal();
        }

        private void Download()
        {
            EditorWindowUtils.Space(20);
            EditorWindowUtils.Button(() => { Application.OpenURL(UpdateChecker.SDKDownloadLink); }, "Download Update");
        }

        public void DrawGUI()
        {
            if(!_isInitialized)
            {
                return;
            }
            UpdateFoldout();

            if (_isExpanded)
            {
                EditorWindowUtils.HorizontalLine();
                Title();
                Changes();
                Download();
            }
            EditorWindowUtils.HorizontalLine();
        }
    }
}
