#define FLAGS

int Color3ToFlags(float3 color)
{
    int r = round(color.r * 255.0);
    int g = round(color.g * 255.0);
    int b = round(color.b * 255.0);
    return b * 65536.0 + g * 256.0 + r;
}

float3 FlagsToColor3(int flags)
{
    half r = (flags & 255) / 255.0;
    half g = ((flags >> 8) & 255) / 255.0;
    half b = ((flags >> 16) & 255) / 255.0;
    return float3(r, g, b);
}