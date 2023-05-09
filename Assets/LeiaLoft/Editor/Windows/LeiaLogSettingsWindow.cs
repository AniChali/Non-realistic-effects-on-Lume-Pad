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

using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace LeiaLoft.Editor
{
    public class LeiaLogSettingsWindow 
    {
        const string prefix = "LEIALOFT_LOGLEVEL_";
        BuildTargetGroup[] targetPlatforms;
 

        public void DrawGUI()
        {
            EditorWindowUtils.BeginVertical();
            if (targetPlatforms == null)
            {
                targetPlatforms = new BuildTargetGroup[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone };
            }
            foreach (var grp in targetPlatforms)
            {
                EnumLogLevel(grp);
            }
            EditorWindowUtils.Space(5);
            EditorWindowUtils.EndVertical();
        }
        static void EnumLogLevel(BuildTargetGroup grp)
        {
            EditorWindowUtils.Label(string.Format("{0} log level", grp), true);
            var defines = CompileDefineUtil.GetCompileDefinesWithPrefix(prefix, grp);
            LogLevel finalValue = LogLevel.Warning;

            foreach (var def in defines)
            {
                var enumValue = def.Substring(prefix.Length);
                finalValue = (LogLevel)System.Enum.Parse(typeof(LogLevel), enumValue, true);
            }
            var logLevel = (LogLevel)EditorWindowUtils.EnumPopup("Log Level", finalValue, GUILayout.Width(300));

            if (logLevel != finalValue)
            {
                foreach (var def in defines)
                {
                    CompileDefineUtil.RemoveCompileDefine(def, new[] { grp });
                }
                CompileDefineUtil.AddCompileDefine(grp, string.Format("{0}{1}", prefix, logLevel).ToUpper(CultureInfo.InvariantCulture));
            }
        }
    }
}
