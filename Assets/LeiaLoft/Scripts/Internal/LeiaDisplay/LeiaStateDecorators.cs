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
using UnityEngine;
using System;

namespace LeiaLoft
{
    /// <summary>
    /// Changeable settings that affect rendering.
    /// </summary>
    [Serializable]
    public struct LeiaStateDecorators
    {
        public bool ShowTiles { get; set; }
        public bool ShowCalibration { get; set; }
        [SerializeField] private bool parallax_tracker;
        public bool ParallaxAutoRotation
        {
            get { return parallax_tracker; }
            set { parallax_tracker = value; }
        }
        public bool ShouldParallaxBePortrait { get; set; }
        [SerializeField] private bool alphaBlendingFlag;
        public bool AlphaBlending {
            get { return alphaBlendingFlag; }
            set { alphaBlendingFlag = value; }
        }
        public float[] DeltaXArray { get; set; }

        /// <summary>
        /// Modifies flag which is read in AbstractLeiaStateTemplate.UpdateEmissionPattern. Defines whether view order shifts as
        /// FOV shifts. This supports usage where a dev wants to track viewer position and avoid showing view jumps as the user moves horizontally.
        ///
        /// Default false, which case FOV shifts camera position but does not shift view order
        /// </summary>
        [SerializeField] private bool _viewPeelEnabled;
        public bool ViewPeelEnabled
        {
            get
            {
                return _viewPeelEnabled;
            }
            set
            {
                _viewPeelEnabled = value;
            }
        }

        public ParallaxOrientation ParallaxOrientation
        {
            get
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                if (!ParallaxAutoRotation)
                {
                    return ParallaxOrientation.Landscape;
                }
                else if (ShouldParallaxBePortrait)
                {
                    return DisplayInfo.IsLeiaDisplayInPortraitMode ? ParallaxOrientation.Landscape : ParallaxOrientation.Portrait;
                }
                else
                {
                    return !DisplayInfo.IsLeiaDisplayInPortraitMode ? ParallaxOrientation.Landscape : ParallaxOrientation.Portrait;
                }
                #else
                return ParallaxOrientation.Landscape;
                #endif
            }
        }
        [SerializeField] private LeiaDisplay.RenderTechnique _renderTechnique;
        public LeiaDisplay.RenderTechnique RenderTechnique
        {
            set { _renderTechnique = value; }
            get { return _renderTechnique; }
        }
        public Vector2 AdaptFOV { get; set; }

        public static LeiaStateDecorators Default
        {
            get
            {
                // when new LeiaStateDecorators() default is constructed, it has AlphaBlending == false
                // AbstractLeiaStateTemplate :: CreateMaterial pulls this default false value
                var Decos = new LeiaStateDecorators();
                // default this value to 1.0f (fully 3D), rather than default(float) = 0.0f
                Decos._backgroundInterpolationCoefficient = 1.0f;
                return Decos;
            }
        }

        /// <summary>
        /// Albedo pixels for a higher-resolution texture which we can mask in. Onscreen pixel[uv.xy] = a weighted mixture of interlaced 3D pixel and backgroundAlbedo[uv.xy]
        /// </summary>
        [SerializeField] public Texture backgroundAlbedoTexture{ get; set; }

        /// <summary>
        /// Alpha channel of this texture defines the interpolation between a full-resolution 2D pixel and a 3D interlaced pixel.
        /// (Assuming backgroundInterpolationCoefficient is 1.0) mask[uv.xy].a == 0 - fully 2D at uv.xy. mask[uv.xy].a == 1 - fully 3D at uv.xy. When unset, expect behavior as if value is 1
        /// </summary>
        [SerializeField] public Texture backgroundMaskTexture { get; set; }

        /// <summary>
        /// 0 - fully 2D. 1 - fully 3D. Default 1. This value can vary independently of the backgroundMask's alpha channel
        /// </summary>
        [SerializeField] private float _backgroundInterpolationCoefficient;
        public float backgroundInterpolationCoefficient {
            get
            {
                return _backgroundInterpolationCoefficient;
            }
            set
            {
                _backgroundInterpolationCoefficient = Mathf.Clamp01(value);
            }
        }

        public override string ToString()
        {
            return string.Format("[LeiaStateDecorators: ShowTiles={0}, ShowCalibration={1},"
                + "ParallaxAutoRotation={2}, ShouldParallaxBePortrait={3}, ParallaxOrientation={4}, RenderTechnique={5}, AdaptFOV={6}, AlphaBlending={7}, backgroundInterpolationCoefficient={8}]",
                ShowTiles, ShowCalibration, ParallaxAutoRotation, ShouldParallaxBePortrait, ParallaxOrientation, RenderTechnique, AdaptFOV, AlphaBlending, backgroundInterpolationCoefficient);
        }
    }
}