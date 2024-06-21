#ifndef SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED
#define SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED

#include "SampleSeamlessMaterial.hlsl"

void SampleSeamlessMaterial_float(
    float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition, // Positions
    float DebuggingIndex, // Enums

    // Layer 1
    float Layer1Settings,
    UnityTexture2D Layer1Albedo,
    UnityTexture2D Layer1Normal,
    float Layer1NormalScale,
    UnityTexture2D Layer1Mask,
    float Layer1HasMask,
    float Layer1Metallic,
    float Layer1Smoothness,
    float4 Layer1_ST,

    float2 BaseNoiseSettings, float4 BaseNoiseMinMax, // Noise

    float BaseVariationMode, float4 BaseVariationSettings, float BaseVariationBrightness, // Variation Settings
    float4 BaseVariationNoiseSettings, // Variation Noise
    UnityTexture2D BaseVariationTexture, float4 BaseVariationTextureTO, // Variation Texture

    // Far Material
    bool DistanceBlendingEnabled, int DistanceBlendingMode, float2 DistanceBlendMinMax, // Distance Blending

    float2 FarSettings, float4 FarTilingOffset, // Tiling & Offset
    UnityTexture2D FarAlbedo, // Albedo
    UnityTexture2D FarMetallicMap, // Metallic
    UnityTexture2D FarSmoothnessMap, // Smoothness
    UnityTexture2D FarRoughnessMap, // Roughness
    UnityTexture2D FarNormalMap, // Normal
    UnityTexture2D FarOcclussionMap, // Occlussion
    UnityTexture2D FarEmissionMap, // Emission
    float4 FarAlbedoTint, float3 FarEmissionColor, // Colors
    float4 FarMaterialProperties1, float2 FarMaterialProperties2, // Material Properties

    float2 FarNoiseSettings, float4 FarNoiseMinMax, // Noise

    float FarVariationMode, float4 FarVariationSettings, float FarVariationBrightness, // Variation Settings
    float4 FarVariationNoiseSettings, // Variation Noise
    UnityTexture2D FarVariationTexture, float4 FarVariationTextureTO, // Variation Texture

    // Blend Material
    float MaterialBlendSettings, int BlendMaskType, float4 BlendMaskDistanceTO,
    float2 MaterialBlendProperties, float3 MaterialBlendNoiseSettings,
    UnityTexture2D BlendMaskTexture, float4 BlendMaskTextureTO,

    float2 BlendSettings, float4 BlendTilingOffset, // Tiling & Offset
    UnityTexture2D BlendAlbedo, // Albedo
    UnityTexture2D BlendMetallicMap, // Metallic
    UnityTexture2D BlendSmoothnessMap, // Smoothness
    UnityTexture2D BlendRoughnessMap, // Roughness
    UnityTexture2D BlendNormalMap, // Normal
    UnityTexture2D BlendOcclussionMap, // Occlussion
    UnityTexture2D BlendEmissionMap, // Emission
    float4 BlendAlbedoTint, float3 BlendEmissionColor, // Colors
    float4 BlendMaterialProperties1, float2 BlendMaterialProperties2, // Material Properties

    float2 BlendNoiseSettings, float4 BlendNoiseMinMax, // Noise

    float BlendVariationMode, float4 BlendVariationSettings, float BlendVariationBrightness, // Variation Settings
    float4 BlendVariationNoiseSettings, // Variation Noise
    UnityTexture2D BlendVariationTexture, float4 BlendVariationTextureTO, // Variation Texture

    // Outputs
    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut)
{
    
}

#endif