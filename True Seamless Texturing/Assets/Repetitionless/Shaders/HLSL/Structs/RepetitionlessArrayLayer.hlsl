#ifndef REPETITIONLESSTERRAINLAYER_INCLUDED
#define REPETITIONLESSTERRAINLAYER_INCLUDED

#include "RepetitionlessArrayMaterial.hlsl"
#include "RepetitionlessMaterial.hlsl"

struct RepetitionlessTerrainLayer
{
    // Base Material
    RepetitionlessMaterial BaseMaterial;

    // Far Material
    bool DistanceBlendEnabled; int DistanceBlendingMode; float2 DistanceBlendMinMax;
    RepetitionlessArrayMaterial FarMaterial;

    // Blend Material
    float MaterialBlendSettings; int BlendMaskType; float4 BlendMaskDistanceTO;
    float2 MaterialBlendProperties; float3 MaterialBlendNoiseSettings;
    UnityTexture2D BlendMaskTexture; float4 BlendMaskTextureTO;
    RepetitionlessArrayMaterial BlendMaterial;
};

#endif