#ifndef SAMPLESEAMLESSMATERIAL_INCLUDED
#define SAMPLESEAMLESSMATERIAL_INCLUDED

#include "../Structs/RepetitionlessMaterial.hlsl"
#include "../Structs/RepetitionlessMaterialArray.hlsl"

#include "../SeamlessHelpers/SeamlessNoise.hlsl"
#include "../SeamlessHelpers/MacroMicroVariation.hlsl"

#include "../Noise/Keijiro/SimplexNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"

/* There isnt really any good way to refactor this code to work with multiple sampling types (regular, texture arrays) in hlsl
 * so as a last resort I have just included both types of materials into the base and the wrappers tell it which one to use.
 * Im not a fan of this hacky approach but I could not figure out any other way
 */

void GetRepetitionlessMaterialColorBase(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, int DebuggingIndex,

    bool usingArrayMaterial,
    in RepetitionlessMaterial Material,
    in RepetitionlessMaterialArray ArrayMaterial,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut
){
    RepetitionlessMaterialData materialData;
    if (usingArrayMaterial) materialData = ArrayMaterial.Data;
    else                    materialData = Material.Data;

    // Default values
    AlbedoColorOut = 1;
    NormalVectorOut = TangentNormalVector;
    MetallicOut = 0;
    SmoothnessOut = 0;
    OcclussionOut = 1;
    EmissionColorOut = 0;

    // Setting Toggles
    int settingToggles = (int)materialData.Settings.x;
    
    bool noiseEnabled = (settingToggles & 1) != 0;
    bool randomiseNoiseScaling = (settingToggles & 2) != 0;
    bool randomiseRotation = (settingToggles & 4) != 0;
    bool smoothnessEnabled = (settingToggles & 8) != 0;
    bool variationEnabled = (settingToggles & 16) != 0;
    bool packedTexture = (settingToggles & 32) != 0;
    bool emissionEnabled = (settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int)materialData.Settings.y;
    
    bool metallicAssigned = (assignedTextures & 1) != 0;
    bool smoothnessAssigned = (assignedTextures & 2) != 0;
    bool roughnessAssigned = (assignedTextures & 4) != 0;
    bool normalAssigned = (assignedTextures & 8) != 0;
    bool occlussionAssigned = (assignedTextures & 16) != 0;
    bool emissionAssigned = (assignedTextures & 32) != 0;
    
    // Material Properties
    float metallic = materialData.Properties1.x;
    float smoothness = materialData.Properties1.y;
    float roughness = materialData.Properties1.z;
    float normalScale = materialData.Properties1.w;
    float occlussionStrength = materialData.Properties2.x;
    float alphaClipping = materialData.Properties2.y;
    
    // Noise Settings
    float noiseAngleOffset = materialData.NoiseSettings.x;
    float noiseScale = materialData.NoiseSettings.y;
    float2 noiseScalingMinMax = materialData.NoiseMinMax.xy;
    float2 randomiseRotationMinMax = materialData.NoiseMinMax.zw;
    
    // Variation Settings
    float variationOpacity = materialData.VariationSettings.x;
    float variationNoiseStrength = materialData.VariationNoiseSettings.x;
    float variationNoiseScale = materialData.VariationNoiseSettings.y;
    float2 variationNoiseOffset = materialData.VariationNoiseSettings.zw;
    
    // Setup UVs
    float2 tiling = materialData.TilingOffset.xy;
    float2 offset = materialData.TilingOffset.zw;
    
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
        switch (materialData.VariationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(materialData.VariationSettings.y, materialData.VariationSettings.z, materialData.VariationSettings.w, materialData.VariationBrightness, materialData.VariationNoiseSettings.x, oriUV, materialData.VariationNoiseSettings.y, materialData.VariationNoiseSettings.z);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(materialData.VariationSettings.y, materialData.VariationSettings.z, materialData.VariationSettings.w, materialData.VariationBrightness, materialData.VariationNoiseSettings.x, oriUV, materialData.VariationNoiseSettings.y, materialData.VariationNoiseSettings.z);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTexture(materialData.VariationSettings.y, materialData.VariationSettings.z, materialData.VariationSettings.w, materialData.VariationBrightness, materialData.VariationTexture, SS, oriUV, materialData.VariationTextureTO.xy, materialData.VariationTextureTO.zw);
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
    if (usingArrayMaterial) AlbedoColorOut = SampleSeamlessArrayTexture(ArrayMaterial.Textures, ArrayMaterial.ArrayAssignedTextures, 0, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
    else                    AlbedoColorOut = SampleSeamlessTexture(Material.Albedo, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);

    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        if (usingArrayMaterial) NormalVectorOut = SampleSeamlessTexture(ArrayMaterial.NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges, true, normalScale).rgb;
        else                    NormalVectorOut = SampleSeamlessTexture(Material.NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges, true, normalScale).rgb;

        // Hacky fix to check if normal is assigned, values set in TextureUtilities::UnpackNormalMap. If not assigned, used default tangent normal vector
        // Implemented to check for terrain data normal maps as there is no variable to tell if its assigned or not
        if (NormalVectorOut.x == 0.5 && NormalVectorOut.y == 0.5 && NormalVectorOut.z == 1) {
            NormalVectorOut = TangentNormalVector;
        }
    } else {
        NormalVectorOut = TangentNormalVector;
    }
    
    // Metallic
    if (metallicAssigned) {
        if (usingArrayMaterial) MetallicOut = SampleSeamlessArrayTexture(ArrayMaterial.Textures, ArrayMaterial.ArrayAssignedTextures, 1, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).r;
        else                    MetallicOut = SampleSeamlessTexture(Material.MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).r;
    } else
        MetallicOut = metallic;

    // Smoothness / Roughness
    if (smoothnessEnabled) {
        if (smoothnessAssigned) {
            float4 smoothnessColor = 0;
            if (usingArrayMaterial) smoothnessColor = SampleSeamlessArrayTexture(ArrayMaterial.Textures, ArrayMaterial.ArrayAssignedTextures, 2, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
            else                    smoothnessColor = SampleSeamlessTexture(Material.SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);

            SmoothnessOut = packedTexture ? smoothnessColor.a : smoothnessColor.r;
        } else
            SmoothnessOut = smoothness;
    } else {
        if (roughnessAssigned) {
            // Roughness = 1 - Smoothness
            float4 roughnessColor = 1;
            if (usingArrayMaterial) roughnessColor -= SampleSeamlessArrayTexture(ArrayMaterial.Textures, ArrayMaterial.ArrayAssignedTextures, 3, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
            else                    roughnessColor -= SampleSeamlessTexture(Material.RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);

            SmoothnessOut = packedTexture ? roughnessColor.a : roughnessColor.r;
        } else
            SmoothnessOut = 1 - roughness;
    }
        
    // Occlussion
    if (occlussionAssigned) {
        float4 occlussionColor = 1;
        if (usingArrayMaterial) occlussionColor = SampleSeamlessArrayTexture(ArrayMaterial.Textures, ArrayMaterial.ArrayAssignedTextures, 4, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
        else                    occlussionColor = SampleSeamlessTexture(Material.OcclussionMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);

        OcclussionOut = packedTexture ? occlussionColor.g : occlussionColor.r;
        OcclussionOut = lerp(1, OcclussionOut, occlussionStrength);
    } else
        OcclussionOut = 1;

    // Emission
    if(emissionEnabled) {
        if (emissionAssigned) {
            if (usingArrayMaterial) EmissionColorOut = SampleSeamlessArrayTexture(ArrayMaterial.Textures, ArrayMaterial.ArrayAssignedTextures, 5, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).rgb;
            else                    EmissionColorOut = SampleSeamlessTexture(Material.EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges).rgb;
            EmissionColorOut *= materialData.EmissionColor;
        } else
            EmissionColorOut = materialData.EmissionColor;
    } else
        EmissionColorOut = 0;
}

void GetRepetitionlessMaterialColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, int DebuggingIndex,

    in RepetitionlessMaterial Material,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut
){
    RepetitionlessMaterialArray emptyMaterial;

    GetRepetitionlessMaterialColorBase(
        SS, UV, TangentNormalVector,
        SurfaceType, DebuggingIndex,
        false, Material, emptyMaterial,
        AlbedoColorOut, NormalVectorOut, MetallicOut, SmoothnessOut, OcclussionOut, EmissionColorOut
    );
}

void GetRepetitionlessMaterialArrayColor(
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, int DebuggingIndex,

    in RepetitionlessMaterialArray Material,

    out float4 AlbedoColorOut, out float3 NormalVectorOut, out float MetallicOut, out float SmoothnessOut, out float OcclussionOut, out float3 EmissionColorOut
)
{
    RepetitionlessMaterial emptyMaterial;

    GetRepetitionlessMaterialColorBase(
        SS, UV, TangentNormalVector,
        SurfaceType, DebuggingIndex,
        true, emptyMaterial, Material,
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