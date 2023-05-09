#if UNITY_EDITOR

using UnityEditor.Build;
using UnityEditor;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace LeiaLoft {

#if UNITY_2018_1_OR_NEWER
    /// <summary>
    /// LeiaLoft's Editor-only pre-build routine.
    /// </summary>
    public class PreBuildProcess : IPreprocessBuildWithReport {
#else
    /// <summary>
    /// LeiaLoft's Editor-only pre-build routine.
    /// </summary>
    public class LeiaPreBuildProcess : IPreprocessBuild {
#endif

        int IOrderedCallback.callbackOrder
        {
            get
            {
                return 0;
            }
        }

#if UNITY_2018_1_OR_NEWER
        /// <summary>
        /// In UnityEngine 2018.1+, sets editor to allow unsafe code.
        /// </summary>
        /// <param name="report"></param>
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
#else
        void IPreprocessBuild.OnPreprocessBuild(BuildTarget target, string path)
        {
#endif

        }

    }

}

#endif
