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

Shader "LeiaLoft/ViewSharpening"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "ViewSharpening"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            uniform float4 _MainTex_TexelSize;

            float _sharpeningCenter;
            float4 _sharpeningXY[18]; // XYACT data set by SlantedLeiaStateTemplate will have max length 18 because we can store up to 9 ACT coeffs and data is symmetrical
            float _sharpeningXY_Length; // but actual data member will be trimmed down
            float _gamma;

            float GetTextureWidth()
            {
                return _MainTex_TexelSize.z;
            }

            float GetTextureHeight()
            {
                return _MainTex_TexelSize.w;
            }

            half3 GammaToLinear(half3 col)
            {
                return pow(col, _gamma);
            }

            half3 LinearToGamma(half3 col)
            {
                return pow(col, 1.0 / _gamma);
            }

            float3 texture_offset(float2 uv, int2 offset) {
                float2 uv_offset = uv + (float2(offset) / float2(GetTextureWidth(), GetTextureHeight()));
                return tex2D(_MainTex, uv_offset).rgb;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 final_color = fixed4(0, 0, 0, 1);
                final_color.rgb = _sharpeningCenter * GammaToLinear(texture_offset(i.uv, int2(0, 0)));

                for (int index = 0; index < _sharpeningXY_Length; ++index) {
                    // symmetrical cases are created in the SlantedLeiaStateUpdateTemplate script when i varies from -1 * count to <= +1 * count
                    // so don't do symmetrical cases in this part of the shader

                    // there is no guarantee that ACT will be symmetrical in the future
                    // the ViewSharpening shader will execute an operation where any arbitrary UV x,y offsets are given along with weights for x,y, and
                    // pixel i,j should be a weighted sum of pixel(i,j) and w_xy * pixel(i+x,j+y)

                    // ACT is a coefficient, so by default its impact would be a linear reweighting
                    // linear increase in light is less impactful when light intensity is already higher. so we want to remap our pixels into a pow space, then do linear addition operations
                    // this is why we convert all our pixels using GammaToLinear, then do linear addition, then map from LinearToGamma
                    final_color.rgb -= _sharpeningXY[index].z * GammaToLinear(texture_offset(i.uv, int2(_sharpeningXY[index].xy)));
                }

                final_color.rgb = clamp(final_color.rgb, 0.0, 1.0);
                final_color.rgb = LinearToGamma(final_color.rgb);

                return final_color;
            }
            
            ENDCG
        }
    }
}