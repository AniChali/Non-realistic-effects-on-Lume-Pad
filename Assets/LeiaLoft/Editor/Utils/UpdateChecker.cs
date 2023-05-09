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
using System.Text;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using LeiaLoft.Diagnostics;
using System.Text.RegularExpressions;

namespace LeiaLoft
{

    [InitializeOnLoad]
    public static class UpdateChecker
    {
        private const string VersionPage = "https://leiainc.github.io/";

        const string sdkDownloadFallbackLink = "https://www.leiainc.com/sdk";
        const string sdkDownloadFallbackVersion = "0.0.0";
        const string sdkPatchnotesFallback = "Missing!";

        private static IEnumerator _pageLoading;

        // set by LeiaAboutWindow. also see SDKStringData.SDKVersionSemantic
        public static string CurrentSDKVersion { get; set; }
        public static string LatestSDKVersion { get; private set; }
        public static string Patchnotes { get; private set; }

        [Obsolete("Deprecated in 0.6.20. Scheduled for removal in 0.6.22. Use UpdateChecker.SDKDownloadLink")]
        public static string SDKDownlad
        {
            get
            {
                return SDKDownloadLink;
            }
        }

        public static string SDKDownloadLink { get; private set; }
        public static bool UpdateChecked { get; private set; }

        public static void CheckForUpdates()
        {
            UpdateChecked = false;

            _pageLoading = CheckUpdateDone();
            EditorApplication.update += UpdateWebRequest;
        }

        public static bool CheckUpToDate()
        {
            if (!String.IsNullOrEmpty(CurrentSDKVersion) && !String.IsNullOrEmpty(LatestSDKVersion))
            {
                List<uint> currentSDKVersionSem = SDKStringData.ParseAsSemantic(CurrentSDKVersion);
                List<uint> latestSDKVersionSem = SDKStringData.ParseAsSemantic(LatestSDKVersion);
                return SDKStringData.IsSemanticPlatformGreaterThanOrEqualTo(currentSDKVersionSem, latestSDKVersionSem);
            }
            return false;
        }

        private static IEnumerator CheckUpdateDone()
        {
            UnityWebRequest www = UnityWebRequest.Get(VersionPage);
            www.SendWebRequest();
            while (!www.downloadHandler.isDone)
            {
                yield return null;
            }

#if UNITY_2020_2_OR_NEWER
            if(www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
#else
            if (www.isHttpError || www.isNetworkError)
#endif
            {
                Debug.Log(www.error);
            }
            else
            {
                string webPageData = www.downloadHandler.text;
                GetVersionInfo(webPageData);
            }
            UpdateChecked = true;
        }

        private static void UpdateWebRequest()
        {
            if (_pageLoading.MoveNext())
            {
                //Do nothing, iterate the IEnumerator
            }
            else
            {
                EditorApplication.update -= UpdateWebRequest;
            }
        }

        //Parses the webpage html to find the version number and patch notes
        private static void GetVersionInfo(string data)
        {

            // find
            // unity_sdk_link_begin
            // non-newlines, followed by newlines (aka regex through multiple lines)
            // unity_sdk_link_end
            Regex reUnityDownloadLink = new Regex("unity_sdk_link_begin\\s*--->\\s*([^\\n]*\\n)*<!---\\s*unity_sdk_link_end", RegexOptions.Compiled);
            Match matchUnityDownloadLink = reUnityDownloadLink.Match(data);

            if (matchUnityDownloadLink.Success)
            {
                // find link to Unity SDK, as well as the text / content that is displayed to user in link
                Regex reUnityDownloadLinkContent = new Regex("<a href=\"(?<linkUnitySDKLatest>.*)\">(?<linkContent>.+)</a>");
                Match matchUnityDownloadLinkContent = reUnityDownloadLinkContent.Match(matchUnityDownloadLink.Value);
                if (matchUnityDownloadLinkContent.Success)
                {
                    SDKDownloadLink = matchUnityDownloadLinkContent.Groups["linkUnitySDKLatest"].Value;

                    // given linkContent containing some chars, numbers, and dots, we just want the numbers and dots
                    LatestSDKVersion = Regex.Replace(matchUnityDownloadLinkContent.Groups["linkContent"].Value, "[^(0-9.)]", "");
                }
                else
                {
                    SDKDownloadLink = sdkDownloadFallbackLink;
                    LatestSDKVersion = sdkDownloadFallbackVersion;
                    LogUtil.Log(LogLevel.Error, "Failed to get SDK download link");
                }
            }
            else
            {
                SDKDownloadLink = sdkDownloadFallbackLink;
                LatestSDKVersion = sdkDownloadFallbackVersion;
                LogUtil.Log(LogLevel.Error, "Failed to get SDK download link");
            }
            // "SDKDownlad" has now been populated
            // "LatestSDKVersion" has now been populated

            // find
            // unity_sdk_text_description_begin
            // content, followed by newlines, captured in a variable
            // unity_sdk_text_description_end
            Regex reUnityTextDescription = new Regex("unity_sdk_text_description_begin\\s*--->\\s*(?<patchNotesContent>(.*\\n)*)<!---\\s*unity_sdk_text_description_end");
            Match matchUnityTextDescription = reUnityTextDescription.Match(data);
            if (matchUnityTextDescription.Success)
            {
                // get text with embedded HTML
                string htmlPatchnotesContent = matchUnityTextDescription.Groups["patchNotesContent"].Value;

                // remove html tags of format <content> from text.
                // ? makes query halt on first match so that just a tag like <li> is matched.
                // otherwise our query matches sequences of HTML tags like <li>content</li>
                Patchnotes = Regex.Replace(htmlPatchnotesContent, "<.*?>", "");
            }
            else
            {
                Patchnotes = sdkPatchnotesFallback;
            }
            // populates "Patchnotes"
        }
    }
}
