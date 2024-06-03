#ifndef SEAMLESSNOISE_INCLUDED
#define SEAMLESSNOISE_INCLUDED

#include "SeamlessTextureUtilities.hlsl"

#include "../Noise/VoronoiNoise2D.hlsl"

void GetSeamlessNoiseUVs(
    float2 UV, // UV
    float NoiseAngleOffset, float NoiseScale, bool RandomiseNoiseScaling, float2 NoiseScalingMinMax, // Noise
    bool RandomiseRotation, float2 RandomiseRotationMinMax, // Noise Rotation
    out float VoronoiCells, out float EdgeMask, out float2 EdgeUV, out float2 TransformedUV) // Outputs
{
    // Generate Noise
    float VoronoiDistFromCenter;
    float VoronoiDistFromEdge;
    VoronoiNoise(UV, NoiseAngleOffset, NoiseScale, VoronoiDistFromCenter, VoronoiDistFromEdge, VoronoiCells);
    
    // Scale Edge UVs
    EdgeUV = UV;
    if (RandomiseNoiseScaling) {
        float minMaxAverage = (NoiseScalingMinMax.x + NoiseScalingMinMax.y) / 2;
        EdgeUV *= minMaxAverage;
    }
    
    // Generate Edge Mask, replicating a Sample Gradient Node
    EdgeMask = lerp(0.23, -1.5, VoronoiDistFromEdge) * 5;
    EdgeMask = clamp(EdgeMask, 0, 1);
    
    // Randomise UV Scaling
    TransformedUV = UV;
    if (RandomiseNoiseScaling) {
        float newUVTiling = Remap(VoronoiCells, float2(0, 1), NoiseScalingMinMax);
        TransformedUV *= newUVTiling;
    }
    
    // Rotate UVs
    if (RandomiseRotation) {
        float randomCellDegrees = Remap(VoronoiCells, float2(0, 1), RandomiseRotationMinMax);
        TransformedUV = RotateUVDegrees(TransformedUV, 0.0, randomCellDegrees);
    }
}

void AddNoise_float(
    UnityTexture2D InputTexture, UnitySamplerState SS, // Texture
    float2 UV, // UV
    float NoiseAngleOffset, float NoiseScale, bool RandomiseNoiseScaling, float2 NoiseScalingMinMax, // Noise
    bool RandomiseRotation, float2 RandomiseRotationMinMax, // Noise Rotation
    out float4 OutputColor) // Outputs
{
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    GetSeamlessNoiseUVs(UV, NoiseAngleOffset, NoiseScale, RandomiseNoiseScaling, NoiseScalingMinMax, RandomiseRotation, RandomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    OutputColor = SampleSeamlessTexture(InputTexture, SS, EdgeMask, EdgeUV, TransformedUV, true);
}

#endif