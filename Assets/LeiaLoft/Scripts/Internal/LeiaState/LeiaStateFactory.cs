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
using System;
using System.Linq;
using System.Collections.Generic;

namespace LeiaLoft
{
    /// <summary>
    /// Is used to issue LeiaStates of different types that depend on device 
    /// specifications and user preference. Delegates creation to *Builder classes
    /// </summary>
    public class LeiaStateFactory
    {
		private const string DisplayNotSupported = "Display type not supported: {0}";
		private const string DisplayNotSupportedException = "[LeiaStateFactory] Display type not supported: {0}";
		private const string StateNotSupported = "State Id not supported: {0}";
		private const string UsingDefault = "Using default: {0}";
		private const string UnknownState = "Unknown State Id: {0}";
		private const string StateNotSupportedException = "[LeiaStateFactory] State Id not supported: {0}";

		private DisplayConfig _displayConfig;
        private AbstractLeiaStateBuilder _builder3D;
        private AbstractLeiaStateBuilder _builder2D;

        private static Dictionary<string,Type> _customBuilders = new Dictionary<string, Type>();
        private static HashSet<string> _availableStateIds = new HashSet<string>();

        public static string[] AvailableStateIds
        {
            get
            {
                return _availableStateIds.ToArray();
            }
        }

		public LeiaStateFactory()
        {
        }

        public void SetDisplayConfig(DisplayConfig displayConfig)
        {
            if (_displayConfig != null && _displayConfig.Equals(displayConfig))
            {
                return;
            }

            this.Debug("SetProfile");

            _displayConfig = displayConfig;
            _displayConfig.RenderModes.ToList().ForEach(m => _availableStateIds.Add(m));

            _builder2D = new TwoDimLeiaStateBuilder(_displayConfig);

            if (_displayConfig.NumViews.x == 1 && _displayConfig.NumViews.y == 1)
            {
                _builder3D = new TwoDimLeiaStateBuilder(_displayConfig);
            }
            else{
                _builder3D = new SlantedLeiaStateBuilder(_displayConfig);
            }
        }

        /// <summary>
        /// Checks if state is valid for this profile and returns default if not
        /// </summary>
        public string ValidateState(string desiredLeiaStateId)
        {
            if (!_availableStateIds.Contains(desiredLeiaStateId))
            {
                return DisplayConfig.DefaultRenderMode;
            }

            return desiredLeiaStateId;
        }

        /// <summary>
        /// Registers an AbstractLeiaStateBuilder (by specified Type) by id.
        /// This id will be available as a new render mode (LeiaStateId) in LeiaDisplay.
        /// Returns true if registration was successful (no name conflict).
        /// </summary>
        public bool RegisterLeiaState(string id, Type builder)
        {
            this.Info("Registering: " + id);

            if (_availableStateIds.Contains(id))
            {
                this.Debug("id already added: " + id);
                return false;
            }

            _customBuilders[id] = builder;
            _availableStateIds.Add(id);
            return true;
        }

        public ILeiaState GetState(string id)
        {
            this.Debug(string.Format("GetState( {0})", id));
            id = ValidateState(id);

            switch (id.ToLower())
            {
                case "basic":
                    return _builder3D.MakeBasic();
                case "hpo":
                    return _builder3D.MakeHPO();
                case "2d":
                    return _builder2D.MakeBasic();
                case "tvo":
                    return _builder3D.MakeTVO();
                default:
                    LogUtil.Warning("Uknown state in builder : " + id);
                    return _builder3D.MakeBasic();
            }

        }
    }
}
