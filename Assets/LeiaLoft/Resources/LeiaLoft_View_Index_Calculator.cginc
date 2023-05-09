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

#ifndef LEIALOFT_VIEW_INDEX_CALCULATIONS
#define LEIALOFT_VIEW_INDEX_CALCULATIONS

// assume that these params are required by the including shader:
/*

uniform float _offsetX;

uniform float _width;
uniform float _height;

uniform float _viewsX;
uniform float _viewsY;
*/

float4x4 _interlace_matrix;

inline float periodic_mod(float a, float b) {
	return a - b * floor(a / b);
}
inline float2 periodic_mod(float2 a, float2 b) {
	return a - b * floor(a / b);
}
inline float3 periodic_mod(float3 a, float3 b) {
	return a - b * floor(a / b);
}
inline float4 periodic_mod(float4 a, float4 b) {
	return a - b * floor(a / b);
}

// gets a matrix whose layout is documented below in function body
float4x4 calculateViewMatrix(float2 normalized_display_coord) {
	float4x4 normalized_display_pixel_matrix = float4x4(
		normalized_display_coord.xxxx,
		normalized_display_coord.yyyy,
		(float4(0, 1, 2, 0) / 3.0),
		float4(0, 0, 0, 0)
	);

	// view matrix format:
	// row 0: uv.x as an int, repeated 4 times
	// row 1: uv.y as an int, repeated 4 times
	// row 2: (1/3, 2/3, 3/3)
	// row 3: viewIndex for channel {R/G/B/empty} / viewCount
	float4x4 viewMatrix = mul(_interlace_matrix, normalized_display_pixel_matrix);
	return viewMatrix;
}

// gets view indices for R/G/B channels
int3 calculateViewIndices(float2 normalized_display_coord) {
	float4x4 viewMatrix = calculateViewMatrix(normalized_display_coord);

	float viewCount = _viewsX * _viewsY;

	// correct some float precision issues with this offset
	float float_precision_offset = 0.5 - 1.0 / max(2.0, viewCount);
	float user_offset = _offsetX;

	// last row / row "3" of viewMatrix is ith view index, 
	int3 views = int3(periodic_mod(viewMatrix._m30_m31_m32 * viewCount + user_offset + float_precision_offset, viewCount));
	return views;
}

// process for converting view indices = int3 [view_index_r, view_index_g, view_index_b] into a float in the span from 0 - 255
float4 pixelFromInts(int3 viewIndices) {
	const float maxScale = 255.0; 
	const float floatOffset = 0.1 / maxScale;

	float4 px = float4(
		(viewIndices.rgb + floatOffset) / maxScale,
		1.0);
	return px;
}

// process for calculating view indices = int3 [view_index_r, view_index_g, view_index_b] from onscreen UV coords and converting into a float in the span from 0 - 255
float4 pixelFromInts(float2 normalized_display_coord) {
	int3 viewIndices = calculateViewIndices(normalized_display_coord);
	return pixelFromInts(viewIndices);
}

// process for sampling view indices [r,g,b] from a texture which was previously written into
// this texture is currently not used, so don't create more properties for Unity to track
// sampler2D _interlaced_view_indices;
int3 sampleViewIndicesFromTexture(float2 normalized_display_coord) {
	const float maxScale = 255.0; 
	const float floatOffset = 0.1 / maxScale;

	// float4 px = tex2D(_interlaced_view_indices, normalized_display_coord);
	float4 px = float4(0,0,0,0);
	// each of the view indices is 0.1/255 units higher than their actual integral desired value
	// taking int3 rounds down
	int3 inds = int3(px.rgb * maxScale);
	return inds;
}

// this cginc is specifically for calculating view index, not for sampling ith texture.
// fixed4 interlaced_sample(float2 uv) is a texture sampling operation, so it goes in another cginc file

#endif // LEIALOFT_VIEW_INDEX_CALCULATIONS
