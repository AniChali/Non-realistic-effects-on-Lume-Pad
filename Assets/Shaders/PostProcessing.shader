//
//File: PostProcessing.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the hatching effect
//

Shader "Hidden/PostProcessing" {

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
            uniform fixed4 _LightColor0;

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


            float4 frag(vertexOutput i) : COLOR {
                // RESOURCE: https://www.shadertoy.com/view/cddGRX

                float4 c = tex2D(_MainTex, i.tex);
                float2 uv = i.tex;

                // line direction
                float3 p = float3(0.6,0.8,0.1);
                // distance to line
                float d = dot(p,float3(uv,1.0));

                float intensity = dot(c,1.0/3.0);
                // max number of hatching lines
                const int nofSteps = 8;
                // map to discrete steps
                float s = floor(intensity * float(nofSteps)) / float(nofSteps);

                float n = fmod(400.0*(1.0-s)*d,1.0) < 0.20 + 0.79*s;
                float4 result = float4(n,n,n,n);
                return result;
                
            }
            ENDCG
        }
    }
}