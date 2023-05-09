//
//File: Tresholding.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the tresholding effect
//

Shader "Hidden/Tresholding" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MainTexBlack ("Base black (RGB)", 2D) = "black" {}
        _bwBlend ("Black & White blend", Range (0, 1)) = 0
    }
    
    SubShader {
        
        
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            uniform sampler2D _MainTex;
            uniform sampler2D _MainTexBlack;
            uniform float _bwBlend;


            float4 frag(v2f_img i) : COLOR {
                float4 c = tex2D(_MainTex, i.uv);
                float4 cW = tex2D(_MainTex, i.uv);
                float4 cB = tex2D(_MainTexBlack, i.uv);

                // RESOURCE: https://www.alanzucconi.com/2015/07/08/screen-shaders-and-postprocessing-effects-in-unity3d/
                float lum = c.r*.3 + c.g*.59 + c.b*.11;
                float3 bw = float3( lum, lum, lum ); 
                
                float4 result = c;
                result.rgb = lerp(c.rgb, bw, _bwBlend);
                cW.rgb = lerp(cW.rgb, float3( 1, 1, 1 ), _bwBlend);
                cB.rgb = lerp(cB.rgb, float3( 0, 0, 0 ), _bwBlend);

                if (result.rgb.r < 0.1) {
                    return cB;
                } else {
                    return cW;
                }
                
            }
            ENDCG
        }
    }
}