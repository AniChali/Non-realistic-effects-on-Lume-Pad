using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace LeiaLoft.Diagnostics
{
	public static class SDKStringData
	{
		public static string VersionFileName
        {
			get
			{
				return "VERSION.txt";
			}
        }

		/// <summary>
        /// Gets the content of the "VERSION.txt" file as a string
        /// </summary>
		public static string SDKVersionFileText
        {
			get
            {
				TextAsset asset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(VersionFileName));
				if (asset != null)
				{
					return asset.text;
				}
				return "";
            }
        }

		/// <summary>
        /// Gets the first line of "VERSION.txt"
        /// </summary>
		public static string SDKVersionLine
        {
			get
			{
				const int versionIndex = 0;
				string sdkVersionFileString = SDKVersionFileText;
				if (!string.IsNullOrEmpty(sdkVersionFileString))
				{
					string[] split = sdkVersionFileString.Split('\n');
					if (split != null && split.Length >= versionIndex)
					{
						return split[versionIndex];
					}
				}
				return "";
			}
        }

		/// <summary>
        /// Gets the second line of "VERSION.txt" if it exists. This is where CICD process puts the commit SHA.
        ///
        /// In SDKs which have not gone through the CICD process, returns ""
        /// </summary>
		public static string SDKVersionCommit
        {
			get
			{
				const int commitIndex = 1;
				string sdkVersionFileString = SDKVersionFileText;
				string[] split = sdkVersionFileString.Split('\n');
				if (split != null && split.Length >= commitIndex)
                {
					return split[commitIndex];
                }
				return "";
			}
        }

		/// <summary>
        /// Splits a string, then tries to parse tokens of the string as ints
        /// </summary>
        /// <param name="mstr">A string</param>
        /// <returns>A collection of integers which were found in the string</returns>
		public static List<uint> ParseAsSemantic(string mstr)
		{
			List<uint> versionInts = new List<uint>();
			string[] semanticIntStrings = mstr.Split(' ', '.', '-', '\n');

			// parse strings as potential ints, add them to a collection
			foreach (string token in semanticIntStrings)
			{
				int val;
				if (!string.IsNullOrEmpty(token) && int.TryParse(token, out val))
				{
					versionInts.Add((uint) val);
				}
			}
			return versionInts;
		}

		/// <summary>
        /// Gets a list of integers in the VERSION.txt file's first line
        /// </summary>
		public static List<uint> SDKVersionSemantic
        {
			get
            {
				return ParseAsSemantic(SDKVersionLine);
			}
        }

		/// <summary>
        /// Test whether the given semantic version number is greater than or equal to the desired semantic version number
        /// </summary>
        /// <param name="basePlatform">A collection of positive integers to start from</param>
        /// <param name="desiredPlatform">A collection of positive integers to test for equality or surplus to</param>
        /// <returns>True if any of the elements of basePlatform from left to right are greater than or equal to to corresponding element of desiredPlatform</returns>
		public static bool IsSemanticPlatformGreaterThanOrEqualTo(List<uint> basePlatform, List<uint> desiredPlatform)
        {
			if (basePlatform == null || desiredPlatform == null)
			{
				return false;
			}
			for (int i = 0; i < desiredPlatform.Count && i < basePlatform.Count; i++)
			{
				if (desiredPlatform[i] > basePlatform[i])
				{
					return false;
				}
				else if (desiredPlatform[i] < basePlatform[i])
                {
					return true;
				}
			}
			// if we got to here, must have had equality in every testable element
			// just handle case where one platform has more positive params than another
			return basePlatform.Count >= desiredPlatform.Count;
		}
	}
}
