#ifndef SAMPLESEAMLESSMATERIAL_CODE_INCLUDED
#define SAMPLESEAMLESSMATERIAL_CODE_INCLUDED

#include "SeamlessHelpers/SeamlessNoise_CODE.hlsl"
#include "SeamlessHelpers/MacroMicroVariation_CODE.hlsl"

#include "Noise/Keijiro/SimplexNoise2D.hlsl"
#include "Noise/Keijiro/ClassicNoise2D.hlsl"

void GetSeamlessMaterialColor(
    float2 UV, float3 TangentNormalVector,
    int SurfaceType, int DebuggingIndex, // Material Properties

    RepetitionlessMaterial material, // Material

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut) // Outputs
{
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setting Toggles
    int settingToggles = (int) material.Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int) material.Settings.y;
    
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = material.MaterialProperties1.x;
    float smoothness = material.MaterialProperties1.y;
    float roughness = material.MaterialProperties1.z;
    float normalScale = material.MaterialProperties1.w;
    float occlussionStrength = material.MaterialProperties2.x;
    float alphaClipping = material.MaterialProperties2.y;
    
    // Noise Settings
    float noiseAngleOffset = material.NoiseSettings.x;
    float noiseScale = material.NoiseSettings.y;
    float2 noiseScalingMinMax = material.NoiseMinMax.xy;
    float2 randomiseRotationMinMax = material.NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = material.VariationSettings.x;
    float variationNoiseStrength = material.VariationNoiseSettings.x;
    float variationNoiseScale = material.VariationNoiseSettings.y;
    float2 variationNoiseOffset = material.VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = material.TilingOffset.xy;
    float2 offset = material.TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    return EdgeMask;

    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (material.VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(material.VariationSettings.y, material.VariationSettings.z, material.VariationSettings.w, material.VariationBrightness, material.VariationNoiseSettings.x, oriUV, material.VariationNoiseSettings.y, material.VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(material.VariationSettings.y, material.VariationSettings.z, material.VariationSettings.w, material.VariationBrightness, material.VariationNoiseSettings.x, oriUV, material.VariationNoiseSettings.y, material.VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(material.VariationSettings.y, material.VariationSettings.z, material.VariationSettings.w, material.VariationBrightness, material.VariationTexture, oriUV, material.VariationTextureTO.xy, material.VariationTextureTO.zw);
                break;
        }
    }
    
    // Debugging
    if (DebuggingIndex != -1) {
        switch (DebuggingIndex) {
            case 0: // Voronoi Cells
                AlbedoColorOut = VoronoiCells;
                break;
            case 1: // Edge Mask
                AlbedoColorOut = EdgeMask;
                break;
            case 4: // Variation Colour
                AlbedoColorOut = variationColor;
                break;
            default:
                AlbedoColorOut = 0;
                break;
        }
        
        return;
    }
    
    // Albedo
    AlbedoColorOut = SampleSeamlessTexture(material.Albedo, EdgeMask, EdgeUV, TransformedUV, noiseEnabled) * material.AlbedoTint;
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = SampleSeamlessTexture(material.NormalMap, EdgeMask, EdgeUV, TransformedUV, noiseEnabled, true, normalScale).rgb;
        
        // Hacky fix to check if normal is assigned, values set in TextureUtilities::UnpackNormalMap. If not assigned, used default tangent normal vector
        // Implemented to check for terrain data normal maps as there is no variable to tell if its assigned or not
        if (NormalVectorOut.x == 0.5 && NormalVectorOut.y == 0.5 && NormalVectorOut.z == 1) {
            NormalVectorOut = TangentNormalVector;
        }
    } else {
        NormalVectorOut = TangentNormalVector;
    }
    
    // Metallic
    if (metallicAssigned)
        MetallicOut = SampleSeamlessTexture(material.MetallicMap, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).r;
    else
        MetallicOut = metallic;
    
    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned) {
            float4 smoothnessColor = SampleSeamlessTexture(material.SmoothnessMap, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        } else
            SmoothnessOut = smoothness;
    } else {
        if (roughnessAssigned) {
            float4 roughnessColor = 1 - SampleSeamlessTexture(material.RoughnessMap, EdgeMask, EdgeUV, TransformedUV, noiseEnabled); // Roughness = 1 - Smoothness
            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        } else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        float4 occlussionColor = SampleSeamlessTexture(material.OcclussionMap, EdgeMask, EdgeUV, TransformedUV, noiseEnabled);
        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(OcclussionOut, 1, 1 - occlussionStrength);
    } else
        OcclussionOut = 1;
    
    // Emission
    if(emissionEnabled) {
        if (emissionAssigned)
            EmissionColorOut = SampleSeamlessTexture(material.EmissionMap, EdgeMask, EdgeUV, TransformedUV, noiseEnabled).rgb * material.EmissionColor;
        else
            EmissionColorOut = material.EmissionColor;
    } else
        EmissionColorOut = 0;
}
#endif