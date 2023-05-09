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

#ifndef LEIALOFT_TWO_DIMENSIONAL_INCLUDED
#define LEIALOFT_TWO_DIMENSIONAL_INCLUDED

#include "Assets/LeiaLoft/Resources/LeiaLoft_InterpolationMaskFullscreenBackground.cginc"

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float2 uv : TEXCOORD0;
	//UNITY_FOG_COORDS(1)
	float4 vertex : SV_POSITION;
};

sampler2D _texture_0;
float4 _texture_0_ST;

	inline v2f ProcessVerts(appdata v)
	{
		v2f o;
	#if UNITY_VERSION >= 560
		o.vertex = UnityObjectToClipPos(v.vertex);
	#else
		o.vertex = UnityObjectToClipPos(v.vertex);
	#endif
		o.uv = TRANSFORM_TEX(v.uv, _texture_0);
		return o;
	}
	
	inline fixed4 ProcessFragment(v2f i)
	{
        float4 interlaced_fragment = tex2D(_texture_0, i.uv);
#if LEIALOFT_INTERPOLATION_ALBEDO_TEXTURE || LEIALOFT_INTERPOLATION_MASK_TEXTURE
    float4 background_fragment = sample_background_albedo(i.uv.xy);
	// mixCoefficient = _texture_background_global_alphamask * _texture_background_alphamask[uv.xy]. alpha = 1: interlaced pixel. alpha = 0: background pixel
    float mixCoefficient = sample_background_alphamask(i.uv.xy);

    // float4 interpolated = lerp(background_fragment * mixCoefficient, interlaced_fragment, mixCoefficient);
	float4 interpolated = (mixCoefficient * interlaced_fragment.a < 0.1? background_fragment * mixCoefficient: interlaced_fragment);
#else
	// pass through the interlaced_fragment pixel data to the interpolated pixel
	float4 interpolated = interlaced_fragment;
#endif
		return interpolated;
	}

#endif // LEIALOFT_TWO_DIMENSIONAL_INCLUDED
