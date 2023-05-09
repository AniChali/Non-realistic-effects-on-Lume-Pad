/****************************************************************
*
* Copyright 2021 © Leia Inc.  All rights reserved.tiled
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

// ----------------------------------------------------------------------------
// Leia Remote
//
// Determines if the current android device is a compatible Leia device
// Sends commands to android device to change lightfield mode, rendering tecnique, or rendering mode (tiled/interlaced)
//
// ----------------------------------------------------------------------------

using UnityEngine;

using System;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace LeiaLoft
{
    [DefaultExecutionOrder(450)]
    [HelpURL("https://docs.leialoft.com/developer/unity-sdk/modules/leia-remote")]
    public class LeiaRemote : MonoBehaviour
    {
#if UNITY_EDITOR

        #region Properties

        private static readonly string WARNING_ANDROID_SDK = "::LeiaRemote::UnityEditor.EditorPrefs.GetString(\"AndroidSdkRoot\") is empty. Please ensure SDK path at Unity->Preferences->External Tools points to a valid Android SDK.";
        private static readonly string WARNING_LEIA_REMOTE_NOT_INSTALLED = "::LeiaRemote::LeiaRemote is not installed on the connected device.";
        private static readonly string WARNING_DEVICE_NOT_CONNECTED = "::LeiaRemote::No Leia Device is connected.";

        private static readonly string ADB_LIGHTFIELD_COMMAND = "shell curl -X LEIA http://127.0.0.1:8005?LIGHTFIELD=";    //Lightfield On/Off
        private static readonly string ADB_RENDER_TECHNIQUE_COMMAND = "shell curl -x LEIA http://127.0.0.1:8005?RENDERING="; //Render Technique Stereo/Default
        private static readonly string ADB_PREFABSTATUS_COMMAND = "shell curl -X LEIA http://127.0.0.1:8005?PREFABSTATUS=";   // Prefab Status: Enabled / Disabled
        private static readonly string ADB_DEVICE_LIST_COMMAND = "devices -l"; //Displays a list of devices
        private static readonly string ADB_PACKAGE_INSTALLED_COMMAND = "shell pm path com.leiainc.unityremote2"; //Returns path of package or empy if not installed
        private static readonly string ADB_APP_START_COMMAND = "shell monkey -p com.leiainc.unityremote2 -c android.intent.category.LAUNCHER 1";
        private static readonly string[] DEVICE_LIST = { "H1A1000",/*Hydrogen*/ "LPD_10W" /*Lumepad*/};
        private string adbPath = "";
        bool isConnectionEstablished = false;
        private CompatabilityStatus compatibleDevice = CompatabilityStatus.None;
        private CompatabilityStatus remoteInstalled = CompatabilityStatus.None;

        private enum CompatabilityStatus
        {
            None = 0,
            Compatible = 1,
            Incompatible = 2
        }

        public enum StreamingMode { Quality = 0, Performance = 1 }
        [SerializeField] private StreamingMode streamingMode;
        public StreamingMode DesiredStreamingMode{
            get { return streamingMode; }
            set { streamingMode = value; }
        }

        public enum ContentCompression { PNG = 0, JPEG = 1 }
        [SerializeField] private ContentCompression contentCompression;
        public ContentCompression DesiredContentCompression
        {
            get { return contentCompression; }
            set { contentCompression = value; }
        }

        public enum ContentResolution { Normal = 0, Downsize = 1 }
        [SerializeField] private ContentResolution contentResolution;
        public ContentResolution DesiredContentResolution
        {
            get { return contentResolution; }
            set { contentResolution = value; }
        }

        #endregion
        #region UnityCallbacks

        void OnEnable()
        {
            PrefabStatusChanged(true);
        }
        void OnDisable()
        {
            PrefabStatusChanged(false);
        }

        private void Awake()
        {
            SetupADBPath();
            StartCoroutine(CheckForCompatability());
        }
        void Start()
        {
            LeiaDisplay.Instance.StateChanged += BacklightModeChanged;
            LeiaDisplay.Instance.StateChanged += RenderingTechniqueChanged;
            StartCoroutine(SetDisplaySettings());
        }

        #endregion
        #region Setup

        private void StartLeiaRemote()
        {
            ExecuteADB(ADB_APP_START_COMMAND);
        }

        private IEnumerator SetDisplaySettings()
        {
            while (!isConnectionEstablished)
            {
                yield return null;
            }
            BacklightModeChanged();
            RenderingTechniqueChanged();
        }
        private void SetupADBPath()
        {
            string sdkPath = UnityEditor.EditorPrefs.GetString("AndroidSdkRoot");
            if (!string.IsNullOrEmpty(sdkPath))
            {
                adbPath = Path.GetFullPath(sdkPath) + Path.DirectorySeparatorChar + "platform-tools" + Path.DirectorySeparatorChar + "adb";

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    adbPath = Path.ChangeExtension(adbPath, "exe");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarningFormat("{0}{1}", this.gameObject.name, WARNING_ANDROID_SDK);
            }
        }

        #endregion
        #region Execute

        private void ExecuteADB(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(adbPath, command)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;
            process.Start();

            if (command == ADB_DEVICE_LIST_COMMAND)
            {
                process.OutputDataReceived += CheckForLeiaDevice;
            }
            else if (command == ADB_PACKAGE_INSTALLED_COMMAND)
            {
                process.OutputDataReceived += CheckForLeiaRemote;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
            process.Dispose();
        }

        private void BacklightModeChanged()
        {
            ExecuteADB(ADB_LIGHTFIELD_COMMAND + LeiaDisplay.Instance.ActualLightfieldMode.ToString());
        }

        private void RenderingTechniqueChanged()
        {
            ExecuteADB(ADB_RENDER_TECHNIQUE_COMMAND + LeiaDisplay.Instance.DesiredRenderTechnique.ToString());
        }

        private void PrefabStatusChanged(bool enabled)
        {
            if (Application.isPlaying)
            {
                ExecuteADB(ADB_PREFABSTATUS_COMMAND + (enabled ? "Enabled" : "Disabled"));
            }
        }

        #endregion
        #region Compatability

        private void CheckForLeiaDevice(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (compatibleDevice == CompatabilityStatus.Compatible)
            {
                return;
            }

            StringReader reader = new StringReader(outLine.Data);
            while (true)
            {
                string device = reader.ReadLine();
                if (device == null) { break; }

                for (int i = 0; i < DEVICE_LIST.Length; i++)
                {
                    if (device.IndexOf(DEVICE_LIST[i], StringComparison.InvariantCulture) != -1)
                    {
                        compatibleDevice = CompatabilityStatus.Compatible;
                        return;
                    }
                }
            }
            compatibleDevice = CompatabilityStatus.Incompatible;
        }

        private void CheckForLeiaRemote(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (remoteInstalled == CompatabilityStatus.Compatible)
            {
                return;
            }
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                remoteInstalled = CompatabilityStatus.Compatible;
            }
            else
            {
                remoteInstalled = CompatabilityStatus.Incompatible;
            }
        }

        private IEnumerator CheckForCompatability()
        {

            ExecuteADB(ADB_DEVICE_LIST_COMMAND);
            ExecuteADB(ADB_PACKAGE_INSTALLED_COMMAND);

            while (compatibleDevice == CompatabilityStatus.None || remoteInstalled == CompatabilityStatus.None)
            {
                yield return null;
            }

            if (compatibleDevice != CompatabilityStatus.Compatible)
            {
                UnityEngine.Debug.LogWarningFormat("{0}{1}", this.gameObject.name, WARNING_DEVICE_NOT_CONNECTED);
            }
            else if (remoteInstalled != CompatabilityStatus.Compatible)
            {
                UnityEngine.Debug.LogWarningFormat("{0}{1}", this.gameObject.name, WARNING_LEIA_REMOTE_NOT_INSTALLED);
            }
            else
            {
                isConnectionEstablished = true;
                StartLeiaRemote();
            }
        }
        #endregion
        
#endif
    }
}
