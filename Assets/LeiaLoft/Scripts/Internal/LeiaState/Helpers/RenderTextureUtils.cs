using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
	/// <summary>
    /// Utility functions on RenderTextures
    /// </summary>
	public static class RenderTextureUtils
	{
		// initialized in constructor. We want to load this file once, and use it to control properties on some RenderTextures that the Leia Unity SDK instantiates
		private static readonly JsonParamCollection intermediateRenderTextureProperties;
		private static readonly JsonParamCollection leiaViewRenderTextureProperties;

		// does file read once of IntermediateRenderTextureProperties.json
		static RenderTextureUtils()
        {
			const string intermediateFilename = "RenderTexturePropertiesIntermediate.json";
			bool successIntermediate = StringAssetUtil.TryGetJsonObjectFromDeviceAwareFilename(intermediateFilename,
				out intermediateRenderTextureProperties);
			Debug.AssertFormat(successIntermediate, "Expected Resource {0} but loading this file failed.", intermediateFilename);
			Debug.Assert(intermediateRenderTextureProperties != null);

			const string leiaViewFilename = "RenderTexturePropertiesLeiaView.json";
			bool successLeiaView = StringAssetUtil.TryGetJsonObjectFromDeviceAwareFilename(leiaViewFilename,
				out leiaViewRenderTextureProperties);
			Debug.AssertFormat(successLeiaView, "Expected Resource {0} but loading this file failed.", leiaViewFilename);
			Debug.Assert(leiaViewRenderTextureProperties != null);
		}

		/// <summary>
        /// Sets aniso level, filter mode, and anti-aliasing on this RenderTexture to the Leia Unity SDK-defined defaults.
        ///
        /// You always Apply RenderTexture.properties, and only once finished should you call RenderTexture.Create()
        /// </summary>
        /// <param name="intermediateRT">A RenderTexture to change properties on</param>
		public static void ApplyIntermediateTextureRecommendedProperties(this RenderTexture intermediateRT)
        {
			intermediateRT.anisoLevel = intermediateRenderTextureProperties.GetSingle<int>("anisoLevel");
			intermediateRT.filterMode = intermediateRenderTextureProperties.GetSingle<FilterMode>("filterMode");
			intermediateRT.antiAliasing = intermediateRenderTextureProperties.GetSingle<int>("antiAliasing");
		}

		/// <summary>
		/// Sets depth bits on this RenderTexture to the Leia Unity SDK-defined defaults for LeiaViews.
		///
		/// You should always call ApplyLeiaRecommendedProperties and ApplyLeiaViewRecommendedProperties before calling .Create.
		/// </summary>
		/// <param name="intermediateRT">A RenderTexture to change properties on</param>
		public static void ApplyLeiaViewRecommendedProperties(this RenderTexture intermediateRT)
        {
			intermediateRT.depth = leiaViewRenderTextureProperties.GetSingle<int>("depthBits");
        }

		// cache for short-circuting lookups
		static readonly Dictionary<int, int[]> lengthColRowLookupTable = new Dictionary<int, int[]>();

		/// <summary>
        /// Utility function for converting a single int into a col-by-row pair
        /// </summary>
        /// <param name="length">An integer to fit to a square-ish shape</param>
        /// <returns>A (col,row) pair of integers where first element <= second element</returns>
		public static int[] LengthAsColsRows(int length)
		{
			if (!lengthColRowLookupTable.ContainsKey(length))
            {
				// compute cols and rows for length
				// for length = 2,3 we get 1col2row or 1col3row structures. this may be unexpected to some users who were expecting len 2 = 2x1 or len 3 = 2x2

				int[] cr = new int[2];
				// ensure cols > 0 so we do not get divide-by-zero errors
				cr[0] = Mathf.Max(1, Mathf.FloorToInt(Mathf.Sqrt(length)));
				cr[1] = Mathf.CeilToInt(length * 1.0f / cr[0]);

				lengthColRowLookupTable[length] = cr;
			}

			return lengthColRowLookupTable[length];
		}

		/// <summary>
		/// Given a renderTexture, returns a larger renderTexture which can fit a [col]x[row] tiled image into it
		/// </summary>
		/// <param name="original">Original RT</param>
		/// <param name="targetLength">Cols x Rows</param>
		/// <returns>A RenderTexture which can have original repeated in it as tiles</returns>
		public static RenderTexture GetColsxRowsRenderTexture(this RenderTexture original, int targetLength)
        {
			int[] crCounts = LengthAsColsRows(targetLength);

			// see LeiaView :: CreateRenderTexture
			RenderTexture tile = new RenderTexture(original.width * crCounts[0], original.height * crCounts[1], original.depth, original.format)
			{
				anisoLevel = original.anisoLevel,
				filterMode = original.filterMode,
				antiAliasing = original.antiAliasing
			};
			tile.Create();
			return tile;
		}

		/// <summary>
		/// Given a target RenderTexture, copies several RenderTextures into that RenderTexture
		/// </summary>
		/// <param name="rtTarget">A RenderTexture to copy pixels into</param>
		/// <param name="rtTiles">A collection of RenderTextures to copy into rtTarget</param>
		/// <returns>The populated rtTarget</returns>
		public static void CopyTiledContentInto(this List<RenderTexture> rtTiles, RenderTexture rtTarget)
		{
			if ((int)(SystemInfo.copyTextureSupport & UnityEngine.Rendering.CopyTextureSupport.Basic) == 0)
			{
				LogUtil.Log(LogLevel.Error, "Device {0}::{1} has copyTextureSupport {2} which is incompatible with copy needs", SystemInfo.graphicsDeviceType, SystemInfo.graphicsDeviceVersion, SystemInfo.copyTextureSupport);
			}
			if (rtTiles == null || rtTiles[0] == null)
			{
				LogUtil.Log(LogLevel.Warning, "Cannot make tiled image out of an empty collection");
			}

			int[] crCounts = LengthAsColsRows(rtTiles.Count);

			if (rtTarget == null || rtTarget.width < rtTiles[0].width * crCounts[0] || rtTarget.height < rtTiles[0].height * crCounts[1])
            {
				// rtTarget should have been created with GetColsxRowsRenderTexture once
				// we don't want to continuously generate new RenderTextures or temp RenderTextures as this causes too much garbage collection
				LogUtil.Log(LogLevel.Error, "Needed to copy into a texture of dimensions {0} x {1} but target rtTarget was null or too small", rtTiles[0].width * crCounts[0], rtTiles[0].height * crCounts[1]);
            }

			// copy pixels of rtTiles into subsections of rtTarget
			for (int i = 0; i < rtTiles.Count; ++i)
            {
				int xInd = i % crCounts[0];
				int yInd = crCounts[1] - 1 - i / crCounts[0];

				Graphics.CopyTexture(rtTiles[i], 0, 0, 0, 0,
					rtTarget.width / crCounts[0], rtTarget.height / crCounts[1], rtTarget, 0, 0,
					xInd * (rtTarget.width / crCounts[0]), yInd * (rtTarget.height / crCounts[1]));
			}
        }

		/// <summary>
        /// Function for slowly copying pixels from a RenderTexture into a new Texture2D using ReadPixels
        /// </summary>
        /// <param name="rtSource">A RenderTexture which provides origin pixels</param>
        /// <returns>A new Texture2D with pixels from the rtSource</returns>
		public static Texture2D AsTexture2D(RenderTexture rtSource)
        {
			Debug.AssertFormat(rtSource != null, "In {0} tried to convert RenderTexture to Texture2D but provided RenderTexture rtSource was null", typeof(RenderTextureUtils));
			Texture2D texOut = new Texture2D(rtSource.width, rtSource.height, TextureFormat.ARGB32, false, false);

			RenderTexture prev = RenderTexture.active;
			RenderTexture.active = rtSource;
			texOut.ReadPixels(new Rect(0, 0, rtSource.width, rtSource.height), 0, 0);
			// data moved from RenderTexture into Texture2D

			RenderTexture.active = prev;

			return texOut;
        }
	}
}
