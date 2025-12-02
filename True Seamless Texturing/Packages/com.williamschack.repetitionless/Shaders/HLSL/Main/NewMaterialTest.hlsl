#ifndef NEWMATERIALTEST_INCLUDED
#define NEWMATERIALTEST_INCLUDED

#include "../Structs/RepetitionlessMaterialDataNew.hlsl"

#include "../RepetitionlessHelpers/RepetitionlessNoise.hlsl"
#include "../RepetitionlessHelpers/RepetitionlessTextureUtilities.hlsl"
#include "../RepetitionlessHelpers/MacroMicroVariation.hlsl"

#include "../Noise/Keijiro/SimplexNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"

#include "../Utilities/BooleanCompression.hlsl"

void GetRepetitionlessMaterialColorTest(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    int SurfaceType, int DebuggingIndex,

    // Textures
    int ArrayLayerIndex,
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    int AssignedAVTextures,
    int AssignedNSOTextures,
    int AssignedEMTextures,

    // Material Data
    RepetitionlessMaterialDataNew MaterialData,

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
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
    
    bool noiseEnabled          = GetCompressedValue(settingToggles, 0);//(settingToggles & 1) != 0;
    bool randomiseNoiseScaling = GetCompressedValue(settingToggles, 1);//(settingToggles & 2) != 0;
    bool randomiseRotation     = GetCompressedValue(settingToggles, 2);//(settingToggles & 4) != 0;
    bool smoothnessEnabled     = GetCompressedValue(settingToggles, 3);//(settingToggles & 8) != 0;
    bool variationEnabled      = GetCompressedValue(settingToggles, 4);//(settingToggles & 16) != 0;
    bool packedTexture         = GetCompressedValue(settingToggles, 5);//(settingToggles & 32) != 0;
    bool emissionEnabled       = GetCompressedValue(settingToggles, 6);//(settingToggles & 64) != 0;
    
    // Get Assigned Textures
    int assignedTextures = (int)MaterialData.Settings.y;
    
    bool metallicAssigned   = GetCompressedValue(assignedTextures, 0);//(assignedTextures & 1) != 0;
    bool smoothnessAssigned = GetCompressedValue(assignedTextures, 1);//(assignedTextures & 2) != 0;
    bool roughnessAssigned  = GetCompressedValue(assignedTextures, 2);//(assignedTextures & 4) != 0;
    bool normalAssigned     = GetCompressedValue(assignedTextures, 3);//(assignedTextures & 8) != 0;
    bool occlussionAssigned = GetCompressedValue(assignedTextures, 4);//(assignedTextures & 16) != 0;
    bool emissionAssigned   = GetCompressedValue(assignedTextures, 5);//(assignedTextures & 32) != 0;
    bool albedoAssigned     = GetCompressedValue(assignedTextures, 6);//(assignedTextures & 64) != 0;
    bool variationAssigned  = GetCompressedValue(assignedTextures, 7);//(assignedTextures & 128) != 0;

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
        GetRepetitionlessNoiseUVs(UV, noiseAngleOffset, noiseScale, randomiseNoiseScaling, noiseScalingMinMax, randomiseRotation, randomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
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
                variationColor = MacroMicroVariationTextureArray(MaterialData.VariationSettings.y, MaterialData.VariationSettings.z, MaterialData.VariationSettings.w, MaterialData.VariationBrightness, AVTextures, AssignedAVTextures, ArrayLayerIndex, 3, SS, oriUV, MaterialData.VariationTextureTO.xy, MaterialData.VariationTextureTO.zw);
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

    // Sample textures
    bool samplingAV  = albedoAssigned; // Variation is sampled later
    bool samplingNSO = normalAssigned || smoothnessAssigned || occlussionAssigned;
    bool samplingEM  = emissionAssigned || metallicAssigned;

    float4 avTexture = 0;
    float4 nsoTexture = 0;
    float4 emTexture = 0;
    if (samplingAV)  avTexture  = SampleRepetitionlessArrayTexture(AVTextures, AssignedAVTextures, ArrayLayerIndex, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
    if (samplingNSO) nsoTexture = SampleRepetitionlessArrayTexture(NSOTextures, AssignedNSOTextures, ArrayLayerIndex, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
    if (samplingEM)  emTexture  = SampleRepetitionlessArrayTexture(EMTextures, AssignedEMTextures, ArrayLayerIndex, SS, EdgeMask, EdgeUV, TransformedUV, sampleEdges);
    
    // Albedo
    AlbedoColorOut = float4(avTexture.rgb, 1);
    AlbedoColorOut *= MaterialData.AlbedoTint;

    // Doesnt do anything at the moment since alpha is forced to 1
    // Still here incase the alpha is readded
    if (SurfaceType == 1)
        clip(AlbedoColorOut.a - alphaClipping);
    
    // Macro/Micro Variation
    if (variationEnabled && variationOpacity > 0)
        AlbedoColorOut = lerp(AlbedoColorOut, variationColor * AlbedoColorOut, variationOpacity);
    
    // Normal Map
    if (normalAssigned) {
        NormalVectorOut = nsoTexture.rgb;

        // Hacky fix to check if normal is assigned, values set in TextureUtilities::UnpackNormalMap. If not assigned, used default tangent normal vector
        // Implemented to check for terrain data normal maps as there is no variable to tell if its assigned or not
        if (NormalVectorOut.x == 0.5 && NormalVectorOut.y == 0.5 && NormalVectorOut.z == 1) {
            NormalVectorOut = TangentNormalVector;
        }
    } else {
        NormalVectorOut = TangentNormalVector;
    }
    
    if (packedTexture) {
        float4 packedTextureColor = float4(emTexture.a, nsoTexture.a, 0, nsoTexture.b);

        MetallicOut = packedTextureColor.r;
        OcclussionOut = lerp(1, packedTextureColor.g, occlussionStrength);
        if (smoothnessEnabled) SmoothnessOut = packedTextureColor.a;
        else                   SmoothnessOut = 1 - packedTextureColor.a;
    } else {
        // Metallic
        MetallicOut = metallicAssigned ? emTexture.a : metallic;

        // Smoothness
        SmoothnessOut = smoothnessAssigned ? nsoTexture.b : smoothness;
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