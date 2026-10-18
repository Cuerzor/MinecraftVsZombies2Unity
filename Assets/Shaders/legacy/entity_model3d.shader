Shader "MinecraftVSZombies2/Legacy/Model"
{
    Properties
    {
        _MainTex ("Diffuse", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorOffset ("Color Offset", Color) = (0,0,0,0)
		[Toggle]
		_GrayscaleMaterial("Grayscale", Int) = 0
		_GrayscaleFactor("Grayscale Factor", Vector) = (0.598, 1.174, 0.228,0)
        
		[Header(Lighting)]
		[Toggle(LIT)]
		_LIT("Lit", Int) = 1
		[HideInInspector] _LightDisabled("Light Disabled", Int) = 0
		[Toggle] _BackgroundLit("Lit by Background", Int) = 0
		[Toggle] _SpotLit("Lit by Spot", Int) = 1
        _LightMapSpot("Light Map Spot", 2D) = "black" {}
        _LevelMapST ("Level Map ST", Vector) = (14, 10.2, 0, 0)

		[Header(DepthTest)]
		[Toggle(DEPTH_TEST)]
		_DepthTestEnabled("Depth Test Enabled", Int) = 1
        _DepthMap("Depth Map Spot", 2D) = "black" {}
        _DepthOffset("Depth Offset", Float) = 100
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
        }
        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Keep
            Fail Keep
            ZFail Keep
        }
        Pass 
        {
            Tags 
            { 
                "RenderType"="TransparentCutout" 
            }
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #include "../hlsl/level.hlsl"
#if DEPTH_TEST
            #include "../hlsl/depth_test.hlsl"
#endif
            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 world : TEXCOORD1;
                float2 levelUV : TEXCOORD2;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;

            v2f vert(a2v IN)
            {
                v2f OUT;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.world = mul(unity_ObjectToWorld, IN.vertex);
                OUT.levelUV = GetLevelUV(IN.vertex);
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
#if DEPTH_TEST
                if (!CheckDepthValid(IN.world, IN.levelUV))
                {
                    discard;
                }
#endif
                half4 baseColor = tex2D(_MainTex, IN.uv) * _Color;
                clip(baseColor.a - 1);
                return half4(0,0,0,0);
            }
            ENDHLSL
        }
        Pass
        {
            Name "Main"

            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend One OneMinusSrcAlpha
            Tags
            { 
                "RenderType"="Transparent" 
                "LightMode" = "ForwardBase" 
            }

            HLSLPROGRAM
            #include "../hlsl/entity.hlsl"
            #pragma vertex EntityVert
            #pragma fragment EntityFrag
            #pragma multi_compile LIT _
            #pragma multi_compile DEPTH_TEST _
            ENDHLSL
        }
    }
}