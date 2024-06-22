#ifndef SAMPLESEAMLESSMATERIAL_INCLUDED
#define SAMPLESEAMLESSMATERIAL_INCLUDED

#include "SeamlessHelpers/SeamlessNoise.hlsl"
#include "SeamlessHelpers/MacroMicroVariation.hlsl"

#include "Noise/Keijiro/SimplexNoise2D.hlsl"
#include "Noise/Keijiro/ClassicNoise2D.hlsl"

void GetSeamlessMaterialColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex, // Material Properties

    float2 Settings, float4 TilingOffset, // Tiling & Offset
    UnityTexture2D Albedo, // Albedo
    UnityTexture2D MetallicMap, // Metallic
    UnityTexture2D SmoothnessMap, // Smoothness
    UnityTexture2D RoughnessMap, // Roughness
    UnityTexture2D NormalMap, // Normal
    UnityTexture2D OcclussionMap, // Occlussion
    UnityTexture2D EmissionMap, // Emission
    float4 AlbedoTint, float3 EmissionColor, // Colors
    float4 MaterialProperties1, float2 MaterialProperties2, // Material Properties

    float2 NoiseSettings, float4 NoiseMinMax, // Noise

    float VariationMode, float4 VariationSettings, float VariationBrightness, // Variation Settings
    float4 VariationNoiseSettings, // Variation Noise
    UnityTexture2D VariationTexture, float4 VariationTextureTO, // Variation Texture

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut) // Outputs
{
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setting Toggles
    int settingToggles = (int) Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int) Settings.y;
    
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = MaterialProperties1.x;
    float smoothness = MaterialProperties1.y;
    float roughness = MaterialProperties1.z;
    float normalScale = MaterialProperties1.w;
    float occlussionStrength = MaterialProperties2.x;
    float alphaClipping = MaterialProperties2.y;
    
    // Noise Settings
    float noiseAngleOffset = NoiseSettings.x;
    float noiseScale = NoiseSettings.y;
    float2 noiseScalingMinMax = NoiseMinMax.xy;
    float2 randomiseRotationMinMax = NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = VariationSettings.x;
    float variationNoiseStrength = VariationNoiseSettings.x;
    float variationNoiseScale = VariationNoiseSettings.y;
    float2 variationNoiseOffset = VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = TilingOffset.xy;
    float2 offset = TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationTexture, SS, oriUV, VariationTextureTO.xy, VariationTextureTO.zw);
                break;
        }
    }
    
    // Debugging
    if (DebuggingIndex != -1) {
        switch (DebuggingIndex) {
            case 0: // Voronoi Cells
                AlbedoColorOut = VoronoiCells;
                break;
            case 1: // Edge Mask
                AlbedoColorOut = EdgeMask;
                break;
            case 4: // Variation Colour
                AlbedoColorOut = variationColor;
                break;
            default:
                AlbedoColorOut = 0;
                break;
        }
        
        return;
    }
    
    // Albedo
    AlbedoColorOut = SampleSeamlessTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled) * AlbedoTint;
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = SampleSeamlessTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled, true, normalScale).rgb;
    } else {
        NormalVectorOut = TangentNormalVector;
    }
       
    // Metallic
    if (metallicAssigned)
        MetallicOut = SampleSeamlessTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
    else
        MetallicOut = metallic;
    
    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned) {
            float4 smoothnessColor = SampleSeamlessTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        } else
            SmoothnessOut = smoothness;
    } else {
        if (roughnessAssigned) {
            float4 roughnessColor = 1 - SampleSeamlessTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled); // Roughness = 1 - Smoothness
            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        } else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        float4 occlussionColor = SampleSeamlessTexture(OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(OcclussionOut, 1, 1 - occlussionStrength);
    } else
        OcclussionOut = 1;
    
    // Emission
    if(emissionEnabled) {
        if (emissionAssigned)
            EmissionColorOut = SampleSeamlessTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).rgb * EmissionColor;
        else
            EmissionColorOut = EmissionColor;
    } else
        EmissionColorOut = 0;
}

void GetSeamlessTerrainLayerColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float DebuggingIndex,

    // Layer Visuals
    float Settings,
    UnityTexture2D AlbedoTexture,
    UnityTexture2D NormalTexture, float NormalScale,
    UnityTexture2D Mask, float HasMask,
    float Metallic,
    float Smoothness,
    float4 TilingOffset,

    float2 NoiseSettings, float4 NoiseMinMax, // Noise

    float VariationMode, float4 VariationSettings, float VariationBrightness, // Variation Settings
    float4 VariationNoiseSettings, // Variation Noise
    UnityTexture2D VariationTexture, float4 VariationTextureTO, // Variation Texture

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut) // Outputs
{
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    
    // Setting Toggles
    int settingToggles = (int) Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool variationEnabled = (settingToggles & 8) != 0;
    
    // Noise Settings
    float noiseAngleOffset = NoiseSettings.x;
    float noiseScale = NoiseSettings.y;
    float2 noiseScalingMinMax = NoiseMinMax.xy;
    float2 randomiseRotationMinMax = NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = VariationSettings.x;
    float variationNoiseStrength = VariationNoiseSettings.x;
    float variationNoiseScale = VariationNoiseSettings.y;
    float2 variationNoiseOffset = VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = TilingOffset.xy;
    float2 offset = TilingOffset.zw;
    float2 oriUV = UV;
    UV = UV * TilingOffset.xy + TilingOffset.zw;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationTexture, SS, oriUV, VariationTextureTO.xy, VariationTextureTO.zw);
                break;
        }
    }
    
    // Debugging
    if (DebuggingIndex != -1) {
        switch (DebuggingIndex) {
            case 0: // Voronoi Cells
                AlbedoColorOut = VoronoiCells;
                break;
            case 1: // Edge Mask
                AlbedoColorOut = EdgeMask;
                break;
            case 4: // Variation Colour
                AlbedoColorOut = variationColor;
                break;
            default:
                AlbedoColorOut = 0;
                break;
        }
        
        return;
    }
    
    // Mask
    float4 maskColor = 0;
    if (HasMask == 1)
        maskColor = SampleSeamlessTexture(Mask, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
    
    // Albedo
    AlbedoColorOut = SampleSeamlessTexture(AlbedoTexture, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal
    NormalVectorOut = SampleSeamlessTexture(NormalTexture, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled, true, NormalScale).rgb;
    
    // Metallic
    if (HasMask == 1)
        MetallicOut = maskColor.r;
    else
        MetallicOut = Metallic;
    
    // Smoothness
    if (HasMask == 1)
        SmoothnessOut = maskColor.a;
    else
        SmoothnessOut = Smoothness;
    
    // Occlussion
    if (HasMask == 1)
        OcclussionOut = maskColor.b;
    else
        OcclussionOut = 1;
}

#endif