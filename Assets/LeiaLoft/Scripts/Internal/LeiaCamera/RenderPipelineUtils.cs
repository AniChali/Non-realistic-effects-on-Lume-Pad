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
using UnityEngine.Rendering;

namespace LeiaLoft
{
    public static class RenderPipelineUtils
    {
#if UNITY_2020_2_OR_NEWER
        static readonly string assetTypeURP = "UniversalRenderPipelineAsset";
        static readonly string assetTypeHDRP = "HDRenderPipelineAsset";
        static string renderAssetName;

#if LEIA_URP_DETECTED
        /// <summary>
        /// For a URP asset with multiple Renderers (renderingPahs) in its Renderer List, gets the active Renderer index
        /// </summary>
        /// <param name="camData">Universal camera data</param>
        /// <returns>Index of URP renderer path if possible; -1 otherwise</returns>
        public static int GetRendererIndex(this UnityEngine.Rendering.Universal.UniversalAdditionalCameraData camData)
        {
            try
            {
                return (int)typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData)
                    .GetField("m_RendererIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(camData);
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "Could not get renderer index. Got error {0}", e);
                return -1;
            }
        }
#endif

        /// <summary>
        /// Retreives the renderPipelineAsset which users can specify in Edit :: Project settings :: Graphics settings :: render asset.
        ///
        /// Checks it against known pipelines which the LeiaLoft Unity SDK provides preliminary support for.
        /// </summary>
        /// <returns>True if renderPipeline is non-null and tested with the LeiaLoft Unity SDK</returns>
        public static bool IsUnityRenderPipeline()
        {
            if (GraphicsSettings.renderPipelineAsset == null) { return false; }
            renderAssetName = GraphicsSettings.renderPipelineAsset.GetType().Name;
            return (renderAssetName.Contains(assetTypeURP) || renderAssetName.Contains(assetTypeHDRP));
        }

        /// <summary>
        /// Retreives the renderPipelineAsset which users can specify in Edit :: Project settings :: Graphics settings :: render asset.
        ///
        /// Checks it against Universal Render Pipeline
        /// </summary>
        /// <returns>True if renderPipelineAsset in graphics Settings is Universal render Pipeline</returns>
        public static bool IsUniversalRenderPipeline()
        {
            return IsUnityRenderPipeline() && renderAssetName == assetTypeURP;
        }

        /// <summary>
        /// Retreives the renderPipelineAsset which users can specify in Edit :: Project settings :: Graphics settings :: render asset.
        ///
        /// Checks it against Universal Render Pipeline
        /// </summary>
        /// <returns>True if renderPipelineAsset in graphics Settings is Universal render Pipeline</returns>
        public static bool IsHDRenderPipline()
        {
            return IsUnityRenderPipeline() && renderAssetName == assetTypeHDRP;
        }
#endif
    }
}
