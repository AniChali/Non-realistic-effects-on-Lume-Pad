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
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// ILeiaState implementation for Square-type displays
    /// </summary>
    public class SquareLeiaStateTemplate : AbstractLeiaStateTemplate
    {
		public static string OpaqueShaderName { get { return "LeiaLoft_Square"; } }
		public static string OpaqueShaderNameLimitedViews { get { return "LeiaLoft_Square_Limited"; } }

        public static string TransparentShaderName { get { return "LeiaLoft_Square_Limited"; } }
        public static string TransparentShaderNameLimitedViews { get { return "LeiaLoft_Square_Limited_Blending"; } }

        //Sharpening
        public static string SharpeningShaderName { get { return "LeiaLoft_ViewSharpening"; } }
        private Material _sharpening;

        [System.Obsolete("Deprecated in 0.6.18. Use Slanted. Scheduled for removal in 0.6.20.")]
        public SquareLeiaStateTemplate(DisplayConfig displayConfig) : base(displayConfig)
        {
            // this method was left blank intentionally
        }

        public override void SetViewCount(int viewsWide, int viewsHigh)
        {
            base.SetViewCount(viewsWide, viewsHigh);
        }

        protected override Material CreateMaterial(bool alphaBlending)
        {
            if (_shaderName == null)
            {
                if (_viewsHigh * _viewsWide <= 4)
                {
                    SetShaderName(OpaqueShaderNameLimitedViews, TransparentShaderNameLimitedViews);
                }
                else
                {
                    SetShaderName(OpaqueShaderName, TransparentShaderName);
                }
            }

            return base.CreateMaterial(alphaBlending);
        }

        public override void DrawImage(LeiaCamera camera, LeiaStateDecorators decorators)
        {
            base.DrawImage(camera, decorators);
            Graphics.Blit(interlacedAlbedoTexture, Camera.current.activeTexture, _sharpening);
        }
        public void UpdateSharpeningParameters()
        {
            if (_sharpening == null)
            {
                _sharpening = new Material(Resources.Load<Shader>(SharpeningShaderName));
            }

            int MAX_ACT_COEEFS = 4;
            int sizeX = Mathf.Min(_displayConfig.ActCoefficients.x.Count, MAX_ACT_COEEFS);
            int sizeY = Mathf.Min(_displayConfig.ActCoefficients.y.Count, MAX_ACT_COEEFS);

            Vector4 x = Vector4.zero;
            Vector4 y = Vector4.zero;
            for (int i = 0; i < sizeX; ++i)
            {
                x[i] = _displayConfig.ActCoefficients.x[i];
            }
            for (int i = 0; i < sizeY; ++i)
            {
                y[i] = _displayConfig.ActCoefficients.y[i];
            }

            float gamma = QualitySettings.activeColorSpace == ColorSpace.Linear ? 1f : _displayConfig.Gamma;

            _sharpening.SetFloat("gamma", gamma);
            // note: beta is not a parameter of viewSharpening, so it is not exported to viewSharpening shader
            _sharpening.SetFloat("sharpening_x_size", sizeX);
            _sharpening.SetFloat("sharpening_y_size", sizeY);
            _sharpening.SetVector("sharpening_x", x);
            _sharpening.SetVector("sharpening_y", y);
            // note: sharpening_center is not set in SquareLeiaStateTemplate, so its default remains 1,
            // which makes it an identity operation, which has no impact on result

            // enable Hydrogen4View compiled shader variant. Additional normalization routines in LeiaViewSharpening shader
            _sharpening.EnableKeyword("Hydrogen4View");
        }

        public override void GetFrameBufferSize(out int width, out int height)
        {
			var tileWidth = _displayConfig.UserViewResolution.x;
			var tileHeight = _displayConfig.UserViewResolution.y;
            width = (int)(_viewsWide * tileWidth);
            height = (int)(_viewsHigh * tileHeight);
        }

        public override void GetTileSize(out int tileWidth, out int tileHeight)
        {
            tileWidth = _displayConfig.UserViewResolution.x;
            tileHeight = _displayConfig.UserViewResolution.y;
        }

        public override void UpdateViews(LeiaCamera leiaCamera)
        {
			base.UpdateViews(leiaCamera);
			var calculated = new CameraCalculatedParams(leiaCamera, _displayConfig);
			var near = Mathf.Max(1.0e-5f, leiaCamera.NearClipPlane);
			var far = Mathf.Max(near, leiaCamera.FarClipPlane);
            var halfDeltaX = calculated.ScreenHalfWidth;
            var halfDeltaY = calculated.ScreenHalfHeight;
            var deltaZ = far - near;

            Matrix4x4 m = Matrix4x4.zero;

            float posx, posy;

            for (int ny = 0; ny < _viewsHigh; ny++)
            {
                for (int nx = 0; nx < _viewsWide; nx++)
                {
                    var viewId = ny * _viewsWide + nx;
					var view = leiaCamera.GetView(viewId);

                    if (view.IsCameraNull)
                    {
                        continue;
                    }

					if(leiaCamera.Camera.orthographic) {
                        // catch case where camera's orthgraphicSize = 0.
                        // In Unity perspective camera cannot have FOV 0 but orthographic can have size 0

                        float orthoSize = Mathf.Max(1E-5f, leiaCamera.Camera.orthographicSize);
                        float halfSizeX  = orthoSize * CameraCalculatedParams.GetViewportAspectFor(leiaCamera, _displayConfig);
                        float halfSizeY  = orthoSize;
						int tileWidth, tileHeight;
						GetTileSize (out tileWidth, out tileHeight);

						float baseline = 2.0f * halfSizeX * leiaCamera.BaselineScaling * _displayConfig.SystemDisparityPixels * leiaCamera.ConvergenceDistance / (tileWidth / _displayConfig.ResolutionScale);

						posx = GetEmissionX (nx, ny) * baseline + leiaCamera.CameraShift.x;
						posy = GetEmissionY (nx, ny) * baseline + leiaCamera.CameraShift.y;

                  			// row 0
                  			m [0, 0] = 1.0f / halfSizeX;
                  			m [0, 1] = 0.0f;
						m [0, 2] = -posx / (halfSizeX * leiaCamera.ConvergenceDistance);
                  			m [0, 3] = 0.0f;

                  			// row 1
                  			m [1, 0] = 0.0f;
                  			m [1, 1] = 1.0f / halfSizeY;
						m [1, 2] = -posy / (halfSizeY * leiaCamera.ConvergenceDistance);
                  			m [1, 3] = 0.0f;

                  			// row 2
                  			m [2, 0] = 0.0f;
                  			m [2, 1] = 0.0f;
                  			m [2, 2] = -2.0f / deltaZ;
                  			m [2, 3] = -(far + near) / deltaZ;

                  			// row 3
                  			m [3, 0] = 0.0f;
                  			m [3, 1] = 0.0f;
                  			m [3, 2] = 0.0f;
                  			m [3, 3] = 1.0f;
                    } else { // perspective
						posx = calculated.EmissionRescalingFactor * (GetEmissionX(nx, ny) + leiaCamera.CameraShift.x);
						posy = calculated.EmissionRescalingFactor * (GetEmissionY(nx, ny) + leiaCamera.CameraShift.y);

                        // row 0
						m[0, 0] = leiaCamera.ConvergenceDistance / halfDeltaX;
                        m[0, 1] = 0.0f;
  	                    m[0, 2] = -posx / halfDeltaX;
                        m[0, 3] = 0.0f;

                        // row 1
                        m[1, 0] = 0.0f;
						m[1, 1] = leiaCamera.ConvergenceDistance / halfDeltaY;
  	                    m[1, 2] = -posy / halfDeltaY;
                        m[1, 3] = 0.0f;

                        // row 2
                        m[2, 0] = 0.0f;
                        m[2, 1] = 0.0f;
                        m[2, 2] = -(far + near) / deltaZ;
                        m[2, 3] = -2.0f * far * near / deltaZ;

                        // row 3
                        m[3, 0] = 0.0f;
                        m[3, 1] = 0.0f;
                        m[3, 2] = -1.0f;
                        m[3, 3] = 0.0f;
					}

                    view.Position = new Vector3(posx, posy, 0);
                    view.Matrix = m;
                    view.NearClipPlane = near;
                    view.FarClipPlane = far;

                    UpdateSharpeningParameters();
                }
            }
        }

        public override void Release()
        {
            // release sharpening material
            if (_sharpening != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(_sharpening);
                }
                else
                {
                    GameObject.DestroyImmediate(_sharpening);
                }
            }
            base.Release();
        }
    }
}
