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

#ifndef LEIALOFT_SQUARE_INCLUDED
#define LEIALOFT_SQUARE_INCLUDED

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
sampler2D _texture_12;
sampler2D _texture_13;
sampler2D _texture_14;
sampler2D _texture_15;

fixed4 _Color;

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

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
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
	
	fixed4 _tex2D(sampler2D tex, float2 coord)
	{
		if (_separateTiles == 1.0 && _enableSuperSampling == 0.0) {
			float2 uv = float2(fmod(coord.x, 1.0 / _viewsX) / (1.0 / _viewsX),
				fmod(coord.y, 1.0 / _viewsY) / (1.0 / _viewsY));
			float index = floor(coord.x * _viewsX) + floor(coord.y * _viewsY) * _viewsX;
			return index < 8 ?
				(index < 4 ?
					(index < 2 ?
						(index < 1 ? tex2D(_texture_0, uv) : tex2D(_texture_1, uv)) :
						(index < 3 ? tex2D(_texture_2, uv) : tex2D(_texture_3, uv))) :
					(index < 6 ?
						(index < 5 ? tex2D(_texture_4, uv) : tex2D(_texture_5, uv)) :
						(index < 7 ? tex2D(_texture_6, uv) : tex2D(_texture_7, uv)))) :
				(index < 12 ?
					(index < 10 ?
						(index < 9 ? tex2D(_texture_8, uv) : tex2D(_texture_9, uv)) :
						(index < 11 ? tex2D(_texture_10, uv) : tex2D(_texture_11, uv))) :
					(index < 14 ?
						(index < 13 ? tex2D(_texture_12, uv) : tex2D(_texture_13, uv)) :
						(index < 15 ? tex2D(_texture_14, uv) : tex2D(_texture_15, uv))));
		}
		else {
			return tex2D(tex, coord);
		}
	}
	
	fixed4 getPixel(float a, sampler2D _tex, float2 viewId, float2 pixId)
{
	float2 id = float2((pixId.x + viewId.x*_viewResX) / _width + 1.0 / (2.0*_width),
		(pixId.y + viewId.y*_viewResY) / _height + 1.0 / (2.0*_height));
	if (_enableSuperSampling < 0.5) {
		fixed4 p = _tex2D(_tex, id);
		return p;
	}
	else {
		const float tiles_x = 2.0 * _viewsX + 1;
		float tiles_y = 2.0 * _viewsY + 1;
		float idx = id.x / tiles_x * _viewsX;
		float idy = id.y / tiles_y * _viewsY;
		float x1 = 1.0 / tiles_x;
		float x2 = 2.0 / tiles_x;
		float y1 = 1.0 / tiles_y;
		float y2 = 2.0 / tiles_y;
		fixed4 diag = tex2D(_tex, float2(idx, idy));
		diag += tex2D(_tex, float2(idx + x2, idy));
		diag += tex2D(_tex, float2(idx + x2, idy + y2));
		diag += tex2D(_tex, float2(idx, idy + y2));
		diag /= 4.0;
		fixed4 uldr = tex2D(_tex, float2(idx + x1, idy));
		uldr += tex2D(_tex, float2(idx + x1, idy + y2));
		uldr += tex2D(_tex, float2(idx, idy + y1));
		uldr += tex2D(_tex, float2(idx + x2, idy + y1));
		uldr /= 4.0;
		fixed4 center = tex2D(_tex, float2(idx + x1, idy + y1));
		return diag * 0.2 + uldr * 0.3 + center * 0.5;
	}
}
	
	inline fixed4 ProcessFragment(v2f i)
	{
		fixed4 pixelRGBA;
	
		float2 pixCoord = float2(floor(i.uv.x * _width) + _offsetX, floor(i.uv.y * _height) + _offsetY);
		
		if (pixCoord.x < 0 || pixCoord.y < 0 || pixCoord.x >= _width || pixCoord.y >= _height)
			return fixed4(0, 0, 0, 1);
		
		float2 viewId = float2(fmod(pixCoord.x, _viewsX), fmod(pixCoord.y, _viewsY));
		float2 pixId = float2(floor(pixCoord.x / _viewsX), floor(pixCoord.y / _viewsY));
		
		float viewWidth = floor(_width / _viewsX);
		
		if (_adaptFOVx > 0.0) {
			if (viewId.x < _adaptFOVx && pixId.x > 0)
				pixId.x -= 1.0;
		}
		else
		{
			if (viewId.x >= _viewsX + _adaptFOVx && pixId.x < viewWidth - 1)
				pixId.x += 1;
		}
		
		if (_enableHoloRendering == 1.0) {
			if (_viewsY == 1.0) {
				if (_viewsX == 2.0) {
					viewId.x = floor(fmod(pixCoord.x, 4.0) / 2.0);
					pixId.x = floor(pixCoord.x / 2);
				}
			}
			else if (_viewsX == 1.0) {
				if (_viewsY == 2.0) {
					viewId.y = floor(fmod(pixCoord.y, 4.0) / 2.0);
					pixId.y = floor(pixCoord.y / 2);
				}
			}
		
			pixelRGBA = _enableSwizzledRendering < 1.0 ?
				_tex2D(_texture_0, i.uv) :
				getPixel(1.0, _texture_0, viewId, pixId);
		
			if (_showCalibrationSquares == 1.0) {
				float squareSide = 100.0;
				float2 squareBlock = float2(_viewsX * squareSide , _viewsY * squareSide);
				float2 squareId = (pixCoord - 0.5*(float2(_width, _height) - squareBlock)) / squareSide;
				float2 squareSine = abs(sin(3.14159*squareId));
				if ((squareId.x >= 0.0) && (squareId.x < _viewsX) && (squareId.y >= 0.0) && (squareId.y < _viewsY)) {
					pixelRGBA = fixed4(0.0, 0.0, 0.0, 1.0);
				}
				if ((viewId.x == floor(squareId.x)) && (viewId.y == floor(squareId.y))) {
					if ((squareSine.x > 0.3) && (squareSine.y > 0.2)) {
						pixelRGBA += fixed4(1.0, 1.0, 1.0, 1.0);
					}
				}
			}
		}
		else {
			pixelRGBA = _tex2D(_texture_0, i.uv);
		}
	
		return pixelRGBA;
	}

#endif // LEIALOFT_SQUARE_INCLUDED