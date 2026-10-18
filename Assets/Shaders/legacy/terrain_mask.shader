Shader "MinecraftVSZombies2/Legacy/TerrainMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TerrainFlags("Terrain Flags", Int) = 0
        _Clip("Alpha Clip", Float) = 0

        [Header(Burn)]
        _BurnNoise("Noise Tex", 2D) = "white"{}
        _BurnValue("Value", Range(0, 1)) = 0
        _BurnEdgeThreshold("Edge Threshold", Float) = 0.2
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

        ZWrite Off
        ZTest Always
        Cull Off
        Blend One One
        BlendOp LogicalOr

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #ifndef BURN
            #include "../hlsl/burn.hlsl"
            #endif
            #ifndef FLAGS
            #include "../hlsl/flags.hlsl"
            #endif

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uint _TerrainFlags;
            half _Clip;
            
            sampler2D _BurnNoise;
            float4 _BurnNoise_ST;
            half _BurnValue;
            half4 _BurnEdgeColor;
            half4 _BurnFireColor;
            float _BurnEdgeThreshold;
            burn_struct GetBurnParameters(v2f i)
            {
                burn_struct o;
                o.noise = _BurnNoise;
                o.value = _BurnValue;
                o.edgeThresold = _BurnEdgeThreshold;
                o.edgeColor = _BurnEdgeColor;
                o.fireColor = _BurnFireColor;
                o.uv = float4(i.uv, i.noiseUV);
                return o;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _BurnNoise);
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * i.color;
                col = saturate(col);

                burn_struct burnParams = GetBurnParameters(i);
                col = Burn(col, burnParams);
                clip(col.a - _Clip);

                col = FlagsToColor4(_TerrainFlags);

                return col;
            }
            ENDHLSL
        }
    }
}