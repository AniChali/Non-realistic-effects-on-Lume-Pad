/****************************************************************
*
* Copyright 2020 © Leia Inc.  All rights reserved.
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// Static helper class for facilitating json object reading and writing of LeiaLoft objects.
    ///
    /// Users will be able to rely upon this class to retrieve objects across
    ///     Android / Windows / Linux / OSX and
    ///     Editor / builds
    /// </summary>
    public static class StringAssetUtil
    {
        // we may move these collections into a LeiaLoft.SupportedPlatforms HashSet<RuntimePlatforms> table later for standardization
        private static readonly HashSet<RuntimePlatform> persistentDataPathPlatforms = new HashSet<RuntimePlatform> { RuntimePlatform.Android };
        private static readonly HashSet<RuntimePlatform> dataPathPlatforms = new HashSet<RuntimePlatform> {
            RuntimePlatform.WindowsPlayer, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxPlayer };
        // next phase will be to retrieve different assets for Lumepad vs Hydrogen, Linux vs Windows, etc.

        /// <summary>
        /// Initializes static data of the StringAssetUtil.
        ///
        /// Populates the deviceAwareDataPath variable to point to correct target on Andoid/standalone/Editor.
        /// </summary>
        static string deviceAwareDataPath = Environment.CurrentDirectory;
        static StringAssetUtil()
        {
            if (persistentDataPathPlatforms.Contains(Application.platform))
            {
                // android build
                deviceAwareDataPath = Application.persistentDataPath;
            }
            else if(dataPathPlatforms.Contains(Application.platform))
            {
                // standalone build
                deviceAwareDataPath = Application.dataPath;
            }
            else
            {
                // editor; content will be loaded using Resources.Load in editor
                // LeiaLoft.StringAssetUtil will only write into Assets/LeiaLoft/Resources folder
                deviceAwareDataPath = Path.Combine(Application.dataPath, Path.Combine("LeiaLoft", "Resources"));
            }
        }

        /// <summary>
        /// <para>Writes an object to the device's file system.</para>
        ///
        /// <para>Data written with WriteJsonObjectToDeviceAwareFilename(filename) can later be retrieved with TryGetJsonObjectFromDeviceAwareFilename(filename)</para>
        /// </summary>
        /// <typeparam name="T">Type of object to convert TextAsset to</typeparam>
        /// <param name="filename">Filename of a while which was created with StringAssetUtil</param>
        /// <param name="jsonObj">An object of type T whose fields are populated by TextAsset's Json</param>
        /// <param name="performEditorResourceReload">Forces editor to reload the asset. Reloading assets can cause other Resources (like Shader properties) to be discarded. Only has an effect in editor.</param>
        /// <returns>True if successful in retrieving a Json object</returns>
        public static bool WriteJsonObjectToDeviceAwareFilename(string filename, object jsonObj, bool performEditorResourceReload)
        {
            try
            {
                string path = Path.Combine(deviceAwareDataPath, filename);
                File.WriteAllText(path, jsonObj.ToString());
#if UNITY_EDITOR
                if (performEditorResourceReload)
                {
                    string subAssetPath = path.Remove(path.IndexOf(Application.dataPath), Application.dataPath.Length);
                    // cannot use Path.Combine because path2 is /LeiaLoft/Resources. interpreted as a top-level path which cannot be prepended to
                    string localPath = string.Format("{0}{1}", "Assets", subAssetPath);

                    /// reload recently written asset. Causes some Shader Resources to also have their properties reset; use cautiously
                    UnityEditor.AssetDatabase.ImportAsset(localPath);
                }
#endif
                return true;
            }
            catch (Exception e)
            {
                return FailCase<object>(out jsonObj, "Failed to write {0} to {1}. Got error {2}", jsonObj, filename, e);
            }
        }

        /// <summary>
        /// <para>Retrieves an object from Json given a filename. Does so in a low-branching, device-aware manner.</para>
        /// 
        /// <para>On Android / standalone, first checks if we can retrieve a file from persistentDataPath / dataPath.</para>
        ///
        /// <para>If no file found in file system, OR if in editor, retrieves an object from virtual file system / Resources folder using Resources.Load</para>
        /// </summary>
        /// <typeparam name="T">Type of object to convert TextAsset to</typeparam>
        /// <param name="filename">Filename of a while which was created with StringAssetUtil</param>
        /// <param name="jsonObj">An object of type T whose fields are populated by TextAsset's Json</param>
        /// <returns>True if successful in retrieving a Json object</returns>
        public static bool TryGetJsonObjectFromDeviceAwareFilename<T>(string filename, out T jsonObj)
        {
            if (Path.GetFileName(filename) != filename)
            {
                LogUtil.Log(LogLevel.Warning, "Warning: tried to call TryGetJsonObjectFromDeviceAwareFilename with filename {0} but we need format {1}",
                    filename, Path.GetFileName(filename));
            }

            try
            {
                string pathFilename = Path.Combine(deviceAwareDataPath, filename);
                if (File.Exists(pathFilename))
                {
                    // across Android / standalone / Editor, try getting from real path first
                    return TryGetJsonObjectFromLiteralFilePath<T>(pathFilename, out jsonObj);
                }
                else
                {
                    // in editor / standalone / android, retrieve possible file from virtual file system using Resources.Load.
                    // there is no way to know whether this file could be loaded, until we try to load it
                    return TryGetJsonObjectFromTextResourceFilename<T>(filename, out jsonObj);
                }
            }
            catch (Exception e)
            {
                return FailCase<T>(out jsonObj, "Tried to get object from device {0}, with filename {1}, got error {2}", Application.platform, filename, e);
            }

        }

        /// <summary>
        /// Private function for exclusively getting from file system file path.
        /// Presumes that user has already checked that file exists and is readable.
        /// </summary>
        /// <typeparam name="T">Type of object to convert TextAsset to</typeparam>
        /// <param name="fullFilePath">Literal filepath in device's file system</param>
        /// <param name="jsonObj">An object of type T whose fields are populated by TextAsset's Json</param>
        /// <returns>True if successful in retrieving a Json object</returns>
        private static bool TryGetJsonObjectFromLiteralFilePath<T>(string fullFilePath, out T jsonObj)
        {
            try
            {
                string fileText = File.ReadAllText(fullFilePath);
                return TryGetJsonObjectFromStringData<T>(fileText, out jsonObj);
            }
            catch (Exception e)
            {
                return FailCase<T>(out jsonObj, "Failed to read {0}. Got error {1}", fullFilePath, e);
            }
        }

        /// <summary>
        /// Private function for getting object exclusively from Resources folder using Resources.Load.
        ///
        /// This function does not necessarily expect the TextAsset to exist before retrieval is attempted. When the TextAsset does not exist,
        /// this function returns false.
        /// </summary>
        /// <typeparam name="T">Type of object to convert TextAsset to</typeparam>
        /// <param name="filename">Filename of a file which was created with StringAssetUtil</param>
        /// <param name="jsonObj">An object of type T whose fields are populated by TextAsset's Json</param>
        /// <returns>True if successful in retrieving a Json object</returns>
        private static bool TryGetJsonObjectFromTextResourceFilename<T>(string filename, out T jsonObj)
        {
            // Resources.Load expects file without filename
            filename = Path.GetFileNameWithoutExtension(filename);

            try
            {
                TextAsset asset = Resources.Load<TextAsset>(filename);
                if (asset == null || string.IsNullOrEmpty(asset.text))
                {
                    // resource might not exist, in which case return false,
                    // but do not report an error because there is no expectation that this file exists
                    jsonObj = default(T);
                    return false;
                }
                else
                {
                    return TryGetJsonObjectFromTextAsset<T>(asset, out jsonObj);
                }
            }
            catch (Exception e)
            {
                return FailCase<T>(out jsonObj, "Error while trying to load from {0}: {1}", filename, e);
            }
        }

        /// <summary>
        /// Utility function for converting a TextAsset of json data into an object.
        /// </summary>
        /// <typeparam name="T">Type of object to convert TextAsset to</typeparam>
        /// <param name="orig">Original TextAsset</param>
        /// <param name="jsonObj">An object of type T whose fields are populated by TextAsset's Json</param>
        /// <returns>True if successful in retrieving a Json object</returns>
        private static bool TryGetJsonObjectFromTextAsset<T>(TextAsset orig, out T jsonObj)
        {
            try
            {
                return TryGetJsonObjectFromStringData<T>(orig.text, out jsonObj);
            }
            catch (Exception e)
            {
                return FailCase<T>(out jsonObj, "Error while trying to convert TextAsset {0} into object {1}: {2}", orig, typeof(T), e);
            }
        }

        /// <summary>
        /// <para>Public utility function for converting string json data into an object.</para>
        /// <para>Users will typically want to call </para>
        /// As long as Json call succeeds, returns true. Success does not necessarily mean that the json file was actually correctly
        /// formatted for conversion into the type given.
        /// </summary>
        /// <typeparam name="T">Type of object to convert TextAsset to</typeparam>
        /// <param name="orig">String content of a file which was created with StringAssetUtil</param>
        /// <param name="jsonObj">An object of type T whose fields are populated by TextAsset's Json</param>
        /// <returns>True if successful in retrieving a Json object</returns>
        public static bool TryGetJsonObjectFromStringData<T>(string orig, out T jsonObj)
        {
            try
            {
                jsonObj = JsonUtility.FromJson<T>(orig);
                return true;
            }
            catch (Exception e)
            {
                return FailCase<T>(out jsonObj, "Error while trying to convert string {0} into object {1}: {2}", orig, typeof(T), e);
            }
        }

        /// <summary>
        /// Factory for returning information in a consistent format.
        /// </summary>
        /// <typeparam name="T">Type that we tried to retrieve</typeparam>
        /// <param name="jsonObj">An object which could not be retrieved from json</param>
        /// <param name="formatStub">String to insert formatted data into</param>
        /// <param name="formattedInfo">Data to insert into string formatting</param>
        /// <returns>False if called</returns>
        private static bool FailCase<T>(out T jsonObj, string formatStub, params object[] formattedInfo)
        {
            jsonObj = default(T);
            LogUtil.Log(LogLevel.Error, formatStub, formattedInfo);
            return false;
        }
    }
}
