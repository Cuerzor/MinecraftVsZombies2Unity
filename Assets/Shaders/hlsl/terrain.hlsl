#ifndef FLAGS
#include "flags.hlsl"
#endif
#define TERRAIN

sampler2D _TerrainTexture;
uint _TerrainMask;
uint _TerrainFlagRequiredNot;
uint _TerrainFlagRequired;
int _TerrainFlagReverse;

const uint WaterFlag = 1;
const uint PoolFlag = 2;
const uint CloudFlag = 4;
const uint GridOverlayFlag = 8;

uint GetTerrainFlags(float2 levelUV)
{
    float4 color = tex2D(_TerrainTexture, levelUV);
    return Color4ToFlags(color);
}
void ClipTerrain(float2 levelUV)
{
    uint flags = GetTerrainFlags(levelUV) & _TerrainMask;
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