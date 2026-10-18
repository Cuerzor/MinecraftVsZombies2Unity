#include "UnityCG.cginc"

#ifndef LEVEL
#include "level.hlsl"
#endif
#include "burn.hlsl"
#include "hsv.hlsl"
#include "grayscale.hlsl"

#ifndef FLAGS
#include "flags.hlsl"
#endif
#ifndef TERRAIN
#include "terrain.hlsl"
#endif

#if LIT
#include "lighting.hlsl"
#endif

#if DEPTH_TEST
#include "depth_test.hlsl"
#endif

#if CIRCLE_TILED
#include "circle_tile.hlsl"
#endif

struct appdata_entity
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

struct v2f_entity
{
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
    float4 world : TEXCOORD1;
    float2 levelUV : TEXCOORD2;
    float2 noiseUV : TEXCOORD3;
};
sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;
half4 _ColorOffset;
float3 _HSVOffset;
int _Grayscale;
int _GrayscaleMaterial;
half3 _GrayscaleFactor;

// Burn
sampler2D _BurnNoise;
float4 _BurnNoise_ST;
half _BurnValue;
half4 _BurnEdgeColor;
half4 _BurnFireColor;
float _BurnEdgeThreshold;
burn_struct GetBurnParameters(v2f_entity i)
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


v2f_entity EntityVert(appdata_entity v)
{
    v2f_entity o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    
    o.levelUV = GetLevelUV(v.vertex);
    o.world = mul(unity_ObjectToWorld, v.vertex);
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.color = v.color;
    o.noiseUV = TRANSFORM_TEX(v.uv, _BurnNoise);
    return o;
}

half4 GetEntityFragColor(v2f_entity i)
{
    if (!CheckTerrainValid(i.levelUV))
    {
        return 0;
    }
#if DEPTH_TEST
    if (!CheckDepthValid(i.world, i.levelUV))
    {
        return 0;
    }
#endif
#if CIRCLE_TILED
    if (!CheckCircleTile(i.uv))
    {
        return 0;
    }
#endif
    
    half4 col = tex2D(_MainTex, i.uv);
    col *= i.color;
    if (_Grayscale > 0 || _GrayscaleMaterial > 0)
    {
        col = Grayscale(col, _GrayscaleFactor);
    }
    col *= _Color;
    col = ModifyHSV(col, _HSVOffset);
                
    col.rgb = col.rgb + _ColorOffset.rgb;
                            
    
    burn_struct burnParams = GetBurnParameters(i);
    col = Burn(col, burnParams);
    if (col.a <= 0)
    {
        return 0;
    }
    
#if LIT
    col = ApplyLight(col, i.levelUV);
#endif
    return col;
}

half4 EntityFrag(v2f_entity i) : SV_Target
{
    half4 col = GetEntityFragColor(i);
    
    clip(col.a - 0.01);
    col.rgb *= col.a;
    
    return col;
}