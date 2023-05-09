Shader "Unlit/FullscreenPeriodicContent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _AlbedoTex;
			float4 _AlbedoTex_TexelSize;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ColCount;
			float _RowCount;
			float _FlipVertical;
			float _ViewSynthesisBaseline;
			float _Periodicity;
			float _PeriodicContentRotation;
			float2 _PeriodicContentUVTranslation;
			float2 _ResolutionChange;
			float2 _PeriodicContentWeight;
			float _BackgroundSelection;
			float _CirclesOverTimeScale;
			float _CirclesOverTimeRate;
			float _CirclesOverTimeIntensity;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			// for screen UV calculates xOffset, yOffset, [0-xOffset*yOffset]
			int3 getSquareViewIndicesFor(float2 screenUV) {
				float4 texSize = float4(0,0, _ScreenParams.x, _ScreenParams.y);
				int xInd = fmod(screenUV.x * texSize[2], _ColCount);
				int yInd = fmod(screenUV.y * texSize[3], _RowCount);

				int viewIndex = (xInd + yInd * _ColCount) % (_ColCount * _RowCount);

				return int3(xInd, yInd, viewIndex);
            }

			// correct an issue where screen coords span (0,0) - (2560,1600) but view UVs are (0,0) - (640, 400).
			// maps values in (k + _ColCount, k_RowCount) to (k, k)
			float2 getGridAlignedUVFor(float2 screenUV) {
				return int2(screenUV.xy * _AlbedoTex_TexelSize.zw * float2(_ColCount, _RowCount)) * 1.0 / float2(_ColCount, _RowCount) / _AlbedoTex_TexelSize.zw;
            }

			fixed4 sampleAlbedo(float2 uv, int backgroundSource) {
				// if backgroundSource is 0, then generate a background color on the fly
				if (backgroundSource == 0) {
					float4 c1 = float4(0.95, 0.90, 0.95, 1.0);
					float4 c2 = float4(0.95, 0.75, 0.78, 1.0);

					float w = cos(3.0 * uv.x + -1.0 * uv.y + _Time.x * 10.0) * 0.5 + 0.5;
					return lerp(c1, c2, w);
                }

				// if backgroundSource is not 0, then sample from the user-provided _AlbedoTex
				return tex2D(_AlbedoTex, uv);
            }

			// given a screen coord, calculate immersive dot pattern for those screen coords. this is similar to
			// sampling a texture with offsets for view index
			float periodicPattern(float2 screenUV) {
				// always apply -0.5 for uv centering. also apply a - 1.0 / _Periodicity term so that we eliminate a symmetry artifact at center of screen
				screenUV -= 0.5;

				float4 texSize = float4(0,0, _ScreenParams.x + _ResolutionChange.x, _ScreenParams.y + _ResolutionChange.y);

				const float PI = 3.1415;

				float p =
						(cos((screenUV.x - 1.0 / _Periodicity + _PeriodicContentUVTranslation[0]) * texSize.z * 2 * PI * _Periodicity) * 0.5 + 0.5) +
						(cos((screenUV.y - 1.0 / _Periodicity + _PeriodicContentUVTranslation[1]) * texSize.w * 2 * PI * _Periodicity) * 0.5 + 0.5);

				p = p / 2;
				return p;
            }

			// given a 2D texture which we wish to display at a synthesized distance from the screen, calculate the offset UV appropriate for view indices
			float2 calculateViewSynthesisUVForViewIndex(float2 screenUV, float xInd, float yInd) {
				float xOffset = (_ColCount / 2.0) + 0.0;
				float yOffset = (_RowCount / 2.0) + 0.0;
				int2 uvTranslationSteps = _ViewSynthesisBaseline * int2(xInd - xOffset, yInd - yOffset);

				// could multiply by depth map here too

				float4 texSize = float4(0,0, _ScreenParams.x, _ScreenParams.y);
				float2 uvTranslation = -1.0 * float2(uvTranslationSteps) / texSize.zw;

				// should also set texture's sampler to clamp but w/e, fix it here

				return clamp(screenUV + uvTranslation, 0.0, 1.0);
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 mainPixel = tex2D(_MainTex, i.uv.xy).a;
				if (mainPixel.a > 0.0) {
					return mainPixel;
                }

				float knockout = cos(length(i.uv.xy - 0.5) * 6.283 * _CirclesOverTimeScale + _Time.x * _CirclesOverTimeRate) * 0.5 + 0.5;
				knockout = (1-_CirclesOverTimeIntensity) + (_CirclesOverTimeIntensity) * knockout;

				float w = periodicPattern(i.uv.xy);

				if (round(_BackgroundSelection) == 0) {
					// for calculating dot pattern using "cosine" style
					
					return lerp(
							sampleAlbedo(i.uv.xy, int(_BackgroundSelection)),
							fixed4(0,0,0,1),
							dot(_PeriodicContentWeight, float2(1, w)) * knockout
						);
				} else if (round(_BackgroundSelection) == 1) {
					return fixed4(w,w,w,1);
                }

				// else do the 16v view synth

				// for doing texture view synthesis so that background can be deep
				// griddedUV = same UV for all views. we want to sample texture at different position based exclusively on view index, not on screen coord

				float2 griddedUV = getGridAlignedUVFor(i.uv.xy);

				// calculate different view offsets based on screen coords
				int3 xycIndices =  getSquareViewIndicesFor(i.uv.xy);

				// leiaUV needs to use griddedUV + viewOffsets(x,y). the only shift in leiaUV is due to view index

				float2 leiaUV = calculateViewSynthesisUVForViewIndex(griddedUV, xycIndices.x, xycIndices.y);

				return lerp(
							sampleAlbedo(leiaUV, int(_BackgroundSelection)),
							float4(0,0,0,1),
							dot(_PeriodicContentWeight, float2(1, w))
						);
			}
			ENDCG
		}
	}
}
