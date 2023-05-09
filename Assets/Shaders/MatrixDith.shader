//
//File: MatrixDith.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the matrix dithering effect
//

Shader "Hidden/MatrixDith" {

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
            float4 _MainTex_ST; 


            // RESOURCE: https://www.shadertoy.com/view/4lcyzn
            float4x4 M2 = float4x4(
                0.0, 8.0, 2.0, 10.0, 
                12.0, 4.0, 14.0, 6.0,
                3.0,11.0,1.0,9.0,
                15.0,7.0,13.0, 5.0);


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

                int j, k;
                int m_side = 4;
                int x = i.pos.xyz.x;
                int y = i.pos.xyz.y;
                j = x % m_side;
                k = y % m_side;
                float r = result.rgb.r;
        
                // RESOURCE: https://www.shadertoy.com/view/4lcyzn
                if (r < (M2[j][k]/16.0)) {
                    return cW;

                } else {
                    
                    return cB;

                }
                
            }
            ENDCG
        }
    }
}