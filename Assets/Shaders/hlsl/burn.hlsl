#define BURN

struct burn_struct
{
    sampler2D noise;
    half value;
    float edgeThresold;
    half4 edgeColor : COLOR0;
    half4 fireColor : COLOR1;
    float4 uv : TEXCOORD0;
};

half4 Burn(half4 col, burn_struct i)
{
    half value = i.value;
    if (value > 0)
    {
        sampler2D noiseTexture = i.noise;
        float2 uv = i.uv.xy;
        float2 noiseUV = i.uv.zw;
        half noise = tex2D(noiseTexture, noiseUV).r - value;

        if (noise <= 0)
        {
            return 0;
        }
        
        float edgeThresold = i.edgeThresold;
        if (noise <= edgeThresold + uv.y)
        {
            half4 edgeColor = i.edgeColor;
            col.rgb = lerp(edgeColor.rgb, col.rgb, saturate(noise / edgeThresold));
        }

        if (noise <= edgeThresold * 0.023)
        {
            half4 fireColor = i.fireColor;
            col.rgb = fireColor.rgb;
        }
    }
    return col;
}