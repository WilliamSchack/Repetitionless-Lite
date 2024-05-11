#ifndef TEXTUREUTILITIES_INCLUDED
#define TEXTUREUTILITIES_INCLUDED

float4 BlendOverwrite(float4 Base, float4 Blend, float Opacity)
{
    return lerp(Base, Blend, Opacity);
}

float Remap(float In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

float2 RotateUVDegrees(float2 UV, float2 Center, float Rotation)
{
    Rotation = Rotation * (3.1415926f / 180.0f);
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;
    return UV;
}

float3 UnpackNormalmap(float4 PackedNormal, float Strength = 1.0)
{
    float3 normal;
    
    normal.xy = PackedNormal.wy * 2 - 1;
    normal.xy *= Strength;
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    
    return normal;
}

float4 SampleTexture(UnityTexture2D Texture, UnitySamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool NormalMap = false, float NormalStrength = 1.0)
{
    float4 baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
    float4 edgeTextureColor = SAMPLE_TEXTURE2D(Texture, SS, EdgeUV);

    if (NormalMap)
    {
        baseTextureColor.rgb = UnpackNormalmap(baseTextureColor, NormalStrength);
        edgeTextureColor.rgb = UnpackNormalmap(edgeTextureColor, NormalStrength);
    }
    
    return BlendOverwrite(baseTextureColor, edgeTextureColor, EdgeMask);
}

#endif