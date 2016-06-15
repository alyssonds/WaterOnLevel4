Shader "Unlit/Lava Simple" 
{
Properties {
	//_Color ("Main Color", Color) = (1,1,1)
	_Magnitude ("Magnitude", Range(0,1)) = 0
	_MainTex ("_MainTex RGBA", 2D) = "white" {}
	_LavaTex ("_LavaTex RGBA", 2D) = "white" {}
	_DirtTex ("_DirtTex RGBA", 2D) = "white" {}
}

Category {
	Tags { "RenderType"="Opaque" }

	Lighting Off
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _LavaTex;
			sampler2D _DirtTex;
			float _Magnitude;

			struct appdata_t {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed2 texcoord2 : TEXCOORD1;
				fixed2 texcoord3 : TEXCOORD2;
			};

			struct v2f {
				fixed4 vertex : SV_POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed2 texcoord2 : TEXCOORD1;
				fixed2 texcoord3 : TEXCOORD2;
			};

			fixed4 _MainTex_ST;
			fixed4 _LavaTex_ST;
			fixed4 _DirtTex_ST;
			//fixed3 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				//from world coordinates to screen cordinates
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//scale and offset texture coordinates
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord2,_LavaTex);
				o.texcoord3 = TRANSFORM_TEX(v.texcoord3,_DirtTex);
				return o;
			}
			
			fixed4 frag (v2f i) : Color
			{
				fixed4 tex = tex2D(_MainTex, i.texcoord);
				fixed4 tex2 = tex2D(_LavaTex, i.texcoord2);
				fixed4 tex3 = tex2D(_DirtTex, i.texcoord3);

				//full river
				tex = tex*tex.a + (1-_Magnitude)*tex2*(1-tex.a) + _Magnitude*tex3*(1-tex.a);;

				return tex;
			}

			ENDCG 
		}
	}	
}
}
