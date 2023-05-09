﻿using UnityEngine;
using UnityEditor;
using System.Linq;

namespace LeiaLoft
{
    /// <summary>
    /// Custom Editor/Inspector for user interactions with LeiaMediaRecorder
    /// </summary>
    [CustomEditor(typeof(LeiaLoft.LeiaMediaRecorder))]
    public class LeiaMediaRecorderEditor : UnityEditor.Editor
    {
        LeiaLoft.LeiaMediaRecorder lmr;

        SerializedProperty lmr_recordingCondition;
        SerializedProperty lmr_recordingFormat;
        SerializedProperty lmr_frameRate;
        SerializedProperty lmr_recordingTimes;
        SerializedProperty lmr_userWriteParentPath;

        private GUIStyle _longTextButtonStyle;
        private GUIStyle pathButtonStyle
        {
            get
            {
                if (_longTextButtonStyle == null)
                {
                    _longTextButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        wordWrap = true
                    };
                }
                return _longTextButtonStyle;
            }
        }



        void OnEnable()
        {
            lmr = (LeiaLoft.LeiaMediaRecorder)target;

            lmr_recordingCondition = serializedObject.FindProperty("_recordingCondition");
            lmr_recordingFormat = serializedObject.FindProperty("_recordingFormat");
            lmr_frameRate = serializedObject.FindProperty("_frameRate");
            lmr_recordingTimes = serializedObject.FindProperty("_recordingTimes");
            lmr_userWriteParentPath = serializedObject.FindProperty("_userWriteParentPath");
            FilterPath(lmr_userWriteParentPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
#if !UNITY_2017_3_OR_NEWER
            EditorGUILayout.HelpBox("Use Unity 2017.3+ to record video", MessageType.Warning);
#endif

            EditorGUILayout.LabelField("Record into a sub-directory of:", EditorStyles.miniLabel);
            if (GUILayout.Button(lmr.UserWriteParentPath, pathButtonStyle))
            {
                string userSelection = EditorUtility.SaveFolderPanel("Select a directory", lmr_userWriteParentPath.stringValue, "");
                if (!string.IsNullOrEmpty(userSelection))
                {
                    if (lmr_userWriteParentPath.stringValue != userSelection)
                    {
                        lmr_userWriteParentPath.stringValue = userSelection;
                        FilterPath(lmr_userWriteParentPath);
                    }

                }
            }

            // if directory actually exists, then we can open it in file explorer
            EditorGUI.BeginDisabledGroup(!System.IO.Directory.Exists(lmr_userWriteParentPath.stringValue));
            if (GUILayout.Button("Open recording directory in OS file explorer"))
            {
                EditorUtility.RevealInFinder(lmr_userWriteParentPath.stringValue);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(lmr_recordingCondition);

            // no support for a popup which supports a subset of potential enum types. have to work with a string[]

            // get array of supported formats
            string[] potentialFormatArray = RecordingFormatUtils.supportedFormatsOnPlatform.Select(elem => elem.ToString()).ToArray();

            // find index within the array
            int previousRecordingFormatIndex = System.Array.IndexOf(potentialFormatArray, lmr.recordingFormat.ToString());
            if (previousRecordingFormatIndex < 0)
            {
                LogUtil.Log(LogLevel.Error, "RecordingFormat {0} is not supported on this machine. Setting to {1}", lmr.recordingFormat, LeiaMediaRecorder.RecordingFormat.png);
                lmr.recordingFormat = LeiaMediaRecorder.RecordingFormat.png;
            }
            // have to convert from potentialFormatArray[i] to RecordingFormat
            UndoableInputFieldUtils.PopupLabeled(
                (int i) =>
                {
                    // convert a string back to a RecordingFormat
                    lmr.recordingFormat = (LeiaMediaRecorder.RecordingFormat) System.Enum.Parse(typeof(LeiaMediaRecorder.RecordingFormat), potentialFormatArray[i], true);
                },
                typeof(LeiaMediaRecorder.RecordingFormat).Name,
                previousRecordingFormatIndex,
                potentialFormatArray
                );

            EditorGUILayout.PropertyField(lmr_frameRate);

            // since we convert value to string, we do not have to worry about pre-2017.3 vs post-2017.3 issues
            string condition = ((LeiaLoft.LeiaMediaRecorder.RecordingCondition)lmr_recordingCondition.enumValueIndex).ToString();

            if (lmr != null && lmr_recordingFormat.enumValueIndex == -1)
            {
                Debug.LogWarningFormat("Project was rolled back to version predating MediaEncoder. Change {0}.LeiaMediaRecorder.recordingCondition to be png or jpg", lmr.transform.name);
            }

            // don't allow users to change frame recording times as application is playing
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            if (lmr != null && condition.Equals("frame"))
            {
                // recruits MinMaxPairDrawer to draw property in custom editor
                EditorGUILayout.PropertyField(lmr_recordingTimes);
            }
            EditorGUI.EndDisabledGroup();

            // don't allow users to use "start/stop recording" buttons while application is paused
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);

            // inactive: offer to start recording
            if (lmr != null && !lmr.GetActive() && condition.Equals("button_click") && GUILayout.Button("Start recording"))
            {
                lmr.BeginRecording();
            }
            // active: offer to stop recording
            if (lmr != null && lmr.GetActive() && condition.Equals("button_click") && GUILayout.Button("Stop recording"))
            {
                lmr.EndRecording();
            }

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }


        void FilterPath(SerializedProperty pathProperty)
        {
            bool directoryExist = System.IO.Directory.Exists(pathProperty.stringValue);
            if (!directoryExist)
            {
                pathProperty.stringValue = System.IO.Directory.GetParent(Application.dataPath).FullName;
                serializedObject.ApplyModifiedProperties();
            }
        }

    }
}