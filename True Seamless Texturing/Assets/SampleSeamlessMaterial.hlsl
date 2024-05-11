#ifndef SAMPLESEAMLESSMATERIAL_INCLUDED
#define SAMPLESEAMLESSMATERIAL_INCLUDED

#include "CustomVoronoi.hlsl"
#include "TextureUtilities.hlsl"

void GetSeamlessTextureUVs(
    float2 UV, float2 Tiling, float2 Offset, // UV
    float NoiseAngleOffset, float NoiseScale, bool RandomiseNoiseScaling, float2 NoiseScalingMinMax, // Noise
    out float VoronoiCells, out float EdgeMask, out float2 EdgeUV, out float2 TransformedUV) // Outputs
{
    // Setup UVs
    UV = UV * Tiling + Offset;
    
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
    float randomCellDegrees = VoronoiCells * 360;
    TransformedUV = RotateUVDegrees(TransformedUV, 0.0, randomCellDegrees);
    
    // --------------------------------------------------------------- //
}

void GetSeamlessMaterialColor(
    UnitySamplerState SS, float2 UV, float3 TangentNormalVector, float2 Tiling, float2 Offset, // UV
    float NoiseAngleOffset, float NoiseScale, bool RandomiseNoiseScaling, float2 NoiseScalingMinMax, // Noise

    UnityTexture2D Albedo, float4 AlbedoTint, // Albedo
    UnityTexture2D NormalMap, float NormalScale, // Normal
    UnityTexture2D MetallicMap, float Metallic, // Metallic
    bool SmoothnessEnabled, UnityTexture2D SmoothnessMap, float Smoothness, // Smoothness
    UnityTexture2D RoughnessMap, float Roughness, // Roughness
    UnityTexture2D EmissionMap, float4 EmissionColor, // Emission
    float AssignedTextures, // Assigned textures

    float DebuggingIndex, // Debugging

    out float4 AlbedoColorOut, out float4 NormalColorOut, out float4 MetallicColorOut, out float4 SmoothnessColorOut, out float4 EmissionColorOut) // Outputs
{
    // Get Assigned Textures
    bool metallicAssigned = ((int)AssignedTextures & 1) != 0;
    bool smoothnessAssigned = ((int)AssignedTextures & 2) != 0;
    bool roughnessAssigned = ((int)AssignedTextures & 4) != 0;
    bool normalAssigned = ((int)AssignedTextures & 8) != 0;
    bool emissionAssigned = ((int)AssignedTextures & 16) != 0;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells;
    float EdgeMask;
    float2 EdgeUV;
    float2 TransformedUV;
    GetSeamlessTextureUVs(UV, Tiling, Offset, NoiseAngleOffset, NoiseScale, RandomiseNoiseScaling, NoiseScalingMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);

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
                AlbedoColorOut = float4(TransformedUV.x, TransformedUV.y, 0, 0);
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
    AlbedoColorOut = SampleTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV) * AlbedoTint;
    
    // Normal Map
    if (normalAssigned) {
        NormalColorOut = SampleTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, true, NormalScale);
    } else {
        float3 normal = TangentNormalVector;
        NormalColorOut = float4(normal.x, normal.y, normal.z, 0);
    }
        
    
    // Metallic
    if (metallicAssigned)
        MetallicColorOut = SampleTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV);
    else
        MetallicColorOut = Metallic;
    
    // Smoothness / Roughness
    if (SmoothnessEnabled) {
        if (smoothnessAssigned)
            SmoothnessColorOut = SampleTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV);
        else
            SmoothnessColorOut = Smoothness;
    } else {
        if(roughnessAssigned)
            SmoothnessColorOut = 1 - SampleTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV); // Roughness = 1 - Smoothness
        else
            SmoothnessColorOut = 1 - Roughness;
    }
        
    // Emission
    if (emissionAssigned)
        EmissionColorOut = SampleTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV) * EmissionColor;
    else
        EmissionColorOut = EmissionColor;
}

void SampleSeamlessMaterial_float(
    UnitySamplerState SS, float2 UV, float3 TangentNormalVector,
    bool DistanceBlendingEnabled, float2 DistanceBlendMinMax, // Distance Blending
    float3 WorldPosition, float3 CameraPosition, // Positions

    // Base Material
    float2 BaseTiling, float2 BaseOffset,
    float BaseNoiseAngleOffset, float BaseNoiseScale, bool BaseRandomiseNoiseScaling, float2 BaseNoiseScalingMinMax, // Noise
    UnityTexture2D BaseAlbedo, float4 BaseAlbedoTint, // Albedo
    UnityTexture2D BaseNormalMap, float BaseNormalScale, // Normal
    UnityTexture2D BaseMetallicMap, float BaseMetallic, // Metallic
    bool BaseSmoothnessEnabled, UnityTexture2D BaseSmoothnessMap, float BaseSmoothness, // Smoothness
    UnityTexture2D BaseRoughnessMap, float BaseRoughness, // Roughness
    UnityTexture2D BaseEmissionMap, float4 BaseEmissionColor, // Emission
    float BaseAssignedTextures, // Assigned Textures

    // Far Material
    float2 FarTiling, float2 FarOffset,
    float FarNoiseAngleOffset, float FarNoiseScale, bool FarRandomiseNoiseScaling, float2 FarNoiseScalingMinMax, // Noise
    UnityTexture2D FarAlbedo, float4 FarAlbedoTint, // Albedo
    UnityTexture2D FarNormalMap, float FarNormalScale, // Normal
    UnityTexture2D FarMetallicMap, float FarMetallic, // Metallic
    bool FarSmoothnessEnabled, UnityTexture2D FarSmoothnessMap, float FarSmoothness, // Smoothness
    UnityTexture2D FarRoughnessMap, float FarRoughness, // Roughness
    UnityTexture2D FarEmissionMap, float4 FarEmissionColor, // Emission
    float FarAssignedTextures, // Assigned Textures

    float DebuggingIndex, // Debugging

    // Outputs
    out float4 AlbedoColorOut, out float4 NormalColorOut, out float4 MetallicColorOut, out float4 SmoothnessColorOut, out float4 EmissionColorOut)
{
    // ----------------------- Base Material ------------------------- //
    
    float4 albedoColor;
    float4 normalColor;
    float4 metallicColor;
    float4 smoothnessColor;
    float4 emissionColor;
    
    GetSeamlessMaterialColor(
        SS, UV, TangentNormalVector, BaseTiling, BaseOffset,
        BaseNoiseAngleOffset, BaseNoiseScale, BaseRandomiseNoiseScaling, BaseNoiseScalingMinMax,
        BaseAlbedo, BaseAlbedoTint,
        BaseNormalMap, BaseNormalScale,
        BaseMetallicMap, BaseMetallic,
        BaseSmoothnessEnabled, BaseSmoothnessMap, BaseSmoothness,
        BaseRoughnessMap, BaseRoughness,
        BaseEmissionMap, BaseEmissionColor,
        BaseAssignedTextures,
        DebuggingIndex,
        albedoColor, normalColor, metallicColor, smoothnessColor, emissionColor);
    
    // --------------------- Distance Blending ----------------------- //
    
    if (DistanceBlendingEnabled) {
        // Distance Mask
        float farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);
        
        // Sample Far Material
        float4 farAlbedoColor;
        float4 farNormalColor;
        float4 farMetallicColor;
        float4 farSmoothnessColor;
        float4 farEmissionColor;
        
        GetSeamlessMaterialColor(
            SS, UV, TangentNormalVector, FarTiling, FarOffset,
            FarNoiseAngleOffset, FarNoiseScale, FarRandomiseNoiseScaling, FarNoiseScalingMinMax,
            FarAlbedo, FarAlbedoTint,
            FarNormalMap, FarNormalScale,
            FarMetallicMap, FarMetallic,
            FarSmoothnessEnabled, FarSmoothnessMap, FarSmoothness,
            FarRoughnessMap, FarRoughness,
            FarEmissionMap, FarEmissionColor,
            FarAssignedTextures,
            DebuggingIndex,
            farAlbedoColor, farNormalColor, farMetallicColor, farSmoothnessColor, farEmissionColor);
        
        // Distance Mask Debug
        if (DebuggingIndex == 2)
            albedoColor = farDistance;
        
        // Combine Far with Base
        albedoColor = BlendOverwrite(albedoColor, farAlbedoColor, farDistance);
        normalColor = BlendOverwrite(normalColor, farNormalColor, farDistance);
        metallicColor = BlendOverwrite(metallicColor, farMetallicColor, farDistance);
        smoothnessColor = BlendOverwrite(smoothnessColor, farSmoothnessColor, farDistance);
        emissionColor = BlendOverwrite(emissionColor, farEmissionColor, farDistance);
    }
    
    // --------------------------------------------------------------- //
    
    AlbedoColorOut = albedoColor;
    NormalColorOut = normalColor;
    MetallicColorOut = metallicColor;
    SmoothnessColorOut = smoothnessColor;
    EmissionColorOut = emissionColor;
}

#endif