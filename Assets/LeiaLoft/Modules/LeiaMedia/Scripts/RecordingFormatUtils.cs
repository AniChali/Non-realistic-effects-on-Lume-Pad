using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft {
    public static class RecordingFormatUtils {

        /// <summary>
        /// Returns a collection of supported recording formats on this Unity version + operating system
        /// </summary>
        public static HashSet<LeiaMediaRecorder.RecordingFormat> supportedFormatsOnPlatform
        {
            get
            {
                HashSet<LeiaMediaRecorder.RecordingFormat> supportedFormats = new HashSet<LeiaMediaRecorder.RecordingFormat>();

                // every platform atomically supports png/jpg
                supportedFormats.UnionWith(GetAllImageFormats());

                // create a collection of supported video formats
                HashSet<LeiaMediaRecorder.RecordingFormat> filteredVideoFormats = GetAllVideoFormats();
                filteredVideoFormats.IntersectWith(GetSupportedVideoFormatsPerRuntimePlatform(Application.platform));
                filteredVideoFormats.IntersectWith(GetSupportedVideoFormatsOnUnityVersion());

                supportedFormats.UnionWith(filteredVideoFormats);

                return supportedFormats;
            }
        }

        public static bool IsVideoFormat(this LeiaMediaRecorder.RecordingFormat recFormat)
        {
            return GetAllVideoFormats().Contains(recFormat);
        }

        public static bool IsImageFormat(this LeiaMediaRecorder.RecordingFormat recFormat)
        {
            return GetAllImageFormats().Contains(recFormat);
        }

        public static HashSet<LeiaMediaRecorder.RecordingFormat> GetSupportedVideoFormatsOnUnityVersion()
        {
            // currently don't support any video recording before Unity 2017.3, do support all video recording in 2017.3+
#if UNITY_2017_3_OR_NEWER
            return GetAllVideoFormats();
#else
            // return empty set
            return new HashSet<LeiaMediaRecorder.RecordingFormat>();
#endif
        }

        public static HashSet<LeiaMediaRecorder.RecordingFormat> GetSupportedVideoFormatsPerRuntimePlatform(RuntimePlatform platform)
        {
            // in linux, only support webm
            if (platform == RuntimePlatform.LinuxEditor)
            {
                return new HashSet<LeiaMediaRecorder.RecordingFormat>(new[] {
                    LeiaMediaRecorder.RecordingFormat.webm
                });
            }
            else if (platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.OSXEditor)
            {
                // currently, all video formats that the LeiaMediaRecorder.RecordingFormat enumerates, are supported by OSX and Windows
                // see https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html for more info
                return GetAllVideoFormats();
            }

            // not a supported OS - return empty set
            return new HashSet<LeiaMediaRecorder.RecordingFormat>();
        }

        public static HashSet<LeiaMediaRecorder.RecordingFormat> GetAllImageFormats()
        {
            return new HashSet<LeiaMediaRecorder.RecordingFormat>(new[] {
                LeiaMediaRecorder.RecordingFormat.png,
                LeiaMediaRecorder.RecordingFormat.jpg
            });
        }

        public static HashSet<LeiaMediaRecorder.RecordingFormat> GetAllVideoFormats()
        {
            return new HashSet<LeiaMediaRecorder.RecordingFormat>(new[] {
                LeiaMediaRecorder.RecordingFormat.mov,
                LeiaMediaRecorder.RecordingFormat.mp4,
                LeiaMediaRecorder.RecordingFormat.mpg,
                LeiaMediaRecorder.RecordingFormat.ogv,
                LeiaMediaRecorder.RecordingFormat.vp8,
                LeiaMediaRecorder.RecordingFormat.webm
            });
        }

    }

}
