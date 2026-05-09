Shader "MinecraftVSZombies2/Legacy/Pool"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _ShadeTex("Shade", 2D) = "white" {}
        _CausticTex("Caustic", 2D) = "white" {}
        _CausticTex1_ST ("Caustic 1 ST", Vector) = (0,0,0,0)
        _CausticTex2_ST ("Caustic 2 ST", Vector) = (0,0,0,0)
        _WarpTile("Warp Tile", Vector) = (0,0,0,0)
        _WarpTime("Warp Time", Range(0,1)) = 0
        _Warp("Warp", Float) = 0
        _CausticAlpha("Caustic Alpha", Float) = 0

		[Header(Lighting)]
		[HideInInspector] _LightDisabled("Light Disabled", Int) = 0
		[Toggle] _BackgroundLit("Lit by Background", Int) = 0
		[Toggle] _SpotLit("Lit by Spot", Int) = 1
        _LightMapSpot("Light Map Spot", 2D) = "black" {}
        _LevelMapST ("Level Map ST", Vector) = (14, 10.2, 0, 0)

        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE

        float GetModifier(half2 uv)
        {
            float2 value, modifierVec;
            value.x = 1 - abs(0.5 - uv.x) * 2;
            value.y = 1 - abs(0.5 - uv.y) * 2;
            modifierVec = -(pow(value - 1, 2)) + 1;
            return modifierVec.x * modifierVec.y;
        }

        float GetCausticModifier(half2 uv)
        {
            float2 value, modifierVec;
            value.x = 1 - abs(0.5 - uv.x) * 2;
            value.y = 1 - abs(0.5 - uv.y) * 2;
            modifierVec = -(pow(value - 1, 4)) + 1;
            return modifierVec.x * modifierVec.y;
        }

        half4 Screen(half4 src, half4 dest)
        {
            src.rgb *= src.a;
            dest.rgb = 1 - (1 - src.rgb) * (1 - dest.rgb) * 2;
            return dest;
        }

        half4 PoolBlend(half4 poolColor, half4 shadeColor, half3 causticColor)
        {
            half4 result;
            result = poolColor;
            shadeColor.rgb = shadeColor.a * shadeColor.rgb + 1 - shadeColor.a;
            result.rgb = shadeColor.a * shadeColor.rgb + (1 - shadeColor.a) * result.rgb;
                    
                    

            half3 caustic = causticColor;

            result.rgb += caustic;

            return result;
        }
        ENDHLSL
            
        Pass
        {
            Name "Main"

            HLSLPROGRAM
            #include "../hlsl/lighting.hlsl"
            #include "../hlsl/level.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            
            
            struct Attributes
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                half2 lightUV : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            sampler2D _ShadeTex;
            sampler2D _CausticTex;

            half4 _MainTex_ST;
            float4 _ShadeTex_ST;
            float4 _CausticTex_ST;
            float4 _CausticTex1_ST;
            float4 _CausticTex2_ST;
            float _CausticAlpha;
            float2 _WarpTile;
            float _Warp;
            half _WarpTime;
            float4 _Color;
            half4 _RendererColor;

            float2 PoolModifyVertex(half2 uv, float2 vertex, float time)
            {
                const float pi = 3.14159;
                    
                float modifier = GetModifier(uv);

                vertex.x += cos(uv.y * _WarpTile.x + time * 2 * pi) * _Warp * modifier;
                vertex.y += sin(uv.x * _WarpTile.x + time * 2 * pi) * _Warp * modifier;

                return vertex;
            }

                

            half2 PoolModifyUV(half2 uv, float time)
            {
                const float pi = 3.14159;
                float modifier = GetModifier(uv);
                uv.x += cos(uv.y * _WarpTile.x + time * 2 * pi) * _Warp * modifier;
                uv.y += sin(uv.x * _WarpTile.y + time * 2 * pi) * _Warp * modifier;

                return uv;
            }

            half3 SampleCaustic(half2 uv, half2 poolUV)
            {
                half2 firstUV = TRANSFORM_TEX(uv, _CausticTex);
                firstUV.xy = firstUV.xy * _CausticTex1_ST.xy + _CausticTex1_ST.zw;

                half2 secondUV = TRANSFORM_TEX(uv, _CausticTex);
                secondUV.xy = secondUV.xy * _CausticTex2_ST.xy + _CausticTex2_ST.zw;
                    
                half3 result = tex2D(_CausticTex, firstUV).rgb;
                half3 secondColor =  tex2D(_CausticTex,secondUV).rgb;
                result = min(secondColor, result);

                result *= GetCausticModifier(poolUV);// * _CausticAlpha;
                result -= _CausticAlpha;

                return result;
            }
            
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings) 0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightUV = GetLevelUV(v.vertex);
                o.color = v.color * _Color * _RendererColor;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                    
                float time = _WarpTime;
				half2 uv = i.uv;
                half2 poolUv = PoolModifyUV(uv, time);
                const half4 poolColor = tex2D(_MainTex, i.uv);
                float2 shadeUV = TRANSFORM_TEX(poolUv, _ShadeTex);
                half4 shadeColor = tex2D(_ShadeTex, shadeUV);
                half3 causticColor = SampleCaustic(poolUv, i.uv);
                    
                half4 result = PoolBlend(poolColor, shadeColor, causticColor) * i.color;
                result = saturate(result);

                return ApplyLight(result, i.lightUV);
            }
            ENDHLSL
        }
    }
}
