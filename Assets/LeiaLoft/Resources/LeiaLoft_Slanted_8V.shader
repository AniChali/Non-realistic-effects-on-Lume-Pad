
Shader "LeiaLoft/LeiaLoft_Slanted_8V"
{
  Properties
  {
    // Shader properties
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}

      _viewsX ("Views X", Float) = 8.0
      _viewsY ("Views Y", Float) = 1.0

      _texture_0 ("Texture", 2D) = "white" { }
      _texture_1 ("Texture", 2D) = "white" { }
      _texture_2 ("Texture", 2D) = "white" { }
      _texture_3 ("Texture", 2D) = "white" { }
      _texture_4 ("Texture", 2D) = "white" { }
      _texture_5 ("Texture", 2D) = "white" { }
      _texture_6 ("Texture", 2D) = "white" { }
      _texture_7 ("Texture", 2D) = "white" { }


      _Color ("Main Color", Color) = (1, 0.5, 1, 0.0)
      _width ("Width", Float) = 1600.0
      _height ("Height", Float) = 1600.0
      _viewResX ("View Resolution X", Float) = 200.0
      _viewResY ("View Resolution Y", Float) = 200.0
      _viewsX ("Views X", Float) = 8.0
      _viewsY ("Views Y", Float) = 8.0
      _offsetX ("Offset X", Float) = 0.0
      _offsetY ("Offset Y", Float) = 0.0
      _viewRectX ("Viewport Rect X", Float) = 0.0
      _viewRectY ("Viewport Rect Y", Float) = 0.0
      _viewRectW ("Viewport Rect W", Float) = 0.0
      _viewRectH ("Viewport Rect H", Float) = 0.0
      _adaptFOVx ("Adapt FOV X", Float) = 0.0
      _adaptFOVy ("Adapt FOV Y", Float) = 0.0
      _orientation ("Orientation", Float) = 0.0
      _showCalibrationSquares ("Calibration Squares", Float) = 0.0
      _enableSwizzledRendering ("Enable Swizzled Rendering", Range(0.0, 1.0)) = 1.0
      _enableHoloRendering ("Enable Holo Rendering", Range(0.0, 1.0)) = 1.0
      _enableSuperSampling ("Enable Super Sampling", Range(0.0, 1.0)) = 0.0
      _separateTiles ("Enable Separate Tiles", Range(0.0, 1.0)) = 0.0
      
      _isFlippedAlignment("Windows 8k = true, Andorid = false", Range(0.0, 1.0)) = 1.0
    }

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
      #pragma multi_compile __ ShowTiles ShowDebugViewColumns
      #pragma multi_compile __ LEIALOFT_INTERPOLATION_MASK_TEXTURE LEIALOFT_INTERPOLATION_ALBEDO_TEXTURE
      #pragma multi_compile __ LEIA_INTERLACING_SUBPIXEL

      #include "UnityCG.cginc"
			#include "Assets/LeiaLoft/Resources/LeiaLoft_Slanted_8V.cginc"

			v2f vert(appdata_base v)
			{
				return ProcessVerts(v);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return ProcessFragment(i);
			}

			ENDCG
		}
	}
}
