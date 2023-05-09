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
    ///<summary>
    ///Override Leia Device to be paired with an overridden DisplayConfig for non-leia device usage.
    ///
    ///This class loads a profile "DisplayConfigOverride.json" which can be applied to any given device (including non-Leia-devices) in builds.
    ///<summary>
    public class OverrideLeiaDevice : OfflineEmulationLeiaDevice
    {
        // provide a fixed string token to refer to
        private const string defaultOverrideDisplayConfigFilename = "DisplayConfiguration_Override";

        public static string DefaultOverrideConfigFilename
        {
            get
            {
                return defaultOverrideDisplayConfigFilename;
            }
        }

        // internally track the actual name of the string which we will load a DisplayConfiguration from
        private string profileName = defaultOverrideDisplayConfigFilename;

        /// <summary>
        /// Provide a way for users to retrieve the config filename that is currently in use. Internally it is tracked at $profileName
        /// </summary>
        public string OverrideDisplayConfigFilename
        {
            get
            {
                return profileName;
            }
            set
            {
                profileName = value;
            }
        }

        /// <summary>
        /// Users can construct a Device with a string name. While the default should be the defaultOverrideConfigFilename, users could provide another name
        /// </summary>
        /// <param name="stubName"></param>
        public OverrideLeiaDevice(string mName) : base(mName)
        {
            // this ctor chains down to OfflineEmulatedLeiaDevice
            // but will only ever load the config profile "DisplayConfigOverride.json"
            OverrideDisplayConfigFilename = mName;
        }

        public override DisplayConfig GetDisplayConfig()
        {
            if (_displayConfig != null)
            {
                return _displayConfig;
            }

            // call DisplayConfig's constructor
            _displayConfig = new DisplayConfig();

            // load data from override profile
            base.ApplyDisplayConfigUpdate(OverrideDisplayConfigFilename);

            // then overpopulate _displayConfig from json with developer-tuned values
            base.ApplyDisplayConfigUpdate(DisplayConfigModifyPermission.Level.DeveloperTuned);

            return _displayConfig;
        }
    }
}
