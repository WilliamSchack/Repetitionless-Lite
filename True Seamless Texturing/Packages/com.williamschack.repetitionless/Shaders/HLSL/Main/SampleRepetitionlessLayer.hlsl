#ifndef SAMPLEREPETITIONLESSLAYER_INCLUDED
#define SAMPLEREPETITIONLESSLAYER_INCLUDED

#include "../Structs/RepetitionlessMaterialData.hlsl"

#include "../RepetitionlessHelpers/GetArrayAssignedTextures.hlsl"

#include "../Noise/VoronoiNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"
#include "../Noise/Keijiro/SimplexNoise2D.hlsl"

#include "../Utilities/TextureUtilities.hlsl"
#include "../Utilities/BooleanCompression.hlsl"

#include "../TextureArrayEssentials/TextureArrayUtilities.hlsl"

#include "SampleRepetitionlessMaterial.hlsl"

// Uses assigned array properties variables
void SampleRepetitionlessLayer_float(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector, float3 WorldNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int UVSpace, int DebuggingIndex,

    // Properties
    int LayerIndex,
    UnityTexture2D PropertiesTexture,
    int AssignedAVTextures0,
    int AssignedAVTextures1,
    int AssignedAVTextures2,
    int AssignedNSOTextures0,
    int AssignedNSOTextures1,
    int AssignedNSOTextures2,
    int AssignedEMTextures0,
    int AssignedEMTextures1,
    int AssignedEMTextures2,
    int AssignedBMTextures0,

    // Textures
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    UnityTexture2DArray BMTextures,

    UnityTexture2D NoiseTexture,

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float  MetallicOut,
    out float  SmoothnessOut,
    out float  OcclussionOut,
    out float3 EmissionColorOut
) {
    // ----------------------- Load Variables From Textures ------------------------- //

    // Base Material
    int indexOffset = 0;
    RepetitionlessMaterialData baseMaterialData = {
        PropertiesTexture.Load(int3(0 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(1 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(2 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(3 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(4 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(5 + indexOffset, LayerIndex, 0)).rgb,
        PropertiesTexture.Load(int3(6 + indexOffset, LayerIndex, 0)).rgb,
        PropertiesTexture.Load(int3(7 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(8 + indexOffset, LayerIndex, 0)).rgba
    };

    // Far Material
    indexOffset += REPETITIONLESS_MATERIAL_VARIABLE_COUNT;
    RepetitionlessMaterialData farMaterialData = {
        PropertiesTexture.Load(int3(0 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(1 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(2 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(3 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(4 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(5 + indexOffset, LayerIndex, 0)).rgb,
        PropertiesTexture.Load(int3(6 + indexOffset, LayerIndex, 0)).rgb,
        PropertiesTexture.Load(int3(7 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(8 + indexOffset, LayerIndex, 0)).rgba
    };

    // Blend Material
    indexOffset += REPETITIONLESS_MATERIAL_VARIABLE_COUNT;
    RepetitionlessMaterialData blendMaterialData = {
        PropertiesTexture.Load(int3(0 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(1 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(2 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(3 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(4 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(5 + indexOffset, LayerIndex, 0)).rgb,
        PropertiesTexture.Load(int3(6 + indexOffset, LayerIndex, 0)).rgb,
        PropertiesTexture.Load(int3(7 + indexOffset, LayerIndex, 0)).rgba,
        PropertiesTexture.Load(int3(8 + indexOffset, LayerIndex, 0)).rgba
    };

    // Layer Settings
    indexOffset += REPETITIONLESS_MATERIAL_VARIABLE_COUNT;

    half4 distanceBlendSettings = PropertiesTexture.Load(int3(0 + indexOffset, LayerIndex, 0));
    half4 blendMaskDistanceTO   = PropertiesTexture.Load(int3(1 + indexOffset, LayerIndex, 0));
    half4 materialBlendSettings = PropertiesTexture.Load(int3(2 + indexOffset, LayerIndex, 0));
    half4 materialBlendMaskTO   = PropertiesTexture.Load(int3(3 + indexOffset, LayerIndex, 0));

    bool  distanceBlendEnabled = distanceBlendSettings.x > 0.99 ? true : false;
    int   distanceBlendMode    = distanceBlendSettings.y;
    half2 distanceBlendMinMax  = distanceBlendSettings.zw;

    int  materialBlendSettingsUnpacked = (int)materialBlendSettings;
    bool materialBlendEnabled          = GetCompressedValue(materialBlendSettingsUnpacked, 0);
    bool BlendMaskAssigned             = GetCompressedValue(materialBlendSettingsUnpacked, 1);
    bool overrideDistanceBlend         = GetCompressedValue(materialBlendSettingsUnpacked, 2);
    bool overrideDistanceBlendTO       = GetCompressedValue(materialBlendSettingsUnpacked, 3);
    
    int  blendMaskType     = materialBlendSettings.y;
    half blendMaskOpacity  = materialBlendSettings.z;
    half blendMaskStrength = materialBlendSettings.w;

    // Construct array assigned textures
    int assignedAVTexturesArray[]  = { AssignedAVTextures0,  AssignedAVTextures1,  AssignedAVTextures2  };
    int assignedNSOTexturesArray[] = { AssignedNSOTextures0, AssignedNSOTextures1, AssignedNSOTextures2 };
    int assignedEMTexturesArray[]  = { AssignedEMTextures0,  AssignedEMTextures1,  AssignedEMTextures2  };
    int assignedBMTexturesArray[]  = { AssignedBMTextures0, 0, 0, 0 };

    // ----------------------- Setup ------------------------- //

    // Variables
    float4 albedoColor   = 1;
    float3 normalVector  = TangentNormalVector;
    float  metallic      = 0;
    float  smoothness    = 0;
    float  occlussion    = 0;
    float3 emissionColor = 0;

    float materialMask = 0;
    float farDistance  = 0;

    if (materialBlendEnabled) {
        // Get mask of blended material
        switch (blendMaskType) {
            case 0: // Perlin Noise
                materialMask = ClassicNoise(UV * materialBlendMaskTO.x + materialBlendMaskTO.zw) * 3;
                break;
            case 1: // Simplex Noise
                materialMask = SimplexNoise(UV * materialBlendMaskTO.x + materialBlendMaskTO.zw) * 2;
                break;
            case 2: // Custom Texture
                float4 bmTextureSample = SampleArrayAtConstantIndex(BMTextures, assignedBMTexturesArray, LayerIndex, UV * materialBlendMaskTO.xy + materialBlendMaskTO.zw, 0, SS);
                materialMask = bmTextureSample.r;
                break;
        }
        
        materialMask *= blendMaskStrength;
        materialMask = clamp(materialMask, 0, 1);
        materialMask *= blendMaskOpacity;
    }

    // ----------------------- Get Materials To Sample ------------------------- //

    int baseLayerIndex  = LayerIndex * 3 + 0;
    int farLayerIndex   = LayerIndex * 3 + 1;
    int blendLayerIndex = LayerIndex * 3 + 2;

    // At most two materials will be sampled when blending between
    // Get the material(s) to be sampled
    bool samplingBase = false;
    bool samplingBlend = false;
    bool samplingDistance = false;
    bool samplingDistanceBlend = false;

    // Check distance blend
    if (distanceBlendEnabled) {
        // Distance Mask
        farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, distanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);

        samplingDistance = farDistance > 0 && (materialMask != 1 || (materialBlendEnabled && !overrideDistanceBlend));
    }

    // Check material blend
    if (materialBlendEnabled) {
        samplingBlend = materialMask > 0;
        if (distanceBlendEnabled && overrideDistanceBlend && overrideDistanceBlendTO && farDistance > 0)
            samplingDistanceBlend = samplingBlend;

        if (samplingDistanceBlend != 0 && farDistance >= 1)
            samplingBlend = false;
    }

    // Check base material
    samplingBase = farDistance != 1 && materialMask != 1;

    // ----------------------- UVs / Triplanar ------------------------- // 

    // Use world space UVs if enabled
    if (UVSpace == 1) {
        // pos / 1000 to allow space for tiling
        // Terrains are default 1000m^2 so this pretty much expands it to 1x1 on a terrain
        UV = WorldPosition.xz / 1000;
    }

#ifdef _REPETITIONLESS_TRIPLANAR
    float3 triplanarWeights = pow(abs(WorldNormalVector), 8);
    triplanarWeights /= dot(triplanarWeights, 1.0);

    float2 triplanarUVs[3] = {
        WorldPosition.yz / 1000,
        WorldPosition.xz / 1000,
        WorldPosition.xy / 1000
    };

    float triplanarWeightsArray[3] = {
        triplanarWeights.x,
        triplanarWeights.y,
        triplanarWeights.z
    };

    float4 triplanarAlbedo = 0;

    [unroll]
    for (int i = 0; i < 3; i++) {
        // Dont sample this side if not used
        if (triplanarWeightsArray[i] < 0.01)
            continue;

        UV = triplanarUVs[i];

#endif

    // ----------------------- Base Material ------------------------- //
    if (samplingBase) {
        SampleRepetitionlessMaterial(
            SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
            baseLayerIndex, AVTextures, NSOTextures, EMTextures, assignedAVTexturesArray, assignedNSOTexturesArray, assignedEMTexturesArray,
            NoiseTexture,
            baseMaterialData,
            albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor
        );
    }

    // ----------------------- Distance Material ------------------------- //
    if (samplingDistance) {
        float4 farAlbedoColor = 1;
        float3 farNormalVector = TangentNormalVector;
        float farMetallic = 0;
        float farSmoothness = 0;
        float farOcclussion = 0;
        float3 farEmissionColor = 0;
    
        switch (distanceBlendMode)
        {
            case 0: // Tiling & Offset
                // Sample Base Material
                // Set far TO, no need to change back it wont be used again
                
                baseMaterialData.TilingOffset = farMaterialData.TilingOffset;
                
                SampleRepetitionlessMaterial(
                    SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                    baseLayerIndex, AVTextures, NSOTextures, EMTextures, assignedAVTexturesArray, assignedNSOTexturesArray, assignedEMTexturesArray,
                    NoiseTexture,
                    baseMaterialData,
                    farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                );
                break;
            case 1: // Material
                // Sample Far Material
                SampleRepetitionlessMaterial(
                    SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                    farLayerIndex, AVTextures, NSOTextures, EMTextures, assignedAVTexturesArray, assignedNSOTexturesArray, assignedEMTexturesArray,
                    NoiseTexture,
                    farMaterialData,
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

    // ----------------------- Blend Material ------------------------- //
    if (samplingBlend) {
        float4 blendAlbedoColor = 1;
        float3 blendNormalVector = TangentNormalVector;
        float blendMetallic = 0;
        float blendSmoothness = 0;
        float blendOcclussion = 0;
        float3 blendEmissionColor = 0;

        SampleRepetitionlessMaterial(
            SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
            blendLayerIndex, AVTextures, NSOTextures, EMTextures, assignedAVTexturesArray, assignedNSOTexturesArray, assignedEMTexturesArray,
            NoiseTexture,
            blendMaterialData,
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

    // ----------------------- Distance Blend Material ------------------------- //
    // Only used when the blend tiling offset is changed at a distance

    if (samplingDistanceBlend) {
        float4 blendAlbedoColor = 1;
        float3 blendNormalVector = TangentNormalVector;
        float blendMetallic = 0;
        float blendSmoothness = 0;
        float blendOcclussion = 0;
        float3 blendEmissionColor = 0;
        
        float4 tilingOffset = float4(blendMaskDistanceTO.xy, blendMaskDistanceTO.zw);
        
        // Sample Blend Material
        // Set blend TO, no need to change back it wont be used again

        blendMaterialData.TilingOffset = tilingOffset;
        
        SampleRepetitionlessMaterial(
            SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
            blendLayerIndex, AVTextures, NSOTextures, EMTextures, assignedAVTexturesArray, assignedNSOTexturesArray, assignedEMTexturesArray,
            NoiseTexture,
            blendMaterialData,
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

#ifdef _REPETITIONLESS_TRIPLANAR
        triplanarAlbedo += albedoColor * triplanarWeightsArray[i];
        albedoColor = 1;
    } // End loop

    albedoColor = triplanarAlbedo;
#endif

    // ----------------------- Output ------------------------- //

    // Debugging
    switch (DebuggingIndex) {
        case 2: albedoColor = farDistance; break;
        case 3: albedoColor = materialMask; break;
    }  
    
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

// Uses assigned array properties texture
void SampleRepetitionlessLayer_float(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector, float3 WorldNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int UVSpace, int DebuggingIndex,

    // Properties
    int LayerIndex,
    UnityTexture2D PropertiesTexture,
    UnityTexture2D AssignedTexturesTexture,

    // Textures
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    UnityTexture2DArray BMTextures,

    UnityTexture2D NoiseTexture,

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float  MetallicOut,
    out float  SmoothnessOut,
    out float  OcclussionOut,
    out float3 EmissionColorOut
){
    int assignedAVTextures[3];
    int assignedNSOTextures[3];
    int assignedEVTextures[3];
    int assignedBMTextures;

    GetArrayAssignedTextures(AssignedTexturesTexture, assignedAVTextures, assignedNSOTextures, assignedEVTextures, assignedBMTextures);

    SampleRepetitionlessLayer_float(
        SS, UV, TangentNormalVector, WorldNormalVector,
        WorldPosition, CameraPosition,
        SurfaceType, UVSpace, DebuggingIndex,
        LayerIndex,
        PropertiesTexture,
        assignedAVTextures[0], assignedAVTextures[1], assignedAVTextures[2],
        assignedNSOTextures[0], assignedNSOTextures[1], assignedNSOTextures[2],
        assignedEVTextures[0], assignedEVTextures[1], assignedEVTextures[2],
        assignedBMTextures,
        AVTextures,
        NSOTextures,
        EMTextures,
        BMTextures,
        NoiseTexture,
        AlbedoColorOut, NormalVectorOut, MetallicOut, SmoothnessOut, OcclussionOut, EmissionColorOut
    );
}

#endif