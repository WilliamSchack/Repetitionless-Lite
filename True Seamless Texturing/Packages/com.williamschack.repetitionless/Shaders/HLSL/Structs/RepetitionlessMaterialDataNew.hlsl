#ifndef REPETITIONLESSMATERIALDATANEW_INCLUDED
#define REPETITIONLESSMATERIALDATANEW_INCLUDED

struct RepetitionlessMaterialDataNew
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
    float4 VariationTextureTO;
};

#endif