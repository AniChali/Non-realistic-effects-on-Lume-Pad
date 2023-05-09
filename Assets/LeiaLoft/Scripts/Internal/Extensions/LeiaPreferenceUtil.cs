#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LeiaLoft {

    /// <summary>
    /// An edit-time-only utility for retrieving user-specified values from Unity editor.
    /// </summary>
    public static class LeiaPreferenceUtil {

        /// <summary>
        /// </summary>
        /// <param name="fallback">A string to return if key does not have a value</param>
        /// <param name="keyname">Key to search for</param>
        /// <param name="specifiers">Optional additional params to bundle with the key name</param>
        /// <returns>A string value which is paired to the keyname + specifiers provided</returns>
        public static string GetUserPreference(string fallback, string keyname, params object[] specifiers)
        {
            string key = KeyWithSpecifiersToString(keyname, specifiers);
            return EditorPrefs.GetString(key, fallback);
        }

        /// <summary>
        /// Stores a key using UnityEditorPreferences.
        /// </summary>
        /// <param name="keyname">Name of key to store</param>
        /// <param name="value">Value of key to store</param>
        /// <param name="specifiers">Optional additional params to bundle with the key name</param>
        public static void SetUserPreference(string keyname, string value, params object[] specifiers)
        {
            string key = KeyWithSpecifiersToString(keyname, specifiers);
            EditorPrefs.SetString(key, value);
        }

        /// <summary>
        /// Get boolean user preference.
        /// </summary>
        /// <param name="fallback">Default value of bool</param>
        /// <param name="keyname">A key to associate with the bool value</param>
        /// <param name="specifiers">Additional values to concatenate to key to make a more specific key</param>
        /// <returns></returns>
        public static bool GetUserPreferenceBool(bool fallback, string keyname, params object[] specifiers)
        {
            string key = KeyWithSpecifiersToString(keyname, specifiers);
            return EditorPrefs.GetBool(key, fallback);
        }

        /// <summary>
        /// Sets boolean user preference.
        /// </summary>
        /// <param name="keyname">A key to associate with the bool value</param>
        /// <param name="value">A value to associate with key</param>
        /// <param name="specifiers">Additional values to concatenate to key to make a more specific key</param>
        public static void SetUserPreferenceBool(string keyname, bool value, params object[] specifiers)
        {
            string key = KeyWithSpecifiersToString(keyname, specifiers);
            EditorPrefs.SetBool(key, value);
        }

        /// <summary>
        /// Concatenates keyname and specifiers into one string
        /// </summary>
        /// <param name="keyname">A string stem to start with</param>
        /// <param name="specifiers">Additional params to be converted into strings and concatenated</param>
        /// <returns>A concatenation of keyname and optional additional specified params.</returns>
        private static string KeyWithSpecifiersToString(string keyname, object[] specifiers)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(keyname);

            foreach (object o in specifiers)
            {
                sb.AppendFormat("_{0}", o == null? "_0": o.ToString());
            }

            return sb.ToString();
        }
    }
}
#endif
