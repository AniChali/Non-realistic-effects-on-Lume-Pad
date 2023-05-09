// callback only accessible after 2017.1

#if UNITY_EDITOR && UNITY_2017_1_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace LeiaLoft
{
    /// <summary>
    /// LeiaLoft Unity SDK callback for when build target changes.
    /// </summary>
    public class LeiaActiveBuildTargetChanged : IActiveBuildTargetChanged
    {
        int IOrderedCallback.callbackOrder { get { return 0; } }
         
        private static bool lockout = false;

        void IActiveBuildTargetChanged.OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            // lockout prevents a buildTargetChanged -> OfflineEmulationLeiaDevice -> buildTargetChanged... endless loop from ever occurring

            if (!lockout)
            {
                /// The EmulatedDisplayConfigFilename is stored along with build target, and triggers a game view res change.
                lockout = true;
                OfflineEmulationLeiaDevice.EmulatedDisplayConfigFilename = OfflineEmulationLeiaDevice.EmulatedDisplayConfigFilename;
                lockout = false;
            }
            else
            {
                LogUtil.Log(LogLevel.Error, "Received buildTargetChanged callback. Tried to set EmulatedDisplayConfigFilename but buildTarget was already being changed");
            }
        }
    }
}

#endif
