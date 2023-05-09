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
    /// <summary>
    /// ILeiaState implementation of common methods (independent of display type)
    /// </summary>
    public abstract class AbstractLeiaStateTemplate : ILeiaState, IReleasable
    {
        private const string SeparateTilesNotSupported = "RenderToSeparateTiles is not supporting more than 16 LeiaViews.";
        private const string overflowTextureName = "_texture_overflow";
        private const string overflowTextureColName = "_texture_overflow_cols";
        private const string overflowTextureRowName = "_texture_overflow_rows";

        protected DisplayConfig _displayConfig;
        protected int _viewsWide;
        protected int _viewsHigh;
        protected int _displayHorizontalViewCount;
        protected int _displayVerticalViewCount;
        protected float _deltaView;
        protected int _backlightMode;
        protected Material _material;
        protected string _shaderName;
        protected string _transparentShaderName;
        private Vector2[] _emissionPattern;
        protected readonly RenderTexture interlacedAlbedoTexture;
        // overflowTexture contains additional tiled views in a c x r tiled image. this avoids the 16-texture read limit when we need to retrieve 16+ views + MainTex + alphaMask
        private RenderTexture overflowTexture;
        // current texture slot is fixed at 12. Check interlacing shaders / cgincs to confirm
        const int overflowLimit = 12;
        // maintain a collection of RenderTextures so that we can fall back and composite them into views
        private readonly List<RenderTexture> overflowUntiledTextures = new List<RenderTexture>();
        private string _viewBinPattern;
        public string GetViewBinPattern()
        {
            return _viewBinPattern;
        }

        public AbstractLeiaStateTemplate(DisplayConfig displayConfig)
        {
            _displayConfig = displayConfig;

            interlacedAlbedoTexture = new RenderTexture(_displayConfig.UserPanelResolution.x, _displayConfig.UserPanelResolution.y, 0)
            {
                name = "interlacedAlbedo"
            };
            interlacedAlbedoTexture.ApplyIntermediateTextureRecommendedProperties();
            interlacedAlbedoTexture.Create();
        }

        public virtual void SetViewCount(int viewsWide, int viewsHigh)
        {
            this.Debug(string.Format("SetViewCount( {0}, {1})", viewsWide, viewsHigh));
            _viewsWide = viewsWide;
            _viewsHigh = viewsHigh;
        }

        public void SetBacklightMode(int modeId)
        {
            _backlightMode = modeId;
        }

        public void SetShaderName(string shaderName, string transparentShaderName)
        {
            this.Debug(string.Format("SetShaderName( {0}, {1})", shaderName, transparentShaderName));
            _shaderName = shaderName;
            _transparentShaderName = transparentShaderName;
        }

        public abstract void GetFrameBufferSize(out int width, out int height);

        public abstract void GetTileSize(out int width, out int height);

        protected virtual Material CreateMaterial(bool alphaBlending)
        {
            var shaderName = alphaBlending ? _transparentShaderName : _shaderName;
            return new Material(Resources.Load<Shader>(shaderName));
        }

        public virtual void DrawImage(LeiaCamera camera, LeiaStateDecorators decorators)
        {
            if (_material == null)
            {
                this.Trace("Creating material");
                _material = CreateMaterial(decorators.AlphaBlending);
            }

            _material.SetFloat("_viewRectX",camera.Camera.rect.x);
            _material.SetFloat("_viewRectY",camera.Camera.rect.y);
            _material.SetFloat("_viewRectW",camera.Camera.rect.width);
            _material.SetFloat("_viewRectH",camera.Camera.rect.height);

            if (_viewsHigh * _viewsWide > 16)
            {
                throw new NotSupportedException(SeparateTilesNotSupported);
            }

            // covers span from 0 to min(displayViewCount, 12)
            for (int i = 0; i < _displayHorizontalViewCount && i < overflowLimit; i++)
            {
                // normalize to 0...1 by dividing by _displayHorizontalViewCount
                // stretch out across views view_0...view_N by multiplying by camera.GetViewCount

                // as i spans 0..._displayHorizontalViewCount, viewIndex spans 0...MaxLeiaViewIndex
                int viewIndex = (int)(i * 1f * camera.GetViewCount() / _displayHorizontalViewCount);
                // since the for loop i runs for i = 0 AND i < horizontalViewCount, we do not have to worry about divide by zero case where _displayHorizontalViewCount == 0

                _material.SetTexture("_texture_" + i, camera.GetView(viewIndex).TargetTexture);
            }

            // if exists, covers span from 12 - numViews. create a collection of RTs to create a tiled RT out of
            for (int i = overflowLimit; i < _displayConfig.NumViews.x; ++i)
            {
                if (i == overflowLimit)
                {
                    overflowUntiledTextures.Clear();
                }

                int viewIndex = (int)(i * (_displayConfig.UserNumViews.x * 1f / _displayConfig.NumViews.x));
                overflowUntiledTextures.Add(camera.GetView(viewIndex).TargetTexture);
            }

            // move RT pixels from individual RTs into one tiled RT
            if (overflowUntiledTextures.Count > 0)
            {
                if (overflowTexture == null)
                {
                    overflowTexture = overflowUntiledTextures[0].GetColsxRowsRenderTexture(overflowUntiledTextures.Count);
                }
                int[] crCounts = RenderTextureUtils.LengthAsColsRows(overflowUntiledTextures.Count);

                overflowUntiledTextures.CopyTiledContentInto(overflowTexture);
                _material.SetTexture(overflowTextureName, overflowTexture);
                _material.SetFloat(overflowTextureColName, crCounts[0]);
                _material.SetFloat(overflowTextureRowName, crCounts[1]);
            }

            // all templates run this line
            // Square and Slanted use it to interlace using an interlacing _material.
            // Abstract uses it to copy data from _texture_0 to template_renderTexture because _material is TWO_DIM shader, i.e. a simple pixel-copy shader
            Graphics.Blit(Texture2D.whiteTexture, interlacedAlbedoTexture, _material);

            // silence a warning RE UnityEditor on OSX when build target is Android and hw emulation is OpenGL ES 3.0.
            // error is Tiled GPU perf. warning: RenderTexture color surface (wxh) was not cleared/discarded. See TiledGPUPerformanceWarning.ColorSurface label in Profiler for info
#if UNITY_EDITOR_OSX
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                interlacedAlbedoTexture.DiscardContents();
            }
#endif

            // Square and Slanted perform additional blits to screen in their override DrawImage classes

            // Square and Slanted are excluded from this line because their _material.name is not OpaqueShaderName or TransparentShaderName
            if (_material != null && !string.IsNullOrEmpty(_material.name) && (_material.name.Equals(TwoDimLeiaStateTemplate.OpaqueShaderName) || _material.name.Equals(TwoDimLeiaStateTemplate.TransparentShaderName)))
            {
                // AbstractLeiaStateTemplate uses this line to copy 2D view data from template_renderTexture to screen
                Graphics.Blit(interlacedAlbedoTexture, Camera.current.activeTexture);
            }
        }

        //potentially ready for removal
        private void DrawQuad(LeiaStateDecorators decorators)
        {
            GL.PushMatrix();
            GL.LoadOrtho();
            _material.SetPass(0);
            GL.Begin(GL.QUADS);

            int o = 1;
            int z = 0;

            if (decorators.ParallaxOrientation.IsInv())
            {
                o = 0;
                z = 1;
            }

            GL.TexCoord2(z, z); GL.Vertex3(0, 0, 0);
            GL.TexCoord2(z, o); GL.Vertex3(0, 1, 0);
            GL.TexCoord2(o, o); GL.Vertex3(1, 1, 0);
            GL.TexCoord2(o, z); GL.Vertex3(1, 0, 0);

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// A method for transferring properties from the LeiaStateDecorator to the interlacing _material shader.
        /// </summary>
        /// <param name="decorators">A collection of properties that should be updated at StateTemplate :: UpdateState time</param>
        protected void SetInterlacedBackgroundPropertiesFromDecorators(LeiaStateDecorators decorators)
        {
            if (_material == null)
            {
                return;
            }

            const string shaderBackgroundAlbedoTexturePropertyName = "_texture_background_albedo";
            const string shaderBackgroundMaskTexturePropertyName = "_texture_background_alphamask";
            const string shaderBackgroundGlobalAlphamaskPropertyName = "_texture_background_global_alphamask";
            const string shaderBackgroundAlphamaskKeyword = "LEIALOFT_INTERPOLATION_MASK_TEXTURE";
            const string shaderBackgroundAlbedoKeyword = "LEIALOFT_INTERPOLATION_ALBEDO_TEXTURE";

            if (decorators.backgroundAlbedoTexture != null)
            {
                _material.EnableKeyword(shaderBackgroundAlbedoKeyword);
                if (decorators.backgroundMaskTexture != null)
                {
                    // if mask is non-null, enable the mask keyword so we can do texture reads from the mask texture in shader
                    _material.EnableKeyword(shaderBackgroundAlphamaskKeyword);
                    _material.SetTexture(shaderBackgroundMaskTexturePropertyName, decorators.backgroundMaskTexture);
                }
                else
                {
                    _material.DisableKeyword(shaderBackgroundAlphamaskKeyword);
                }

                // we should always have an albedo and a general interpolation coefficient
                _material.SetTexture(shaderBackgroundAlbedoTexturePropertyName, decorators.backgroundAlbedoTexture);
                _material.SetFloat(shaderBackgroundGlobalAlphamaskPropertyName, decorators.backgroundInterpolationCoefficient);
            }
            else
            {
                _material.DisableKeyword(shaderBackgroundAlbedoKeyword);
                // when the texture is null, set the general interpolation coefficient to 1.0f
                _material.SetFloat(shaderBackgroundGlobalAlphamaskPropertyName, 1.0f);
            }
        }

        /// <summary>
        /// Given a State with a _displayConfig.NumViews, decorators which may determine stereo, and a device which may be in a particular orientation:
        /// - write into _displayConfig.UserNumViews,
        /// - track the orientation-aware device view counts in _display...ViewCount, and
        /// - call SetViewCount.
        /// </summary>
        /// <param name="decorators">Contextual info regarding the LeiaState. Relevant part here is RenderTechnique</param>
        /// <param name="device">A device with a view count in an orientation, and an orientation</param>
        protected void SetUserNumViewsFromDecoratorsAndDevice(LeiaStateDecorators decorators, ILeiaDevice device)
        {
            // this code covers combinations of
            // {stereo default device view count} x
            // {portrait, landscape orientation} x
            // {n x m horizontal parallax, including n x n "square" devices}

            _displayConfig.UserNumViews = new XyPair<int>(1, 1);
            _displayVerticalViewCount = 1;
            if (device.IsScreenOrientationLandscape())
            {
                _displayConfig.UserNumViews.x = _displayConfig.NumViews.x;
                _displayHorizontalViewCount = _displayConfig.NumViews.x;
            }
            else
            {
                _displayConfig.UserNumViews.x = _displayConfig.NumViews.y;
                _displayHorizontalViewCount = _displayConfig.NumViews.y;
            }

            if (decorators.RenderTechnique == LeiaDisplay.RenderTechnique.Stereo)
            {
                // if opted into stereo, then view count can decrease to 2.
                // but if it was already 1 due to orientation, then it cannot go to 2
                _displayConfig.UserNumViews.x = Mathf.Min(_displayConfig.UserNumViews.x, 2);
            }

            // make the final call to SetViewCount. this determines rendered LeiaView grid dimensions
            SetViewCount(_displayConfig.UserNumViews.x, _displayConfig.UserNumViews.y);

            // _displayHorizontalViewCount contains display horizontal view count
            // _displayConfig.NumViews contains (landscape view count, portrait view count) regardless of orientation
            // _displayConfig.UserNumViews contains (horizontal-or-stereo-view-count-or-1, 1) and is orientation-aware
            // _viewsWide contains UserNumViews.x, _viewsHigh contains UserNumViews.y
        }

        protected void SetShaderSubpixelKeywordsFromMatrix(Matrix4x4 interlacingMatrix)
        {
            const string shaderSubpixelKeyword = "LEIA_INTERLACING_SUBPIXEL";
            float uvyRate = interlacingMatrix[3, 1];
            float rgbRate = interlacingMatrix[3, 2];

            if (Mathf.Approximately(uvyRate, 0f) && Mathf.Approximately(rgbRate, 0f))
            {
                // is not subpixel.
                // currently only support interlacing as a function of uv.x, or uv.x and uv.y and RGB
                _material.DisableKeyword(shaderSubpixelKeyword);
            }
            // later we need to have another case for uv.x and uv.y but not rgb
            else
            {
                // is subpixel. use the more intensive rendering steps
                _material.EnableKeyword(shaderSubpixelKeyword);
            }
        }

        protected virtual int YOffsetWhenInverted()
        {
            return 0;
        }

        protected virtual int XOffsetWhenInverted()
        {
            return 0;
        }

        protected void RespectOrientation(LeiaStateDecorators decorators)
        {
            if (_viewsWide == _viewsHigh)
            {
                return;
            }

            var wide = _viewsWide > _viewsHigh;

            if (decorators.ParallaxOrientation.IsLandscape() != wide)
            {
                var tmp = _viewsWide;
                _viewsWide = _viewsHigh;
                _viewsHigh = tmp;
            }
        }

#region view_peeling_code
        private float getNxf(LeiaStateDecorators decorators, int nx)
        {
            int xPeel = (int)decorators.AdaptFOV.x;
            int initialNxf = nx;
            int terminalNxf = nx;
            if (xPeel > 0.0f)
            {
                if (initialNxf < xPeel)
                {
                    int offset = (xPeel - initialNxf + _viewsWide) / _viewsWide * _viewsWide;
                    terminalNxf = initialNxf + offset;
                }
            }
            else if (xPeel < 0.0f)
            {
                if (initialNxf >= _viewsWide + xPeel)
                {
                    int offset = (-xPeel + initialNxf) / _viewsWide * _viewsWide;
                    terminalNxf = initialNxf - offset;
                }
            }
            return terminalNxf;
        }

        // viewConfig defines the rate at which parallax views are shifted. further from 4 -> faster parallax shift for same adaptFOV.x
        private readonly int[] viewConfig = new [] { 2, 2, 2, 2, 6, 6, 6, 6 };
        private float getNxfForStereo(LeiaStateDecorators decorators, int nx)
        {
            return viewConfig[getShiftPosition(decorators, nx)];
        }

        public int getShiftPosition(LeiaStateDecorators decorators, int nx)
        {
            int xPeel = (int)decorators.AdaptFOV.x;
            int xShift = nx + Mathf.Abs(xPeel);
            int position = (xShift) % viewConfig.Length;
            return position;
        }
#endregion 

        protected void UpdateEmissionPattern(LeiaStateDecorators decorators)
        {
            _emissionPattern = new Vector2[_viewsWide * _viewsHigh];
            float offsetX = -0.5f * (_viewsWide - 1.0f);
            float offsetY = -0.5f * (_viewsHigh - 1.0f);
            float[] nxfs = new float[_viewsWide];
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            for (int ny = 0; ny < _viewsHigh; ny++)
            {
                for (int nx = 0; nx < _viewsWide; nx++)
                {
                    float nxf;
                    if (decorators.ViewPeelEnabled)
                    {
                        // parallax shift cycles cameras
                        nxf = getNxf(decorators, nx);
                    }
                    else
                    {
                        // parallax shift is a function of FOV and camera x position in array
                        nxf = decorators.AdaptFOV.x + nx;
                    }

                    nxfs[nx] = nxf;
                    float nyf = ny;

                    _emissionPattern[nx + ny * _viewsWide] = new Vector2(offsetX + nxf, offsetY + nyf);
                }
            }

            for (int i = 0; i < nxfs.Length; i++)
            {
                stringBuilder.AppendFormat("{0:0.}|", nxfs[i]);
            }
            _viewBinPattern = stringBuilder.ToString();
        }

		public virtual void UpdateState(LeiaStateDecorators decorators, ILeiaDevice device)
        {
            this.Debug("UpdateState");
            if (_material == null)
            {
                _material = CreateMaterial(decorators.AlphaBlending);
            }

            // inside of CheckRenderTechnique, write into UserNumViews based upon stereo and device orientation and call SetViewCount
            SetUserNumViewsFromDecoratorsAndDevice(decorators, device);

            RespectOrientation(decorators);
            UpdateEmissionPattern(decorators);
            var shaderParams = new ShaderFloatParams();

            shaderParams._width = _displayConfig.UserPanelResolution.x ;
            shaderParams._height = _displayConfig.UserPanelResolution.y ;
            shaderParams._viewResX = _displayConfig.UserViewResolution.x / _displayConfig.ResolutionScale;
            shaderParams._viewResY = _displayConfig.UserViewResolution.y / _displayConfig.ResolutionScale;

            var offset = new [] {(int)_displayConfig.AlignmentOffset.x, (int)_displayConfig.AlignmentOffset.y };
            shaderParams._offsetX = offset[0] + (decorators.ParallaxOrientation.IsInv() ? XOffsetWhenInverted() : 0);
            shaderParams._offsetY = offset[1] + (decorators.ParallaxOrientation.IsInv() ? YOffsetWhenInverted() : 0);

            // _displayHorizontalViewCount represents the display's actual view count along the horizontal / major axis
            // can be NumViews.x or NumViews.y depending upon display orientation
            shaderParams._viewsX = _displayHorizontalViewCount;
            // due to SetUserNumViewsFromDecoratorsAndDevice calling SetViewCount (h, 1), _viewsHigh will always be 1
            shaderParams._viewsY = _viewsHigh;

            shaderParams._orientation = decorators.ParallaxOrientation.IsLandscape() ? 1 : 0;
            shaderParams._adaptFOVx = decorators.AdaptFOV.x;
            shaderParams._adaptFOVy = decorators.AdaptFOV.y;
            shaderParams._enableSwizzledRendering = 1;
            shaderParams._enableHoloRendering = 1;
            shaderParams._enableSuperSampling = 0;
            shaderParams._separateTiles = 1;


            var is2d = shaderParams._viewsY == 1 && shaderParams._viewsX == 1;

            if (decorators.ShowTiles || is2d)
            {
                shaderParams._enableSwizzledRendering = 0;
                shaderParams._enableHoloRendering = 0;
            }

            if (decorators.ShowTiles)
            {
                _material.EnableKeyword("ShowTiles");
            }
            else
            {
                _material.DisableKeyword("ShowTiles");
            }

            SetShaderSubpixelKeywordsFromMatrix(shaderParams._interlace_matrix);

            foreach (string keyword in _displayConfig.InterlacingMaterialEnableKeywords)
            {
                _material.EnableKeyword(keyword);
            }

            SetInterlacedBackgroundPropertiesFromDecorators(decorators);

            shaderParams._showCalibrationSquares = decorators.ShowCalibration ? 1 : 0;
            shaderParams.ApplyTo(_material);
        }

        public virtual void UpdateViews(LeiaCamera leiaCamera)
        {
            this.Debug("UpdateViews");
            if (_viewsWide != _displayConfig.UserNumViews.x)
            {
                SetViewCount(_displayConfig.UserNumViews.x, _viewsHigh);
                UpdateEmissionPattern(LeiaDisplay.Instance.Decorators);
            }
            leiaCamera.SetViewCount(_viewsWide * _viewsHigh);

            int width, height;
            GetTileSize(out width, out height);

            int id = 0;
            for (int ny = 0; ny < _viewsHigh; ny++)
            {
                for (int nx = 0; nx < _viewsWide; nx++)
                {
                    int viewId = ny * _viewsWide + nx;
					var view = leiaCamera.GetView(viewId);

                    if (view.IsCameraNull)
                    {
                        continue;
                    }
                    string viewIdStr = string.Format("view_{0}_{1}", nx, ny);
                    view.SetTextureParams(width, height, viewIdStr);
                    view.ViewIndexX = nx;
                    view.ViewIndexY = ny;
                    view.AttachLeiaMediaCommandBuffersForIndex(id);
                    view.ViewIndex = id++;

                    // the CommandBuffer attached by view.AttachLeiaMediaCommandBuffer... is not executed in Unity's scriptable render pipelines.
                    // in parallel, in SRP with Unity 2020_2+, the LeiaRenderCamera attaches an event which triggers before each Camera renders, which sets Shader Global floats
                }
            }
        }

        public virtual int GetViewsCount()
        {
            return _viewsWide * _viewsHigh;
        }

        public int GetBacklightMode()
        {
            return _backlightMode;
        }

        protected float GetEmissionX(int nx, int ny)
        {
            return _emissionPattern[nx + ny * _viewsWide].x;
        }

        protected float GetEmissionY(int nx, int ny)
        {
            return _emissionPattern[nx + ny * _viewsWide].y;
        }

        public virtual void Release()
        {
            // release interlacing _material
            if (_material != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(_material);
                }
                else
                {
                    GameObject.DestroyImmediate(_material);
                }
            }

            RenderTexture[] releasableRTs = new[] { interlacedAlbedoTexture, overflowTexture };

            // release template_renderTexture and overflowTexture
            for (int i = 0; i < releasableRTs.Length; ++i)
            {
                if (releasableRTs[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        releasableRTs[i].Release();
                        GameObject.Destroy(releasableRTs[i]);
                    }
                    else
                    {
                        releasableRTs[i].Release();
                        GameObject.DestroyImmediate(releasableRTs[i]);
                    }
                }
            }

            // set the RTs to null so that we have a record that they have been released
            overflowTexture = null;
        }

    }
}
