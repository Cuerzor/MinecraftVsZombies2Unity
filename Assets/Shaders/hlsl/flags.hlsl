#define FLAGS

int Color3ToFlags(float3 color)
{
    int r = (int) round(color.r * 255.0);
    int g = (int) round(color.g * 255.0);
    int b = (int) round(color.b * 255.0);

    return (b << 16) | (g << 8) | r;
}

float3 FlagsToColor3(int flags)
{
    float r = (flags & 255) / 255.0;
    float g = ((flags >> 8) & 255) / 255.0;
    float b = ((flags >> 16) & 255) / 255.0;
    return float3(r, g, b);
}