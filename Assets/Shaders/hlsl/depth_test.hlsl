#include "UnityCG.cginc"

int _DepthTest;
sampler2D _DepthMap;
float _DepthOffset;


fixed4 GetBackgroundZColor(fixed2 depthUV)
{
    return tex2D(_DepthMap, depthUV);
}
fixed4 GetBackgroundZColorLod(fixed4 depthUV)
{
    return tex2Dlod(_DepthMap, depthUV);
}
float DecodeBackgroundZByColor(fixed4 color)
{
    if (color.a < 0.5)
        return 1024;
    int hi = floor(color.g * 7 + 0.5) - 3;
    float lo = color.b * 255.0;
    return hi * 256.0 + lo;
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
void ClipDepth(float4 world, fixed2 depthUV)
{
    if (_DepthTest < 0.5)
        return;
    float backgroundZ = SampleBackgroundZ(depthUV);
    if ((world.z - _DepthOffset) * 100.0 > backgroundZ - 210.0)
    {
        discard;
    }
}