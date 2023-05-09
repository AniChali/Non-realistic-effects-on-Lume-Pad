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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    public class LeiaSettings : MonoBehaviour
    {
        public static string GameObjectName { get { return "LeiaLoft.LeiaSettings"; } }

        /// <summary>
        /// Profile stub used when no profile is obtained from device
        /// </summary>
        [SerializeField, HideInInspector]
        private string _profileStubName = "GRATING_4X4";
        [SerializeField, HideInInspector]
        private LeiaStateDecorators _decorators = LeiaStateDecorators.Default;
        [SerializeField, HideInInspector]
        private string _leiaStateId = "HPO";
        [SerializeField, HideInInspector]
        private string _desiredLeiaStateId = "HPO";
        [SerializeField, HideInInspector]
        private int _antiAliasing = 1;
        [SerializeField, HideInInspector]
        public int AntiAliasing { get { return _antiAliasing; } set { _antiAliasing = value; } }
        public string ProfileStubName { get { return _profileStubName; } set { _profileStubName = value; } }
        public string LeiaStateId { get { return _leiaStateId; } set { _leiaStateId = value; } }
        public string DesiredLeiaStateID { get { return _desiredLeiaStateId; } set { _desiredLeiaStateId = value; } }
        public LeiaStateDecorators Decorators { get { return _decorators; } set { _decorators = value; } }
        public override string ToString()
        {
            return string.Format("[LeiaSettings: AntiAliasing={0}, ProfileStubName={1}, LeiaStateId={2}, Decorators={3}]", AntiAliasing, ProfileStubName, LeiaStateId, Decorators);
        }
    }
}
