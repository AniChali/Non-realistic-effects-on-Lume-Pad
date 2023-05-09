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
using System.IO;

namespace LeiaLoft.Editor
{

    public class LeiaReleaseNotesWindow
    {

        private string releaseNotesText;
        private static bool isChangelogFoldOut = true;
        private static Vector2 scrollPosition;
        private static readonly float changelogWidth = 500;
        static GUIStyle changeLogStyle;
        const int maxNoteLength = 16000;
        private const string RELEASE = "RELEASE";
        private const string CHANGELOG = "CHANGELOG";

        public void DrawGUI()
        {
            readReleaseNotes();
            if (changeLogStyle == null) {
                changeLogStyle = new GUIStyle(EditorStyles.textArea) {
                    richText = true
                };
            }
            isChangelogFoldOut = EditorWindowUtils.Foldout(isChangelogFoldOut, "Release Notes");

            if (isChangelogFoldOut)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorWindowUtils.Space(10);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        scrollPosition = EditorWindowUtils.DrawScrollableSelectableLabel(
                            scrollPosition,
                            changelogWidth,
                            releaseNotesText,
                            changeLogStyle);
                    }
                }
            }
        }

        private void readReleaseNotes()
        {
            if (releaseNotesText == null)
                releaseNotesText = ((releaseNotesText = readReleaseNotesTxt()) != null) ? releaseNotesText : readReleaseNotesMarkDown();
        }

        private string readReleaseNotesTxt()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(RELEASE);

            if (textAsset != null)
            {
                releaseNotesText = textAsset.text;
                if (!string.IsNullOrEmpty(releaseNotesText) && releaseNotesText.Length > maxNoteLength)
                {
                    return releaseNotesText.Substring(0, maxNoteLength) + "\nTruncated...\n";
                }
            }
            return null;
        }

        private string readReleaseNotesMarkDown()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(CHANGELOG);

            if (textAsset != null)
            {
                releaseNotesText = textAsset.text;
                if (!string.IsNullOrEmpty(releaseNotesText) && releaseNotesText.Length > maxNoteLength)
                {
                    return releaseNotesText.Substring(0, maxNoteLength) + "\nTruncated...\n";
                }
            }
            return null;
        }

    }
}
