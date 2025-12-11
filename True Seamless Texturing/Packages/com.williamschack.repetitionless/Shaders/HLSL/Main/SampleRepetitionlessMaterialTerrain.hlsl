#ifndef SAMPLEREPETITIONLESSMATERIALTERRAIN_INCLUDED
#define SAMPLEREPETITIONLESSMATERIALTERRAIN_INCLUDED

#include "SampleRepetitionlessLayer.hlsl"

// Essentially just calling the main SampleRepetitionlessMaterial function 4 times, once for each layer
// It does do more things and the function is modified for texture arrays but you get the idea

/*

void SampleRepetitionlessMaterialTerrain_float(
    SamplerState SS, float2 UV, float3 TangentNormalVector, // Control requires clamped sampler state, otherwise fades off at edges of terrain
    float3 WorldPosition, float3 CameraPosition, // Positions
    UnityTexture2D Holes, UnityTexture2D Control,
    int SurfaceType, int DebuggingIndex, // Enums

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

    // Variables
    float4 albedoColor = backgroundControl;
    float3 normalVector = TangentNormalVector * backgroundControl;
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
        
        // Create layer from inputs
        RepetitionlessLayerTerrain layer = {
            {   // Base Material
                Layer1BaseAlbedo,
                Layer1BaseMetallicMap,
                Layer1BaseSmoothnessMap,
                Layer1BaseRoughnessMap,
                Layer1BaseNormalMap,
                Layer1BaseOcclussionMap,
                Layer1BaseEmissionMap,
                {
                    Layer1BaseSettings, Layer1BaseTilingOffset,
                    Layer1BaseAlbedoTint, Layer1BaseEmissionColor,
                    Layer1BaseMaterialProperties1, Layer1BaseMaterialProperties2,
                    Layer1BaseNoiseSettings, Layer1BaseNoiseMinMax,
                    Layer1BaseVariationMode, Layer1BaseVariationSettings, Layer1BaseVariationBrightness,
                    Layer1BaseVariationNoiseSettings,
                    Layer1BaseVariationTexture, Layer1BaseVariationTextureTO
                }
            },
            {   // Far Material
                Layer1FarTextures, Layer1FarNormalMap,
                Layer1FarArrayAssignedTextures,
                {
                    Layer1FarSettings, Layer1FarTilingOffset,
                    Layer1FarAlbedoTint, Layer1FarEmissionColor,
                    Layer1FarMaterialProperties1, Layer1FarMaterialProperties2,
                    Layer1FarNoiseSettings, Layer1FarNoiseMinMax,
                    Layer1FarVariationMode, Layer1FarVariationSettings, Layer1FarVariationBrightness,
                    Layer1FarVariationNoiseSettings,
                    Layer1FarVariationTexture, Layer1FarVariationTextureTO
                }
            },
            {   // Blend Material
                Layer1BlendTextures, Layer1BlendNormalMap,
                Layer1BlendArrayAssignedTextures,
                {
                    Layer1BlendSettings, Layer1BlendTilingOffset,
                    Layer1BlendAlbedoTint, Layer1BlendEmissionColor,
                    Layer1BlendMaterialProperties1, Layer1BlendMaterialProperties2,
                    Layer1BlendNoiseSettings, Layer1BlendNoiseMinMax,
                    Layer1BlendVariationMode, Layer1BlendVariationSettings, Layer1BlendVariationBrightness,
                    Layer1BlendVariationNoiseSettings,
                    Layer1BlendVariationTexture, Layer1BlendVariationTextureTO
                }
            },
            {   // Data
                Layer1DistanceBlendEnabled, Layer1DistanceBlendingMode, Layer1DistanceBlendMinMax,
                Layer1MaterialBlendSettings, Layer1BlendMaskType, Layer1BlendMaskDistanceTO,
                Layer1MaterialBlendProperties, Layer1MaterialBlendNoiseSettings,
                Layer1BlendMaskTexture, Layer1BlendMaskTextureTO
            }
        };

        // Sample layer
        SampleRepetitionlessLayerTerrain(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            SurfaceType, DebuggingIndex,
            layer,
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
        
        // Create layer from inputs
        RepetitionlessLayerTerrain layer = {
            {   // Base Material
                Layer2BaseAlbedo,
                Layer2BaseMetallicMap,
                Layer2BaseSmoothnessMap,
                Layer2BaseRoughnessMap,
                Layer2BaseNormalMap,
                Layer2BaseOcclussionMap,
                Layer2BaseEmissionMap,
                {
                    Layer2BaseSettings, Layer2BaseTilingOffset,
                    Layer2BaseAlbedoTint, Layer2BaseEmissionColor,
                    Layer2BaseMaterialProperties1, Layer2BaseMaterialProperties2,
                    Layer2BaseNoiseSettings, Layer2BaseNoiseMinMax,
                    Layer2BaseVariationMode, Layer2BaseVariationSettings, Layer2BaseVariationBrightness,
                    Layer2BaseVariationNoiseSettings,
                    Layer2BaseVariationTexture, Layer2BaseVariationTextureTO
                }
            },
            {   // Far Material
                Layer2FarTextures, Layer2FarNormalMap,
                Layer2FarArrayAssignedTextures,
                {
                    Layer2FarSettings, Layer2FarTilingOffset,
                    Layer2FarAlbedoTint, Layer2FarEmissionColor,
                    Layer2FarMaterialProperties1, Layer2FarMaterialProperties2,
                    Layer2FarNoiseSettings, Layer2FarNoiseMinMax,
                    Layer2FarVariationMode, Layer2FarVariationSettings, Layer2FarVariationBrightness,
                    Layer2FarVariationNoiseSettings,
                    Layer2FarVariationTexture, Layer2FarVariationTextureTO
                }
            },
            {   // Blend Material
                Layer2BlendTextures, Layer2BlendNormalMap,
                Layer2BlendArrayAssignedTextures,
                {
                    Layer2BlendSettings, Layer2BlendTilingOffset,
                    Layer2BlendAlbedoTint, Layer2BlendEmissionColor,
                    Layer2BlendMaterialProperties1, Layer2BlendMaterialProperties2,
                    Layer2BlendNoiseSettings, Layer2BlendNoiseMinMax,
                    Layer2BlendVariationMode, Layer2BlendVariationSettings, Layer2BlendVariationBrightness,
                    Layer2BlendVariationNoiseSettings,
                    Layer2BlendVariationTexture, Layer2BlendVariationTextureTO
                }
            },
            {   // Data
                Layer2DistanceBlendEnabled, Layer2DistanceBlendingMode, Layer2DistanceBlendMinMax,
                Layer2MaterialBlendSettings, Layer2BlendMaskType, Layer2BlendMaskDistanceTO,
                Layer2MaterialBlendProperties, Layer2MaterialBlendNoiseSettings,
                Layer2BlendMaskTexture, Layer2BlendMaskTextureTO
            }
        };

        // Sample layer
        SampleRepetitionlessLayerTerrain(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            SurfaceType, DebuggingIndex,
            layer,
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
        
        // Create layer from inputs
        RepetitionlessLayerTerrain layer = {
            {   // Base Material
                Layer3BaseAlbedo,
                Layer3BaseMetallicMap,
                Layer3BaseSmoothnessMap,
                Layer3BaseRoughnessMap,
                Layer3BaseNormalMap,
                Layer3BaseOcclussionMap,
                Layer3BaseEmissionMap,
                {
                    Layer3BaseSettings, Layer3BaseTilingOffset,
                    Layer3BaseAlbedoTint, Layer3BaseEmissionColor,
                    Layer3BaseMaterialProperties1, Layer3BaseMaterialProperties2,
                    Layer3BaseNoiseSettings, Layer3BaseNoiseMinMax,
                    Layer3BaseVariationMode, Layer3BaseVariationSettings, Layer3BaseVariationBrightness,
                    Layer3BaseVariationNoiseSettings,
                    Layer3BaseVariationTexture, Layer3BaseVariationTextureTO
                }
            },
            {   // Far Material
                Layer3FarTextures, Layer3FarNormalMap,
                Layer3FarArrayAssignedTextures,
                {
                    Layer3FarSettings, Layer3FarTilingOffset,
                    Layer3FarAlbedoTint, Layer3FarEmissionColor,
                    Layer3FarMaterialProperties1, Layer3FarMaterialProperties2,
                    Layer3FarNoiseSettings, Layer3FarNoiseMinMax,
                    Layer3FarVariationMode, Layer3FarVariationSettings, Layer3FarVariationBrightness,
                    Layer3FarVariationNoiseSettings,
                    Layer3FarVariationTexture, Layer3FarVariationTextureTO
                }
            },
            {   // Blend Material
                Layer3BlendTextures, Layer3BlendNormalMap,
                Layer3BlendArrayAssignedTextures,
                {
                    Layer3BlendSettings, Layer3BlendTilingOffset,
                    Layer3BlendAlbedoTint, Layer3BlendEmissionColor,
                    Layer3BlendMaterialProperties1, Layer3BlendMaterialProperties2,
                    Layer3BlendNoiseSettings, Layer3BlendNoiseMinMax,
                    Layer3BlendVariationMode, Layer3BlendVariationSettings, Layer3BlendVariationBrightness,
                    Layer3BlendVariationNoiseSettings,
                    Layer3BlendVariationTexture, Layer3BlendVariationTextureTO
                }
            },
            {   // Data
                Layer3DistanceBlendEnabled, Layer3DistanceBlendingMode, Layer3DistanceBlendMinMax,
                Layer3MaterialBlendSettings, Layer3BlendMaskType, Layer3BlendMaskDistanceTO,
                Layer3MaterialBlendProperties, Layer3MaterialBlendNoiseSettings,
                Layer3BlendMaskTexture, Layer3BlendMaskTextureTO
            }
        };

        // Sample layer
        SampleRepetitionlessLayerTerrain(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            SurfaceType, DebuggingIndex,
            layer,
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
        
        // Create layer from inputs
        RepetitionlessLayerTerrain layer = {
            {   // Base Material
                Layer4BaseAlbedo,
                Layer4BaseMetallicMap,
                Layer4BaseSmoothnessMap,
                Layer4BaseRoughnessMap,
                Layer4BaseNormalMap,
                Layer4BaseOcclussionMap,
                Layer4BaseEmissionMap,
                {
                    Layer4BaseSettings, Layer4BaseTilingOffset,
                    Layer4BaseAlbedoTint, Layer4BaseEmissionColor,
                    Layer4BaseMaterialProperties1, Layer4BaseMaterialProperties2,
                    Layer4BaseNoiseSettings, Layer4BaseNoiseMinMax,
                    Layer4BaseVariationMode, Layer4BaseVariationSettings, Layer4BaseVariationBrightness,
                    Layer4BaseVariationNoiseSettings,
                    Layer4BaseVariationTexture, Layer4BaseVariationTextureTO
                }
            },
            {   // Far Material
                Layer4FarTextures, Layer4FarNormalMap,
                Layer4FarArrayAssignedTextures,
                {
                    Layer4FarSettings, Layer4FarTilingOffset,
                    Layer4FarAlbedoTint, Layer4FarEmissionColor,
                    Layer4FarMaterialProperties1, Layer4FarMaterialProperties2,
                    Layer4FarNoiseSettings, Layer4FarNoiseMinMax,
                    Layer4FarVariationMode, Layer4FarVariationSettings, Layer4FarVariationBrightness,
                    Layer4FarVariationNoiseSettings,
                    Layer4FarVariationTexture, Layer4FarVariationTextureTO
                }
            },
            {   // Blend Material
                Layer4BlendTextures, Layer4BlendNormalMap,
                Layer4BlendArrayAssignedTextures,
                {
                    Layer4BlendSettings, Layer4BlendTilingOffset,
                    Layer4BlendAlbedoTint, Layer4BlendEmissionColor,
                    Layer4BlendMaterialProperties1, Layer4BlendMaterialProperties2,
                    Layer4BlendNoiseSettings, Layer4BlendNoiseMinMax,
                    Layer4BlendVariationMode, Layer4BlendVariationSettings, Layer4BlendVariationBrightness,
                    Layer4BlendVariationNoiseSettings,
                    Layer4BlendVariationTexture, Layer4BlendVariationTextureTO
                }
            },
            {   // Data
                Layer4DistanceBlendEnabled, Layer4DistanceBlendingMode, Layer4DistanceBlendMinMax,
                Layer4MaterialBlendSettings, Layer4BlendMaskType, Layer4BlendMaskDistanceTO,
                Layer4MaterialBlendProperties, Layer4MaterialBlendNoiseSettings,
                Layer4BlendMaskTexture, Layer4BlendMaskTextureTO
            }
        };

        // Sample layer
        SampleRepetitionlessLayerTerrain(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            SurfaceType, DebuggingIndex,
            layer,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerEmission
        );

        albedoColor += layerAlbedo * layer4Control;
        normalVector += layerNormal * layer4Control;
        metallic += layerMetallic * layer4Control;
        smoothness += layerSmoothness * layer4Control;
        occlussion += layerOcclussion * layer4Control;
        emission += layerEmission * layer4Control;
    }

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

*/

#endif