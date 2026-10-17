#ifndef FLAGS
#include "flags.hlsl"
#endif
#define TERRAIN

sampler2D _TerrainTexture;
int _TerrainMode;
int _TerrainMask;
int _TerrainFlagRequiredNot;
int _TerrainFlagRequired;
int _TerrainFlagReverse;

const int WaterFlag = 1;
const int PoolFlag = 2;
const int CloudFlag = 4;
const int GridOverlayFlag = 8;

int GetTerrainFlags(float2 levelUV)
{
    float4 color = tex2D(_TerrainTexture, levelUV);
    return Color3ToFlags(color.rgb);
}
void ClipTerrain(float2 levelUV)
{
    int flags = GetTerrainFlags(levelUV) & _TerrainMask;
    bool valid = ((flags & _TerrainFlagRequired) != 0) || ((flags & _TerrainFlagRequiredNot) == 0);
    if (_TerrainFlagReverse > 0.5)
    {
        valid = !valid;
    }
    if (!valid)
    {
        discard;
    }
}
