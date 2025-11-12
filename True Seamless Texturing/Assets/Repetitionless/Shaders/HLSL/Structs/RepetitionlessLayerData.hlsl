#ifndef REPETITIONLESSLAYERDATA_INCLUDED
#define REPETITIONLESSLAYERDATA_INCLUDED

struct RepetitionlessLayerData
{
    // Far Material Settings
    bool DistanceBlendEnabled; int DistanceBlendingMode; float2 DistanceBlendMinMax;

    // Blend Material Settings
    float MaterialBlendSettings; int BlendMaskType; float4 BlendMaskDistanceTO;
    float2 MaterialBlendProperties; float3 MaterialBlendNoiseSettings;
    UnityTexture2D BlendMaskTexture; float4 BlendMaskTextureTO;
};

#endif