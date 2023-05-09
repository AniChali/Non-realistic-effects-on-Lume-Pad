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
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    public class ScreenResolutionNotifier : MonoBehaviour 
    {
		private const string InstanceAlreadyExist = "[ScreenResolutionNotifier] Instance already exists, destroying newer instance";

        public const string GameObjectName = "ScreenResolutionNotifier";

        private static ScreenResolutionNotifier _instance = null;

        public static ScreenResolutionNotifier Instance
        {
            get
            {
                if (_instance == null)
                {
                    new GameObject(GameObjectName).AddComponent<ScreenResolutionNotifier>();
                }

                return _instance;
            }
        }

        private Vector2 _prevResolution = Vector2.zero;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            if (_instance != null && _instance != this)
            {
                this.Info(InstanceAlreadyExist);
                DestroyImmediate(this.gameObject);
            }
        }

        public event System.Action OnResolutionChanged;

        void Update () 
        {
            var currResolution = new Vector2(Screen.width, Screen.height);

            if (_prevResolution != currResolution)
            {
                _prevResolution = currResolution;

                if (OnResolutionChanged != null)
                {
                    OnResolutionChanged();
                }
            }
        }
    }
}