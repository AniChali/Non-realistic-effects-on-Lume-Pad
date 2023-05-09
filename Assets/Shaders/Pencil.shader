//
//File: Pencil.shader
//Author: Aneta Chalivopulosova (xchali00)
//Description: shader used for the pencil effect
// Code is mainly copied from: http://www.shaderslab.com/demo-99---pencil-effect-1.html, author: is Shaders Laboratory
//

Shader "Hidden/Pencil" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _bwBlend ("Black & White blend", Range (0, 1)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
       
        Pass
        {
            // RESOURCE: http://www.shaderslab.com/demo-99---pencil-effect-1.html
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            uniform float _bwBlend;
            
            // gradient treshold
            float _GradThresh = 0.01;
            float _ColorThreshold = 0.5;
            float _Intensity;
 
            struct v2f {
                float4 pos : SV_POSITION;
                float4 screenuv : TEXCOORD0;
            };
 
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenuv = ComputeScreenPos(o.pos);
                return o;
            }
 
            #define PI2 6.28318530717959
            #define STEP 5.0 
            #define RANGE 10.0 
            #define ANGLENUM 4.0 
            #define GRADTHRESH 0.01 
            #define SENSITIVITY 10.0 
 
            float4 getCol(float2 pos)
            {
                // get color on position
                return tex2D(_MainTex, pos / _ScreenParams.xy);
            }
 
            float getVal(float2 pos)
            {
                // get grayscale value of pixel color
                float4 c = getCol(pos);
                return dot(c.xyz, float3(0.2126, 0.7152, 0.0722));
            }
 
            float2 getGrad(float2 pos, float delta)
            {
                // get gradient - partial derivative 
                float2 d = float2(delta, 0.0);
                return float2(getVal(pos + d.xy) - getVal(pos - d.xy),
                              getVal(pos + d.yx) - getVal(pos - d.yx)) / delta / 2.0;
            }
            
            // rotate direction of line by angle
            void pR(inout float2 p, float a) {
                p =  cos(a) * p + sin(a) * float2(p.y, -p.x);
            }
 
            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenuv = i.screenuv.xy / i.screenuv.w;
                float2 screenPos = float2(i.screenuv.x * _ScreenParams.x, i.screenuv.y * _ScreenParams.y);
                // initialize weight to 1
                float weight = 1.0;
 
                for(int j = 0; j < ANGLENUM; j++)
                {  
                    // initialize direction
                    float2 dir = float2(1.0, 0.0) ;
                    // rotate direction based on loop variable
                    pR(dir, j * PI2 / (2.0 * ANGLENUM));

                    float2 grad = float2(-dir.y, dir.x);
       
                    for(int i = -RANGE; i <= RANGE; i += STEP)
                    {
                        float2 b = normalize(dir);
                        float2 pos2 = screenPos + float2(b.x, b.y) * i;

                        // if position is out of bounds continue
                        if (pos2.y < 0.0 || pos2.x < 0.0 || pos2.x > _ScreenParams.x || pos2.y > _ScreenParams.y)
                            continue;

                        // get gradient on position - get extreme of that pixel
                        float2 g = getGrad(pos2, 1.0);

                        if (sqrt(dot(g,g)) < _GradThresh)
                            continue;
                        
                        // update weight
                        weight -= pow(abs(dot(normalize(grad), normalize(g))), SENSITIVITY) / floor((2.0 * RANGE + 1.0) / STEP) / ANGLENUM;
                    }
                }

                // get color and convert it to grayscale
                float4 col = getCol(screenPos);
                float lum = col.r*.3 + col.g*.59 + col.b*.11;
                float3 bw = float3( lum, lum, lum ); 
                
                float4 result = col;
                result.rgb = lerp(col.rgb, bw, _bwBlend);
                float4 background = lerp(result, float4(1.0, 1.0, 1.0, 1.0), _ColorThreshold);

                // use linear interpolation to calculate color of pixel based on weight
                return lerp(float4(0.0, 0.0, 0.0, 0.0), background, weight);
            }
 
            ENDCG
        }
    }
}