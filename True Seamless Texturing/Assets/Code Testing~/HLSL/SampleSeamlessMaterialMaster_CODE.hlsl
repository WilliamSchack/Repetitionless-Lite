#ifndef SAMPLESEAMLESSMATERIALMASTER_INCLUDED
#define SAMPLESEAMLESSMATERIALMASTER_INCLUDED

#include "SampleSeamlessMaterial_CODE.hlsl"

void SampleSeamlessMaterialMaster(
    float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition, // Positions
    int SurfaceType, int DebuggingIndex, // Enums

    // Materials
    RepetitionlessMaterial baseMaterial,
    RepetitionlessMaterial farMaterial,
    RepetitionlessMaterial blendMaterial,

    // Far Material Settings
    bool DistanceBlendEnabled, int DistanceBlendingMode, float2 DistanceBlendMinMax,

    // Blend Material Settings
    float MaterialBlendSettings, int BlendMaskType, float4 BlendMaskDistanceTO,
    float2 MaterialBlendProperties, float3 MaterialBlendNoiseSettings,
    sampler2D BlendMaskTexture, float4 BlendMaskTextureTO,

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
        UV, TangentNormalVector, SurfaceType, DebuggingIndex, baseMaterial,
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
                materialMask = tex2D(BlendMaskTexture, UV * blendMaskNoiseScale + blendMaskNoiseOffset).r;
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
                UV, TangentNormalVector, SurfaceType, DebuggingIndex, blendMaterial,
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
    
    if (DistanceBlendEnabled)
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
                    // Sample Base Material with far TO
                    RepetitionlessMaterial baseWithFarTO = baseMaterial;
                    baseWithFarTO.TilingOffset = farMaterial.TilingOffset;
                
                    GetSeamlessMaterialColor(
                        UV, TangentNormalVector, SurfaceType, DebuggingIndex, baseWithFarTO,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                    break;
                case 1: // Material
                    // Sample Far Material
                    GetSeamlessMaterialColor(
                        UV, TangentNormalVector, SurfaceType, DebuggingIndex, farMaterial,
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
                
                float2 tiling = blendMaterial.TilingOffset.xy;
                float2 offset = blendMaterial.TilingOffset.zw;
                if (DistanceBlendingMode == 0)
                {
                    tiling = overrideDistanceBlendingTO ? BlendMaskDistanceTO.xy : farMaterial.TilingOffset.xy;
                    offset = overrideDistanceBlendingTO ? BlendMaskDistanceTO.zw : farMaterial.TilingOffset.zw;
                }
                
                float4 tilingOffset = float4(tiling.x, tiling.y, offset.x, offset.y);
                
                // Sample Blend Material with modified TO
                RepetitionlessMaterial blendWithNewTO = blendMaterial;
                blendWithNewTO.TilingOffset = tilingOffset;
                
                GetSeamlessMaterialColor(
                        UV, TangentNormalVector, SurfaceType, DebuggingIndex, blendWithNewTO,
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