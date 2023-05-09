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

#ifndef LEIALOFT_INTERPOLATION_MASK_FULLSCREEN_BACKGROUND
#define LEIALOFT_INTERPOLATION_MASK_FULLSCREEN_BACKGROUND

sampler2D _texture_background_albedo;
sampler2D _texture_background_alphamask;
float4 _texture_background_alphamask_texel_size;
uniform float _texture_background_global_alphamask;

// function for retrieving a texture's pixel at uv.xy. Made into a function now so we can extend later
float4 sample_background_albedo(float2 uv) {
	// this cginc should only be called if the _texture_background_albedo has been set, and the LEIALOFT_INTERPOLATION_ALBEDO_TEXTURE keyword is enabled. Ensure both occur in C#
	return tex2D(_texture_background_albedo, uv.xy);
}

float4 sample_background_alphamask_from_texture(float2 uv) {
#if LEIALOFT_INTERPOLATION_MASK_TEXTURE
	// shader keyword LEIALOFT_INTERPOLATION_MASK_TEXTURE distinguishes whether we sample from the texture or not
	return tex2D(_texture_background_alphamask, uv.xy);
#else
	return float4(0.0, 0.0, 0.0, 1.0);
#endif
}

float sample_background_alphamask(float2 uv) {
	return sample_background_alphamask_from_texture(uv)[3] * _texture_background_global_alphamask;
}

#endif // LEIALOFT_INTERPOLATION_MASK_FULLSCREEN_BACKGROUND
