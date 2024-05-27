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
    UnityTexture2D EmissionMap, float4 EmissionColor, // Emission

    float DebuggingIndex, // Debugging

    out float4 AlbedoColorOut, out float4 NormalColorOut, out float4 MetallicColorOut, out float4 SmoothnessColorOut, out float4 OcclussionColorOut, out float4 EmissionColorOut) // Outputs
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
        
        // Set default values on everything but albedo
        float3 normal = TangentNormalVector;
        NormalColorOut = float4(normal.x, normal.y, normal.z, 0);
        MetallicColorOut = 0;
        SmoothnessColorOut = 0;
        EmissionColorOut = float4(0, 0, 0, 0);
        
        return;
    }
    
    // Albedo
    AlbedoColorOut = SampleTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled) * AlbedoTint;
    if (SurfaceType == 1) clip(AlbedoColorOut.a - AlphaClipping);
    
    // Normal Map
    if (normalAssigned) {
        NormalColorOut = SampleTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled, true, NormalScale);
    } else {
        float3 normal = TangentNormalVector;
        NormalColorOut = float4(normal.x, normal.y, normal.z, 0);
    }
        
    
    // Metallic
    if (metallicAssigned)
        MetallicColorOut = SampleTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
    else
        MetallicColorOut = Metallic;
    
    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned)
            SmoothnessColorOut = SampleTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
        else
            SmoothnessColorOut = Smoothness;
    } else {
        if(roughnessAssigned)
            SmoothnessColorOut = 1 - SampleTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled); // Roughness = 1 - Smoothness
        else
            SmoothnessColorOut = 1 - Roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        OcclussionColorOut = SampleTexture(OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
        OcclussionColorOut = lerp(OcclussionColorOut, 1, 1 - OcclussionStrength);
    } else
        OcclussionColorOut = 1;
    
    // Emission
    if (emissionAssigned)
        EmissionColorOut = SampleTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled) * EmissionColor;
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
    UnityTexture2D BaseEmissionMap, float4 BaseEmissionColor, // Emission

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
    UnityTexture2D FarEmissionMap, float4 FarEmissionColor, // Emission

    float DebuggingIndex, // Debugging

    // Outputs
    out float4 AlbedoColorOut, out float4 NormalColorOut, out float4 MetallicColorOut, out float4 SmoothnessColorOut, out float4 OcclussionColorOut, out float4 EmissionColorOut)
{
    // ----------------------- Base Material ------------------------- //
    
    float4 albedoColor = 1;
    float4 normalColor = float4(TangentNormalVector.x, TangentNormalVector.y, TangentNormalVector.z, 0);
    float4 metallicColor = 0;
    float4 smoothnessColor = 0;
    float4 occlussionColor = 0;
    float4 emissionColor = 0;
    
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
        albedoColor, normalColor, metallicColor, smoothnessColor, occlussionColor, emissionColor);
    
    // --------------------- Distance Blending ----------------------- //
    
    if (DistanceBlendingEnabled) {
        // Distance Mask
        float farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);
        
        float4 farAlbedoColor = 1;
        float4 farNormalColor = float4(TangentNormalVector.x, TangentNormalVector.y, TangentNormalVector.z, 0);
        float4 farMetallicColor = 0;
        float4 farSmoothnessColor = 0;
        float4 farOcclussionColor = 0;
        float4 farEmissionColor = 0;
        
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
                    farAlbedoColor, farNormalColor, farMetallicColor, farSmoothnessColor, farOcclussionColor, farEmissionColor);
            
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
                    farAlbedoColor, farNormalColor, farMetallicColor, farSmoothnessColor, farOcclussionColor, farEmissionColor);
            
                break;
        }
        
        // Distance Mask Debug
        if (DebuggingIndex == 2)
            albedoColor = farDistance;
        
        // Combine Far with Base
        albedoColor = BlendOverwrite(albedoColor, farAlbedoColor, farDistance);
        normalColor = BlendOverwrite(normalColor, farNormalColor, farDistance);
        metallicColor = BlendOverwrite(metallicColor, farMetallicColor, farDistance);
        smoothnessColor = BlendOverwrite(smoothnessColor, farSmoothnessColor, farDistance);
        occlussionColor = BlendOverwrite(occlussionColor, farOcclussionColor, farDistance);
        emissionColor = BlendOverwrite(emissionColor, farEmissionColor, farDistance);
    }
    
    // --------------------------------------------------------------- //
    
    // If Transparency Disabled
    if (SurfaceType == 0 || DebuggingIndex != -1)
        albedoColor.a = 1;
    
    AlbedoColorOut = albedoColor;
    NormalColorOut = normalColor;
    MetallicColorOut = metallicColor;
    SmoothnessColorOut = smoothnessColor;
    OcclussionColorOut = occlussionColor;
    EmissionColorOut = emissionColor;
}

#endif