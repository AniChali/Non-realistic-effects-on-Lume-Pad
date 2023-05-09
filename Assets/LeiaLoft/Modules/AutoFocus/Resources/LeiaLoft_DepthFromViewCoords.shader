/***************************************************************
*
* Copyright 2018 © Leia Inc.  All rights reserved.
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

Shader "LeiaLoft/DepthFromViewCoords"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float normalized_depth : DEPTH;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normalized_depth = (-UnityObjectToViewPos(v.vertex).z - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            	fixed interior = 1.0 - step(1.0, i.normalized_depth) - step(1.0, 1.0 - i.normalized_depth);
            	fixed depth_min = interior * i.normalized_depth + (1.0 - interior);
            	fixed htped_min = interior * (1.0 - i.normalized_depth) + (1.0 - interior);
            	return fixed4(depth_min, htped_min, 0.0, 0.0);
            }
            ENDCG
        }
    }
}
