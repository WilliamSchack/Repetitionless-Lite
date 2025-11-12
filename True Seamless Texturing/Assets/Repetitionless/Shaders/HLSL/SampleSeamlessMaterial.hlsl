#ifndef SAMPLESEAMLESSMATERIAL_INCLUDED
#define SAMPLESEAMLESSMATERIAL_INCLUDED

#include "Structs/RepetitionlessMaterial.hlsl"
#include "Structs/RepetitionlessArrayMaterial.hlsl"

#include "SeamlessHelpers/SeamlessNoise.hlsl"
#include "SeamlessHelpers/MacroMicroVariation.hlsl"

#include "Noise/Keijiro/SimplexNoise2D.hlsl"
#include "Noise/Keijiro/ClassicNoise2D.hlsl"

/* The defines are for abstraction to remove duplicate code
 * Only defined here since they must be defined before the function definition
 * They are expected to be redefined in the respective function calls
 */

#define REPETITIONLESS_SAMPLE_ALBEDO(ss, em, euv, tuv, se)     float4(0, 0, 0, 0)
#define REPETITIONLESS_SAMPLE_NORMAL(ss, em, euv, tuv, se, ns) float4(0, 0, 0, 0)
#define REPETITIONLESS_SAMPLE_METALLIC(ss, em, euv, tuv, se)   float4(0, 0, 0, 0)
#define REPETITIONLESS_SAMPLE_SMOOTHNESS(ss, em, euv, tuv, se) float4(0, 0, 0, 0)
#define REPETITIONLESS_SAMPLE_ROUGHNESS(ss, em, euv, tuv, se)  float4(0, 0, 0, 0)
#define REPETITIONLESS_SAMPLE_OCCLUSSION(ss, em, euv, tuv, se) float4(0, 0, 0, 0)
#define REPETITIONLESS_SAMPLE_EMISSION(ss, em, euv, tuv, se)   float4(0, 0, 0, 0)

void GetRepetitionlessMaterialColourBase(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex,

    in RepetitionlessMaterialData MaterialData,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut
){
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setting Toggles
    int settingToggles = (int)MaterialData.Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int)MaterialData.Settings.y;
    
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = MaterialData.Properties1.x;
    float smoothness = MaterialData.Properties1.y;
    float roughness = MaterialData.Properties1.z;
    float normalScale = MaterialData.Properties1.w;
    float occlussionStrength = MaterialData.Properties2.x;
    float alphaClipping = MaterialData.Properties2.y;
    
    // Noise Settings
    float noiseAngleOffset = MaterialData.NoiseSettings.x;
    float noiseScale = MaterialData.NoiseSettings.y;
    float2 noiseScalingMinMax = MaterialData.NoiseMinMax.xy;
    float2 randomiseRotationMinMax = MaterialData.NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = MaterialData.VariationSettings.x;
    float variationNoiseStrength = MaterialData.VariationNoiseSettings.x;
    float variationNoiseScale = MaterialData.VariationNoiseSettings.y;
    float2 variationNoiseOffset = MaterialData.VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = MaterialData.TilingOffset.xy;
    float2 offset = MaterialData.TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    bool sampleEdges = EdgeMask > 0;

    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (MaterialData.VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(MaterialData.VariationSettings.y, MaterialData.VariationSettings.z, MaterialData.VariationSettings.w, MaterialData.VariationBrightness, MaterialData.VariationNoiseSettings.x, oriUV, MaterialData.VariationNoiseSettings.y, MaterialData.VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(MaterialData.VariationSettings.y, MaterialData.VariationSettings.z, MaterialData.VariationSettings.w, MaterialData.VariationBrightness, MaterialData.VariationNoiseSettings.x, oriUV, MaterialData.VariationNoiseSettings.y, MaterialData.VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(MaterialData.VariationSettings.y, MaterialData.VariationSettings.z, MaterialData.VariationSettings.w, MaterialData.VariationBrightness, MaterialData.VariationTexture, SS, oriUV, MaterialData.VariationTextureTO.xy, MaterialData.VariationTextureTO.zw);
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
    AlbedoColorOut = REPETITIONLESS_SAMPLE_ALBEDO(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = REPETITIONLESS_SAMPLE_NORMAL(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges, normalScale).rgb;
        
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
        MetallicOut = REPETITIONLESS_SAMPLE_METALLIC(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).r;
    else
        MetallicOut = metallic;

    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned) {
            float4 smoothnessColor = REPETITIONLESS_SAMPLE_SMOOTHNESS(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        } else
            SmoothnessOut = smoothness;
    } else {
        if (roughnessAssigned) {
            float4 roughnessColor = 1 - REPETITIONLESS_SAMPLE_ROUGHNESS(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges); // Roughness = 1 - Smoothness
            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        } else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        float4 occlussionColor = REPETITIONLESS_SAMPLE_OCCLUSSION(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(1, OcclussionOut, occlussionStrength);
    } else
        OcclussionOut = 1;

    // Emission
    if(emissionEnabled) {
        if (emissionAssigned)
            EmissionColorOut = REPETITIONLESS_SAMPLE_EMISSION(SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).rgb * MaterialData.EmissionColor;
        else
            EmissionColorOut = MaterialData.EmissionColor;
    } else
        EmissionColorOut = 0;
}

void GetRepetitionlessMaterialColour(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex,

    in RepetitionlessMaterial Material,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut
){
    // Undefine macros before redefinition
    #undef REPETITIONLESS_SAMPLE_ALBEDO
    #undef REPETITIONLESS_SAMPLE_NORMAL
    #undef REPETITIONLESS_SAMPLE_METALLIC
    #undef REPETITIONLESS_SAMPLE_SMOOTHNESS
    #undef REPETITIONLESS_SAMPLE_ROUGHNESS
    #undef REPETITIONLESS_SAMPLE_OCCLUSSION
    #undef REPETITIONLESS_SAMPLE_EMISSION

    // Define macros
    #define REPETITIONLESS_SAMPLE_ALBEDO(ss, em, euv, tuv, se)     SampleSeamlessTexture(Material.Albedo, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_NORMAL(ss, em, euv, tuv, se, ns) SampleSeamlessTexture(Material.NormalMap, ss, em, euv, tuv, se, true, ns)
    #define REPETITIONLESS_SAMPLE_METALLIC(ss, em, euv, tuv, se)   SampleSeamlessTexture(Material.MetallicMap, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_SMOOTHNESS(ss, em, euv, tuv, se) SampleSeamlessTexture(Material.SmoothnessMap, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_ROUGHNESS(ss, em, euv, tuv, se)  SampleSeamlessTexture(Material.RoughnessMap, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_OCCLUSSION(ss, em, euv, tuv, se) SampleSeamlessTexture(Material.OcclussionMap, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_EMISSION(ss, em, euv, tuv, se)   SampleSeamlessTexture(Material.EmissionMap, ss, em, euv, tuv, se)

    // Sample the material
    GetRepetitionlessMaterialColourBase(
        SS, UV, TangentNormalVector,
        SurfaceType, DebuggingIndex,
        Material.Data,
        AlbedoColorOut, NormalVectorOut, MetallicOut, SmoothnessOut, OcclussionOut, EmissionColorOut
    );
}

void GetRepetitionlessArrayMaterialColour(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex,

    in RepetitionlessArrayMaterial Material,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut
)
{
    // Undefine macros before redefinition
    #undef REPETITIONLESS_SAMPLE_ALBEDO
    #undef REPETITIONLESS_SAMPLE_NORMAL
    #undef REPETITIONLESS_SAMPLE_METALLIC
    #undef REPETITIONLESS_SAMPLE_SMOOTHNESS
    #undef REPETITIONLESS_SAMPLE_ROUGHNESS
    #undef REPETITIONLESS_SAMPLE_OCCLUSSION
    #undef REPETITIONLESS_SAMPLE_EMISSION

    // Define macros
    #define REPETITIONLESS_SAMPLE_ALBEDO(ss, em, euv, tuv, se)     SampleSeamlessArrayTexture(Material.Textures, Material.ArrayAssignedTextures, 0, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_NORMAL(ss, em, euv, tuv, se, ns) SampleSeamlessTexture(Material.NormalMap, ss, em, euv, tuv, se, true, ns)
    #define REPETITIONLESS_SAMPLE_METALLIC(ss, em, euv, tuv, se)   SampleSeamlessArrayTexture(Material.Textures, Material.ArrayAssignedTextures, 1, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_SMOOTHNESS(ss, em, euv, tuv, se) SampleSeamlessArrayTexture(Material.Textures, Material.ArrayAssignedTextures, 2, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_ROUGHNESS(ss, em, euv, tuv, se)  SampleSeamlessArrayTexture(Material.Textures, Material.ArrayAssignedTextures, 3, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_OCCLUSSION(ss, em, euv, tuv, se) SampleSeamlessArrayTexture(Material.Textures, Material.ArrayAssignedTextures, 4, ss, em, euv, tuv, se)
    #define REPETITIONLESS_SAMPLE_EMISSION(ss, em, euv, tuv, se)   SampleSeamlessArrayTexture(Material.Textures, Material.ArrayAssignedTextures, 5, ss, em, euv, tuv, se)

    // Sample the material
    GetRepetitionlessMaterialColourBase(
        SS, UV, TangentNormalVector,
        SurfaceType, DebuggingIndex,
        Material.Data,
        AlbedoColorOut, NormalVectorOut, MetallicOut, SmoothnessOut, OcclussionOut, EmissionColorOut
    );
}

void GetSeamlessMaterialColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex, // Material Properties

    float2 Settings, float4 TilingOffset, // Tiling & Offset
    UnityTexture2D Albedo, // Albedo
    UnityTexture2D MetallicMap, // Metallic
    UnityTexture2D SmoothnessMap, // Smoothness
    UnityTexture2D RoughnessMap, // Roughness
    UnityTexture2D NormalMap, // Normal
    UnityTexture2D OcclussionMap, // Occlussion
    UnityTexture2D EmissionMap, // Emission
    float4 AlbedoTint, float3 EmissionColor, // Colors
    float4 MaterialProperties1, float2 MaterialProperties2, // Material Properties

    float2 NoiseSettings, float4 NoiseMinMax, // Noise

    float VariationMode, float4 VariationSettings, float VariationBrightness, // Variation Settings
    float4 VariationNoiseSettings, // Variation Noise
    UnityTexture2D VariationTexture, float4 VariationTextureTO, // Variation Texture

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut) // Outputs
{
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setting Toggles
    int settingToggles = (int) Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int) Settings.y;
    
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = MaterialProperties1.x;
    float smoothness = MaterialProperties1.y;
    float roughness = MaterialProperties1.z;
    float normalScale = MaterialProperties1.w;
    float occlussionStrength = MaterialProperties2.x;
    float alphaClipping = MaterialProperties2.y;
    
    // Noise Settings
    float noiseAngleOffset = NoiseSettings.x;
    float noiseScale = NoiseSettings.y;
    float2 noiseScalingMinMax = NoiseMinMax.xy;
    float2 randomiseRotationMinMax = NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = VariationSettings.x;
    float variationNoiseStrength = VariationNoiseSettings.x;
    float variationNoiseScale = VariationNoiseSettings.y;
    float2 variationNoiseOffset = VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = TilingOffset.xy;
    float2 offset = TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    bool sampleEdges = EdgeMask > 0;

    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationTexture, SS, oriUV, VariationTextureTO.xy, VariationTextureTO.zw);
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
    AlbedoColorOut = SampleSeamlessTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges) * AlbedoTint;
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = SampleSeamlessTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges, true, normalScale).rgb;
        
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
        MetallicOut = SampleSeamlessTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).r;
    else
        MetallicOut = metallic;

    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned) {
            float4 smoothnessColor = SampleSeamlessTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        } else
            SmoothnessOut = smoothness;
    } else {
        if (roughnessAssigned) {
            float4 roughnessColor = 1 - SampleSeamlessTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges); // Roughness = 1 - Smoothness
            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        } else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        float4 occlussionColor = SampleSeamlessTexture(OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(1, OcclussionOut, occlussionStrength);
    } else
        OcclussionOut = 1;

    // Emission
    if(emissionEnabled) {
        if (emissionAssigned)
            EmissionColorOut = SampleSeamlessTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).rgb * EmissionColor;
        else
            EmissionColorOut = EmissionColor;
    } else
        EmissionColorOut = 0;
}

void GetSeamlessMaterialColorNEW(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex,

    in RepetitionlessMaterial Material,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut)
{
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setting Toggles
    int settingToggles = (int)Material.Data.Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int)Material.Data.Settings.y;
    
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = Material.Data.Properties1.x;
    float smoothness = Material.Data.Properties1.y;
    float roughness = Material.Data.Properties1.z;
    float normalScale = Material.Data.Properties1.w;
    float occlussionStrength = Material.Data.Properties2.x;
    float alphaClipping = Material.Data.Properties2.y;
    
    // Noise Settings
    float noiseAngleOffset = Material.Data.NoiseSettings.x;
    float noiseScale = Material.Data.NoiseSettings.y;
    float2 noiseScalingMinMax = Material.Data.NoiseMinMax.xy;
    float2 randomiseRotationMinMax = Material.Data.NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = Material.Data.VariationSettings.x;
    float variationNoiseStrength = Material.Data.VariationNoiseSettings.x;
    float variationNoiseScale = Material.Data.VariationNoiseSettings.y;
    float2 variationNoiseOffset = Material.Data.VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = Material.Data.TilingOffset.xy;
    float2 offset = Material.Data.TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    bool sampleEdges = EdgeMask > 0;

    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (Material.Data.VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(Material.Data.VariationSettings.y, Material.Data.VariationSettings.z, Material.Data.VariationSettings.w, Material.Data.VariationBrightness, Material.Data.VariationNoiseSettings.x, oriUV, Material.Data.VariationNoiseSettings.y, Material.Data.VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(Material.Data.VariationSettings.y, Material.Data.VariationSettings.z, Material.Data.VariationSettings.w, Material.Data.VariationBrightness, Material.Data.VariationNoiseSettings.x, oriUV, Material.Data.VariationNoiseSettings.y, Material.Data.VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(Material.Data.VariationSettings.y, Material.Data.VariationSettings.z, Material.Data.VariationSettings.w, Material.Data.VariationBrightness, Material.Data.VariationTexture, SS, oriUV, Material.Data.VariationTextureTO.xy, Material.Data.VariationTextureTO.zw);
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
    AlbedoColorOut = SampleSeamlessTexture(Material.Albedo, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges) * Material.Data.AlbedoTint;
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = SampleSeamlessTexture(Material.NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges, true, normalScale).rgb;
        
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
        MetallicOut = SampleSeamlessTexture(Material.MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).r;
    else
        MetallicOut = metallic;

    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned) {
            float4 smoothnessColor = SampleSeamlessTexture(Material.SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        } else
            SmoothnessOut = smoothness;
    } else {
        if (roughnessAssigned) {
            float4 roughnessColor = 1 - SampleSeamlessTexture(Material.RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges); // Roughness = 1 - Smoothness
            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        } else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        float4 occlussionColor = SampleSeamlessTexture(Material.OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(1, OcclussionOut, occlussionStrength);
    } else
        OcclussionOut = 1;

    // Emission
    if(emissionEnabled) {
        if (emissionAssigned)
            EmissionColorOut = SampleSeamlessTexture(Material.EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).rgb * Material.Data.EmissionColor;
        else
            EmissionColorOut = Material.Data.EmissionColor;
    } else
        EmissionColorOut = 0;
}

// Couldnt think of a clean way to reuse the code from the previous function so just copying it over
// Just changed the texture sampling to use a texture array
void GetSeamlessArrayMaterialColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, float DebuggingIndex, // Material Properties

    float2 Settings, float4 TilingOffset, // Tiling & Offset
    UnityTexture2DArray Textures, UnityTexture2D NormalMap, // Textures
    int ArrayAssignedTextures, // Array Assigned Textures
    float4 AlbedoTint, float3 EmissionColor, // Colors
    float4 MaterialProperties1, float2 MaterialProperties2, // Material Properties

    float2 NoiseSettings, float4 NoiseMinMax, // Noise

    float VariationMode, float4 VariationSettings, float VariationBrightness, // Variation Settings
    float4 VariationNoiseSettings, // Variation Noise
    UnityTexture2D VariationTexture, float4 VariationTextureTO, // Variation Texture

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut) // Outputs
{
    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;
    
    // Setting Toggles
    int settingToggles = (int) Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int) Settings.y;
    
    bool albedoAssigned = (ArrayAssignedTextures & 1) != 0;
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = MaterialProperties1.x;
    float smoothness = MaterialProperties1.y;
    float roughness = MaterialProperties1.z;
    float normalScale = MaterialProperties1.w;
    float occlussionStrength = MaterialProperties2.x;
    float alphaClipping = MaterialProperties2.y;
    
    // Noise Settings
    float noiseAngleOffset = NoiseSettings.x;
    float noiseScale = NoiseSettings.y;
    float2 noiseScalingMinMax = NoiseMinMax.xy;
    float2 randomiseRotationMinMax = NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = VariationSettings.x;
    float variationNoiseStrength = VariationNoiseSettings.x;
    float variationNoiseScale = VariationNoiseSettings.y;
    float2 variationNoiseOffset = VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = TilingOffset.xy;
    float2 offset = TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    if (noiseEnabled)
        GetSeamlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    bool sampleEdges = EdgeMask > 0;

    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0)
    {
        switch (VariationMode)
        {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationNoiseSettings.x, oriUV, VariationNoiseSettings.y, VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(VariationSettings.y, VariationSettings.z, VariationSettings.w, VariationBrightness, VariationTexture, SS, oriUV, VariationTextureTO.xy, VariationTextureTO.zw);
                break;
        }
    }
    
    // Debugging
    if (DebuggingIndex != -1)
    {
        switch (DebuggingIndex)
        {
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
    // Directly return colour when not assigned, sampling an empty texture array index will return 0, cancelling out the colour otherwise
    if(albedoAssigned)
        AlbedoColorOut = SampleSeamlessArrayTexture(Textures, ArrayAssignedTextures, 0, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges) * AlbedoTint;
    else
        AlbedoColorOut = AlbedoTint;
    
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned)
    {
        NormalVectorOut = SampleSeamlessTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges, true, normalScale).rgb;
        
        // Check if normal is assigned. If not assigned, used default normal vector
        // Implemented to check for terrain data normal maps as there is no variable to tell if its assigned or not
        if (NormalVectorOut.x == 0.5 && NormalVectorOut.y == 0.5 && NormalVectorOut.z == 1) {
            NormalVectorOut = TangentNormalVector;
        }
    }
    else
    {
        NormalVectorOut = TangentNormalVector;
    }
    
    // Metallic
    if (metallicAssigned)
        MetallicOut = SampleSeamlessArrayTexture(Textures, ArrayAssignedTextures, 1, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).r;
    else
        MetallicOut = metallic;
    
    // Smoothness / Roughness
    if (smoothnessEnabled)
    {
        if (smoothnessAssigned)
        {
            float4 smoothnessColor = SampleSeamlessArrayTexture(Textures, ArrayAssignedTextures, 2, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        }
        else
            SmoothnessOut = smoothness;
    }
    else
    {
        if (roughnessAssigned)
        {
            float4 roughnessColor = 1 - SampleSeamlessArrayTexture(Textures, ArrayAssignedTextures, 3, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges); // Roughness = 1 - Smoothness
            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        }
        else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned)
    {
        float4 occlussionColor = SampleSeamlessArrayTexture(Textures, ArrayAssignedTextures, 4, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(1, OcclussionOut, occlussionStrength);
    }
    else
        OcclussionOut = 1;
    
    // Emission
    if (emissionEnabled)
    {
        if (emissionAssigned)
            EmissionColorOut = SampleSeamlessArrayTexture(Textures, ArrayAssignedTextures, 5, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).rgb * EmissionColor;
        else
            EmissionColorOut = EmissionColor;
    }
    else
        EmissionColorOut = 0;
}

// Undefine macros as they are local to this file
#undef REPETITIONLESS_SAMPLE_ALBEDO
#undef REPETITIONLESS_SAMPLE_NORMAL
#undef REPETITIONLESS_SAMPLE_METALLIC
#undef REPETITIONLESS_SAMPLE_SMOOTHNESS
#undef REPETITIONLESS_SAMPLE_ROUGHNESS
#undef REPETITIONLESS_SAMPLE_OCCLUSSION
#undef REPETITIONLESS_SAMPLE_EMISSION

#endif