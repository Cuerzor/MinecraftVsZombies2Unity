Shader "MinecraftVSZombies2/Legacy/Particle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _SoftFactor ("Soft Depth Factor", Range(0.1, 10)) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _ZTest ("Z Test", Int) = 4

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
		_DepthTest("Depth Test Enabled", Int) = 1
        _DepthMap("Depth Map Spot", 2D) = "black" {}
        _DepthOffset("Depth Offset", Float) = 100
    }
    SubShader
    {
        Tags
        { 
            "Queue" = "Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent" 
        }

        ZWrite Off
        Cull Off
        ZTest [_ZTest]
        Lighting Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ LIT
            #pragma multi_compile _ DEPTH_TEST
            #include "../hlsl/level.hlsl"
            #if LIT
            #include "../hlsl/lighting.hlsl"
            #endif

            #if DEPTH_TEST
            #include "../hlsl/depth_test.hlsl"
            #endif

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            half _SoftFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;

                float2 levelUV = GetLevelUV(v.vertex);

                float4 lodUV = float4(levelUV, 0.0, 0.0);
                // π‚’’
                #if LIT
                o.color = ApplyLightParticle(o.color, lodUV);
                #endif


                // …Ó∂»≤‚ ‘
                #if DEPTH_TEST
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                float factor = SampleDepthColorFactor(world, lodUV, _SoftFactor);
                o.color.a *= factor;
                #endif

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * i.color;
                col.rgb *= col.a;
                return col;
            }
            ENDHLSL
        }
    }
}