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
namespace LeiaLoft
{
    public interface ILeiaDevice
    {
        string GetProfileStubName();
        void SetProfileStubName(string name);
		void SetBacklightMode(int modeId);
		void RequestBacklightMode(int modeId);
		void SetBacklightMode(int modeId, int delay);
		void RequestBacklightMode(int modeId, int delay);
		int GetBacklightMode();
        string GetSensors();
        bool IsSensorsAvailable();
        bool IsConnected();
        void CalibrateSensors();
		DisplayConfig GetDisplayConfig();
        DisplayConfig GetDisplayConfig(bool forceReload);
        int[] CalibrationOffset { get; set; }
        bool IsScreenOrientationLandscape();
        bool HasDeviceOrientationChangedSinceLastQuery();
        UnityEngine.ScreenOrientation GetScreenOrientationRGB();
    }
}
