Shader "MinecraftVSZombies2/Fire"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Header(Color)]
        _Color("Tint", Color) = (1,1,1,1)
        _ColorOffset("Color Offset", Color) = (0,0,0,0)
        _HSVOffset("HSV Offset", Vector) = (0,0,0,0)

		[Header(Fire)]
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_FireHSVOffset ("Fire HSV Offset", Vector) = (0, 0, 0, 0)
		_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_LocalRect("Local Rect", Vector) = (0, 0, 1, 1)
		_FireTime("Fire Time", Float) = 0
		_LifeTime("Life Time", Range(0, 1)) = 0
		_ClipThresold("Clip Thresold", Range(0, 1)) = 0.5

		[Header(Level)]
        _LevelMapST ("Level Map ST", Vector) = (14, 10.2, 0, 0)

        [Header(DepthTest)]
        [Toggle(DEPTH_TEST)]
        _DepthTestEnabled("Depth Test Enabled", Int) = 1
        _DepthMap("Depth Map Spot", 2D) = "black" {}
        _DepthOffset("Depth Offset", Float) = 100
    }

	HLSLINCLUDE
	#include "UnityCG.cginc"
	#include "..\hlsl\hsv.hlsl"
#if DEPTH_TEST
	#include "..\hlsl\depth_test.hlsl"
#endif
#ifndef LEVEL
	#include "..\hlsl\level.hlsl"
#endif
	#pragma target 3.0

	sampler2D _MainTex;
	float _FireTime;
	float4 _MainTex_ST;
	half4 _EdgeColor;
	sampler2D _NoiseTex;
	float4 _NoiseTex_ST;
	float4 _LocalRect;
	half _LifeTime;
	half _ClipThresold;
	float4 _Color;
	half4 _ColorOffset;
	float3 _HSVOffset;
	float3 _FireHSVOffset;

	struct a2v
	{
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
		half4 color : COLOR;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float2 noise_uv : TEXCOORD1;
		float4 vertex : SV_POSITION;
		half4 color : COLOR;
		half lifetime : TEXCOORD2;
		float4 world : TEXCOORD3;
		float2 levelUV : TEXCOORD4;
	};

	v2f vert(a2v v)
	{
		v2f o;
		o.levelUV = GetLevelUV(v.vertex);
		o.world = mul(unity_ObjectToWorld, v.vertex);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.noise_uv = TRANSFORM_TEX(v.texcoord, _NoiseTex);
		o.color = v.color;
		o.lifetime = v.texcoord.z + _LifeTime;
		return o;
	}


	half4 frag(v2f i) :SV_Target
	{
#if DEPTH_TEST
		if (!CheckDepthValid(i.world, i.levelUV))
		{
			discard;
		}
#endif
		// 获取原本贴图颜色
		half4 col = tex2D(_MainTex, i.uv);
		col *= i.color;
		col *= _Color;
		col = ModifyHSV(col, _HSVOffset + _FireHSVOffset);
                
		col.rgb = col.rgb + _ColorOffset.rgb;
		clip(col.a - 0.1);
		
		// 获取对应位置的噪声颜色。
		float2 noise_uv = i.noise_uv - _FireTime * _NoiseTex_ST.xy;
		half noise = tex2D(_NoiseTex, noise_uv).r;
		
		// 根据噪声的明度决定是否裁去像素。
		half2 localUV = (i.uv - _LocalRect.xy) / (_LocalRect.zw - _LocalRect.xy);
		float clipValue = noise + _ClipThresold - localUV.x - i.lifetime;
		clip(clipValue);

		col = lerp(_EdgeColor, col, clamp(clipValue * 3, 0, 1));
		col.a *= lerp(0, 1, i.lifetime * 4);
		return col;
	}

	ENDHLSL

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			ZWrite Off
			Cull Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma shader_feature _ DEPTH_TEST
			ENDHLSL

		}

	}
	FallBack "Sprites/Default"
}
