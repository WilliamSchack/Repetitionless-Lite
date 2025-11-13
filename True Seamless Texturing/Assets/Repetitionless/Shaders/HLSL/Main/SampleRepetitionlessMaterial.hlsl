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
    AlbedoColorOut *= materialData.AlbedoTint;

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

#endif