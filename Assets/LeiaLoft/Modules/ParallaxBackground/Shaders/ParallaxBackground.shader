// Some of this work is based on discussions with Jason Orozco and David Fattal.

Shader "LeiaLoft/ParallaxBackground"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BackgroundAlbedoTex("Background albedo texture", 2D) = "white" {}
		[Enum(DepthPerViewOnly,0, RealtimeDepthPerViewOnly,1, PeriodicBackground, 2)] _BackgroundMode("Mode", Float) = 0
		_BackgroundAlbedoPixelShift("Background albedo pixel shift scale", Float) = -1
		_Mode0Baseline("Baseline in DepthPerViewOnly", Float) = 0.4
		_Mode1Baseline("Baseline in RealtimeDepthPerViewOnly", Float) = 1.0
		
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			// set automatically by Unity using blit
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			// set automatically by each LeiaView to value = 0,1,2,3... etc.
			float _LeiaViewID;
			// set automatically by LeiaCamera
			// 0, count, +offset/2, -centeredOffset/2
			float4 _LeiaViewHorizontalDescription;

			// set by Fullscreen Camera Effect's ShaderParams
			float _BackgroundMode;
			sampler2D _BackgroundAlbedoTex;
			float _BackgroundAlbedoPixelShift; // more negative -> pixels get shifted more intensely for background
			float _Mode0Baseline;
			float _Mode1Baseline;
			float4 _BackgroundAlbedoTilingAndOffset;
			sampler2D _BackgroundAlphaMap;

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float4(v.uv.xy, _BackgroundAlbedoTilingAndOffset.xy + ((v.uv.xy - 0.5) * _BackgroundAlbedoTilingAndOffset.zw + 0.5));
				return o;
			}

			// Signed offset for the current _LeiaView in span from -1.0 to +1.0. For views [0,1,2,3] the values would be -0.75, -0.25, +0.25, +0.75
			inline float LeiaTranslationPerView() {
				return (_LeiaViewID - _LeiaViewHorizontalDescription.w) / _LeiaViewHorizontalDescription.y;
            }

			// Signed pixel translation in UVX steps
			inline float LeiaTranslationPerViewInUVX() {
				const int systemDisparityPixels = 8;
				// _MainTex_TexelSize.x is 1.0 / _MainTex.width
				return LeiaTranslationPerView() * _MainTex_TexelSize.x * systemDisparityPixels * _BackgroundAlbedoPixelShift;
            }

			// assume for now that it is intended that a = 0 is arbitrarily, and a = 1 is arbitrarily far
			inline float LeiaTranslationPerNormalizedDepth(float a) {
				return a;
            }

			// Converts a depth value in span [0 - 1] (0.5 being on-zero-disparity-plane) into a value in span [+1 to -1]
			inline float LeiaTranslationPerNonnormalizedDepth(float a) {
				return LeiaTranslationPerNormalizedDepth((0.5 - a) * 2.0);
            }

			// Samples the alpha of a pixel and uses it to compute a distance-from-ZDP (0 is in front of ZDP, 1 is behind ZDP)
			inline float LeiaTranslationPerPixelAlpha(sampler2D sam, float2 uv) {
				float4 px = tex2D(sam, uv);
				return LeiaTranslationPerNonnormalizedDepth(px);
            }

			// For mode 1, calculates depth
			inline float RealtimeCalculatedDepth(float2 uv) {
				// in this example, our depth function is in span [0-1] depending upon position in tiled UV coords. Our depth function also happens to be seamless
				return cos(uv.x * 3.1415) * 0.5 + 0.5;
            }
			
			fixed4 frag (v2f i) : SV_Target
			{
				// start texture read of unmodified original "main" pixel
				float4 mainPix = tex2D(_MainTex, i.uv.xy);
				float lerpWeight = 1.0 - mainPix.a;

				// allow users to control background texture's x/y offset and horizontal / vertical stretch
				float2 _backgroundUV = i.uv.zw;

				// sign to apply to UV translations per LeiaView. In span -k pixels to +k pixels, where k = _BackgroundAlbedoPixelShift
				float leiaViewTranslation = LeiaTranslationPerViewInUVX();

				const int modeCount = 3;
				int backgroundModeId = floor(abs(_BackgroundMode + 0.0001)) % modeCount;

				// case: user wants to see a texture2D with no depth in alpha channel, at a far position
				if (backgroundModeId == 0) {
					// in the case of this background image demo, we do not sample from a texture's alpha channel in order to get signed-z-distance-off-ZDP
					// effectively, the leiaPixelTranslation is x1 (far)
					_backgroundUV.x += leiaViewTranslation * _Mode0Baseline;

					float4 parallaxBackgroundPixel = tex2D(_BackgroundAlbedoTex, _backgroundUV);
					return lerp(mainPix, parallaxBackgroundPixel, lerpWeight);
                }

				// case: user wants to see a texure2D with no depth in alpha channel, with effective Z position changing in real time
				else if (backgroundModeId == 1) {
					float d = RealtimeCalculatedDepth(_backgroundUV);

					// in the case of this background image demo, we do not sample from a texture's alpha channel in order to get signed-z-distance-off-ZDP
					// instead we compute an analogous value, d * cos(Time), to shift pixels at a distance d away from center
					// pixels which are far from center will flex forward / backward
					_backgroundUV.x += leiaViewTranslation * d * _Mode1Baseline;

					float4 parallaxBackgroundPixel = tex2D(_BackgroundAlbedoTex, _backgroundUV);
					return lerp(mainPix, parallaxBackgroundPixel, lerpWeight);
                }

				// case: user wants to transform a texture into an infinite parallax background texture
				else if (backgroundModeId == 2) {
					// apply a perfect baseline for the tiled repetition. for n views, the jump from view n-1 to view 0 will be perceptually same as jump from view 0 to view 1
					_backgroundUV.x -= 0.5 + (_LeiaViewID * 1.0 / _LeiaViewHorizontalDescription.y) + _LeiaViewHorizontalDescription.w;

					float4 parallaxBackgroundPixel = tex2D(_BackgroundAlbedoTex, _backgroundUV);
					return lerp(mainPix, parallaxBackgroundPixel, lerpWeight);
                }

				// if we are getting to here, check modeCount and _BackgroundMode values
				return float4(0,0,0,1);
			}
			ENDCG
		}
	}
}
