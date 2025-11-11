#ifndef REPETITIONLESSMATERIAL_INCLUDED
#define REPETITIONLESSMATERIAL_INCLUDED

struct RepetitionlessMaterial
{
    float2 Settings; float4 TilingOffset;

    UnityTexture2D Albedo;
    UnityTexture2D MetallicMap;
    UnityTexture2D SmoothnessMap;
    UnityTexture2D RoughnessMap;
    UnityTexture2D NormalMap;
    UnityTexture2D OcclussionMap;
    UnityTexture2D EmissionMap;
    float4 AlbedoTint; float3 EmissionColor;
    float4 Properties1; float2 Properties2;

    float2 NoiseSettings; float4 NoiseMinMax;

    float VariationMode; float4 VariationSettings; float VariationBrightness;
    float4 VariationNoiseSettings;
    UnityTexture2D VariationTexture; float4 VariationTextureTO;
};

#endif