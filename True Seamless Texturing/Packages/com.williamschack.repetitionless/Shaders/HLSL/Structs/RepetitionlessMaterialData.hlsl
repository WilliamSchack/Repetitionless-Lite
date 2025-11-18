#ifndef REPETITIONLESSMATERIALDATA_INCLUDED
#define REPETITIONLESSMATERIALDATA_INCLUDED

struct RepetitionlessMaterialData
{
    float2 Settings;
    float4 TilingOffset;

    float4 AlbedoTint;
    float3 EmissionColor;
    float4 Properties1;
    float2 Properties2;

    float2 NoiseSettings;
    float4 NoiseMinMax;

    float VariationMode;
    float4 VariationSettings;
    float VariationBrightness;
    float4 VariationNoiseSettings;
    UnityTexture2D VariationTexture;
    float4 VariationTextureTO;
};

#endif