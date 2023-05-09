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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// Provides number-name bi-directional conversion and list of possible antialiasing setting values
    /// </summary>
    public static class AntiAliasingHelper 
    {
        private static readonly int[] _availableValues = { 1, 2, 4, 8};

        private static readonly string[] _namedValues = { "None", "2 samples", "4 samples", "8 samples" };

        public static int[] Values
        {
            get {
                return (int[])_availableValues.Clone();
            }
        }

        public static string[] NamedValues
        {
            get
            {
                return (string[])_namedValues.Clone();
            }
        }

        public static int? GetValue(string name)
        {
            for (int i = 0; i < _namedValues.Length; i++)
            {
                if (_namedValues.Equals(name))
                {
                    return _availableValues[i];
                }
            }
            return null;
        }
    }
}