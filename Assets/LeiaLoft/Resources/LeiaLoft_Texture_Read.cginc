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
// these occur sometimes when we use preprocessor
#pragma warning (disable:4000)
float4 _texture_0_ST;
sampler2D _texture_0;
sampler2D _texture_1;
sampler2D _texture_2;
sampler2D _texture_3;
sampler2D _texture_4;
sampler2D _texture_5;
sampler2D _texture_6;
sampler2D _texture_7;
sampler2D _texture_8;
sampler2D _texture_9;
sampler2D _texture_10;
sampler2D _texture_11;
sampler2D _texture_overflow;

sampler2D _MainTex; 
 
uniform float _texture_overflow_cols;
uniform float _texture_overflow_rows;

// switch point between _texture_11 and _texture_overflow
static const float overflowLimit = 12.0;

// samples from a subsection of a tiled "overflow" texture, as if the uv coord were in the tile of the "overflow" texture
inline float2 uv_in_overflow(float2 uv, int view) {
	// tiled texture slots do not span from 0...11, they start at 12+. but math is for index i of a tiled image, so map 12+ to index 0,1,2...
	view = view - overflowLimit;

	// then read out of a texture with tiles. see LeiaMediaMaterial shader's remap
	float xoffset = _texture_overflow_cols - 1.0 - fmod(view, _texture_overflow_cols);
	float yoffset = floor(view / _texture_overflow_cols);

	return float2((uv + float2(xoffset, yoffset)) / float2(_texture_overflow_cols, _texture_overflow_rows));
}

float4 sample_view(float2 uv, int view) {
    // gets compiled to jump table. no need for binary search through 16 elements
	if (view == 0) return tex2D(_texture_0, uv.xy);
	else if (view == 1) return tex2D(_texture_1, uv.xy);
	else if (view == 2) return tex2D(_texture_2, uv.xy);
	else if (view == 3) return tex2D(_texture_3, uv.xy);
    else if (view == 4) return tex2D(_texture_4, uv.xy);
    else if (view == 5) return tex2D(_texture_5, uv.xy);
    else if (view == 6) return tex2D(_texture_6, uv.xy);
    else if (view == 7) return tex2D(_texture_7, uv.xy);
    else if (view == 8) return tex2D(_texture_8, uv.xy);
    else if (view == 9) return tex2D(_texture_9, uv.xy);
    else if (view == 10) return tex2D(_texture_10, uv.xy);
    else if (view == 11) return tex2D(_texture_11, uv.xy);
    else if (view == 12) return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
    else if (view == 13) return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
    else if (view == 14) return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
    else if (view == 15) return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));

	return float4(0.0, 0.0, 0.0, 1.0);
}

 

 