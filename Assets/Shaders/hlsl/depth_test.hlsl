#include "UnityCG.cginc"

int _DepthTest;
sampler2D _DepthMap;
float _DepthOffset;


float DecodeBackgroundZByColor(fixed4 color)
{
    if (color.a < 0.5)
        return 1024;
    int hi = floor(color.g * 7 + 0.5) - 3;
    float lo = color.b * 255.0;
    return hi * 256.0 + lo;
}
fixed4 GetBackgroundZColor(fixed2 depthUV)
{
    return tex2D(_DepthMap, depthUV);
}
fixed4 GetBackgroundZColorLod(fixed4 depthUV)
{
    return tex2Dlod(_DepthMap, depthUV);
}
float SampleBackgroundZ(fixed2 depthUV)
{
    fixed4 color = GetBackgroundZColor(depthUV);
    return DecodeBackgroundZByColor(color);
}
float SampleBackgroundZParticle(fixed4 depthUV)
{
    fixed4 color = GetBackgroundZColorLod(depthUV);
    return DecodeBackgroundZByColor(color);
}
float SampleDepth(fixed2 depthUV)
{
    if (_DepthTest < 0.5)
        return 0;
    float backgroundZ = SampleBackgroundZ(depthUV);
    return backgroundZ / 100 - 2.1 + _DepthOffset;
}

float SampleDepthParticle(fixed4 depthUV)
{
    if (_DepthTest < 0.5)
        return 0;
    float backgroundZ = SampleBackgroundZParticle(depthUV);
    return backgroundZ / 100 - 2.1 + _DepthOffset;
}
void ClipDepth(float4 world, fixed2 depthUV)
{
    if (_DepthTest < 0.5)
        return;
    float backgroundDepth = SampleDepth(depthUV);
    if (world.z > backgroundDepth)
    {
        discard;
    }
}
float SampleDepthColorFactor(float4 world, fixed4 depthUV, float softFactor)
{
    if (_DepthTest < 0.5)
        return 1;
    float backgroundZ = SampleDepthParticle(depthUV);
    float depth = world.z - backgroundZ;
    return saturate(1.0 - depth * softFactor);
}