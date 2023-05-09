//
//File: ColorShading.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the color shading effect
//

Shader "Hidden/ColorShading" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
       
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float _PixelSize = 5.0;
            float2 texel_size = float2(6.0,6.0);
 
            fixed4 frag(v2f_img i) : SV_Target
            {
                // RESOURCE: http://www.shaderslab.com/demo-98---game-boy-effect.html
                fixed4 col;
                float ratioX = (i.uv.x) * _PixelSize;
                float ratioY = (i.uv.y) * _PixelSize;
                col = tex2D(_MainTex, i.uv + float2(ratioX, ratioY));

                // convert to grayscale
                col = dot(col.rgb, float3(0.3, 0.59, 0.11));
               
                // treshold colors
                if (col.r <= 0.1)
                {
                    col = fixed4(0.06, 0.22, 0.06, 1.0);
                }
                else if (col.r > 0.5)
                {
                    col = fixed4(0.6, 0.74, 0.06, 1.0);
                }
                else if (col.r > 0.1 && col.r <= 0.5)
                {
                    col = fixed4(0.19, 0.38, 0.19, 1.0);
                }

                return col;
            }
 
            ENDCG
        }
    }
}