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

Shader "LeiaLoft/LeiaLoft_Square"
{
	Properties
	{
		_Color ("Main Color", Color) = (1, 0.5, 1, 0.0)
		_texture_0 ("Texture", 2D) = "white" { }
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
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "Assets/LeiaLoft/Resources/LeiaLoft_Square.cginc"

			v2f vert (appdata_base v)
			{
				return ProcessVerts(v);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return ProcessFragment(i);
			}

			ENDCG
		}
	}
}