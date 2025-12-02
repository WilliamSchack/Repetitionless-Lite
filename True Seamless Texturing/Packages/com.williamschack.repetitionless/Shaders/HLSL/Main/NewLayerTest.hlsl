#ifndef NEWLAYERTEST_INCLUDED
#define NEWLAYERTEST_INCLUDED

#include "../Structs/RepetitionlessMaterialDataNew.hlsl"

#include "../Utilities/TextureUtilities.hlsl"

#include "../Noise/VoronoiNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"
#include "../Noise/Keijiro/SimplexNoise2D.hlsl"

#include "NewMaterialTest.hlsl"

#define ARRAY_LAYER_INDEX_TEMP 0

void SampleRepetitionlessLayerBase_float(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int DebuggingIndex,

    // Textures
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    int AssignedAVTextures,
    int AssignedNSOTextures,
    int AssignedEMTextures,

    // Base Material
    float2 BaseSettings, float4 BaseTilingOffset, // Tiling & Offset
    float4 BaseAlbedoTint, float3 BaseEmissionColor, // Colors
    float4 BaseMaterialProperties1, float2 BaseMaterialProperties2, // Material Properties

    float2 BaseNoiseSettings, float4 BaseNoiseMinMax, // Noise

    float BaseVariationMode, float4 BaseVariationSettings, float BaseVariationBrightness, // Variation Settings
    float4 BaseVariationNoiseSettings, // Variation Noise
    float4 BaseVariationTextureTO, // Variation Texture

    // Far Material
    bool DistanceBlendEnabled, int DistanceBlendingMode, float2 DistanceBlendMinMax,

    float2 FarSettings, float4 FarTilingOffset, // Tiling & Offset
    float4 FarAlbedoTint, float3 FarEmissionColor, // Colors
    float4 FarMaterialProperties1, float2 FarMaterialProperties2, // Material Properties

    float2 FarNoiseSettings, float4 FarNoiseMinMax, // Noise

    float FarVariationMode, float4 FarVariationSettings, float FarVariationBrightness, // Variation Settings
    float4 FarVariationNoiseSettings, // Variation Noise
    float4 FarVariationTextureTO, // Variation Texture

    // Blend Material
    float MaterialBlendSettings, int BlendMaskType, float4 BlendMaskDistanceTO,
    float2 MaterialBlendProperties, float3 MaterialBlendNoiseSettings,
    UnityTexture2D BlendMaskTexture, float4 BlendMaskTextureTO,

    float2 BlendSettings, float4 BlendTilingOffset, // Tiling & Offset
    float4 BlendAlbedoTint, float3 BlendEmissionColor, // Colors
    float4 BlendMaterialProperties1, float2 BlendMaterialProperties2, // Material Properties

    float2 BlendNoiseSettings, float4 BlendNoiseMinMax, // Noise

    float BlendVariationMode, float4 BlendVariationSettings, float BlendVariationBrightness, // Variation Settings
    float4 BlendVariationNoiseSettings, // Variation Noise
    float4 BlendVariationTextureTO, // Variation Texture

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
) {
    // ----------------------- Setup ------------------------- //

    // Variables
    float4 albedoColor = 1;
    float3 normalVector = TangentNormalVector;
    float metallic = 0;
    float smoothness = 0;
    float occlussion = 0;
    float3 emissionColor = 0;
    
    float materialMask = 0;
    float farDistance = 0;

    // Calculate mask
    int materialBlendSettings       = (int)MaterialBlendSettings;
    bool materialBlendEnabled       = (materialBlendSettings & 1) != 0;
    bool overrideDistanceBlending   = (materialBlendSettings & 2) != 0;
    bool overrideDistanceBlendingTO = (materialBlendSettings & 4) != 0;

    if (materialBlendEnabled) {
        float blendMaskNoiseScale = MaterialBlendNoiseSettings.x;
        float2 blendMaskNoiseOffset = MaterialBlendNoiseSettings.yz;
        
        // Get mask of blended material
        switch (BlendMaskType) {
            case 0: // Perlin Noise
                materialMask = ClassicNoise(UV * blendMaskNoiseScale + blendMaskNoiseOffset) * 3;
                break;
            case 1: // Simplex Noise
                materialMask = SimplexNoise(UV * blendMaskNoiseScale + blendMaskNoiseOffset) * 2;
                break;
            case 2: // Custom Texture
                materialMask = SAMPLE_TEXTURE2D(BlendMaskTexture, SS, UV * blendMaskNoiseScale + blendMaskNoiseOffset).r;
                break;
        }

        float blendMaskOpacity = MaterialBlendProperties.x;
        float blendMaskStrength = MaterialBlendProperties.y;
        
        materialMask *= blendMaskStrength;
        materialMask = clamp(materialMask, 0, 1);
        materialMask *= blendMaskOpacity;
    }

    // ----------------------- Package Material Data ------------------------- //

    RepetitionlessMaterialDataNew BaseMaterialData = {
        BaseSettings, BaseTilingOffset,
        BaseAlbedoTint, BaseEmissionColor,
        BaseMaterialProperties1, BaseMaterialProperties2,
        BaseNoiseSettings, BaseNoiseMinMax,
        BaseVariationMode, BaseVariationSettings, BaseVariationBrightness,
        BaseVariationNoiseSettings,
        BaseVariationTextureTO
    };

    RepetitionlessMaterialDataNew FarMaterialData = {
        FarSettings, FarTilingOffset,
        FarAlbedoTint, FarEmissionColor,
        FarMaterialProperties1, FarMaterialProperties2,
        FarNoiseSettings, FarNoiseMinMax,
        FarVariationMode, FarVariationSettings, FarVariationBrightness,
        FarVariationNoiseSettings,
        FarVariationTextureTO
    };

    RepetitionlessMaterialDataNew BlendMaterialData = {
        BlendSettings, BlendTilingOffset,
        BlendAlbedoTint, BlendEmissionColor,
        BlendMaterialProperties1, BlendMaterialProperties2,
        BlendNoiseSettings, BlendNoiseMinMax,
        BlendVariationMode, BlendVariationSettings, BlendVariationBrightness,
        BlendVariationNoiseSettings,
        BlendVariationTextureTO
    };

    // ----------------------- Get Materials To Sample ------------------------- //

    // At most two materials will be sampled when blending between
    // Get the material(s) to be sampled
    bool samplingBase = false;
    bool samplingBlend = false;
    bool samplingDistance = false;
    bool samplingDistanceBlend = false;

    // Check distance blend
    if (DistanceBlendEnabled) {
        // Distance Mask
        farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);

        samplingDistance = farDistance > 0 && materialMask != 1;
        samplingDistanceBlend = farDistance > 0 && materialBlendEnabled && materialMask > 0 && overrideDistanceBlending;
    }

    // Check material blend
    if (materialBlendEnabled) {
        samplingBlend = materialMask > 0;
        if (DistanceBlendEnabled && overrideDistanceBlending && farDistance >= 1)
            samplingBlend = false;
    }

    // Check base material
    samplingBase = farDistance != 1 && materialMask != 1;

    // ----------------------- Base Material ------------------------- //
    if (samplingBase) {
        GetRepetitionlessMaterialColorTest(
            SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
            ARRAY_LAYER_INDEX_TEMP, AVTextures, NSOTextures, EMTextures, AssignedAVTextures, AssignedNSOTextures, AssignedEMTextures,
            BaseMaterialData,
            albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor
        );
    }

    // ----------------------- Blend Material ------------------------- //
    if (samplingBlend) {
        float4 blendAlbedoColor = 1;
        float3 blendNormalVector = TangentNormalVector;
        float blendMetallic = 0;
        float blendSmoothness = 0;
        float blendOcclussion = 0;
        float3 blendEmissionColor = 0;

        GetRepetitionlessMaterialColorTest(
            SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
            ARRAY_LAYER_INDEX_TEMP, AVTextures, NSOTextures, EMTextures, AssignedAVTextures, AssignedNSOTextures, AssignedEMTextures,
            BlendMaterialData,
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
    
    // ----------------------- Distance Material ------------------------- //
    if (samplingDistance) {
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
                // Set far TO, no need to change back it wont be used again
                
                BaseMaterialData.TilingOffset = FarTilingOffset;
                
                GetRepetitionlessMaterialColorTest(
                    SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                    ARRAY_LAYER_INDEX_TEMP, AVTextures, NSOTextures, EMTextures, AssignedAVTextures, AssignedNSOTextures, AssignedEMTextures,
                    BaseMaterialData,
                    farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                );
                break;
            case 1: // Material
                // Sample Far Material
                GetRepetitionlessMaterialColorTest(
                    SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                    ARRAY_LAYER_INDEX_TEMP, AVTextures, NSOTextures, EMTextures, AssignedAVTextures, AssignedNSOTextures, AssignedEMTextures,
                    FarMaterialData,
                    farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                );
                break;
        }

        // Combine Far with Base
        albedoColor = lerp(albedoColor, farAlbedoColor, farDistance);
        normalVector = lerp(normalVector, farNormalVector, farDistance);
        metallic = lerp(metallic, farMetallic, farDistance);
        smoothness = lerp(smoothness, farSmoothness, farDistance);
        occlussion = lerp(occlussion, farOcclussion, farDistance);
        emissionColor = lerp(emissionColor, farEmissionColor, farDistance);
    }

    // ----------------------- Distance Blend Material ------------------------- //
    if (samplingDistanceBlend) {
        float4 blendAlbedoColor = 1;
        float3 blendNormalVector = TangentNormalVector;
        float blendMetallic = 0;
        float blendSmoothness = 0;
        float blendOcclussion = 0;
        float3 blendEmissionColor = 0;
        
        float2 tiling = BlendTilingOffset.xy;
        float2 offset = BlendTilingOffset.zw;
        if (DistanceBlendingMode == 0) {
            tiling = overrideDistanceBlendingTO ? BlendMaskDistanceTO.xy : FarTilingOffset.xy;
            offset = overrideDistanceBlendingTO ? BlendMaskDistanceTO.zw : FarTilingOffset.zw;
        }
        
        float4 tilingOffset = float4(tiling.x, tiling.y, offset.x, offset.y);
        
        // Sample Blend Material
        // Set blend TO, no need to change back it wont be used again

        BlendMaterialData.TilingOffset = tilingOffset;
        
        GetRepetitionlessMaterialColorTest(
            SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
            ARRAY_LAYER_INDEX_TEMP, AVTextures, NSOTextures, EMTextures, AssignedAVTextures, AssignedNSOTextures, AssignedEMTextures,
            BlendMaterialData,
            blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
        );
        
        // Combine Far Blend with Base 
        float lerpFactor = farDistance * materialMask;
        albedoColor = lerp(albedoColor, blendAlbedoColor, lerpFactor);
        normalVector = lerp(normalVector, blendNormalVector, lerpFactor);
        metallic = lerp(metallic, blendMetallic, lerpFactor);
        smoothness = lerp(smoothness, blendSmoothness, lerpFactor);
        occlussion = lerp(occlussion, blendOcclussion, lerpFactor);
        emissionColor = lerp(emissionColor, blendEmissionColor, lerpFactor);
    }

    // ----------------------- Output ------------------------- //

    // Debugging
    if (DebuggingIndex == 2)
        albedoColor = farDistance;
    else if (DebuggingIndex == 3)
        albedoColor = materialMask;
    
    // If Transparency Disabled
    if (SurfaceType == 0 || DebuggingIndex != -1)
        albedoColor.a = 1;

    // Output
    AlbedoColorOut = albedoColor;
    NormalVectorOut = normalVector;
    MetallicOut = metallic;
    SmoothnessOut = smoothness;
    OcclussionOut = occlussion;
    EmissionColorOut = emissionColor;
}

#endif