#define FLAGS

#define BITS_PER_CHANNEL 6
#define MASK_PER_CHANNEL 63
#define MULTIPLIER_PER_CHANNEL 63.0

uint Color3ToFlags(float3 color)
{
    int r = (uint) round(color.r * MULTIPLIER_PER_CHANNEL);
    int g = (uint) round(color.g * MULTIPLIER_PER_CHANNEL);
    int b = (uint) round(color.b * MULTIPLIER_PER_CHANNEL);

    return (b << (BITS_PER_CHANNEL * 2)) | (g << BITS_PER_CHANNEL) | r;
}

float3 FlagsToColor3(uint flags)
{
    float r = (flags & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    float g = ((flags >> BITS_PER_CHANNEL) & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    float b = ((flags >> (BITS_PER_CHANNEL * 2)) & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    return float3(r, g, b);
}
uint Color4ToFlags(float4 color)
{
    int r = (uint) round(color.r * MULTIPLIER_PER_CHANNEL);
    int g = (uint) round(color.g * MULTIPLIER_PER_CHANNEL);
    int b = (uint) round(color.b * MULTIPLIER_PER_CHANNEL);
    int a = (uint) round(color.a * MULTIPLIER_PER_CHANNEL);

    return (a << (BITS_PER_CHANNEL * 3)) | (b << (BITS_PER_CHANNEL * 2)) | (g << BITS_PER_CHANNEL) | r;
}

float4 FlagsToColor4(uint flags)
{
    float r = (flags & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    float g = ((flags >> BITS_PER_CHANNEL) & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    float b = ((flags >> (BITS_PER_CHANNEL * 2)) & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    float a = ((flags >> (BITS_PER_CHANNEL * 3)) & MASK_PER_CHANNEL) / MULTIPLIER_PER_CHANNEL;
    return float4(r, g, b, a);
}