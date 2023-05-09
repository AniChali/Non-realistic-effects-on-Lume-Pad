//
//File: Wobble.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the wobble effect
//

Shader "Hidden/Wobble" {

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
            //uniform float _Range;
            uniform float _bwBlend;

            // jump using intensity as angle
            float2 jump(float2 uv, float radius) {
                float3 color = tex2D(_MainTex, uv);
                fixed intensity = color.r*.3 + color.g*.59 + color.b*.11;
                float angle = radians(intensity*500.0);
                return uv+float2(cos(angle),sin(angle))*radius;
            }

            float4 frag(v2f_img i) : COLOR {
                // RESOURCE: https://www.shadertoy.com/view/ds3GD8

                float4 c = tex2D(_MainTex, i.uv);
                float2 uv = i.uv;
                
                // jump five times to get to final position
                for (int i=0; i<5; ++i) {
                    uv = jump(uv,0.0005);
                }

                // get grayscale color of pixel on final position
                c = tex2D(_MainTex,uv);
                float lum = c.r*.3 + c.g*.59 + c.b*.11;
                float3 bw = float3( lum, lum, lum ); 
                
                float4 result = c;
                result.rgb = lerp(c.rgb, bw, _bwBlend);
                
                return result;

            }
            ENDCG
        }
    }
}