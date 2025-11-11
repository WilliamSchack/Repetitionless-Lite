#ifndef REPETITIONLESSLAYER_INCLUDED
#define REPETITIONLESSLAYER_INCLUDED

#include "RepetitionlessMaterial.hlsl"

struct RepetitionlessLayer
{
    // Base Material
    RepetitionlessMaterial BaseMaterial;

    // Far Material
    bool DistanceBlendEnabled; int DistanceBlendingMode; float2 DistanceBlendMinMax;
    RepetitionlessMaterial FarMaterial;

    // Blend Material
    float MaterialBlendSettings; int BlendMaskType; float4 BlendMaskDistanceTO;
    float2 MaterialBlendProperties; float3 MaterialBlendNoiseSettings;
    UnityTexture2D BlendMaskTexture; float4 BlendMaskTextureTO;
    RepetitionlessMaterial BlendMaterial;
};

#endif