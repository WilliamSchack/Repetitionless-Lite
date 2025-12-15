#ifndef MULTIPLELAYERSTEST_INCLUDED
#define MULTIPLELAYERSTEST_INCLUDED

#include "NewLayerTest.hlsl"

// Assume control and terrain holes properties are:
// _TerrainHoles
// _Control{Index}

#define SAMPLE_CONTROL(i, uv) LayersCount > i ? SAMPLE_TEXTURE2D(_Control##i, sampler_Control##i, uv) : 0

void SampleMultipleRepetitionlessLayers_float(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int UVSpace, int DebuggingIndex,

    // Properties
    int LayersCount,
    UnityTexture2D PropertiesTexture,
    UnityTexture2D AssignedTexturesTexture,

    // Textures
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    UnityTexture2DArray BMTextures,

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
) {
    float4 albedoColor  = 1;
    float3 normalVector = TangentNormalVector;
    float  metallic     = 0;
    float  smoothness   = 0;
    float  occlussion   = 0;
    float3 emission     = 0;

    // Terrain Holes
    float holeColour = SAMPLE_TEXTURE2D(_TerrainHoles, sampler_TerrainHoles, UV).r;
    clip(albedoColor.a - (1 - (holeColour - 0.01)));

    if (holeColour == 0) {
        AlbedoColorOut   = albedoColor;
        NormalVectorOut  = normalVector;
        MetallicOut      = metallic;
        SmoothnessOut    = smoothness;
        OcclussionOut    = occlussion;
        EmissionColorOut = emission;
        return;
    }

    // Sample control
    half4 controlColours[8] = {
        SAMPLE_CONTROL(0, UV),
        SAMPLE_CONTROL(1, UV),
        SAMPLE_CONTROL(2, UV),
        SAMPLE_CONTROL(3, UV),
        SAMPLE_CONTROL(4, UV),
        SAMPLE_CONTROL(5, UV),
        SAMPLE_CONTROL(6, UV),
        SAMPLE_CONTROL(7, UV)
    };

    // Get individual weights and sum
    half controlWeights[32];
    half controlSum = 0;

    [unroll]
    for (int controlLayer = 0; controlLayer < 8; controlLayer++) {
        controlWeights[controlLayer * 4 + 0] = controlColours[controlLayer].x;
        controlWeights[controlLayer * 4 + 1] = controlColours[controlLayer].y;
        controlWeights[controlLayer * 4 + 2] = controlColours[controlLayer].z;
        controlWeights[controlLayer * 4 + 3] = controlColours[controlLayer].w;

        controlSum += dot(controlColours[controlLayer], 1.0);
    }

    half backgroundControl = saturate(1 - controlSum);

    // Normalize weights for additive blending
    if (controlSum > 1) {
        [unroll]
        for (int controlWeightIndex = 0; controlWeightIndex < 32; controlWeightIndex++) {
            controlWeights[controlWeightIndex] /= controlSum;
        }
    }

    // Read array assigned textures
    int assignedAVTextures[3];
    int assignedNSOTextures[3];
    int assignedEVTextures[3];
    int assignedBMTextures;

    GetArrayAssignedTextures(AssignedTexturesTexture, assignedAVTextures, assignedNSOTextures, assignedEVTextures, assignedBMTextures);

    // Variables
    albedoColor  = backgroundControl;
    normalVector = TangentNormalVector * backgroundControl;
    metallic     = 0;
    smoothness   = 0;
    occlussion   = backgroundControl;
    emission     = 0;

    // Sample Layers
    [loop]
    for (int i = 0; i < LayersCount; i++) {
        half layerControl = controlWeights[i];
        if (layerControl == 0)
            continue;

        float4 layerAlbedo    = albedoColor;
        float3 layerNormal    = normalVector;
        float layerMetallic   = metallic;
        float layerSmoothness = smoothness;
        float layerOcclussion = occlussion;
        float3 layerEmission  = emission;

        SampleRepetitionlessLayerBase_float(
            SS, UV, TangentNormalVector,
            WorldPosition, CameraPosition,
            SurfaceType, UVSpace, DebuggingIndex,
            i,
            PropertiesTexture,
            assignedAVTextures[0], assignedAVTextures[1], assignedAVTextures[2],
            assignedNSOTextures[0], assignedNSOTextures[1], assignedNSOTextures[2],
            assignedEVTextures[0], assignedEVTextures[1], assignedEVTextures[2],
            assignedBMTextures,
            AVTextures,
            NSOTextures,
            EMTextures,
            BMTextures,
            layerAlbedo, layerNormal, layerMetallic, layerSmoothness, layerOcclussion, layerEmission
        );

        albedoColor  += layerAlbedo     * layerControl;
        normalVector += layerNormal     * layerControl;
        metallic     += layerMetallic   * layerControl;
        smoothness   += layerSmoothness * layerControl;
        occlussion   += layerOcclussion * layerControl;
        emission     += layerEmission   * layerControl;
    }

    AlbedoColorOut   = albedoColor;
    NormalVectorOut  = normalVector;
    MetallicOut      = metallic;
    SmoothnessOut    = smoothness;
    OcclussionOut    = occlussion;
    EmissionColorOut = emission;
}

#endif