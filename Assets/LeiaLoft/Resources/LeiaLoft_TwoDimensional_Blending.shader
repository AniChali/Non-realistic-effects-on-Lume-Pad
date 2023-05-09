/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
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

Shader "LeiaLoft_TwoDimensional_Blending"
{
	Properties
	{
		_texture_0 ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ LEIALOFT_INTERPOLATION_MASK_TEXTURE
			
			#include "UnityCG.cginc"
			#include "Assets/LeiaLoft/Resources/LeiaLoft_TwoDimensional.cginc"

			v2f vert(appdata v)
			{
				return ProcessVerts(v);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return ProcessFragment(i);
			}

			ENDCG
		}
	}
}
