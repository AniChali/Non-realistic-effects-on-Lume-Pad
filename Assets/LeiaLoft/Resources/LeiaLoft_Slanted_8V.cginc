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

// disable reports of use of potentially uninitialized variable
// these occur whenever we use preprocessor for LEIA_TEXTURE_CASE_X
#pragma warning (disable:4000)

#include "Assets/LeiaLoft/Resources/LeiaLoft_Texture_Read.cginc"
#include "Assets/LeiaLoft/Resources/LeiaLoft_InterpolationMaskFullscreenBackground.cginc"

// seems like Unity preprocessor directive does not support macro which is a-function-of(i)


#ifndef LEIALOFT_SLANTED_LIMITED_INCLUDED
#define LEIALOFT_SLANTED_LIMITED_INCLUDED

uniform float _width;
uniform float _height;

uniform float _viewResX;
uniform float _viewResY;

uniform float _viewsX;
uniform float _viewsY;

uniform float _offsetX;
uniform float _offsetY;

uniform float _adaptFOVx;
uniform float _adaptFOVy;

uniform float _orientation;
uniform float _showCalibrationSquares;

uniform float _enableSwizzledRendering;
uniform float _enableHoloRendering;
uniform float _enableSuperSampling;
uniform float _separateTiles;

// need several of the above vars to be shared between shaders, so have to include this shader here
#include "./LeiaLoft_View_Index_Calculator.cginc"

struct v2f {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
};

// called by LeiaLoft_Slanted_8V
inline v2f ProcessVerts(appdata_base v)
{
	v2f o;
#if UNITY_VERSION >= 560
	o.pos = UnityObjectToClipPos(v.vertex);
#else
	o.pos = UnityObjectToClipPos(v.vertex);
#endif
	o.uv = TRANSFORM_TEX(v.texcoord, _texture_0);
	
	return o;
}

#ifdef ShowTiles
    // user enables from SlantedLeiaStateTemplate :: UpdateState by calling _material.EnableKeyword("ShowTiles")
    // given uv coordinates, assumes the user wants a tiled n x m texture and
    // determines which of n x m textures to retrieve
    inline float4 getTilePixel(float2 uv) {
        int texCount = _viewsX * _viewsY;
        int xTileCount = floor(sqrt(texCount));
        int yTileCount = ceil(texCount * 1.0 / xTileCount);

        int xInd = floor(uv.x * xTileCount);
        int yInd = floor((1.0 - uv.y) * yTileCount);
        int camInd = abs(int (xInd + yInd * xTileCount));

		if (camInd >= _viewsX * _viewsY) return float4(0.0, 0.0, 0.0, 1.0);

        uv = (uv * float2(xTileCount, yTileCount)) % 1;
        return sample_view(uv, camInd);
        }
#endif

inline fixed4 ProcessFragment(v2f i)
{
#ifdef ShowTiles
    // compiler variant - for showing tiles
    return(getTilePixel(i.uv));
#endif

	// shift by 0.5 pixels so by default when we read pixel at (k1 / w, k2 / h) we sample the "center" of a pixel
	// probably better if we make this + 0.5 but this is here to stay
	float2 normalized_display_coord = i.uv.xy - 0.5 / float2(_width, _height);

	int3 viewIndices = calculateViewIndices(normalized_display_coord);

	float4 interlaced_fragment = float4(0,0,0,1);

#if LEIA_INTERLACING_SUBPIXEL
		// subpixel RGB sampling
		interlaced_fragment[0] = sample_view(normalized_display_coord.xy, viewIndices[0]).r;
		interlaced_fragment[1] = sample_view(normalized_display_coord.xy, viewIndices[1]).g;
		interlaced_fragment[2] = sample_view(normalized_display_coord.xy, viewIndices[2]).b;
		
		// for subpixel Alpha channel sampling, use medial view
		interlaced_fragment[3] = sample_view(normalized_display_coord, _viewsX / 2)[3];
#else
		// sample single view
		interlaced_fragment = sample_view(normalized_display_coord.xy, viewIndices[0]);
#endif

	// if user has provided a background albedo texture, sample it and mix into the "interpolated" pixel
#if LEIALOFT_INTERPOLATION_ALBEDO_TEXTURE || LEIALOFT_INTERPOLATION_MASK_TEXTURE
    float4 background_fragment = sample_background_albedo(i.uv.xy);
	// mixCoefficient = _texture_background_global_alphamask * _texture_background_alphamask[uv.xy]. alpha = 1: interlaced pixel. alpha = 0: background pixel
    float mixCoefficient = sample_background_alphamask(i.uv.xy);

    // float4 interpolated = lerp(background_fragment * mixCoefficient, interlaced_fragment, mixCoefficient);

	// note: background * mixCoefficient

	background_fragment *= (1.0 - mixCoefficient);

	// use abs(dist from center) here because there is a float-propagating error which causes values in two vertical bars onscreen to be wildly wrong on Android
	float4 interpolated = (mixCoefficient > interlaced_fragment.a || abs(normalized_display_coord.x - 0.5) > 0.2 ? background_fragment * 1.0: interlaced_fragment);
#else
	// pass through the interlaced_fragment pixel data to the interpolated pixel
	float4 interpolated = interlaced_fragment;
#endif
		
	return interpolated;
}

#endif // LEIALOFT_SLANTED_LIMITED_INCLUDED
