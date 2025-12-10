#ifndef NEWLAYERTEST_INCLUDED
#define NEWLAYERTEST_INCLUDED

void SampleMultipleRepetitionlessLayers_float(
    // General Settings
    SamplerState SS, float2 UV, float3 TangentNormalVector,
    float3 WorldPosition, float3 CameraPosition,
    int SurfaceType, int DebuggingIndex,

    // Properties
    int LayersCount,
    UnityTexture2D PropertiesTexture,

    // Textures
    UnityTexture2DArray AVTextures,
    UnityTexture2DArray NSOTextures,
    UnityTexture2DArray EMTextures,
    UnityTexture2DArray BMTextures,
    int AssignedAVTextures,
    int AssignedNSOTextures,
    int AssignedEMTextures,
    int AssignedBMTextures,

    // Outputs
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
) {
    float4 albedoColor   = 1;
    float3 normalVector  = TangentNormalVector;
    float  metallic      = 0;
    float  smoothness    = 0;
    float  occlussion    = 0;
    float3 emissionColor = 0;
    
    albedoColor = SAMPLE_TEXTURE2D(_TerrainHoles, sampler_TerrainHoles, UV);

    AlbedoColorOut = albedoColor;
    NormalVectorOut = normalVector;
    MetallicOut = metallic;
    SmoothnessOut = smoothness;
    OcclussionOut = occlussion;
    EmissionColorOut = emissionColor;
}

#endif