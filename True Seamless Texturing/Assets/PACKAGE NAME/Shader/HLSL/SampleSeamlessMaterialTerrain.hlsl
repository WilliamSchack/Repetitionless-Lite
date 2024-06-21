#ifndef SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED
#define SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED

#include "SampleSeamlessMaterial.hlsl"

void SampleSeamlessMaterial_float(
    float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition, // Positions
    UnityTexture2D Holes, UnityTexture2D Control,
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

    float2 Layer1NoiseSettings, float4 Layer1NoiseMinMax, // Noise

    float Layer1VariationMode, float4 Layer1VariationSettings, float Layer1VariationBrightness, // Variation Settings
    float4 Layer1VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer1VariationTexture, float4 Layer1VariationTextureTO, // Variation Texture

    float2 Layer1DistanceBlendMinMax, float4 Layer1DistanceBlendTO, // Distance Blend

    // Layer 2
    float Layer2Settings,
    UnityTexture2D Layer2Albedo,
    UnityTexture2D Layer2Normal,
    float Layer2NormalScale,
    UnityTexture2D Layer2Mask,
    float Layer2HasMask,
    float Layer2Metallic,
    float Layer2Smoothness,
    float4 Layer2_ST,

    float2 Layer2NoiseSettings, float4 Layer2NoiseMinMax, // Noise

    float Layer2VariationMode, float4 Layer2VariationSettings, float Layer2VariationBrightness, // Variation Settings
    float4 Layer2VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer2VariationTexture, float4 Layer2VariationTextureTO, // Variation Texture

    float2 Layer2DistanceBlendMinMax, float4 Layer2DistanceBlendTO, // Distance Blend

    // Layer 3
    float Layer3Settings,
    UnityTexture2D Layer3Albedo,
    UnityTexture2D Layer3Normal,
    float Layer3NormalScale,
    UnityTexture2D Layer3Mask,
    float Layer3HasMask,
    float Layer3Metallic,
    float Layer3Smoothness,
    float4 Layer3_ST,

    float2 Layer3NoiseSettings, float4 Layer3NoiseMinMax, // Noise

    float Layer3VariationMode, float4 Layer3VariationSettings, float Layer3VariationBrightness, // Variation Settings
    float4 Layer3VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer3VariationTexture, float4 Layer3VariationTextureTO, // Variation Texture

    float2 Layer3DistanceBlendMinMax, float4 Layer3DistanceBlendTO, // Distance Blend

    // Layer 4
    float Layer4Settings,
    UnityTexture2D Layer4Albedo,
    UnityTexture2D Layer4Normal,
    float Layer4NormalScale,
    UnityTexture2D Layer4Mask,
    float Layer4HasMask,
    float Layer4Metallic,
    float Layer4Smoothness,
    float4 Layer4_ST,

    float2 Layer4NoiseSettings, float4 Layer4NoiseMinMax, // Noise

    float Layer4VariationMode, float4 Layer4VariationSettings, float Layer4VariationBrightness, // Variation Settings
    float4 Layer4VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer4VariationTexture, float4 Layer4VariationTextureTO, // Variation Texture

    float2 Layer4DistanceBlendMinMax, float4 Layer4DistanceBlendTO, // Distance Blend

    // Outputs
    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut)
{
    float4 albedoColor = 1;
    float3 normalVector = TangentNormalVector;
    float metallic = 0;
    float smoothness = 0;
    float occlussion = 0;
    
    // Output
    AlbedoColorOut = albedoColor;
    NormalVectorOut = normalVector;
    MetallicOut = metallic;
    SmoothnessOut = smoothness;
    OcclussionOut = occlussion;
}

#endif