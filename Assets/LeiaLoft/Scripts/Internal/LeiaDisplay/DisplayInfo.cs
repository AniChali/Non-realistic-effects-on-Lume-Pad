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
namespace LeiaLoft
{
    public class DisplayInfo
    {
        public static bool IsLeiaDisplayInPortraitMode
        {
            get
            {
                #if UNITY_EDITOR

                if(UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
                {
                    return UnityEditor.PlayerSettings.defaultInterfaceOrientation == UnityEditor.UIOrientation.Portrait 
                        || UnityEditor.PlayerSettings.defaultInterfaceOrientation == UnityEditor.UIOrientation.PortraitUpsideDown;
                } 
                else 
                {
                    return Screen.width < Screen.height;
                }
                
                #else
                return Screen.width < Screen.height;
                #endif
            }
        }
    }
}