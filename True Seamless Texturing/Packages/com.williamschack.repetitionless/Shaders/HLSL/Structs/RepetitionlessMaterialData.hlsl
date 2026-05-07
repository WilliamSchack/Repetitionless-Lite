#ifndef REPETITIONLESSMATERIALDATA_INCLUDED
#define REPETITIONLESSMATERIALDATA_INCLUDED

#define REPETITIONLESS_MATERIAL_VARIABLE_COUNT 9

struct RepetitionlessMaterialDataPacked
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

struct RepetitionlessMaterialData
{
    // Settings1
    bool NoiseEnabled;
    bool RandomiseNoiseScaling;
    bool RandomiseRotation;
    bool SmoothnessEnabled;
    bool VariationEnabled;
    bool PackedTexture;
    bool EmissionEnabled;

    bool AlbedoAssigned;
    bool MetallicAssigned;
    bool SmoothnessAssigned;
    bool NormalAssigned;
    bool OcclussionAssigned;
    bool EmissionAssigned;
    bool VariationAssigned;
    bool packedTextureAssigned;

    half Metallic;

    half SmoothnessRoughness;

    // Settings2
    half NormalScale;
    half OcclussionStrength;
    half AlphaClipping;
    half NoiseAngleOffset;

    // Settings3
    half NoiseScale;
    int VariationMode;
    half VariationOpacity;
    half VariationBrightness;

    // Settings4
    half VariationSmallScale;
    half VariationMediumScale;
    half VariationLargeScale;
    half VariationNoiseStrength;

    // Settings5
    half2 NoiseScalingMinMax;
    half2 NoiseRandomiseRotationMinMax;

    half3 AlbedoTint;
    half3 EmissionColor;

    half4 TilingOffset;
    half4 VariationTO;
};

#endif