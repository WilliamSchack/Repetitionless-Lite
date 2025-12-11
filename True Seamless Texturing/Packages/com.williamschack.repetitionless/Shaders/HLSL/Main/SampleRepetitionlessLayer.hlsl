#ifndef SAMPLEREPETITIONLESSLAYER_INCLUDED
#define SAMPLEREPETITIONLESSLAYER_INCLUDED

#include "../Structs/RepetitionlessLayer.hlsl"
#include "../Structs/RepetitionlessLayerTerrain.hlsl"

#include "../Utilities/TextureUtilities.hlsl"

#include "../Noise/VoronoiNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"
#include "../Noise/Keijiro/SimplexNoise2D.hlsl"

#include "SampleRepetitionlessMaterial.hlsl"

/* There isnt really any good way to refactor this code to work with multiple types in hlsl (No inheritance or callbacks)
 * so as a last resort I have just included both types of materials into the base and the wrappers tell it which one to use.
 * Im not a fan of this hacky approach but I could not figure out any other way
 * If anyone has a better solution please contact me I will change it asap
 */

 /*

void SampleRepetitionlessLayerBase(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int DebuggingIndex,

    bool UsingTerrainLayer,
    in RepetitionlessLayer Layer,
    in RepetitionlessLayerTerrain LayerTerrain,

    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
){
    RepetitionlessLayerData layerData;
    if (UsingTerrainLayer) layerData = LayerTerrain.Data;
    else                   layerData = Layer.Data;

    // Variables
    float4 albedoColor = 1;
    float3 normalVector = TangentNormalVector;
    float metallic = 0;
    float smoothness = 0;
    float occlussion = 0;
    float3 emissionColor = 0;
    
    float materialMask = 0;
    float farDistance = 0;

    float4 farTilingOffset = 0;
    if (UsingTerrainLayer) farTilingOffset = LayerTerrain.FarMaterial.Data.TilingOffset;
    else                   farTilingOffset = Layer.FarMaterial.Data.TilingOffset;

    float4 blendTilingOffset = 0;
    if (UsingTerrainLayer) blendTilingOffset = LayerTerrain.BlendMaterial.Data.TilingOffset;
    else                   blendTilingOffset = Layer.BlendMaterial.Data.TilingOffset;

    // Calculate mask
    int materialBlendSettings       = (int)layerData.MaterialBlendSettings;
    bool materialBlendEnabled       = (materialBlendSettings & 1) != 0;
    bool overrideDistanceBlending   = (materialBlendSettings & 2) != 0;
    bool overrideDistanceBlendingTO = (materialBlendSettings & 4) != 0;

    if (materialBlendEnabled) {
        float blendMaskNoiseScale = layerData.MaterialBlendNoiseSettings.x;
        float2 blendMaskNoiseOffset = layerData.MaterialBlendNoiseSettings.yz;
        
        // Get mask of blended material
        switch (layerData.BlendMaskType) {
            case 0: // Perlin Noise
                materialMask = ClassicNoise(UV * blendMaskNoiseScale + blendMaskNoiseOffset) * 3;
                break;
            case 1: // Simplex Noise
                materialMask = SimplexNoise(UV * blendMaskNoiseScale + blendMaskNoiseOffset) * 2;
                break;
            case 2: // Custom Texture
                materialMask = SAMPLE_TEXTURE2D(layerData.BlendMaskTexture, SS, UV * blendMaskNoiseScale + blendMaskNoiseOffset).r;
                break;
        }

        float blendMaskOpacity = layerData.MaterialBlendProperties.x;
        float blendMaskStrength = layerData.MaterialBlendProperties.y;
        
        materialMask *= blendMaskStrength;
        materialMask = clamp(materialMask, 0, 1);
        materialMask *= blendMaskOpacity;
    }

    // ----------------------- Get Materials To Sample ------------------------- //

    // At most two materials will be sampled when blending between
    // Get the material(s) to be sampled
    bool samplingBase = false;
    bool samplingBlend = false;
    bool samplingDistance = false;
    bool samplingDistanceBlend = false;

    // Check distance blend
    if (layerData.DistanceBlendEnabled) {
        // Distance Mask
        farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, layerData.DistanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);

        samplingDistance = farDistance > 0 && materialMask != 1;
        samplingDistanceBlend = farDistance > 0 && materialBlendEnabled && materialMask > 0 && overrideDistanceBlending;
    }

    // Check material blend
    if (materialBlendEnabled) {
        samplingBlend = materialMask > 0;
        if (layerData.DistanceBlendEnabled && overrideDistanceBlending && farDistance >= 1)
            samplingBlend = false;
    }

    // Check base material
    samplingBase = farDistance != 1 && materialMask != 1;

    // ----------------------- Base Material ------------------------- //
    if (samplingBase) {
        if (UsingTerrainLayer) {
            GetRepetitionlessMaterialColor(
                SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                LayerTerrain.BaseMaterial,
                albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor
            );
        } else {
            GetRepetitionlessMaterialColor(
                SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                Layer.BaseMaterial,
                albedoColor, normalVector, metallic, smoothness, occlussion, emissionColor
            );
        }
    }

    // ----------------------- Blend Material ------------------------- //
    if (samplingBlend) {
        float4 blendAlbedoColor = 1;
        float3 blendNormalVector = TangentNormalVector;
        float blendMetallic = 0;
        float blendSmoothness = 0;
        float blendOcclussion = 0;
        float3 blendEmissionColor = 0;

        if (UsingTerrainLayer) {
            GetRepetitionlessMaterialArrayColor(
                SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                LayerTerrain.BlendMaterial,
                blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
            );
        } else {
            GetRepetitionlessMaterialColor(
                SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                Layer.BlendMaterial,
                blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
            );
        }
        
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
    
        switch (layerData.DistanceBlendingMode)
        {
            case 0: // Tiling & Offset
                // Sample Base Material
                // Set far TO, no need to change back it wont be used again
                
                if (UsingTerrainLayer) {
                    LayerTerrain.BaseMaterial.Data.TilingOffset = farTilingOffset;
                    
                    GetRepetitionlessMaterialColor(
                        SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        LayerTerrain.BaseMaterial,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                } else {
                    Layer.BaseMaterial.Data.TilingOffset = farTilingOffset;

                    GetRepetitionlessMaterialColor(
                        SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        Layer.BaseMaterial,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                }
                break;
            case 1: // Material
                // Sample Far Material
                if (UsingTerrainLayer) {
                    GetRepetitionlessMaterialArrayColor(
                        SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        LayerTerrain.FarMaterial,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                } else {
                    GetRepetitionlessMaterialColor(
                        SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                        Layer.FarMaterial,
                        farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion, farEmissionColor
                    );
                }
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
        
        float2 tiling = blendTilingOffset.xy;
        float2 offset = blendTilingOffset.zw;
        if (layerData.DistanceBlendingMode == 0)
        {
            tiling = overrideDistanceBlendingTO ? layerData.BlendMaskDistanceTO.xy : farTilingOffset.xy;
            offset = overrideDistanceBlendingTO ? layerData.BlendMaskDistanceTO.zw : farTilingOffset.zw;
        }
        
        float4 tilingOffset = float4(tiling.x, tiling.y, offset.x, offset.y);
        
        // Sample Blend Material
        // Set blend TO, no need to change back it wont be used again

        if (UsingTerrainLayer) {
            LayerTerrain.BlendMaterial.Data.TilingOffset = tilingOffset;
            
            GetRepetitionlessMaterialArrayColor(
                SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                LayerTerrain.BlendMaterial,
                blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
            );
        } else {
            Layer.BlendMaterial.Data.TilingOffset = tilingOffset;

            GetRepetitionlessMaterialColor(
                SS, UV, TangentNormalVector, SurfaceType, DebuggingIndex,
                Layer.BlendMaterial,
                blendAlbedoColor, blendNormalVector, blendMetallic, blendSmoothness, blendOcclussion, blendEmissionColor
            );
        }
        
        // Combine Far Blend with Base 
        float lerpFactor = farDistance * materialMask;
        albedoColor = lerp(albedoColor, blendAlbedoColor, lerpFactor);
        normalVector = lerp(normalVector, blendNormalVector, lerpFactor);
        metallic = lerp(metallic, blendMetallic, lerpFactor);
        smoothness = lerp(smoothness, blendSmoothness, lerpFactor);
        occlussion = lerp(occlussion, blendOcclussion, lerpFactor);
        emissionColor = lerp(emissionColor, blendEmissionColor, lerpFactor);
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

void SampleRepetitionlessLayer(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int DebuggingIndex,

    in RepetitionlessLayer Layer,

    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
){
    RepetitionlessLayerTerrain emptyLayer;

    SampleRepetitionlessLayerBase(
        SS, UV, TangentNormalVector,
        WorldPosition, CameraPosition,
        SurfaceType, DebuggingIndex,
        false, Layer, emptyLayer,
        AlbedoColorOut, NormalVectorOut, MetallicOut, SmoothnessOut, OcclussionOut, EmissionColorOut
    );
}

void SampleRepetitionlessLayerTerrain(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int DebuggingIndex,

    in RepetitionlessLayerTerrain Layer,

    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
){
    RepetitionlessLayer emptyLayer;

    SampleRepetitionlessLayerBase(
        SS, UV, TangentNormalVector,
        WorldPosition, CameraPosition,
        SurfaceType, DebuggingIndex,
        true, emptyLayer, Layer,
        AlbedoColorOut, NormalVectorOut, MetallicOut, SmoothnessOut, OcclussionOut, EmissionColorOut
    );
}

*/

#endif