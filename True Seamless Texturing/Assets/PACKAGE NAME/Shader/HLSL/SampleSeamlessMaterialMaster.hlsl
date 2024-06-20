#ifndef SAMPLESEAMLESSMATERIALMASTER_INCLUDED
#define SAMPLESEAMLESSMATERIALMASTER_INCLUDED

#include "SampleSeamlessMaterial.hlsl"

void SampleSeamlessMaterial_float(
    float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition, // Positions
    int SurfaceType, float DebuggingIndex, // Enums

    // Base Material
    float2 BaseSettings, float4 BaseTilingOffset, // Tiling & Offset
    UnityTexture2D BaseAlbedo, // Albedo
    UnityTexture2D BaseMetallicMap, // Metallic
    UnityTexture2D BaseSmoothnessMap, // Smoothness
    UnityTexture2D BaseRoughnessMap, // Roughness
    UnityTexture2D BaseNormalMap, // Normal
    UnityTexture2D BaseOcclussionMap, // Occlussion
    UnityTexture2D BaseEmissionMap, // Emission
    float4 BaseAlbedoTint, float3 BaseEmissionColor, // Colors
    float4 BaseMaterialProperties1, float2 BaseMaterialProperties2, // Material Properties

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
        sampler_BaseAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
        BaseSettings, BaseTilingOffset,
        BaseAlbedo,
        BaseMetallicMap,
        BaseSmoothnessMap,
        BaseRoughnessMap,
        BaseNormalMap,
        BaseOcclussionMap,
        BaseEmissionMap,
        BaseAlbedoTint, BaseEmissionColor,
        BaseMaterialProperties1, BaseMaterialProperties2,
        BaseNoiseSettings, BaseNoiseMinMax,
        BaseVariationMode, BaseVariationSettings, BaseVariationBrightness,
        BaseVariationNoiseSettings,
        BaseVariationTexture, BaseVariationTextureTO,
        albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor
    );
    
    // --------------------- Material Blending ----------------------- //
    
    int materialBlendSettings = (int)MaterialBlendSettings;
    
    bool materialBlendEnabled       = (materialBlendSettings & 1) != 0;
    bool overrideDistanceBlending   = (materialBlendSettings & 2) != 0;
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
                materialMask = SAMPLE_TEXTURE2D(BlendMaskTexture, sampler_BlendMaskTexture, UV * blendMaskNoiseScale + blendMaskNoiseOffset).r;
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
            GetSeamlessMaterialColor(
                sampler_BaseAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                BlendSettings, BlendTilingOffset,
                BlendAlbedo,
                BlendMetallicMap,
                BlendSmoothnessMap,
                BlendRoughnessMap,
                BlendNormalMap,
                BlendOcclussionMap,
                BlendEmissionMap,
                BlendAlbedoTint, BlendEmissionColor,
                BlendMaterialProperties1, BlendMaterialProperties2,
                BlendNoiseSettings, BlendNoiseMinMax,
                BlendVariationMode, BlendVariationSettings, BlendVariationBrightness,
                BlendVariationNoiseSettings,
                BlendVariationTexture, BlendVariationTextureTO,
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
        
        // Only sample far material if required
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
                    GetSeamlessMaterialColor(
                        sampler_BaseAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        BaseSettings, FarTilingOffset,
                        BaseAlbedo,
                        BaseMetallicMap,
                        BaseSmoothnessMap,
                        BaseRoughnessMap,
                        BaseNormalMap,
                        BaseOcclussionMap,
                        BaseEmissionMap,
                        BaseAlbedoTint, BaseEmissionColor,
                        BaseMaterialProperties1, BaseMaterialProperties2,
                        BaseNoiseSettings, BaseNoiseMinMax,
                        BaseVariationMode, BaseVariationSettings, BaseVariationBrightness,
                        BaseVariationNoiseSettings,
                        BaseVariationTexture, BaseVariationTextureTO,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                    break;
                case 1: // Material
                    // Sample Far Material
                    GetSeamlessMaterialColor(
                        sampler_FarAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        FarSettings, FarTilingOffset,
                        FarAlbedo,
                        FarMetallicMap,
                        FarSmoothnessMap,
                        FarRoughnessMap,
                        FarNormalMap,
                        FarOcclussionMap,
                        FarEmissionMap,
                        FarAlbedoTint, FarEmissionColor,
                        FarMaterialProperties1, FarMaterialProperties2,
                        FarNoiseSettings, FarNoiseMinMax,
                        FarVariationMode, FarVariationSettings, FarVariationBrightness,
                        FarVariationNoiseSettings,
                        FarVariationTexture, FarVariationTextureTO,
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
                
                // Sample Blend Material
                GetSeamlessMaterialColor(
                        sampler_BlendAlbedo, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        BlendSettings, tilingOffset,
                        BlendAlbedo,
                        BlendMetallicMap,
                        BlendSmoothnessMap,
                        BlendRoughnessMap,
                        BlendNormalMap,
                        BlendOcclussionMap,
                        BlendEmissionMap,
                        BlendAlbedoTint, BlendEmissionColor,
                        BlendMaterialProperties1, BlendMaterialProperties2,
                        BlendNoiseSettings, BlendNoiseMinMax,
                        BlendVariationMode, BlendVariationSettings, BlendVariationBrightness,
                        BlendVariationNoiseSettings,
                        BlendVariationTexture, BlendVariationTextureTO,
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
    // Would use a switch statement here but it bugs out the material on my laptop so I assume it would happen to others also
    // Really weird bug that should not be happening but better safe than sorry :/
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