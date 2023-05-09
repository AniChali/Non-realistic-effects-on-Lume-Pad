Shader "LeiaLoft/LeiaMaterial"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "black" {}
        _OnscreenPercent("OnscreenPercent", Vector) = (0,0,1,1)
        _EnableOnscreenPercent("EnableOnscreenPercent", float) = 0
        _ColCount("LeiaMedia column count", float) = 0
        _RowCount("LeiaMedia row count", float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque"}
        Lighting Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            float _ColCount;
            float _RowCount;

            float _LeiaViewID;
            float _UserViewCount;
            float _DeviceViewCount;
            sampler2D _MainTex;

            float4 _OnscreenPercent;
            float _EnableOnscreenPercent;
    
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                /* 
                v2f.rect describes local uv coordinates for each view , where:
                .x offset from left = view column *(1/_ColCount) 
                .y offset from bottom = view row *(1/_RowCount)
                .z = width = (1/_ColCount)
                .w = height = (1/_ColCount)

                For example for LumePad with 4 views i.rect provides to fragment
                view 0 = [0.0, 0.5, 0.5, 0.5 ]
                view 1 = [0.5, 0.5, 0.5, 0.5 ]
                view 2 = [0.0, 0.0, 0.5, 0.5 ]
                view 2 = [0.5, 0.0, 0.5, 0.5 ]
                */
                
                float4 rect : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
             };

             /* return coordinates of given media rect */
            float4 getLocalNormalizedMediaRect( float index){
                index += 0.001; /* increase index a bit because occasionally trunc(2/2) return 0 , not 1 */
                float width = 1.0/_ColCount;
                float height = 1.0/_RowCount;
                float column = trunc( fmod(index, _ColCount));
                float row =   trunc( index/_ColCount );
                row = _RowCount-1-row;
                return float4(column*width, row*height, width, height);
            }


            float4 remapVert ( int view )
            {
                float tiles_count = _ColCount * _RowCount;
                float userViewCount = _UserViewCount;
                float leiaViewID = view;
                if (userViewCount == 1) {
                    /*  in case of single cam and c x r image, sample middle-most tile */
                    leiaViewID = tiles_count / 2;
                    return getLocalNormalizedMediaRect(  leiaViewID);
                }
                if(userViewCount<3)
                {
                    userViewCount = 4;
                    leiaViewID = leiaViewID+1;
                }
                float userViewCountMinus1 = userViewCount-1;
                float tilesPerView = (tiles_count-1)/userViewCountMinus1;
                float _mediaRectId = leiaViewID*tilesPerView;
                float left_right = step( userViewCountMinus1/2.0 , leiaViewID  );
                float rounded_mediaRectId = round(_mediaRectId);
                float snap = step(  0, 0.001 - abs( _mediaRectId - rounded_mediaRectId));
                float mediaRectId = trunc(_mediaRectId) + left_right;
                mediaRectId = lerp( mediaRectId, rounded_mediaRectId, snap);
                return getLocalNormalizedMediaRect(  mediaRectId );
            }

 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 onscreenPercentClipPos = float4(0,0,0,1);
                o.uv = v.uv;
                float offsetX = _OnscreenPercent.x ;
                float offsetY = _OnscreenPercent.y ;
                float width = _OnscreenPercent.z ;
                float height = _OnscreenPercent.w ;
#if defined(UNITY_REVERSED_Z)
                /*DirectX 11, DirectX 12, PS4, Xbox One, Metal*/
                onscreenPercentClipPos.x = ( v.uv.x * width + offsetX ) * 2 - 1;
                onscreenPercentClipPos.y = -( v.uv.y  * height + offsetY ) * 2 + 1;
                onscreenPercentClipPos.z = 1;
#else
                /* Android ...*/
                onscreenPercentClipPos.x = (v.uv.x * width + offsetX) * 2 - 1;
                onscreenPercentClipPos.y = -(( 1 - v.uv.y) * height - offsetY + 1 - height ) * 2 + 1;
                onscreenPercentClipPos.z = -1;
#endif
                // each LeiaView sets _LeiaViewID = -1 after they finish their rendering passes
                _EnableOnscreenPercent = _EnableOnscreenPercent * (_LeiaViewID >= 0);
                o.vertex = lerp(o.vertex ,onscreenPercentClipPos, _EnableOnscreenPercent);

                o.rect = remapVert(_LeiaViewID);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                
                float2 uv = float2(i.rect.x + i.uv.x * i.rect.z, i.rect.y + i.uv.y * i.rect.w);
                fixed4 col = tex2D(_MainTex, uv);
 
                // addresses a reported issue with pixel hue in LeiaMediaViewer being warmer than in VLC.
                // per-pixel differences between LeiaMediaViewer and VLC were computed in ImageJ and 
                // a low-error gamma function was calculated that transforms LeiaMediaViewer output into 
                // VLC output
                col = pow(col, 1.0 / 0.9953616);
                return col;
            }
 
            
        ENDCG
        }
    }
    // FallBack "Diffuse"
}
