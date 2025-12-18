#ifndef SAMPLEREPETITIONLESSMATERIAL_INCLUDED
#define SAMPLEREPETITIONLESSMATERIAL_INCLUDED

#include "../Structs/RepetitionlessMaterialData.hlsl"

#include "../RepetitionlessHelpers/RepetitionlessNoise.hlsl"
#include "../RepetitionlessHelpers/RepetitionlessTextureUtilities.hlsl"
#include "../RepetitionlessHelpers/MacroMicroVariation.hlsl"
#include "../RepetitionlessHelpers/GetArrayAssignedTextures.hlsl"

#include "../Noise/Keijiro/SimplexNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"

#include "../Utilities/BooleanCompression.hlsl"
#include "../Utilities/TextureUtilities.hlsl"

void SampleRepetitionlessMaterial(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, int DebuggingIndex,

    // Textures
    int ArrayLayerIndex,
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    int AssignedAVTextures[3],
    int AssignedNSOTextures[3],
    int AssignedEMTextures[3],

    // Material Data
    in RepetitionlessMaterialData MaterialData,

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float  MetallicOut,
    out float  SmoothnessOut,
    out float  OcclussionOut,
    out float3 EmissionColorOut
){
    // Get variables from compressed
    int  settingToggles        = (int)MaterialData.Settings1.x;
    bool noiseEnabled          = GetCompressedValue(settingToggles, 0);
    bool randomiseNoiseScaling = GetCompressedValue(settingToggles, 1);
    bool randomiseRotation     = GetCompressedValue(settingToggles, 2);
    bool smoothnessEnabled     = GetCompressedValue(settingToggles, 3);
    bool variationEnabled      = GetCompressedValue(settingToggles, 4);
    bool packedTexture         = GetCompressedValue(settingToggles, 5);
    bool emissionEnabled       = GetCompressedValue(settingToggles, 6);

    int  assignedTextures      = (int)MaterialData.Settings1.y;
    bool albedoAssigned        = GetCompressedValue(assignedTextures, 0);
    bool metallicAssigned      = GetCompressedValue(assignedTextures, 1);
    bool smoothnessAssigned    = GetCompressedValue(assignedTextures, 2);
    bool normalAssigned        = GetCompressedValue(assignedTextures, 3);
    bool occlussionAssigned    = GetCompressedValue(assignedTextures, 4);
    bool emissionAssigned      = GetCompressedValue(assignedTextures, 5);
    bool variationAssigned     = GetCompressedValue(assignedTextures, 6);
    bool packedTextureAssigned = GetCompressedValue(assignedTextures, 7);

    half metallic            = MaterialData.Settings1.z;
    half smoothnessRoughness = MaterialData.Settings1.w;
    half normalScale         = MaterialData.Settings2.x;
    half occlussionStrength  = MaterialData.Settings2.y;
    half alphaClipping       = MaterialData.Settings2.z;

    half  noiseAngleOffset             = MaterialData.Settings2.w;
    half  noiseScale                   = MaterialData.Settings3.x;
    half2 noiseScalingMinMax           = MaterialData.Settings5.xy;
    half2 noiseRandomiseRotationMinMax = MaterialData.Settings5.zw;

    int  variationMode          = (int)MaterialData.Settings3.y;
    half variationOpacity       = MaterialData.Settings3.z;
    half variationBrightness    = MaterialData.Settings3.w;
    half variationSmallScale    = MaterialData.Settings4.x;
    half variationMediumScale   = MaterialData.Settings4.y;
    half variationLargeScale    = MaterialData.Settings4.z;
    half variationNoiseStrength = MaterialData.Settings4.w;

    // Default values
    AlbedoColorOut   = 1;
    NormalVectorOut  = TangentNormalVector;
    MetallicOut      = 0;
    SmoothnessOut    = 0;
    OcclussionOut    = 1;
    EmissionColorOut = 0;

    // Setup UVs
    float2 tiling = MaterialData.TilingOffset.xy;
    float2 offset = MaterialData.TilingOffset.zw;
    
    float2 oriUV = UV;
    UV = UV * tiling + offset;
    
    // Change UVs & Get Edge Mask
    float voronoiCells = 1;
    float edgeMask = 0;
    float2 edgeUV = UV;
    float2 transformedUV = UV;
    if (noiseEnabled)
        GetRepetitionlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, noiseRandomiseRotationMinMax, voronoiCells, edgeMask, edgeUV, transformedUV);
    
    bool sampleEdges = edgeMask > 0;

    // Get Macro/Micro Variation Multiplier
    float variationColor = 0;
    if (variationEnabled && variationOpacity > 0) {
        switch (variationMode) {
            case 0: // Perlin Noise
                variationColor = MacroMicroVariationPerlinNoise(variationSmallScale, variationMediumScale, variationLargeScale, variationBrightness, variationNoiseStrength, oriUV, MaterialData.VariationTO.x, MaterialData.VariationTO.zw);
                break;
            case 1: // Simplex Noise
                variationColor = MacroMicroVariationSimplexNoise(variationSmallScale, variationMediumScale, variationLargeScale, variationBrightness, variationNoiseStrength, oriUV, MaterialData.VariationTO.x, MaterialData.VariationTO.zw);
                break;
            case 2: // Custom Texture
                variationColor = MacroMicroVariationTextureArray(variationSmallScale, variationMediumScale, variationLargeScale, variationBrightness, AVTextures, AssignedAVTextures, ArrayLayerIndex, 3, SS, oriUV, MaterialData.VariationTO.xy, MaterialData.VariationTO.zw);
                break;
        }
    }

    // Debugging
    if (DebuggingIndex != -1) {
        switch (DebuggingIndex) {
            case 0: // Voronoi Cells
                AlbedoColorOut = voronoiCells;
                break;
            case 1: // Edge Mask
                AlbedoColorOut = edgeMask;
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

    // Sample textures
    bool samplingAV  = albedoAssigned; // Variation is sampled later
    bool samplingNSO = normalAssigned || smoothnessAssigned || occlussionAssigned;
    bool samplingEM  = emissionAssigned || metallicAssigned;

    float4 avTexture = 0;
    float4 nsoTexture = 0;
    float4 emTexture = 0;
    if (samplingAV)  avTexture  = SampleRepetitionlessArrayTexture(AVTextures, AssignedAVTextures, ArrayLayerIndex, SS, edgeMask, edgeUV, transformedUV, sampleEdges);
    if (samplingNSO) nsoTexture = SampleRepetitionlessArrayTexture(NSOTextures, AssignedNSOTextures, ArrayLayerIndex, SS, edgeMask, edgeUV, transformedUV, sampleEdges);
    if (samplingEM)  emTexture  = SampleRepetitionlessArrayTexture(EMTextures, AssignedEMTextures, ArrayLayerIndex, SS, edgeMask, edgeUV, transformedUV, sampleEdges);

    // Albedo
    //if (samplingAV) avTexture.rgb = LinearToSRGB(avTexture).rgb;
    AlbedoColorOut = samplingAV ? float4(avTexture.rgb, 1) : 1;
    AlbedoColorOut *= float4(MaterialData.AlbedoTint, 1);

    // Doesnt do anything at the moment since alpha is forced to 1
    // Still here incase the alpha is readded
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    NormalVectorOut = normalAssigned ? UnpackNormalMap(float4(nsoTexture.rg, 1, 1), normalScale) : TangentNormalVector;
    
    if (packedTexture) {
        float4 packedTextureColor = packedTextureAssigned ? float4(emTexture.a, nsoTexture.a, 0, nsoTexture.b) : float4(0, 1, 0, 0);

        MetallicOut = packedTextureColor.r;
        OcclussionOut = lerp(1, packedTextureColor.g, occlussionStrength);
        if (smoothnessEnabled) SmoothnessOut = packedTextureColor.a;
        else                   SmoothnessOut = 1 - packedTextureColor.a;
    } else {
        // Metallic
        MetallicOut = metallicAssigned ? emTexture.a : metallic;

        // Smoothness
        SmoothnessOut = smoothnessAssigned ? nsoTexture.b : smoothnessRoughness;
        if (!smoothnessEnabled) SmoothnessOut = 1 - SmoothnessOut; // Roughness
            
        // Occlussion
        if (occlussionAssigned) {
            OcclussionOut = nsoTexture.a;
            OcclussionOut = lerp(1, OcclussionOut, occlussionStrength);
        } else {
            OcclussionOut = 1;
        }
    }

    // Emission
    EmissionColorOut = 0;
    if(emissionEnabled) {
        EmissionColorOut = emissionAssigned ? emTexture.rgb : 1;
        EmissionColorOut *= MaterialData.EmissionColor;
    }
}

#endif