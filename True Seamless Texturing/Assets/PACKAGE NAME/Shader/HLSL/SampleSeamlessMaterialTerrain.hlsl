#ifndef SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED
#define SAMPLESEAMLESSMATERIALTERRAIN_INCLUDED

#include "SampleSeamlessMaterial.hlsl"
#include "Utilities/TextureUtilities.hlsl"

void SampleTerrainLayer(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition, // Positions
    float DebuggingIndex,

    bool DistanceBlendEnabled, float2 DistanceBlendMinMax, float4 DistanceBlendTO, // Distance Blending

    // Layer Visuals
    float LayerSettings,
    UnityTexture2D LayerAlbedo,
    UnityTexture2D LayerNormal, float LayerNormalScale,
    UnityTexture2D LayerMask, float LayerHasMask,
    float LayerMetallic,
    float LayerSmoothness,
    float4 Layer_ST,

    float2 LayerNoiseSettings, float4 LayerNoiseMinMax, // Noise

    float LayerVariationMode, float4 LayerVariationSettings, float LayerVariationBrightness, // Variation Settings
    float4 LayerVariationNoiseSettings, // Variation Noise
    UnityTexture2D LayerVariationTexture, float4 LayerVariationTextureTO, // Variation Texture

    float2 LayerDistanceBlendMinMax, float4 LayerDistanceBlendTO, // Distance Blend

    // Outputs
    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float FarDistanceOut)
{
    float4 layerAlbedo = 1;
    float3 layerNormal = TangentNormalVector;
    float layerMetallic = 0;
    float layerSmoothness = 0;
    float layerOcclussion = 1;
    float farDistance = 0;
    
    GetSeamlessTerrainLayerColor(
            SS, UV, TangentNormalVector, DebuggingIndex,
            LayerSettings,
            LayerAlbedo,
            LayerNormal, LayerNormalScale,
            LayerMask, LayerHasMask,
            LayerMetallic,
            LayerSmoothness,
            Layer_ST,
            LayerNoiseSettings, LayerNoiseMinMax,
            LayerVariationMode, LayerVariationSettings, LayerVariationBrightness,
            LayerVariationNoiseSettings,
            LayerVariationTexture, LayerVariationTextureTO,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion);
        
    if (DistanceBlendEnabled)
    {
        // Settings
        int settings = (int) LayerSettings;
        bool overrideDistanceBlendMinMax = (settings & 16) != 0;
        bool overrideDistanceBlendTO = (settings & 32) != 0;
            
        float2 distanceBlendMinMax = overrideDistanceBlendMinMax ? LayerDistanceBlendMinMax : DistanceBlendMinMax;
        float4 distanceBlendTO = overrideDistanceBlendTO ? LayerDistanceBlendTO : DistanceBlendTO;
            
        // Distance Mask
        farDistance = distance(WorldPosition, CameraPosition);
        farDistance = Remap(farDistance, distanceBlendMinMax, float2(0, 1));
        farDistance = clamp(farDistance, 0, 1);
            
        // Sample material with far tiling & offset
        float4 farAlbedoColor = 1;
        float3 farNormalVector = TangentNormalVector;
        float farMetallic = 0;
        float farSmoothness = 0;
        float farOcclussion = 1;
        
        GetSeamlessTerrainLayerColor(
                SS, UV, TangentNormalVector, DebuggingIndex,
                LayerSettings,
                LayerAlbedo,
                LayerNormal, LayerNormalScale,
                LayerMask, LayerHasMask,
                LayerMetallic,
                LayerSmoothness,
                Layer_ST,
                LayerNoiseSettings, LayerNoiseMinMax,
                LayerVariationMode, LayerVariationSettings, LayerVariationBrightness,
                LayerVariationNoiseSettings,
                LayerVariationTexture, distanceBlendTO,
                farAlbedoColor, farNormalVector, farMetallic, farSmoothness, farOcclussion);
            
            // Blend with base material
        layerAlbedo = lerp(layerAlbedo, farAlbedoColor, farDistance);
        layerNormal = lerp(layerNormal, farNormalVector, farDistance);
        layerMetallic = lerp(layerMetallic, farMetallic, farDistance);
        layerSmoothness = lerp(layerSmoothness, farSmoothness, farDistance);
        layerOcclussion = lerp(layerOcclussion, farOcclussion, farDistance);
    }
    
    AlbedoColorOut = layerAlbedo;
    NormalVectorOut = layerNormal;
    MetallicOut = layerMetallic;
    SmoothnessOut = layerSmoothness;
    OcclussionOut = layerOcclussion;
    FarDistanceOut = farDistance;
}

void SampleSeamlessMaterialTerrain_float(
    SamplerState SS, SamplerState ControlSS, float2 UV, float3 TangentNormalVector, // Control requires clamped sampler state, otherwise fades off at edges of terrain
    float3 WorldPosition, float3 CameraPosition, // Positions
    UnityTexture2D Holes, UnityTexture2D Control,
    float SurfaceType, float DebuggingIndex, // Enums

    bool DistanceBlendEnabled, float2 DistanceBlendMinMax, float4 DistanceBlendTO, // Distance Blending

    // Layer 1
    float Layer1Settings,
    UnityTexture2D Layer1Albedo,
    UnityTexture2D Layer1Normal, float Layer1NormalScale,
    UnityTexture2D Layer1Mask, float Layer1HasMask,
    float Layer1Metallic,
    float Layer1Smoothness,
    float4 Layer1_ST,

    float2 Layer1NoiseSettings, float4 Layer1NoiseMinMax, // Noise

    float Layer1VariationMode, float4 Layer1VariationSettings, float Layer1VariationBrightness, // Variation Settings
    float4 Layer1VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer1VariationTexture, float4 Layer1VariationTextureTO, // Variation Texture

    float2 Layer1DistanceBlendMinMax, float4 Layer1DistanceBlendTO, // Distance Blend

    // Layer 2
    float Layer2Settings,
    UnityTexture2D Layer2Albedo,
    UnityTexture2D Layer2Normal, float Layer2NormalScale,
    UnityTexture2D Layer2Mask, float Layer2HasMask,
    float Layer2Metallic,
    float Layer2Smoothness,
    float4 Layer2_ST,

    float2 Layer2NoiseSettings, float4 Layer2NoiseMinMax, // Noise

    float Layer2VariationMode, float4 Layer2VariationSettings, float Layer2VariationBrightness, // Variation Settings
    float4 Layer2VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer2VariationTexture, float4 Layer2VariationTextureTO, // Variation Texture

    float2 Layer2DistanceBlendMinMax, float4 Layer2DistanceBlendTO, // Distance Blend

    // Layer 3
    float Layer3Settings,
    UnityTexture2D Layer3Albedo,
    UnityTexture2D Layer3Normal, float Layer3NormalScale,
    UnityTexture2D Layer3Mask, float Layer3HasMask,
    float Layer3Metallic,
    float Layer3Smoothness,
    float4 Layer3_ST,

    float2 Layer3NoiseSettings, float4 Layer3NoiseMinMax, // Noise

    float Layer3VariationMode, float4 Layer3VariationSettings, float Layer3VariationBrightness, // Variation Settings
    float4 Layer3VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer3VariationTexture, float4 Layer3VariationTextureTO, // Variation Texture

    float2 Layer3DistanceBlendMinMax, float4 Layer3DistanceBlendTO, // Distance Blend

    // Layer 4
    float Layer4Settings,
    UnityTexture2D Layer4Albedo,
    UnityTexture2D Layer4Normal, float Layer4NormalScale,
    UnityTexture2D Layer4Mask, float Layer4HasMask,
    float Layer4Metallic,
    float Layer4Smoothness,
    float4 Layer4_ST,

    float2 Layer4NoiseSettings, float4 Layer4NoiseMinMax, // Noise

    float Layer4VariationMode, float4 Layer4VariationSettings, float Layer4VariationBrightness, // Variation Settings
    float4 Layer4VariationNoiseSettings, // Variation Noise
    UnityTexture2D Layer4VariationTexture, float4 Layer4VariationTextureTO, // Variation Texture

    float2 Layer4DistanceBlendMinMax, float4 Layer4DistanceBlendTO, // Distance Blend

    // Outputs
    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut)
{
    // Setup control for additive blending (Using over lerp as it removes white borders)
    float4 controlColor = SAMPLE_TEXTURE2D(Control, ControlSS, UV);
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
    float farDistance = 0;
    
    // ----------------------- Layer 1 ------------------------- //

    float layer1Control = controlColor.r;
    if (layer1Control > 0) {
        float4 layerAlbedo = albedoColor;
        float3 layerNormal = normalVector;
        float layerMetallic = metallic;
        float layerSmoothness = smoothness;
        float layerOcclussion = occlussion;
        float layerFarDistance = farDistance;
        
        SampleTerrainLayer(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            DebuggingIndex,
            DistanceBlendEnabled, DistanceBlendMinMax, DistanceBlendTO,
            Layer1Settings,
            Layer1Albedo,
            Layer1Normal, Layer1NormalScale,
            Layer1Mask, Layer1HasMask,
            Layer1Metallic,
            Layer1Smoothness,
            Layer1_ST,
            Layer1NoiseSettings, Layer1NoiseMinMax,
            Layer1VariationMode, Layer1VariationSettings, Layer1VariationBrightness,
            Layer1VariationNoiseSettings,
            Layer1VariationTexture, Layer1VariationTextureTO,
            Layer1DistanceBlendMinMax, Layer1DistanceBlendTO,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerFarDistance);
        
        albedoColor += layerAlbedo * layer1Control;
        normalVector += layerNormal * layer1Control;
        metallic += layerMetallic * layer1Control;
        smoothness += layerSmoothness * layer1Control;
        occlussion += layerOcclussion * layer1Control;
        farDistance += layerFarDistance * layer1Control;
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
        float layerFarDistance = farDistance;
        
        SampleTerrainLayer(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            DebuggingIndex,
            DistanceBlendEnabled, DistanceBlendMinMax, DistanceBlendTO,
            Layer2Settings,
            Layer2Albedo,
            Layer2Normal, Layer1NormalScale,
            Layer2Mask, Layer1HasMask,
            Layer2Metallic,
            Layer2Smoothness,
            Layer2_ST,
            Layer2NoiseSettings, Layer2NoiseMinMax,
            Layer2VariationMode, Layer2VariationSettings, Layer2VariationBrightness,
            Layer2VariationNoiseSettings,
            Layer2VariationTexture, Layer2VariationTextureTO,
            Layer2DistanceBlendMinMax, Layer2DistanceBlendTO,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerFarDistance);
        
        albedoColor += layerAlbedo * layer2Control;
        normalVector += layerNormal * layer2Control;
        metallic += layerMetallic * layer2Control;
        smoothness += layerSmoothness * layer2Control;
        occlussion += layerOcclussion * layer2Control;
        farDistance += layerFarDistance * layer2Control;
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
        float layerFarDistance = farDistance;
        
        SampleTerrainLayer(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            DebuggingIndex,
            DistanceBlendEnabled, DistanceBlendMinMax, DistanceBlendTO,
            Layer3Settings,
            Layer3Albedo,
            Layer3Normal, Layer1NormalScale,
            Layer3Mask, Layer1HasMask,
            Layer3Metallic,
            Layer3Smoothness,
            Layer3_ST,
            Layer3NoiseSettings, Layer3NoiseMinMax,
            Layer3VariationMode, Layer3VariationSettings, Layer3VariationBrightness,
            Layer3VariationNoiseSettings,
            Layer3VariationTexture, Layer3VariationTextureTO,
            Layer3DistanceBlendMinMax, Layer3DistanceBlendTO,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerFarDistance);
        
        albedoColor += layerAlbedo * layer3Control;
        normalVector += layerNormal * layer3Control;
        metallic += layerMetallic * layer3Control;
        smoothness += layerSmoothness * layer3Control;
        occlussion += layerOcclussion * layer3Control;
        farDistance += layerFarDistance * layer3Control;
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
        float layerFarDistance = farDistance;
        
        SampleTerrainLayer(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            DebuggingIndex,
            DistanceBlendEnabled, DistanceBlendMinMax, DistanceBlendTO,
            Layer4Settings,
            Layer4Albedo,
            Layer4Normal, Layer4NormalScale,
            Layer4Mask, Layer4HasMask,
            Layer4Metallic,
            Layer4Smoothness,
            Layer4_ST,
            Layer4NoiseSettings, Layer4NoiseMinMax,
            Layer4VariationMode, Layer4VariationSettings, Layer4VariationBrightness,
            Layer4VariationNoiseSettings,
            Layer4VariationTexture, Layer4VariationTextureTO,
            Layer4DistanceBlendMinMax, Layer4DistanceBlendTO,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerFarDistance);
        
        albedoColor += layerAlbedo * layer4Control;
        normalVector += layerNormal * layer4Control;
        metallic += layerMetallic * layer4Control;
        smoothness += layerSmoothness * layer4Control;
        occlussion += layerOcclussion * layer4Control;
        farDistance += layerFarDistance * layer4Control;
    }
    
    // Debugging
    if (DebuggingIndex == 2)
        albedoColor = farDistance;
    
    // Holes
    float4 holesColor = SAMPLE_TEXTURE2D(Holes, SS, UV);
    clip(holesColor.r - 0.001); // If hole value will be 0, subtract small value to get it below 0, will not clip otherwise
    
    // If Transparency Disabled
    if (SurfaceType == 0 || DebuggingIndex != -1)
        albedoColor.a = 1.0;
    
    // Output
    AlbedoColorOut = albedoColor;
    NormalVectorOut = normalVector;
    MetallicOut = metallic;
    SmoothnessOut = smoothness;
    OcclussionOut = occlussion;
}
#endif