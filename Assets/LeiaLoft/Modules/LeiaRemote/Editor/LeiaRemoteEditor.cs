
using UnityEngine;
using UnityEditor;

namespace LeiaLoft
{
    [DefaultExecutionOrder(451)]
    [CustomEditor(typeof(LeiaRemote))]
    public class LeiaRemoteEditor : UnityEditor.Editor

    #region Properties
    {
        private const string RemoteDevice = "Any Android Device";
        private const string StreamingModeLabel = "Streaming Mode";
        private const string CompressionModeLabel = "Compression Mode";
        private const string ResolutionModeLabel = "Resolution Mode";
        private const string PerformanceModeHeader = "Streaming Optimizations";
        private const string PerformanceModeHelper = "Performance Mode offers optimizations for streaming to the device. The quality displayed in this mode does not reflect built content and will lessen 3D precision. We recommend Quality Mode whenever possible.";
       
        private LeiaRemote _controller;

        #endregion
        #region UnityCallbacks

        void OnValidate()
        {
            UpdateRemoteEditorSettings();
        }
        public override void OnInspectorGUI()
        {
            if (_controller == null)
            {
                _controller = (LeiaRemote)target;
                UpdateRemoteEditorSettings();
            }
            if (!_controller.enabled)
            {
                return;
            }
            ShowStreamingModeControl();
            if (_controller.DesiredStreamingMode == LeiaRemote.StreamingMode.Performance)
            {
                ShowPerformanceModeOptions();
            }
        }

        #endregion
        #region Settings

        private void ForceQualityOptions()
        {
            if (_controller.DesiredContentResolution == LeiaRemote.ContentResolution.Normal && _controller.DesiredContentCompression == LeiaRemote.ContentCompression.PNG)
            {
                return;
            }
            _controller.DesiredContentResolution = LeiaRemote.ContentResolution.Normal;
            _controller.DesiredContentCompression = LeiaRemote.ContentCompression.PNG;
            UpdateRemoteEditorSettings();
        }
        private void UpdateRemoteEditorSettings()
        {
            //Editor Settings related to Unity Remote are compatible with Leia Remote 2
            EditorSettings.unityRemoteDevice = RemoteDevice;
            EditorSettings.unityRemoteCompression = _controller.DesiredContentCompression.ToString();
            EditorSettings.unityRemoteResolution = _controller.DesiredContentResolution.ToString();
        }

        #endregion
        #region GUI

        private void ShowPerformanceModeOptions()
        {
            EditorWindowUtils.Space(10);
            EditorWindowUtils.HelpBox(PerformanceModeHelper, MessageType.Info);
            EditorWindowUtils.Space(10); 
            bool isShowPerformanceOptions = true;
            isShowPerformanceOptions = EditorWindowUtils.Foldout(isShowPerformanceOptions, PerformanceModeHeader);
            if (!isShowPerformanceOptions)
            {
                return;
            }
            ShowCompressionControl();
            ShowResolutionControl();
        }
        private void ShowStreamingModeControl()
        {
            LeiaRemote.StreamingMode[] modes = { LeiaRemote.StreamingMode.Quality, LeiaRemote.StreamingMode.Performance };
            string[] options = { modes[0].ToString(), modes[1].ToString() };

            int previousIndex = (int)_controller.DesiredStreamingMode;
            UndoableInputFieldUtils.PopupLabeled(index =>{
                _controller.DesiredStreamingMode = modes[index];
                UpdateRemoteEditorSettings();
            }, StreamingModeLabel, previousIndex, options, _controller);
        }

        private void ShowCompressionControl()
        {
            LeiaRemote.ContentCompression[] modes = new[] { LeiaRemote.ContentCompression.PNG, LeiaRemote.ContentCompression.JPEG };
            string[] options = { modes[0].ToString(), modes[1].ToString() };

            int previousIndex = (int)_controller.DesiredContentCompression;
            UndoableInputFieldUtils.PopupLabeled(index => {
                _controller.DesiredContentCompression = modes[index];
                UpdateRemoteEditorSettings();
            }, CompressionModeLabel, previousIndex, options, _controller);
        }
        private void ShowResolutionControl()
        {
            LeiaRemote.ContentResolution[] modes = new[] { LeiaRemote.ContentResolution.Normal, LeiaRemote.ContentResolution.Downsize };
            string[] options = { modes[0].ToString(), modes[1].ToString() };

            int previousIndex = (int)_controller.DesiredContentResolution;
            UndoableInputFieldUtils.PopupLabeled(index => {
                _controller.DesiredContentResolution = modes[index];
                UpdateRemoteEditorSettings();
            }, ResolutionModeLabel, previousIndex, options, _controller);
        }
        #endregion
    }
}
