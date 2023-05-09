//
//File: Wobble2.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the wobble 2 effect
//

Shader "Hidden/Wobble2" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
    SubShader {
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            uniform sampler2D _MainTex;
            float4 _MainTex_ST; 


            struct vertexInput {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };
            struct vertexOutput {
                float4 pos : SV_POSITION;
                float4 tex : TEXCOORD0;
            };
    
            vertexOutput vert(vertexInput input) 
            {
                vertexOutput output;
                output.tex = input.texcoord;
                output.pos = UnityObjectToClipPos(input.vertex);
                return output;
            }

     
            float2 wobble (float2 uv, float amplitude, float frequence, float speed) {
                // get offset on x-axis with sinus function
                float offset = amplitude*sin(uv.y*frequence+speed);
                return float2(uv.x+offset,uv.y);	
            }


            float4 frag(vertexOutput i) : COLOR {

                // RESOURCE: https://www.shadertoy.com/view/MdS3RV
                
                float2 uv = i.tex;
                float amplitude = 0.005;
                float frequence = 25.00;
                float speed = 10.0;
                // create offset with wobble function
                uv = wobble(uv,amplitude,frequence,speed);
                
                float4 c = tex2D(_MainTex, uv);
                return c;
                   
            }
            ENDCG
        }
    }
}