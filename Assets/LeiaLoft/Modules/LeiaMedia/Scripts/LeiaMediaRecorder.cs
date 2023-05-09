using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER
using UnityEditor.Media;
#endif

namespace LeiaLoft
{
    /// <summary>
    /// Class for recording Leia content as a c x r grid of views
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(LeiaLoft.LeiaCamera))]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/modules/leia-media#recording-leia-media")]
    public class LeiaMediaRecorder : UnityEngine.MonoBehaviour
    {
        // enums - recording format, recording on/off trigger

        public enum RecordingCondition { button_click, frame, script};

#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER
        private MediaEncoder encoder;
#endif
        // some formats not available on some OSes or Unity versions
        public enum RecordingFormat { png, jpg, ogv, vp8, webm, mp4, mpg, mov};

        [SerializeField] private RecordingCondition _recordingCondition;
        public RecordingCondition recordingCondition
        {
            get { return _recordingCondition; }
            set { _recordingCondition = value; }
        }

        [SerializeField] private RecordingFormat _recordingFormat;
        public RecordingFormat recordingFormat
        {
            get { return _recordingFormat; }
            set
            {
                if (RecordingFormatUtils.supportedFormatsOnPlatform.Contains(value))
                {
                    _recordingFormat = value;
                }
                else
                {
                    // if user skips LeiaMediaRecorder Inspector and tries to set recordingFormat using C#, they could pass in an unsupported format.
                    // warn them of lack of support
                    LogUtil.Log(LogLevel.Error, "Tried to set recording format to {0} but this format is not supported on {1} or {2}", value, Application.platform, Application.unityVersion);
                }
            }
        }

        [Range(1, 144)][SerializeField] private int _frameRate = 60;
        public int frameRate
        {
            get { return _frameRate; }
            set { _frameRate = value; }
        }

        [SerializeField] private MinMaxPair _recordingTimes = new MinMaxPair(0.03f, 0, "Start time", 1.0f, float.MaxValue, "End time");

        // these properties now wrap around _recordingTimes
        public float startTime
        {
            get { return _recordingTimes.min; }
            set { _recordingTimes.min = value; }
        }

        // these properties now wrap around _recordingTimes
        public float endTime
        {
            get { return _recordingTimes.max; }
            set { _recordingTimes.max = value; }
        }

        // camera params
        private int record_w;
        private int record_h;
        private int cols;
        private int rows;

        private RenderTexture[] views;
        private LeiaCamera leia_cam;
        private Texture2D receivingTex;

        // save params
        private int frame_id;

        [SerializeField]
        private string _userWriteParentPath = "";
        public string UserWriteParentPath
        {
            get
            {
                return _userWriteParentPath;
            }
            set
            {
                _userWriteParentPath = value;
            }
        }

        // we always record to a sub-folder below the "leiaMediaRecorderFolder" so that we don't have to worry about accidentally overwriting files
        // this var tracks an additional unique identifier which is generated from Time each time that recording is activated
        private string UserWriteSingleRunPath = "";

        // tracker - is recorder active?
        private bool isActive;

        /// <summary>
        /// LeiaMediaRecorder.Update():
        ///     if not recording, checks if it is time to activate
        ///     if recording, checks if it is time to deactivate
        ///     
        ///     if recording, copies RenderTextures into a 2x4 texture and writes it
        /// </summary>
        private void Update()
        {
            if (!isActive && recordingCondition.ToString().Equals("frame") && Time.time >= startTime && Time.time < endTime)
            {
                BeginRecording();
            }
            if (isActive && recordingCondition.ToString().Equals("frame") && Time.time >= endTime)
            {
                EndRecording();
            }

            if (isActive && views != null && views.Length > 0)
            {
                if (receivingTex == null)
                {
                    // assume that all frames will have same resolution
                    receivingTex = new Texture2D(record_w, record_h);
                }

                // stack on the current RT
                RenderTexture prev = RenderTexture.active;

                for (int c = 0; c < cols; c++)
                {
                    for (int r = 0; r < rows; r++)
                    {
                        // stack on the ith view
                        RenderTexture.active = views[r * cols + c];
                        receivingTex.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), RenderTexture.active.width * c, RenderTexture.active.height * (rows - 1 - r));
                        // push updates to GPU as well
                        receivingTex.Apply();
                    }
                }

                if (recordingFormat.IsImageFormat())
                {
                    AddFrameAsImage(receivingTex);
                }

                else if (RecordingFormatUtils.supportedFormatsOnPlatform.Contains(recordingFormat) && recordingFormat.IsVideoFormat())
                {
                    // is supported, and is a video format -> record with MediaEncoder
                    AddFrameToEncoder(receivingTex);
                }

                // pop off the views, reset the previously active RT
                RenderTexture.active = prev;
            }
        }

        /// <summary>
        /// Sets up
        ///     recording object(s),
        ///     recording framerate,
        ///     folders
        /// </summary>
        public void BeginRecording()
        {
            // only allow recording in edit mode
#if !UNITY_EDITOR
            return;
#endif

#pragma warning disable CS0162 // Suppress unreachable code warning

            // in all cases: regulate the passage of time
            // Time.captureFramerate fixes Update() calls such that Update() effectively only gets called
            // after enough simulated playback time has passed to warrant a new frame

            // forcing captureFramerate = 60 ensures that the correct video framerate AND duration are made
            // without Unity time at 60, 24 fps recordings are 2.5x longer than they should be
            Time.captureFramerate = frameRate;
            isActive = true;

            // in all cases, we will need RenderTextures from LeiaCamera
            if (leia_cam == null)
            {
                leia_cam = transform.GetComponent<LeiaCamera>();

                // if LeiaCamera has clear flag "Solid color" in PNG format with a weak alpha, background pixels will be dimmed by alpha
                if ((leia_cam.Camera.clearFlags == CameraClearFlags.Color || leia_cam.Camera.clearFlags == CameraClearFlags.SolidColor) &&
                    recordingFormat == RecordingFormat.png &&
                    leia_cam.Camera.backgroundColor.a < 1.0f
                    )
                {
                    LogUtil.Log(LogLevel.Warning, "When recording in format {0} from {1} with clear flag {2} and background {3}:\n\tBackground pixels will be dimmed by alpha channel of color {3}", recordingFormat, leia_cam, leia_cam.Camera.clearFlags, leia_cam.Camera.backgroundColor);
                }
            }

            if (leia_cam != null && leia_cam.GetView(0) != null && leia_cam.GetView(0).TargetTexture != null)
            {
                RenderTexture view_prime = leia_cam.GetView(0).TargetTexture;
                cols = Mathf.FloorToInt(Mathf.Sqrt(leia_cam.GetViewCount()));
                rows = (cols == 0 ? 0 : leia_cam.GetViewCount() / cols);
                record_w = view_prime.width * cols;
                record_h = view_prime.height * rows;

                if (receivingTex == null)
                {
                    receivingTex = new Texture2D(record_w, record_h);
                }
                else
                {
#if UNITY_2021_2_OR_NEWER
                    receivingTex.Reinitialize(record_w, record_h);
#else
                    receivingTex.Resize(record_w, record_h);
#endif
                }

                views = new RenderTexture[leia_cam.GetViewCount()];
                for (int i = 0; i < leia_cam.GetViewCount(); i++)
                {
                    views[i] = leia_cam.GetView(i).TargetTexture;
                }
            }

            System.DateTime currTime = System.DateTime.Now;
            UserWriteSingleRunPath = Path.Combine(UserWriteParentPath, string.Format("{0:D3}_{1:D2}_{2:D2}_{3:D2}", currTime.DayOfYear, currTime.Hour, currTime.Minute, currTime.Second));
            Directory.CreateDirectory(UserWriteSingleRunPath);

            // if png/jpg
            // no additional behavior

            // if a supported video format and in Unity 2017.3+, then we can do additional video encoder setup here
            if (RecordingFormatUtils.supportedFormatsOnPlatform.Contains(recordingFormat) && recordingFormat.IsVideoFormat())
            {
#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER
                VideoTrackAttributes videoAttr = new VideoTrackAttributes()
                {
                    frameRate = new MediaRational(frameRate),
                    width = (uint)record_w,
                    height = (uint)record_h,
                    includeAlpha = false
                };

                // TODO
                string vid_name = string.Format("recording_{0}x{1}.{2}", cols, rows, recordingFormat.ToString());
                encoder = new MediaEncoder(Path.Combine(UserWriteSingleRunPath, vid_name), videoAttr);
#endif
            }

#pragma warning restore CS0162 // Suppress unreachable code warning
        }

        /// <summary>
        /// Resets state to become available for recording again.
        /// Removes framerate regulation
        /// </summary>
        public void EndRecording()
        {
            if (isActive)
            {
                Debug.LogFormat("Completed recording. Wrote to {0}", UserWriteSingleRunPath);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += Callback_PlayMode_RefreshAssetDB;
#endif
                isActive = false;
            }
            Time.captureFramerate = 0;

#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER
            if (encoder != null)
            {
                encoder.Dispose();
            }
#endif

            // free the texture
            if (receivingTex != null)
            {
                Destroy(receivingTex);
            }
        }

#if UNITY_EDITOR
        private static bool dbRefreshRan;
        private void Callback_PlayMode_RefreshAssetDB(UnityEditor.PlayModeStateChange changeTo)
        {
            if (changeTo == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                // reimport assets once. for example, if user has written many textures to the editor window, reimport them now
                if (!dbRefreshRan)
                {
                    dbRefreshRan = true;
                    UnityEditor.AssetDatabase.Refresh();
                }

                // every call to this callback, triggers a call to remove this callback and reset the state of dbRefreshRequested
                UnityEditor.EditorApplication.playModeStateChanged -= Callback_PlayMode_RefreshAssetDB;
            }
        }
#endif
        /// <summary>
        /// Queries status of active recording
        /// </summary>
        /// <returns></returns>
        public bool GetActive()
        {
            return (isActive);
        }

        /// <summary>
        /// Adds col x row frame to a MediaEncoder. Nonfunctional before Unity 2017.3
        /// </summary>
        /// <param name="frame">A frame to append to a media file</param>
        private void AddFrameToEncoder(Texture2D frame)
        {
#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER
            if (encoder == null || !isActive)
            {
                BeginRecording();
            }
            encoder.AddFrame(frame);
#else
            throw new System.NotSupportedException("AddFrameToEncoder is not supported on " + UnityEngine.Application.version.ToString());
#endif
        }

        /// <summary>
        /// Writes a single frame as a col x row texture.
        /// </summary>
        /// <param name="frame"></param>
        private void AddFrameAsImage(Texture2D frame)
        {
            if (recordingFormat == RecordingFormat.png) {
                byte[] data = frame.EncodeToPNG();
                string frame_name = string.Format("{0:D5}_{1}x{2}.png", frame_id, cols, rows);
                File.WriteAllBytes(Path.Combine(UserWriteSingleRunPath, frame_name), data);
            }
            if (recordingFormat == RecordingFormat.jpg)
            {
                byte[] data = frame.EncodeToJPG();
                string frame_name = string.Format("{0:D5}_{1}x{2}.jpg", frame_id, cols, rows);
                File.WriteAllBytes(Path.Combine(UserWriteSingleRunPath, frame_name), data);
            }
            frame_id++;
        }

        /// <summary>
        /// Automatically calls for termination of recording
        /// </summary>
        private void OnApplicationQuit()
        {
            EndRecording();
        }
    }

}
