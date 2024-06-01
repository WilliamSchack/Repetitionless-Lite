#ifndef SAMPLESEAMLESSMATERIAL_INCLUDED
#define SAMPLESEAMLESSMATERIAL_INCLUDED

#include "TextureUtilities.hlsl"

#include "Noise/VoronoiNoise2D.hlsl"
#include "Noise/Keijiro/SimplexNoise2D.hlsl"
#include "Noise/Keijiro/ClassicNoise2D.hlsl"

void GetSeamlessTextureUVs(
    float2 UV, float2 Tiling, float2 Offset, // UV
    float NoiseAngleOffset, float NoiseScale, bool RandomiseNoiseScaling, float2 NoiseScalingMinMax, // Noise
    bool RandomiseRotation, float2 RandomiseRotationMinMax, // Noise Rotation
    out float VoronoiCells, out float EdgeMask, out float2 EdgeUV, out float2 TransformedUV) // Outputs
{
    // ------------------------ Generate Noise ----------------------- //
    
    float VoronoiDistFromCenter;
    float VoronoiDistFromEdge;
    VoronoiNoise(UV, NoiseAngleOffset, NoiseScale, VoronoiDistFromCenter, VoronoiDistFromEdge, VoronoiCells);
    
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
            default:
                AlbedoColorOut = 0;
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
    float3 WorldPosition, float3 CameraPosition, // Positions
    int SurfaceType, float DebuggingIndex, // Enums

    // Base Material
    float2 BaseTiling, float2 BaseOffset, // Tiling & Offset
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
    bool DistanceBlendingEnabled, float2 DistanceBlendMinMax, int DistanceBlendingMode, // Distance Blending

    float2 FarTiling, float2 FarOffset, // Tiling & Offset
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

    // Blend Material
    bool MaterialBlendEnabled,
    bool BlendOverrideDistanceBlending, bool BlendOverrideDistanceBlendingTO, float2 BlendDistanceBlendingScale, float2 BlendDistanceBlendingOffset,
    int BlendMaskType, float BlendMaskOpacity, float BlendMaskStrength,
    float BlendMaskNoiseScale, float2 BlendMaskNoiseOffset, // Noise Blend Mask
    UnityTexture2D BlendMaskTexture, float2 BlendMaskTextureScale, float2 BlendMaskTextureOffset, // Texture Blend Mask

    float2 BlendTiling, float2 BlendOffset, // Tiling & Offset
    float BlendAlphaClipping, // Alpha Clipping
    float BlendSettingToggles, float BlendAssignedTextures, // Settings
    float BlendNoiseAngleOffset, float BlendNoiseScale, float2 BlendNoiseScalingMinMax, float2 BlendRandomiseRotationMinMax, // Noise
    UnityTexture2D BlendAlbedo, float4 BlendAlbedoTint, // Albedo
    UnityTexture2D BlendMetallicMap, float BlendMetallic, // Metallic
    UnityTexture2D BlendSmoothnessMap, float BlendSmoothness, // Smoothness
    UnityTexture2D BlendRoughnessMap, float BlendRoughness, // Roughness
    UnityTexture2D BlendNormalMap, float BlendNormalScale, // Normal
    UnityTexture2D BlendOcclussionMap, float BlendOcclussionStrength, // Occlussion
    UnityTexture2D BlendEmissionMap, float3 BlendEmissionColor, // Emission

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
    
    float materialMask = 0;
    float farDistance = 0;
    
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
    
    // --------------------- Material Blending ----------------------- //
    
    if (MaterialBlendEnabled)
    {
        // Get mask of blended material
        switch (BlendMaskType)
        {
            case 0: // Perlin Noise
                materialMask = ClassicNoise(UV * BlendMaskNoiseScale + BlendMaskNoiseOffset) * 3;
                break;
            case 1: // Simplex Noise
                materialMask = SimplexNoise(UV * BlendMaskNoiseScale + BlendMaskNoiseOffset) * 2;
                break;
            case 2: // Custom Texture
                materialMask = SAMPLE_TEXTURE2D(BlendMaskTexture, sampler_BlendMaskTexture, UV * BlendMaskTextureScale + BlendMaskTextureOffset);
                break;
        }
        materialMask *= BlendMaskStrength;
        materialMask = clamp(materialMask, 0, 1);
        materialMask *= BlendMaskOpacity;
        
        if (materialMask > 0)
        {
            float4 blendAlbedoColor = 1;
            float3 blendNormalVector = TangentNormalVector;
            float blendMetallic = 0;
            float blendSmoothness = 0;
            float blendOcclussion = 0;
            float3 blendEmissionColor = 0;
            
            // Sample Blend Material
            GetSeamlessMaterialColor(
                    sampler_BlendAlbedo, UV, TangentNormalVector, BlendTiling, BlendOffset,
                    SurfaceType, BlendAlphaClipping,
                    BlendSettingToggles, BlendAssignedTextures,
                    BlendNoiseAngleOffset, BlendNoiseScale, BlendNoiseScalingMinMax, BlendRandomiseRotationMinMax,
                    BlendAlbedo, BlendAlbedoTint,
                    BlendNormalMap, BlendNormalScale,
                    BlendMetallicMap, BlendMetallic,
                    BlendSmoothnessMap, BlendSmoothness,
                    BlendRoughnessMap, BlendRoughness,
                    BlendOcclussionMap, BlendOcclussionStrength,
                    BlendEmissionMap, BlendEmissionColor,
                    DebuggingIndex,
                    blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor);
            
            // Combine Blend with Base
            albedoColor = lerp(albedoColor, blendAlbedoColor, materialMask);
            normalVector = lerp(normalVector, blendNormalVector, materialMask);
            metallic = lerp(metallic, blendMetallic, materialMask);
            smoothness = lerp(smoothness, blendSmoothness, materialMask);
            occlussion = lerp(occlussion, blendOcclussion, materialMask);
            emissionColor = lerp(emissionColor, blendEmissionColor, materialMask);
        }
    }
    
    // --------------------- Distance Blending ----------------------- //
    
    if (DistanceBlendingEnabled) {
        // Distance Mask
        farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);
        
        // Only calculate far distance if required
        if (farDistance > 0) {
            float4 farAlbedoColor = 1;
            float3 farNormalVector = TangentNormalVector;
            float farMetallic = 0;
            float farSmoothness = 0;
            float farOcclussion = 0;
            float3 farEmissionColor = 0;
        
            switch (DistanceBlendingMode) {
                case 0: // Tiling & Offset
                    // Sample Base Material
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
        
            // Blend material
            if (materialMask > 0 && BlendOverrideDistanceBlending)
            {
                float4 blendAlbedoColor = 1;
                float3 blendNormalVector = TangentNormalVector;
                float blendMetallic = 0;
                float blendSmoothness = 0;
                float blendOcclussion = 0;
                float3 blendEmissionColor = 0;
                
                float2 Tiling = BlendTiling;
                float2 Offset = BlendOffset;
                if (DistanceBlendingMode == 0)
                {
                    Tiling = BlendOverrideDistanceBlendingTO ? BlendDistanceBlendingScale : FarTiling;
                    Offset = BlendOverrideDistanceBlendingTO ? BlendDistanceBlendingOffset : FarOffset;
                }
                
                // Sample Material Blending Material
                GetSeamlessMaterialColor(
                    sampler_BlendAlbedo, UV, TangentNormalVector, Tiling, Offset,
                    SurfaceType, BlendAlphaClipping,
                    BlendSettingToggles, BlendAssignedTextures,
                    BlendNoiseAngleOffset, BlendNoiseScale, BlendNoiseScalingMinMax, BlendRandomiseRotationMinMax,
                    BlendAlbedo, BlendAlbedoTint,
                    BlendNormalMap, BlendNormalScale,
                    BlendMetallicMap, BlendMetallic,
                    BlendSmoothnessMap, BlendSmoothness,
                    BlendRoughnessMap, BlendRoughness,
                    BlendOcclussionMap, BlendOcclussionStrength,
                    BlendEmissionMap, BlendEmissionColor,
                    DebuggingIndex,
                    blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor);
                
                // Combine Blend with Base 
                farAlbedoColor = lerp(farAlbedoColor, blendAlbedoColor, materialMask);
                farNormalVector = lerp(farNormalVector, blendNormalVector, materialMask);
                farMetallic = lerp(farMetallic, blendMetallic, materialMask);
                farSmoothness = lerp(farSmoothness, blendSmoothness, materialMask);
                farOcclussion = lerp(farOcclussion, blendOcclussion, materialMask);
                farEmissionColor = lerp(farEmissionColor, blendEmissionColor, materialMask);
            }
            
            // Combine Far with Base
            albedoColor = lerp(albedoColor, farAlbedoColor, farDistance);
            normalVector = lerp(normalVector, farNormalVector, farDistance);
            metallic = lerp(metallic, farMetallic, farDistance);
            smoothness = lerp(smoothness, farSmoothness, farDistance);
            occlussion = lerp(occlussion, farOcclussion, farDistance);
            emissionColor = lerp(emissionColor, farEmissionColor, farDistance);
        }
    }
    
    // --------------------------------------------------------------- //
    
    // Debugging
    switch (DebuggingIndex) {
        case 2: // Distance Mask
            albedoColor = farDistance;
            break;
        case 3: // Blend Material mask
            albedoColor = materialMask;
            break;
    }
    
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

void GetSeamlessMaterialColorNEW(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex, // Material Properties

    float2 Settings, float4 TilingOffset, // Tiling & Offset
    float2 NoiseSettings, float4 NoiseMinMax, // Noise
    UnityTexture2D Albedo, // Albedo
    UnityTexture2D MetallicMap, // Metallic
    UnityTexture2D SmoothnessMap, // Smoothness
    UnityTexture2D RoughnessMap, // Roughness
    UnityTexture2D NormalMap, // Normal
    UnityTexture2D OcclussionMap, // Occlussion
    UnityTexture2D EmissionMap, // Emission
    float4 AlbedoTint, float4 EmissionColor, // Colors
    float4 MaterialProperties1, float2 MaterialProperties2, // Material Properties

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut) // Outputs
{
    // Get Setting Toggles
    int settingToggles = (int)Settings.x;
    
    bool noiseEnabled =          (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation =     (settingToggles & 4) != 0;
    bool smoothnessEnabled =     (settingToggles & 8) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int)Settings.y;
    
    bool metallicAssigned =   (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned =  (assignedTextures & 4) != 0;
    bool normalAssigned =     (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned =   (assignedTextures & 32) != 0;
    
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setup UVs
    float2 tiling = TilingOffset.xy;
    float2 offset = TilingOffset.zw;
    
    UV = UV * tiling + offset;
    
    // Noise Variables
    float noiseAngleOffset = NoiseSettings.x;
    float noiseScale = NoiseSettings.y;
    float2 noiseScalingMinMax = NoiseMinMax.xy;
    float2 randomiseRotationMinMax = NoiseMinMax.zw;
    
    // Material Properties
    float metallic = MaterialProperties1.x;
    float smoothness = MaterialProperties1.y;
    float roughness = MaterialProperties1.z;
    float normalScale = MaterialProperties1.w;
    float occlussionStrength = MaterialProperties2.x;
    float alphaClipping = MaterialProperties2.y;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessTextureUVs(UV, tiling, offset, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    // Debugging
    if (DebuggingIndex != -1) {
        switch (DebuggingIndex)
        {
            case 0: // Voronoi Cells
                AlbedoColorOut = VoronoiCells;
                break;
            case 1: // Edge Mask
                AlbedoColorOut = EdgeMask;
                break;
            default:
                AlbedoColorOut = 0;
                break;
        }
        
        return;
    }
    
    // Albedo
    AlbedoColorOut = SampleTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled) * AlbedoTint;
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Normal Map
    if (normalAssigned)
    {
        NormalVectorOut = SampleTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled, true, normalScale).rgb;
    }
    else
    {
        NormalVectorOut = TangentNormalVector;
    }
        
    
    // Metallic
    if (metallicAssigned)
        MetallicOut = SampleTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
    else
        MetallicOut = metallic;
    
    // Smoothness / Roughness
    if (smoothnessEnabled)
    {
        if (smoothnessAssigned)
            SmoothnessOut = SampleTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
        else
            SmoothnessOut = smoothness;
    }
    else
    {
        if (roughnessAssigned)
            SmoothnessOut = 1 - SampleTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r; // Roughness = 1 - Smoothness
        else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned)
    {
        OcclussionOut = SampleTexture(OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
        OcclussionOut = lerp(OcclussionOut, 1, 1 - occlussionStrength);
    }
    else
        OcclussionOut = 1;
    
    // Emission
    if (emissionAssigned)
        EmissionColorOut = SampleTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).rbg * EmissionColor;
    else
        EmissionColorOut = EmissionColor;
}

void SampleSeamlessMaterialNEW_float(
    float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition, // Positions
    int SurfaceType, float DebuggingIndex, // Enums

    // Base Material
    float2 BaseSettings, float4 BaseTilingOffset, // Tiling & Offset
    float2 BaseNoiseSettings, float4 BaseNoiseMinMax, // Noise
    UnityTexture2D BaseAlbedo, // Albedo
    UnityTexture2D BaseMetallicMap, // Metallic
    UnityTexture2D BaseSmoothnessMap, // Smoothness
    UnityTexture2D BaseRoughnessMap, // Roughness
    UnityTexture2D BaseNormalMap, // Normal
    UnityTexture2D BaseOcclussionMap, // Occlussion
    UnityTexture2D BaseEmissionMap, // Emission
    float4 BaseAlbedoTint, float4 BaseEmissionColor, // Colors
    float4 BaseMaterialProperties1, float2 BaseMaterialProperties2, // Material Properties

    // Far Material
    bool DistanceBlendingEnabled, int DistanceBlendingMode, float2 DistanceBlendMinMax, // Distance Blending

    float2 FarSettings, float4 FarTilingOffset, // Tiling & Offset
    float2 FarNoiseSettings, float4 FarNoiseMinMax, // Noise
    UnityTexture2D FarAlbedo, // Albedo
    UnityTexture2D FarMetallicMap, // Metallic
    UnityTexture2D FarSmoothnessMap, // Smoothness
    UnityTexture2D FarRoughnessMap, // Roughness
    UnityTexture2D FarNormalMap, // Normal
    UnityTexture2D FarOcclussionMap, // Occlussion
    UnityTexture2D FarEmissionMap, // Emission
    float4 FarAlbedoTint, float4 FarEmissionColor, // Colors
    float4 FarMaterialProperties1, float2 FarMaterialProperties2, // Material Properties

    // Blend Material
    float MaterialBlendSettings, int BlendMaskType, float4 BlendMaskDistanceTO,
    float2 MaterialBlendProperties, float3 MaterialBlendNoiseSettings,
    UnityTexture2D BlendMaskTexture, float4 BlendMaskTextureTO,

    float2 BlendSettings, float4 BlendTilingOffset, // Tiling & Offset
    float2 BlendNoiseSettings, float4 BlendNoiseMinMax, // Noise
    UnityTexture2D BlendAlbedo, // Albedo
    UnityTexture2D BlendMetallicMap, // Metallic
    UnityTexture2D BlendSmoothnessMap, // Smoothness
    UnityTexture2D BlendRoughnessMap, // Roughness
    UnityTexture2D BlendNormalMap, // Normal
    UnityTexture2D BlendOcclussionMap, // Occlussion
    UnityTexture2D BlendEmissionMap, // Emission
    float4 BlendAlbedoTint, float4 BlendEmissionColor, // Colors
    float4 BlendMaterialProperties1, float2 BlendMaterialProperties2, // Material Properties

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
    
    float materialMask = 0;
    float farDistance = 0;
    
    GetSeamlessMaterialColorNEW(
        sampler_BaseAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
        BaseSettings, BaseTilingOffset,
        BaseNoiseSettings, BaseNoiseMinMax,
        BaseAlbedo,
        BaseMetallicMap,
        BaseSmoothnessMap,
        BaseRoughnessMap,
        BaseNormalMap,
        BaseOcclussionMap,
        BaseEmissionMap,
        BaseAlbedoTint, BaseEmissionColor,
        BaseMaterialProperties1, BaseMaterialProperties2,
        albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor
    );
    
    // --------------------- Material Blending ----------------------- //
    
    int materialBlendSettings = (int)MaterialBlendSettings;
    
    bool materialBlendEnabled =       (materialBlendSettings & 1) != 0;
    bool overrideDistanceBlending =   (materialBlendSettings & 2) != 0;
    bool overrideDistanceBlendingTO = (materialBlendSettings & 4) != 0;
    
    if (materialBlendEnabled)
    {
        float blendMaskNoiseScale = MaterialBlendNoiseSettings.x;
        float2 blendMaskNoiseOffset = MaterialBlendNoiseSettings.yz;
        
        // Get mask of blended material
        switch (BlendMaskType)
        {
            case 0: // Perlin Noise
                materialMask = ClassicNoise(UV * blendMaskNoiseScale + blendMaskNoiseOffset) * 3;
                break;
            case 1: // Simplex Noise
                materialMask = SimplexNoise(UV * blendMaskNoiseScale + blendMaskNoiseOffset) * 2;
                break;
            case 2: // Custom Texture
                materialMask = SAMPLE_TEXTURE2D(BlendMaskTexture, sampler_BlendMaskTexture, UV * blendMaskNoiseScale + blendMaskNoiseOffset);
                break;
        }
        
        float blendMaskOpacity = MaterialBlendProperties.x;
        float blendMaskStrength = MaterialBlendProperties.y;
        
        materialMask *= blendMaskStrength;
        materialMask = clamp(materialMask, 0, 1);
        materialMask *= blendMaskOpacity;
        
        if (materialMask > 0)
        {
            float4 blendAlbedoColor = 1;
            float3 blendNormalVector = TangentNormalVector;
            float blendMetallic = 0;
            float blendSmoothness = 0;
            float blendOcclussion = 0;
            float3 blendEmissionColor = 0;
            
            // Sample Blend Material
            GetSeamlessMaterialColorNEW(
                sampler_BaseAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                BlendSettings, BlendTilingOffset,
                BlendNoiseSettings, BlendNoiseMinMax,
                BlendAlbedo,
                BlendMetallicMap,
                BlendSmoothnessMap,
                BlendRoughnessMap,
                BlendNormalMap,
                BlendOcclussionMap,
                BlendEmissionMap,
                BlendAlbedoTint, BlendEmissionColor,
                BlendMaterialProperties1, BlendMaterialProperties2,
                blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
            );
            
            // Combine Blend with Base
            albedoColor = lerp(albedoColor, blendAlbedoColor, materialMask);
            normalVector = lerp(normalVector, blendNormalVector, materialMask);
            metallic = lerp(metallic, blendMetallic, materialMask);
            smoothness = lerp(smoothness, blendSmoothness, materialMask);
            occlussion = lerp(occlussion, blendOcclussion, materialMask);
            emissionColor = lerp(emissionColor, blendEmissionColor, materialMask);
        }
    }
    
    // --------------------- Distance Blending ----------------------- //
    
    if (DistanceBlendingEnabled)
    {
        // Distance Mask
        farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);
        
        // Only calculate far distance if required
        if (farDistance > 0)
        {
            float4 farAlbedoColor = 1;
            float3 farNormalVector = TangentNormalVector;
            float farMetallic = 0;
            float farSmoothness = 0;
            float farOcclussion = 0;
            float3 farEmissionColor = 0;
        
            switch (DistanceBlendingMode)
            {
                case 0: // Tiling & Offset
                    // Sample Base Material
                    GetSeamlessMaterialColorNEW(
                        sampler_BaseAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        BaseSettings, FarTilingOffset,
                        BaseNoiseSettings, BaseNoiseMinMax,
                        BaseAlbedo,
                        BaseMetallicMap,
                        BaseSmoothnessMap,
                        BaseRoughnessMap,
                        BaseNormalMap,
                        BaseOcclussionMap,
                        BaseEmissionMap,
                        BaseAlbedoTint, BaseEmissionColor,
                        BaseMaterialProperties1, BaseMaterialProperties2,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                    break;
                case 1: // Material
                // Sample Far Material
                    GetSeamlessMaterialColorNEW(
                        sampler_FarAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        FarSettings, FarTilingOffset,
                        FarNoiseSettings, FarNoiseMinMax,
                        FarAlbedo,
                        FarMetallicMap,
                        FarSmoothnessMap,
                        FarRoughnessMap,
                        FarNormalMap,
                        FarOcclussionMap,
                        FarEmissionMap,
                        FarAlbedoTint, FarEmissionColor,
                        FarMaterialProperties1, FarMaterialProperties2,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                    break;
            }
        
            // Blend material
            if (materialMask > 0 && overrideDistanceBlending)
            {
                float4 blendAlbedoColor = 1;
                float3 blendNormalVector = TangentNormalVector;
                float blendMetallic = 0;
                float blendSmoothness = 0;
                float blendOcclussion = 0;
                float3 blendEmissionColor = 0;
                
                float2 tiling = BlendTilingOffset.xy;
                float2 offset = BlendTilingOffset.zw;
                if (DistanceBlendingMode == 0)
                {
                    tiling = overrideDistanceBlendingTO ? BlendMaskDistanceTO.xy : FarTilingOffset.xy;
                    offset = overrideDistanceBlendingTO ? BlendMaskDistanceTO.zw : FarTilingOffset.zw;
                }
                
                float4 tilingOffset = float4(tiling.x, tiling.y, offset.x, offset.y);
                
                // Sample Material Blending Material
                GetSeamlessMaterialColorNEW(
                        sampler_BlendAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        BlendSettings, tilingOffset,
                        BlendNoiseSettings, BlendNoiseMinMax,
                        BlendAlbedo,
                        BlendMetallicMap,
                        BlendSmoothnessMap,
                        BlendRoughnessMap,
                        BlendNormalMap,
                        BlendOcclussionMap,
                        BlendEmissionMap,
                        BlendAlbedoTint, BlendEmissionColor,
                        BlendMaterialProperties1, BlendMaterialProperties2,
                        blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
                    );
                
                // Combine Blend with Base 
                farAlbedoColor = lerp(farAlbedoColor, blendAlbedoColor, materialMask);
                farNormalVector = lerp(farNormalVector, blendNormalVector, materialMask);
                farMetallic = lerp(farMetallic, blendMetallic, materialMask);
                farSmoothness = lerp(farSmoothness, blendSmoothness, materialMask);
                farOcclussion = lerp(farOcclussion, blendOcclussion, materialMask);
                farEmissionColor = lerp(farEmissionColor, blendEmissionColor, materialMask);
            }
            
            // Combine Far with Base
            albedoColor = lerp(albedoColor, farAlbedoColor, farDistance);
            normalVector = lerp(normalVector, farNormalVector, farDistance);
            metallic = lerp(metallic, farMetallic, farDistance);
            smoothness = lerp(smoothness, farSmoothness, farDistance);
            occlussion = lerp(occlussion, farOcclussion, farDistance);
            emissionColor = lerp(emissionColor, farEmissionColor, farDistance);
        }
    }
    
    // --------------------------------------------------------------- //
    
    // Debugging
    switch (DebuggingIndex)
    {
        case 2: // Distance Mask
            albedoColor = farDistance;
            break;
        case 3: // Blend Material mask
            albedoColor = materialMask;
            break;
    }
    
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