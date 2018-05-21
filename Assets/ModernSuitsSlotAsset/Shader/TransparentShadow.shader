// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Transparent Shadow" {
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
	}

	SubShader{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
	}

		Pass{
		Stencil{
		Ref 2
		Comp NotEqual
		Pass keep
		Fail keep
		ZFail keep
	}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Mode Off }
		Offset -1, -1
		Blend One OneMinusSrcAlpha

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
		   sampler2D _MainTex;
		//float4 _MainTex_ST;

	struct appdata {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		fixed4 color : COLOR;
	};
	struct v2f {
		float4 vertex : SV_POSITION;
		half2 texcoord : TEXCOORD0;
		fixed4 color : COLOR;
	};

	v2f o;


	fixed4 SampleSpriteTexture(float2 uv)
	{
		fixed4 color = tex2D(_MainTex, uv);
#if ETC1_EXTERNAL_ALPHA
		// get the color from an external texture (usecase: Alpha support for ETC1 on android)
		color.a = tex2D(_AlphaTex, uv).a* _EffectAmount;
#endif //ETC1_EXTERNAL_ALPHA
		return color;
	}


	v2f vert(appdata v)
	{
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.texcoord = v.texcoord;
		o.color = v.color;
		return o;
	}

	fixed4 frag(v2f IN) : COLOR
	{
		fixed4 c = SampleSpriteTexture(IN.texcoord);
	return float4(c.r*c.a, c.g*c.a, c.b*c.a, c.a);
		//return IN.color;
	}
		ENDCG
	}
	}
}