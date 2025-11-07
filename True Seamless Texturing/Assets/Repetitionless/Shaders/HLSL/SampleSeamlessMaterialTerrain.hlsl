#ifndef SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED
#define SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED

#include "SampleSeamlessArrayMaterial.hlsl"
#include "Utilities/TextureUtilities.hlsl"

// Essentially just calling the main SampleSeamlessMaterial function 4 times, once for each layer
// It does do more things and the function is modified for texture arrays but you get the idea

void SampleSeamlessMaterialTerrain_float(
    SamplerState SS, float2 UV, float3 WorldNormalVector, // Control requires clamped sampler state, otherwise fades off at edges of terrain
    float3 WorldPosition, float3 CameraPosition, // Positions
    UnityTexture2D Holes, UnityTexture2D Control,
    float SurfaceType, float DebuggingIndex, // Enums

    // ------- TERRAIN LAYER 1 ------- //

    // Base Material
    float2 Layer1BaseSettings, float4 Layer1BaseTilingOffset, // Tiling & Offset
    UnityTexture2D Layer1BaseAlbedo, // Albedo
    UnityTexture2D Layer1BaseMetallicMap, // Metallic
    UnityTexture2D Layer1BaseSmoothnessMap, // Smoothness
    UnityTexture2D Layer1BaseRoughnessMap, // Roughness
    UnityTexture2D Layer1BaseNormalMap, // Normal
    UnityTexture2D Layer1BaseOcclussionMap, // Occlussion
    UnityTexture2D Layer1BaseEmissionMap, // Emission
    float4 Layer1BaseAlbedoTint, float3 Layer1BaseEmissionColor, // Colors
    float4 Layer1BaseMaterialProperties1, float2 Layer1BaseMaterialProperties2, // Material Properties

    float2 Layer1BaseNoiseSettings, float4 Layer1BaseNoiseMinMax, // Noise

    float Layer1BaseVariationMode, float4 Layer1BaseVariationSettings, float Layer1BaseVariationBrightness, // Variation Settings
    float4 Layer1BaseVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer1BaseVariationTexture, float4 Layer1BaseVariationTextureTO, // Variation Texture

    // Far Material
    bool Layer1DistanceBlendEnabled, int Layer1DistanceBlendingMode, float2 Layer1DistanceBlendMinMax, // Distance Blending

    float2 Layer1FarSettings, float4 Layer1FarTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer1FarTextures, UnityTexture2D Layer1FarNormalMap, // Textures
    int Layer1FarArrayAssignedTextures, // Array Assigned Textures
    float4 Layer1FarAlbedoTint, float3 Layer1FarEmissionColor, // Colors
    float4 Layer1FarMaterialProperties1, float2 Layer1FarMaterialProperties2, // Material Properties

    float2 Layer1FarNoiseSettings, float4 Layer1FarNoiseMinMax, // Noise

    float Layer1FarVariationMode, float4 Layer1FarVariationSettings, float Layer1FarVariationBrightness, // Variation Settings
    float4 Layer1FarVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer1FarVariationTexture, float4 Layer1FarVariationTextureTO, // Variation Texture

    // Blend Material
    float Layer1MaterialBlendSettings, int Layer1BlendMaskType, float4 Layer1BlendMaskDistanceTO,
    float2 Layer1MaterialBlendProperties, float3 Layer1MaterialBlendNoiseSettings,
    UnityTexture2D Layer1BlendMaskTexture, float4 Layer1BlendMaskTextureTO,

    float2 Layer1BlendSettings, float4 Layer1BlendTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer1BlendTextures, UnityTexture2D Layer1BlendNormalMap, // Textures
    int Layer1BlendArrayAssignedTextures, // Array Assigned Textures
    float4 Layer1BlendAlbedoTint, float3 Layer1BlendEmissionColor, // Colors
    float4 Layer1BlendMaterialProperties1, float2 Layer1BlendMaterialProperties2, // Material Properties

    float2 Layer1BlendNoiseSettings, float4 Layer1BlendNoiseMinMax, // Noise

    float Layer1BlendVariationMode, float4 Layer1BlendVariationSettings, float Layer1BlendVariationBrightness, // Variation Settings
    float4 Layer1BlendVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer1BlendVariationTexture, float4 Layer1BlendVariationTextureTO, // Variation Texture

    // ------- TERRAIN LAYER 2 ------- //

    // Base Material
    float2 Layer2BaseSettings, float4 Layer2BaseTilingOffset, // Tiling & Offset
    UnityTexture2D Layer2BaseAlbedo, // Albedo
    UnityTexture2D Layer2BaseMetallicMap, // Metallic
    UnityTexture2D Layer2BaseSmoothnessMap, // Smoothness
    UnityTexture2D Layer2BaseRoughnessMap, // Roughness
    UnityTexture2D Layer2BaseNormalMap, // Normal
    UnityTexture2D Layer2BaseOcclussionMap, // Occlussion
    UnityTexture2D Layer2BaseEmissionMap, // Emission
    float4 Layer2BaseAlbedoTint, float3 Layer2BaseEmissionColor, // Colors
    float4 Layer2BaseMaterialProperties1, float2 Layer2BaseMaterialProperties2, // Material Properties

    float2 Layer2BaseNoiseSettings, float4 Layer2BaseNoiseMinMax, // Noise

    float Layer2BaseVariationMode, float4 Layer2BaseVariationSettings, float Layer2BaseVariationBrightness, // Variation Settings
    float4 Layer2BaseVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer2BaseVariationTexture, float4 Layer2BaseVariationTextureTO, // Variation Texture

    // Far Material
    bool Layer2DistanceBlendEnabled, int Layer2DistanceBlendingMode, float2 Layer2DistanceBlendMinMax, // Distance Blending

    float2 Layer2FarSettings, float4 Layer2FarTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer2FarTextures, UnityTexture2D Layer2FarNormalMap, // Textures
    int Layer2FarArrayAssignedTextures, // Array Assigned Textures
    float4 Layer2FarAlbedoTint, float3 Layer2FarEmissionColor, // Colors
    float4 Layer2FarMaterialProperties1, float2 Layer2FarMaterialProperties2, // Material Properties

    float2 Layer2FarNoiseSettings, float4 Layer2FarNoiseMinMax, // Noise

    float Layer2FarVariationMode, float4 Layer2FarVariationSettings, float Layer2FarVariationBrightness, // Variation Settings
    float4 Layer2FarVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer2FarVariationTexture, float4 Layer2FarVariationTextureTO, // Variation Texture

    // Blend Material
    float Layer2MaterialBlendSettings, int Layer2BlendMaskType, float4 Layer2BlendMaskDistanceTO,
    float2 Layer2MaterialBlendProperties, float3 Layer2MaterialBlendNoiseSettings,
    UnityTexture2D Layer2BlendMaskTexture, float4 Layer2BlendMaskTextureTO,

    float2 Layer2BlendSettings, float4 Layer2BlendTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer2BlendTextures, UnityTexture2D Layer2BlendNormalMap, // Textures
    int Layer2BlendArrayAssignedTextures, // Array Assigned Textures
    float4 Layer2BlendAlbedoTint, float3 Layer2BlendEmissionColor, // Colors
    float4 Layer2BlendMaterialProperties1, float2 Layer2BlendMaterialProperties2, // Material Properties

    float2 Layer2BlendNoiseSettings, float4 Layer2BlendNoiseMinMax, // Noise

    float Layer2BlendVariationMode, float4 Layer2BlendVariationSettings, float Layer2BlendVariationBrightness, // Variation Settings
    float4 Layer2BlendVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer2BlendVariationTexture, float4 Layer2BlendVariationTextureTO, // Variation Texture

    // ------- TERRAIN LAYER 3 ------- //

    // Base Material
    float2 Layer3BaseSettings, float4 Layer3BaseTilingOffset, // Tiling & Offset
    UnityTexture2D Layer3BaseAlbedo, // Albedo
    UnityTexture2D Layer3BaseMetallicMap, // Metallic
    UnityTexture2D Layer3BaseSmoothnessMap, // Smoothness
    UnityTexture2D Layer3BaseRoughnessMap, // Roughness
    UnityTexture2D Layer3BaseNormalMap, // Normal
    UnityTexture2D Layer3BaseOcclussionMap, // Occlussion
    UnityTexture2D Layer3BaseEmissionMap, // Emission
    float4 Layer3BaseAlbedoTint, float3 Layer3BaseEmissionColor, // Colors
    float4 Layer3BaseMaterialProperties1, float2 Layer3BaseMaterialProperties2, // Material Properties

    float2 Layer3BaseNoiseSettings, float4 Layer3BaseNoiseMinMax, // Noise

    float Layer3BaseVariationMode, float4 Layer3BaseVariationSettings, float Layer3BaseVariationBrightness, // Variation Settings
    float4 Layer3BaseVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer3BaseVariationTexture, float4 Layer3BaseVariationTextureTO, // Variation Texture

    // Far Material
    bool Layer3DistanceBlendEnabled, int Layer3DistanceBlendingMode, float2 Layer3DistanceBlendMinMax, // Distance Blending

    float2 Layer3FarSettings, float4 Layer3FarTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer3FarTextures, UnityTexture2D Layer3FarNormalMap, // Textures
    int Layer3FarArrayAssignedTextures, // Array Assigned Textures
    float4 Layer3FarAlbedoTint, float3 Layer3FarEmissionColor, // Colors
    float4 Layer3FarMaterialProperties1, float2 Layer3FarMaterialProperties2, // Material Properties

    float2 Layer3FarNoiseSettings, float4 Layer3FarNoiseMinMax, // Noise

    float Layer3FarVariationMode, float4 Layer3FarVariationSettings, float Layer3FarVariationBrightness, // Variation Settings
    float4 Layer3FarVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer3FarVariationTexture, float4 Layer3FarVariationTextureTO, // Variation Texture

    // Blend Material
    float Layer3MaterialBlendSettings, int Layer3BlendMaskType, float4 Layer3BlendMaskDistanceTO,
    float2 Layer3MaterialBlendProperties, float3 Layer3MaterialBlendNoiseSettings,
    UnityTexture2D Layer3BlendMaskTexture, float4 Layer3BlendMaskTextureTO,

    float2 Layer3BlendSettings, float4 Layer3BlendTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer3BlendTextures, UnityTexture2D Layer3BlendNormalMap, // Textures
    int Layer3BlendArrayAssignedTextures, // Array Assigned Textures
    float4 Layer3BlendAlbedoTint, float3 Layer3BlendEmissionColor, // Colors
    float4 Layer3BlendMaterialProperties1, float2 Layer3BlendMaterialProperties2, // Material Properties

    float2 Layer3BlendNoiseSettings, float4 Layer3BlendNoiseMinMax, // Noise

    float Layer3BlendVariationMode, float4 Layer3BlendVariationSettings, float Layer3BlendVariationBrightness, // Variation Settings
    float4 Layer3BlendVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer3BlendVariationTexture, float4 Layer3BlendVariationTextureTO, // Variation Texture

    // ------- TERRAIN LAYER 4 ------- //

    // Base Material
    float2 Layer4BaseSettings, float4 Layer4BaseTilingOffset, // Tiling & Offset
    UnityTexture2D Layer4BaseAlbedo, // Albedo
    UnityTexture2D Layer4BaseMetallicMap, // Metallic
    UnityTexture2D Layer4BaseSmoothnessMap, // Smoothness
    UnityTexture2D Layer4BaseRoughnessMap, // Roughness
    UnityTexture2D Layer4BaseNormalMap, // Normal
    UnityTexture2D Layer4BaseOcclussionMap, // Occlussion
    UnityTexture2D Layer4BaseEmissionMap, // Emission
    float4 Layer4BaseAlbedoTint, float3 Layer4BaseEmissionColor, // Colors
    float4 Layer4BaseMaterialProperties1, float2 Layer4BaseMaterialProperties2, // Material Properties

    float2 Layer4BaseNoiseSettings, float4 Layer4BaseNoiseMinMax, // Noise

    float Layer4BaseVariationMode, float4 Layer4BaseVariationSettings, float Layer4BaseVariationBrightness, // Variation Settings
    float4 Layer4BaseVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer4BaseVariationTexture, float4 Layer4BaseVariationTextureTO, // Variation Texture

    // Far Material
    bool Layer4DistanceBlendEnabled, int Layer4DistanceBlendingMode, float2 Layer4DistanceBlendMinMax, // Distance Blending

    float2 Layer4FarSettings, float4 Layer4FarTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer4FarTextures, UnityTexture2D Layer4FarNormalMap, // Textures
    int Layer4FarArrayAssignedTextures, // Array Assigned Textures
    float4 Layer4FarAlbedoTint, float3 Layer4FarEmissionColor, // Colors
    float4 Layer4FarMaterialProperties1, float2 Layer4FarMaterialProperties2, // Material Properties

    float2 Layer4FarNoiseSettings, float4 Layer4FarNoiseMinMax, // Noise

    float Layer4FarVariationMode, float4 Layer4FarVariationSettings, float Layer4FarVariationBrightness, // Variation Settings
    float4 Layer4FarVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer4FarVariationTexture, float4 Layer4FarVariationTextureTO, // Variation Texture

    // Blend Material
    float Layer4MaterialBlendSettings, int Layer4BlendMaskType, float4 Layer4BlendMaskDistanceTO,
    float2 Layer4MaterialBlendProperties, float3 Layer4MaterialBlendNoiseSettings,
    UnityTexture2D Layer4BlendMaskTexture, float4 Layer4BlendMaskTextureTO,

    float2 Layer4BlendSettings, float4 Layer4BlendTilingOffset, // Tiling & Offset
    UnityTexture2DArray Layer4BlendTextures, UnityTexture2D Layer4BlendNormalMap, // Textures
    int Layer4BlendArrayAssignedTextures, // Array Assigned Textures
    float4 Layer4BlendAlbedoTint, float3 Layer4BlendEmissionColor, // Colors
    float4 Layer4BlendMaterialProperties1, float2 Layer4BlendMaterialProperties2, // Material Properties

    float2 Layer4BlendNoiseSettings, float4 Layer4BlendNoiseMinMax, // Noise

    float Layer4BlendVariationMode, float4 Layer4BlendVariationSettings, float Layer4BlendVariationBrightness, // Variation Settings
    float4 Layer4BlendVariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer4BlendVariationTexture, float4 Layer4BlendVariationTextureTO, // Variation Texture

    // Outputs
    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut)
{
    // Setup control for additive blending (Using over lerp as it removes white borders)
    // Use control sampler as a custom one will cause layers above 0 to fade out at the edge of terrains
    float4 controlColor = SAMPLE_TEXTURE2D(Control, sampler_Control, UV);
    float controlSum = dot(controlColor, 1.0);
    float backgroundControl = saturate(1 - controlSum);
    if (controlSum > 1.0)
        controlColor /= controlSum;
    
    // Matrix for transforming tangent to world normals
    //float3x3 TBN = float3x3(WorldTangent, WorldBitangent, WorldNormalVector);

    // Variables
    float4 albedoColor = backgroundControl;
    float3 normalVector = WorldNormalVector * backgroundControl;
    float metallic = 0;
    float smoothness = 0;
    float occlussion = backgroundControl;
    float3 emission = 0;
    
    // ----------------------- Layer 1 ------------------------- //

    float layer1Control = controlColor.r;
    if (layer1Control > 0) {
        float4 layerAlbedo = albedoColor;
        float3 layerNormal = normalVector;
        float layerMetallic = metallic;
        float layerSmoothness = smoothness;
        float layerOcclussion = occlussion;
        float3 layerEmission = emission;
        
        // Sample layer
        SampleSeamlessArrayMaterial(
            SS, UV, WorldNormalVector,
            WorldPosition, CameraPosition, // Positions
            SurfaceType, DebuggingIndex, // Enums
        
            // Base Material
            Layer1BaseSettings, Layer1BaseTilingOffset, // Tiling & Offset
            Layer1BaseAlbedo, // Albedo
            Layer1BaseMetallicMap, // Metallic
            Layer1BaseSmoothnessMap, // Smoothness
            Layer1BaseRoughnessMap, // Roughness
            Layer1BaseNormalMap, // Normal
            Layer1BaseOcclussionMap, // Occlussion
            Layer1BaseEmissionMap, // Emission
            Layer1BaseAlbedoTint, Layer1BaseEmissionColor, // Colors
            Layer1BaseMaterialProperties1, Layer1BaseMaterialProperties2, // Material Properties
        
            Layer1BaseNoiseSettings, Layer1BaseNoiseMinMax, // Noise
        
            Layer1BaseVariationMode, Layer1BaseVariationSettings, Layer1BaseVariationBrightness, // Variation Settings
            Layer1BaseVariationNoiseSettings, // Variation Noise
            Layer1BaseVariationTexture, Layer1BaseVariationTextureTO, // Variation Texture
        
            // Far Material
            Layer1DistanceBlendEnabled, Layer1DistanceBlendingMode, Layer1DistanceBlendMinMax, // Distance Blending
        
            Layer1FarSettings, Layer1FarTilingOffset, // Tiling & Offset
            Layer1FarTextures, Layer1FarNormalMap, // Textures
            Layer1FarArrayAssignedTextures, // Array Assigned Textures
            Layer1FarAlbedoTint, Layer1FarEmissionColor, // Colors
            Layer1FarMaterialProperties1, Layer1FarMaterialProperties2, // Material Properties
        
            Layer1FarNoiseSettings, Layer1FarNoiseMinMax, // Noise
        
            Layer1FarVariationMode, Layer1FarVariationSettings, Layer1FarVariationBrightness, // Variation Settings
            Layer1FarVariationNoiseSettings, // Variation Noise
            Layer1FarVariationTexture, Layer1FarVariationTextureTO, // Variation Texture
        
            // Blend Material
            Layer1MaterialBlendSettings, Layer1BlendMaskType, Layer1BlendMaskDistanceTO,
            Layer1MaterialBlendProperties, Layer1MaterialBlendNoiseSettings,
            Layer1BlendMaskTexture, Layer1BlendMaskTextureTO,
        
            Layer1BlendSettings, Layer1BlendTilingOffset, // Tiling & Offset
            Layer1BlendTextures, Layer1BlendNormalMap, // Textures
            Layer1BlendArrayAssignedTextures, // Array Assigned Textures
            Layer1BlendAlbedoTint, Layer1BlendEmissionColor, // Colors
            Layer1BlendMaterialProperties1, Layer1BlendMaterialProperties2, // Material Properties
        
            Layer1BlendNoiseSettings, Layer1BlendNoiseMinMax, // Noise
        
            Layer1BlendVariationMode, Layer1BlendVariationSettings, Layer1BlendVariationBrightness, // Variation Settings
            Layer1BlendVariationNoiseSettings, // Variation Noise
            Layer1BlendVariationTexture, Layer1BlendVariationTextureTO, // Variation Texture
        
            // Outputs
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerEmission
        );

        albedoColor += layerAlbedo * layer1Control;
        normalVector += layerNormal * layer1Control;
        metallic += layerMetallic * layer1Control;
        smoothness += layerSmoothness * layer1Control;
        occlussion += layerOcclussion * layer1Control;
        emission += layerEmission * layer1Control;
    }
    
    // ----------------------- Layer 2 ------------------------- //
    
    float layer2Control = controlColor.g;
    if (layer2Control > 0)
    {
        float4 layerAlbedo = albedoColor;
        float3 layerNormal = normalVector;
        float layerMetallic = metallic;
        float layerSmoothness = smoothness;
        float layerOcclussion = occlussion;
        float3 layerEmission = emission;
        
        // Sample layer
        SampleSeamlessArrayMaterial(
            SS, UV, WorldNormalVector,
            WorldPosition, CameraPosition, // Positions
            SurfaceType, DebuggingIndex, // Enums
        
            // Base Material
            Layer2BaseSettings, Layer2BaseTilingOffset, // Tiling & Offset
            Layer2BaseAlbedo, // Albedo
            Layer2BaseMetallicMap, // Metallic
            Layer2BaseSmoothnessMap, // Smoothness
            Layer2BaseRoughnessMap, // Roughness
            Layer2BaseNormalMap, // Normal
            Layer2BaseOcclussionMap, // Occlussion
            Layer2BaseEmissionMap, // Emission
            Layer2BaseAlbedoTint, Layer2BaseEmissionColor, // Colors
            Layer2BaseMaterialProperties1, Layer2BaseMaterialProperties2, // Material Properties
        
            Layer2BaseNoiseSettings, Layer2BaseNoiseMinMax, // Noise
        
            Layer2BaseVariationMode, Layer2BaseVariationSettings, Layer2BaseVariationBrightness, // Variation Settings
            Layer2BaseVariationNoiseSettings, // Variation Noise
            Layer2BaseVariationTexture, Layer2BaseVariationTextureTO, // Variation Texture
        
            // Far Material
            Layer2DistanceBlendEnabled, Layer2DistanceBlendingMode, Layer2DistanceBlendMinMax, // Distance Blending
        
            Layer2FarSettings, Layer2FarTilingOffset, // Tiling & Offset
            Layer2FarTextures, Layer2FarNormalMap, // Textures
            Layer2FarArrayAssignedTextures, // Array Assigned Textures
            Layer2FarAlbedoTint, Layer2FarEmissionColor, // Colors
            Layer2FarMaterialProperties1, Layer2FarMaterialProperties2, // Material Properties
        
            Layer2FarNoiseSettings, Layer2FarNoiseMinMax, // Noise
        
            Layer2FarVariationMode, Layer2FarVariationSettings, Layer2FarVariationBrightness, // Variation Settings
            Layer2FarVariationNoiseSettings, // Variation Noise
            Layer2FarVariationTexture, Layer2FarVariationTextureTO, // Variation Texture
        
            // Blend Material
            Layer2MaterialBlendSettings, Layer2BlendMaskType, Layer2BlendMaskDistanceTO,
            Layer2MaterialBlendProperties, Layer2MaterialBlendNoiseSettings,
            Layer2BlendMaskTexture, Layer2BlendMaskTextureTO,
        
            Layer2BlendSettings, Layer2BlendTilingOffset, // Tiling & Offset
            Layer2BlendTextures, Layer2BlendNormalMap, // Textures
            Layer2BlendArrayAssignedTextures, // Array Assigned Textures
            Layer2BlendAlbedoTint, Layer2BlendEmissionColor, // Colors
            Layer2BlendMaterialProperties1, Layer2BlendMaterialProperties2, // Material Properties
        
            Layer2BlendNoiseSettings, Layer2BlendNoiseMinMax, // Noise
        
            Layer2BlendVariationMode, Layer2BlendVariationSettings, Layer2BlendVariationBrightness, // Variation Settings
            Layer2BlendVariationNoiseSettings, // Variation Noise
            Layer2BlendVariationTexture, Layer2BlendVariationTextureTO, // Variation Texture
        
            // Outputs
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerEmission
        );
        
        albedoColor += layerAlbedo * layer2Control;
        normalVector += layerNormal * layer2Control;
        metallic += layerMetallic * layer2Control;
        smoothness += layerSmoothness * layer2Control;
        occlussion += layerOcclussion * layer2Control;
        emission += layerEmission * layer2Control;
    }
    
    // ----------------------- Layer 3 ------------------------- //
    
    float layer3Control = controlColor.b;
    if (layer3Control > 0)
    {
        float4 layerAlbedo = albedoColor;
        float3 layerNormal = normalVector;
        float layerMetallic = metallic;
        float layerSmoothness = smoothness;
        float layerOcclussion = occlussion;
        float3 layerEmission = emission;
        
        // Sample layer
        SampleSeamlessArrayMaterial(
            SS, UV, WorldNormalVector,
            WorldPosition, CameraPosition, // Positions
            SurfaceType, DebuggingIndex, // Enums
        
            // Base Material
            Layer3BaseSettings, Layer3BaseTilingOffset, // Tiling & Offset
            Layer3BaseAlbedo, // Albedo
            Layer3BaseMetallicMap, // Metallic
            Layer3BaseSmoothnessMap, // Smoothness
            Layer3BaseRoughnessMap, // Roughness
            Layer3BaseNormalMap, // Normal
            Layer3BaseOcclussionMap, // Occlussion
            Layer3BaseEmissionMap, // Emission
            Layer3BaseAlbedoTint, Layer3BaseEmissionColor, // Colors
            Layer3BaseMaterialProperties1, Layer3BaseMaterialProperties2, // Material Properties
        
            Layer3BaseNoiseSettings, Layer3BaseNoiseMinMax, // Noise
        
            Layer3BaseVariationMode, Layer3BaseVariationSettings, Layer3BaseVariationBrightness, // Variation Settings
            Layer3BaseVariationNoiseSettings, // Variation Noise
            Layer3BaseVariationTexture, Layer3BaseVariationTextureTO, // Variation Texture
        
            // Far Material
            Layer3DistanceBlendEnabled, Layer3DistanceBlendingMode, Layer3DistanceBlendMinMax, // Distance Blending
        
            Layer3FarSettings, Layer3FarTilingOffset, // Tiling & Offset
            Layer3FarTextures, Layer3FarNormalMap, // Textures
            Layer3FarArrayAssignedTextures, // Array Assigned Textures
            Layer3FarAlbedoTint, Layer3FarEmissionColor, // Colors
            Layer3FarMaterialProperties1, Layer3FarMaterialProperties2, // Material Properties
        
            Layer3FarNoiseSettings, Layer3FarNoiseMinMax, // Noise
        
            Layer3FarVariationMode, Layer3FarVariationSettings, Layer3FarVariationBrightness, // Variation Settings
            Layer3FarVariationNoiseSettings, // Variation Noise
            Layer3FarVariationTexture, Layer3FarVariationTextureTO, // Variation Texture
        
            // Blend Material
            Layer3MaterialBlendSettings, Layer3BlendMaskType, Layer3BlendMaskDistanceTO,
            Layer3MaterialBlendProperties, Layer3MaterialBlendNoiseSettings,
            Layer3BlendMaskTexture, Layer3BlendMaskTextureTO,
        
            Layer3BlendSettings, Layer3BlendTilingOffset, // Tiling & Offset
            Layer3BlendTextures, Layer3BlendNormalMap, // Textures
            Layer3BlendArrayAssignedTextures, // Array Assigned Textures
            Layer3BlendAlbedoTint, Layer3BlendEmissionColor, // Colors
            Layer3BlendMaterialProperties1, Layer3BlendMaterialProperties2, // Material Properties
        
            Layer3BlendNoiseSettings, Layer3BlendNoiseMinMax, // Noise
        
            Layer3BlendVariationMode, Layer3BlendVariationSettings, Layer3BlendVariationBrightness, // Variation Settings
            Layer3BlendVariationNoiseSettings, // Variation Noise
            Layer3BlendVariationTexture, Layer3BlendVariationTextureTO, // Variation Texture
        
            // Outputs
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerEmission
        );
        
        albedoColor += layerAlbedo * layer3Control;
        normalVector += layerNormal * layer3Control;
        metallic += layerMetallic * layer3Control;
        smoothness += layerSmoothness * layer3Control;
        occlussion += layerOcclussion * layer3Control;
        emission += layerEmission * layer3Control;
    }
    
    // ----------------------- Layer 4 ------------------------- //
    
    float layer4Control = controlColor.a;
    if (layer4Control > 0)
    {
        float4 layerAlbedo = albedoColor;
        float3 layerNormal = normalVector;
        float layerMetallic = metallic;
        float layerSmoothness = smoothness;
        float layerOcclussion = occlussion;
        float3 layerEmission = emission;
        
        // Sample layer
        SampleSeamlessArrayMaterial(
            SS, UV, WorldNormalVector,
            WorldPosition, CameraPosition, // Positions
            SurfaceType, DebuggingIndex, // Enums
        
            // Base Material
            Layer4BaseSettings, Layer4BaseTilingOffset, // Tiling & Offset
            Layer4BaseAlbedo, // Albedo
            Layer4BaseMetallicMap, // Metallic
            Layer4BaseSmoothnessMap, // Smoothness
            Layer4BaseRoughnessMap, // Roughness
            Layer4BaseNormalMap, // Normal
            Layer4BaseOcclussionMap, // Occlussion
            Layer4BaseEmissionMap, // Emission
            Layer4BaseAlbedoTint, Layer4BaseEmissionColor, // Colors
            Layer4BaseMaterialProperties1, Layer4BaseMaterialProperties2, // Material Properties
        
            Layer4BaseNoiseSettings, Layer4BaseNoiseMinMax, // Noise
        
            Layer4BaseVariationMode, Layer4BaseVariationSettings, Layer4BaseVariationBrightness, // Variation Settings
            Layer4BaseVariationNoiseSettings, // Variation Noise
            Layer4BaseVariationTexture, Layer4BaseVariationTextureTO, // Variation Texture
        
            // Far Material
            Layer4DistanceBlendEnabled, Layer4DistanceBlendingMode, Layer4DistanceBlendMinMax, // Distance Blending
        
            Layer4FarSettings, Layer4FarTilingOffset, // Tiling & Offset
            Layer4FarTextures, Layer4FarNormalMap, // Textures
            Layer4FarArrayAssignedTextures, // Array Assigned Textures
            Layer4FarAlbedoTint, Layer4FarEmissionColor, // Colors
            Layer4FarMaterialProperties1, Layer4FarMaterialProperties2, // Material Properties
        
            Layer4FarNoiseSettings, Layer4FarNoiseMinMax, // Noise
        
            Layer4FarVariationMode, Layer4FarVariationSettings, Layer4FarVariationBrightness, // Variation Settings
            Layer4FarVariationNoiseSettings, // Variation Noise
            Layer4FarVariationTexture, Layer4FarVariationTextureTO, // Variation Texture
        
            // Blend Material
            Layer4MaterialBlendSettings, Layer4BlendMaskType, Layer4BlendMaskDistanceTO,
            Layer4MaterialBlendProperties, Layer4MaterialBlendNoiseSettings,
            Layer4BlendMaskTexture, Layer4BlendMaskTextureTO,
        
            Layer4BlendSettings, Layer4BlendTilingOffset, // Tiling & Offset
            Layer4BlendTextures, Layer4BlendNormalMap, // Textures
            Layer4BlendArrayAssignedTextures, // Array Assigned Textures
            Layer4BlendAlbedoTint, Layer4BlendEmissionColor, // Colors
            Layer4BlendMaterialProperties1, Layer4BlendMaterialProperties2, // Material Properties
        
            Layer4BlendNoiseSettings, Layer4BlendNoiseMinMax, // Noise
        
            Layer4BlendVariationMode, Layer4BlendVariationSettings, Layer4BlendVariationBrightness, // Variation Settings
            Layer4BlendVariationNoiseSettings, // Variation Noise
            Layer4BlendVariationTexture, Layer4BlendVariationTextureTO, // Variation Texture
        
            // Outputs
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerEmission
        );
        
        albedoColor += layerAlbedo * layer4Control;
        normalVector += layerNormal * layer4Control;
        metallic += layerMetallic * layer4Control;
        smoothness += layerSmoothness * layer4Control;
        occlussion += layerOcclussion * layer4Control;
        emission += layerEmission * layer4Control;
    }

    // Transform tangent normals to world normals
    //normalVector = normalize(mul(normalVector, TBN));

    // If Transparency Disabled
    if (SurfaceType < 2)
        albedoColor.a = 1;
    if (DebuggingIndex != -1)
        albedoColor.a = 1.0;
    
    // Holes
    // Use holes sampler as a custom one will cause a line at the edge of terrains
    float4 holesColor = SAMPLE_TEXTURE2D(Holes, sampler_TerrainHolesTexture, UV);
    clip(albedoColor.a - (1 - (holesColor.r - 0.01))); // Remove 1 from holes otherwise it will equal 0, not clipping the pixel
    
    // Output
    AlbedoColorOut = albedoColor;
    NormalVectorOut = normalVector;
    MetallicOut = metallic;
    SmoothnessOut = smoothness;
    OcclussionOut = occlussion;
    EmissionColorOut = emission;
}

#endif