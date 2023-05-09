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
using System.Linq;
using System.Collections.Generic;

namespace LeiaLoft
{
    public static class CompileDefineUtil
    {
        public static void RemoveCompileDefine(string defineCompileConstant, BuildTargetGroup[] targetGroups = null)
        {
            if (targetGroups == null)
            {
                targetGroups = (BuildTargetGroup[])System.Enum.GetValues(typeof(BuildTargetGroup));
            }

            char separateChar = ';';

            foreach (BuildTargetGroup grp in targetGroups)
            {
                var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(grp);
                string newDefines = "";

                if (!defines.Contains(defineCompileConstant))
                {
                    continue;
                }

                foreach (var define in defines.Split(separateChar))
                {
                    if (define != defineCompileConstant)
                    {
                        if (newDefines.Length != 0)
                        {
                            newDefines += separateChar;
                        }
                        newDefines += define;
                    }
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(grp, newDefines);
            }
        }

        public static string[] GetCompileDefinesWithPrefix(string prefix, BuildTargetGroup platform)
        {
            char separateChar = ';';
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);
            var result = new List<string>();

            if (!defines.Contains(prefix))
            {
                return new string[0];
            }

            foreach (var define in defines.Split(separateChar))
            {
                if (define.StartsWith(prefix))
                {
                    result.Add(define);
                }
            }

            return result.ToArray();
        }

        public static void AddCompileDefine(BuildTargetGroup platform, string newDefine)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(platform);

            char separateChar = ';';

            if (!defines.Split(separateChar).Contains(newDefine))
            {
                if (defines.Length != 0)
                {
                    defines += separateChar;
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(platform, defines + newDefine);
            }
        }

        //Static call to be made from a console which will set windows compiler flags to contain "LEIALOFT_CONFIG_OVERRIDE"
        public static void AddCompileDefineConfigOverride()
        {
            AddCompileDefine(BuildTargetGroup.Standalone, "LEIALOFT_CONFIG_OVERRIDE");
            AddCompileDefine(BuildTargetGroup.Android, "LEIALOFT_CONFIG_OVERRIDE");
        }

        public static void RemoveCompileDefineConfigOverride()
        {
            RemoveCompileDefine("LEIALOFT_CONFIG_OVERRIDE", new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android });
        }
    }
}