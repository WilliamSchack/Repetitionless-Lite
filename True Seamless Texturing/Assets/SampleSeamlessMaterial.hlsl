#ifndef SAMPLESEAMLESSMATERIAL_INCLUDED
#define SAMPLESEAMLESSMATERIAL_INCLUDED

#include "CustomVoronoi.hlsl"
#include "TextureUtilities.hlsl"

void GetSeamlessTextureUVs(
    float2 UV, float2 Tiling, float2 Offset, // UV
    float NoiseAngleOffset, float NoiseScale, bool RandomiseNoiseScaling, float2 NoiseScalingMinMax, // Noise
    bool RandomiseRotation, float2 RandomiseRotationMinMax, // Noise Rotation
    out float VoronoiCells, out float EdgeMask, out float2 EdgeUV, out float2 TransformedUV) // Outputs
{
    // ------------------------ Generate Noise ----------------------- //
    
    float VoronoiDistFromCenter;
    float VoronoiDistFromEdge;
    CustomVoronoi_float(UV, NoiseAngleOffset, NoiseScale, VoronoiDistFromCenter, VoronoiDistFromEdge, VoronoiCells);
    
    // ----------------------- Noise Edge Mask ----------------------- //
    
    // Scale Edge UVs
    EdgeUV = UV;
    if (RandomiseNoiseScaling) {
        float minMaxAverage = (NoiseScalingMinMax.x + NoiseScalingMinMax.y) / 2;
        EdgeUV *= minMaxAverage;
    }
    
    // Generate Edge Mask, replicating a Sample Gradient Node
    EdgeMask = lerp(0.23, -1.5, VoronoiDistFromEdge) * 5;
    EdgeMask = clamp(EdgeMask, 0, 1);
    
    // ------------------------- Modify UVs -------------------------- //
    
    // Randomise UV Scaling
    TransformedUV = UV;
    if (RandomiseNoiseScaling) {
        float newUVTiling = Remap(VoronoiCells, float2(0, 1), NoiseScalingMinMax);
        TransformedUV *= newUVTiling;
    }
    
    // Rotate UVs
    if (RandomiseRotation) {
        float randomCellDegrees = Remap(VoronoiCells, float2(0, 1), RandomiseRotationMinMax);
        TransformedUV = RotateUVDegrees(TransformedUV, 0.0, randomCellDegrees);
    }
    
    // --------------------------------------------------------------- //
}

void GetSeamlessMaterialColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector, float2 Tiling, float2 Offset, // UV
    int SurfaceType, float AlphaClipping, // Alpha Clipping
    float SettingToggles, float AssignedTextures, // Settings
    float NoiseAngleOffset, float NoiseScale, float2 NoiseScalingMinMax, float2 RandomiseRotationMinMax, // Noise

    UnityTexture2D Albedo, float4 AlbedoTint, // Albedo
    UnityTexture2D NormalMap, float NormalScale, // Normal
    UnityTexture2D MetallicMap, float Metallic, // Metallic
    UnityTexture2D SmoothnessMap, float Smoothness, // Smoothness
    UnityTexture2D RoughnessMap, float Roughness, // Roughness
    UnityTexture2D OcclussionMap, float OcclussionStrength, // Occlussion
    UnityTexture2D EmissionMap, float3 EmissionColor, // Emission

    float DebuggingIndex, // Debugging

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut) // Outputs
{
    // Get Setting Toggles
    bool noiseEnabled =          ((int)SettingToggles & 1) != 0;
    bool randomiseNoiseScaling = ((int)SettingToggles & 2) != 0;
    bool randomiseRotation =     ((int)SettingToggles & 4) != 0;
    bool smoothnessEnabled =     ((int)SettingToggles & 8) != 0;
    
    // Get Assigned Textures
    bool metallicAssigned =   ((int)AssignedTextures & 1) != 0;
    bool smoothnessAssigned = ((int)AssignedTextures & 2) != 0;
    bool roughnessAssigned =  ((int)AssignedTextures & 4) != 0;
    bool normalAssigned =     ((int)AssignedTextures & 8) != 0;
    bool occlussionAssigned = ((int)AssignedTextures & 16) != 0;
    bool emissionAssigned =   ((int)AssignedTextures & 32) != 0;
    
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setup UVs
    UV = UV * Tiling + Offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessTextureUVs(UV, Tiling, Offset, NoiseAngleOffset, NoiseScale, randomiseNoiseScaling, NoiseScalingMinMax, randomiseRotation, RandomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    // Debugging
    if (DebuggingIndex != -1) {
        switch (DebuggingIndex) {
            case 0: // Voronoi Cells
                AlbedoColorOut = VoronoiCells;
                break;
            case 1: // Edge Mask
                AlbedoColorOut = EdgeMask;
                break;
            case 2: // Distance Mask, handled later
                AlbedoColorOut = 1;
                break;
            case 3: // TransformedUV
                AlbedoColorOut = float4(TransformedUV.x, TransformedUV.y, 0, 1);
                break;
            default:
                AlbedoColorOut = 1;
                break;
        }
        
        return;
    }
    
    // Albedo
    AlbedoColorOut = SampleTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled) * AlbedoTint;
    if (SurfaceType == 1) clip(AlbedoColorOut.a - AlphaClipping);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = SampleTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled, true, NormalScale).rgb;
    } else {
        NormalVectorOut = TangentNormalVector;
    }
        
    
    // Metallic
    if (metallicAssigned)
        MetallicOut = SampleTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
    else
        MetallicOut = Metallic;
    
    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned)
            SmoothnessOut = SampleTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
        else
            SmoothnessOut = Smoothness;
    } else {
        if(roughnessAssigned)
            SmoothnessOut = 1 - SampleTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r; // Roughness = 1 - Smoothness
        else
            SmoothnessOut = 1 - Roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        OcclussionOut = SampleTexture(OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
        OcclussionOut = lerp(OcclussionOut, 1, 1 - OcclussionStrength);
    } else
        OcclussionOut = 1;
    
    // Emission
    if (emissionAssigned)
        EmissionColorOut = SampleTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).rbg * EmissionColor;
    else
        EmissionColorOut = EmissionColor;
}

void SampleSeamlessMaterial_float(
    float2 UV, float3 TangentNormalVector,
    bool DistanceBlendingEnabled, float2 DistanceBlendMinMax, // Distance Blending
    float3 WorldPosition, float3 CameraPosition, // Positions
    int SurfaceType, int DistanceBlendingMode, // Enums

    // Base Material
    float2 BaseTiling, float2 BaseOffset,
    float BaseAlphaClipping, // Alpha Clipping
    float BaseSettingToggles, float BaseAssignedTextures, // Settings
    float BaseNoiseAngleOffset, float BaseNoiseScale, float2 BaseNoiseScalingMinMax, float2 BaseRandomiseRotationMinMax, // Noise
    UnityTexture2D BaseAlbedo, float4 BaseAlbedoTint, // Albedo
    UnityTexture2D BaseMetallicMap, float BaseMetallic, // Metallic
    UnityTexture2D BaseSmoothnessMap, float BaseSmoothness, // Smoothness
    UnityTexture2D BaseRoughnessMap, float BaseRoughness, // Roughness
    UnityTexture2D BaseNormalMap, float BaseNormalScale, // Normal
    UnityTexture2D BaseOcclussionMap, float BaseOcclussionStrength, // Occlussion
    UnityTexture2D BaseEmissionMap, float3 BaseEmissionColor, // Emission

    // Far Material
    float2 FarTiling, float2 FarOffset,
    float FarAlphaClipping, // Alpha Clipping
    float FarSettingToggles, float FarAssignedTextures, // Settings
    float FarNoiseAngleOffset, float FarNoiseScale, float2 FarNoiseScalingMinMax, float2 FarRandomiseRotationMinMax, // Noise
    UnityTexture2D FarAlbedo, float4 FarAlbedoTint, // Albedo
    UnityTexture2D FarMetallicMap, float FarMetallic, // Metallic
    UnityTexture2D FarSmoothnessMap, float FarSmoothness, // Smoothness
    UnityTexture2D FarRoughnessMap, float FarRoughness, // Roughness
    UnityTexture2D FarNormalMap, float FarNormalScale, // Normal
    UnityTexture2D FarOcclussionMap, float FarOcclussionStrength, // Occlussion
    UnityTexture2D FarEmissionMap, float3 FarEmissionColor, // Emission

    float DebuggingIndex, // Debugging

    // Outputs
    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut)
{
    // ----------------------- Base Material ------------------------- //
    
    float4 albedoColor = 1;
    float3 normalVector = TangentNormalVector;
    float metallic = 0;
    float smoothness = 0;
    float occlussion = 0;
    float3 emissionColor = 0;
    
    GetSeamlessMaterialColor(
        sampler_BaseAlbedo, UV, TangentNormalVector, BaseTiling, BaseOffset,
        SurfaceType, BaseAlphaClipping,
        BaseSettingToggles, BaseAssignedTextures,
        BaseNoiseAngleOffset, BaseNoiseScale, BaseNoiseScalingMinMax, BaseRandomiseRotationMinMax,
        BaseAlbedo, BaseAlbedoTint,
        BaseNormalMap, BaseNormalScale,
        BaseMetallicMap, BaseMetallic,
        BaseSmoothnessMap, BaseSmoothness,
        BaseRoughnessMap, BaseRoughness,
        BaseOcclussionMap, BaseOcclussionStrength,
        BaseEmissionMap, BaseEmissionColor,
        DebuggingIndex,
        albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor);
    
    // --------------------- Distance Blending ----------------------- //
    
    if (DistanceBlendingEnabled) {
        // Distance Mask
        float farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);
        
        float4 farAlbedoColor = 1;
        float3 farNormalVector = TangentNormalVector;
        float farMetallic = 0;
        float farSmoothness = 0;
        float farOcclussion = 0;
        float3 farEmissionColor = 0;
        
        switch (DistanceBlendingMode) {
            case 0: // Tiling & Offset
                // Sample Base Material With Tiling & Offset
                GetSeamlessMaterialColor(
                    sampler_BaseAlbedo, UV, TangentNormalVector, FarTiling, FarOffset,
                    SurfaceType, BaseAlphaClipping,
                    BaseSettingToggles, BaseAssignedTextures,
                    BaseNoiseAngleOffset, BaseNoiseScale, BaseNoiseScalingMinMax, BaseRandomiseRotationMinMax,
                    BaseAlbedo, BaseAlbedoTint,
                    BaseNormalMap, BaseNormalScale,
                    BaseMetallicMap, BaseMetallic,
                    BaseSmoothnessMap, BaseSmoothness,
                    BaseRoughnessMap, BaseRoughness,
                    BaseOcclussionMap, BaseOcclussionStrength,
                    BaseEmissionMap, BaseEmissionColor,
                    DebuggingIndex,
                    farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor);
            
                break;
            case 1: // Material
                // Sample Far Material
                GetSeamlessMaterialColor(
                    sampler_FarAlbedo, UV, TangentNormalVector, FarTiling, FarOffset,
                    SurfaceType, FarAlphaClipping,
                    FarSettingToggles, FarAssignedTextures,
                    FarNoiseAngleOffset, FarNoiseScale, FarNoiseScalingMinMax, FarRandomiseRotationMinMax,
                    FarAlbedo, FarAlbedoTint,
                    FarNormalMap, FarNormalScale,
                    FarMetallicMap, FarMetallic,
                    FarSmoothnessMap, FarSmoothness,
                    FarRoughnessMap, FarRoughness,
                    FarOcclussionMap, FarOcclussionStrength,
                    FarEmissionMap, FarEmissionColor,
                    DebuggingIndex,
                    farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor);
            
                break;
        }
        
        // Distance Mask Debug
        if (DebuggingIndex == 2)
            albedoColor = farDistance;
        
        // Combine Far with Base
        albedoColor = lerp(albedoColor, farAlbedoColor, farDistance);
        normalVector = lerp(normalVector, farNormalVector, farDistance);
        metallic = lerp(metallic, farMetallic, farDistance);
        smoothness = lerp(smoothness, farSmoothness, farDistance);
        occlussion = lerp(occlussion, farOcclussion, farDistance);
        emissionColor = lerp(emissionColor, farEmissionColor, farDistance);
    }
    
    // --------------------------------------------------------------- //
    
    // If Transparency Disabled
    if (SurfaceType == 0 || DebuggingIndex != -1)
        albedoColor.a = 1;
    
    AlbedoColorOut = albedoColor;
    NormalVectorOut = normalVector;
    MetallicOut = metallic;
    SmoothnessOut = smoothness;
    OcclussionOut = occlussion;
    EmissionColorOut = emissionColor;
}

#endif