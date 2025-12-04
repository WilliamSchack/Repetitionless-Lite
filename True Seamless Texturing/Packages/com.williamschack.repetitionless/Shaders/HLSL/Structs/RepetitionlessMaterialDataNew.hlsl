#ifndef REPETITIONLESSMATERIALDATANEW_INCLUDED
#define REPETITIONLESSMATERIALDATANEW_INCLUDED

#define REPETITIONLESS_MATERIAL_VARIABLE_COUNT 9

struct RepetitionlessMaterialDataNew
{
    half4 Settings1;
    half4 Settings2;
    half4 Settings3;
    half4 Settings4;
    half4 Settings5;

    half3 AlbedoTint;
    half3 EmissionColor;

    half4 TilingOffset;
    half4 VariationTO;
};

struct RepetitionlessMaterialDataNewOLD
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