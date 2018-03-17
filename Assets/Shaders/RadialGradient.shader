Shader "Unlit/RadialGradient"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_UVXOffset("UV X Offset", float) = 0
		_UVYOffset("UV Y Offset", float) = 0
		_UVXScale("UV X Scale", float) = 1
		_UVYScale("UV Y Scale", float) = 1
		_Offset("Offset", float) = 0
	}
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent"
			"RenderType"="Transparent"
		}

		Pass
		{
			Cull Off
			ZTest LEqual
			ZWrite On
			AlphaTest Off
			Lighting Off
			ColorMask RGBA
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform fixed4 _Color;
			uniform float _UVXOffset;
			uniform float _UVYOffset;
			uniform float _UVXScale;
			uniform float _UVYScale;
			uniform float _Offset;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float2((v.uv.x + _UVXOffset) * _UVXScale, (v.uv.y + _UVYOffset) * _UVYScale);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(lerp(_Color, fixed4(_Color.rgb,0), sqrt((i.uv.x * i.uv.x) + (i.uv.y * i.uv.y)) + _Offset));
			}
			ENDCG
		}
	}
}