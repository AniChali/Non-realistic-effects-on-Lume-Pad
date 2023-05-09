//
//File: OutlineOnly.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the outline effect
//

Shader "Hidden/OutlineOnly" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MainTexBlack ("Base black (RGB)", 2D) = "black" {}
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
            
            // function that returns grayscale color of pixel
            float getGrayscale (float x, float y, float2 uv) {
                x = x / _ScreenParams.x;
                y = y / _ScreenParams.y;
                float4 c = tex2D(_MainTex, uv + float2(x,y));
                float lum = c.r*.3 + c.g*.59 + c.b*.11;
                float3 bw = float3( lum, lum, lum ); 
                
                float4 grayscaleC = c;
                grayscaleC.rgb = lerp(c.rgb, bw, _bwBlend);
                return grayscaleC.r;
            }
            

            float4 frag(v2f_img i) : COLOR {

                float4 c = tex2D(_MainTex, i.uv);
                
                // RESOURCE: https://www.alanzucconi.com/2015/07/08/screen-shaders-and-postprocessing-effects-in-unity3d/
                float lum = c.r*.3 + c.g*.59 + c.b*.11;
                float3 bw = float3( lum, lum, lum ); 
                
                float4 grayscaleC = c;
                grayscaleC.rgb = lerp(c.rgb, bw, _bwBlend);
                float2 uv = i.uv;
                float x = uv.x;
                float y = uv.y;

                // RESOURCE: https://www.imageeprocessing.com/2011/12/sobel-edge-detection.html
                // get gradients using the sobel operator
                float Gx=((2*getGrayscale(2,1,uv)+getGrayscale(2,0,uv)+getGrayscale(2,2,uv))-(2*getGrayscale(0,1,uv)+getGrayscale(0,0,uv)+getGrayscale(0,2,uv)));
                float Gy=((2*getGrayscale(1,2,uv)+getGrayscale(0,2,uv)+getGrayscale(2,2,uv))-(2*getGrayscale(1,0,uv)+getGrayscale(0,0,uv)+getGrayscale(2,0,uv)));

                // magnitude of gradient vector
                float tmp = sqrt((Gx*Gx)+(Gy*Gy));
                grayscaleC = float4(tmp,tmp,tmp,tmp);
                
                float tres = 0.2;
                tmp = max(tmp,tres);
                // tresholding
                if (tmp > tres) {
                    tmp = 1;
                    grayscaleC = float4(tmp,tmp,tmp,tmp);
                    return grayscaleC;
                } else {
                    return c;
                }
                
                
            }
            ENDCG
        }
    }
}