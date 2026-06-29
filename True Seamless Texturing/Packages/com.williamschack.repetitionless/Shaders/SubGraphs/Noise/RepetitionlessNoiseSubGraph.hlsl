#ifndef REPETITIONLESSNOISESUBGRAPH_INCLUDED
#define REPETITIONLESSNOISESUBGRAPH_INCLUDED

#include "../../Common/RepetitionlessHelpers/RepetitionlessNoise.hlsl"

// Samples the given texture using modified UVs based on voronoi noise
// Samples the voronoi cells base and edge colour if required and lerps them together
void AddRepetitionlessNoise_float(
    UnityTexture2D InputTexture,
    SamplerState SS,
    float2 UV,

    float NoiseAngleOffset,
    float NoiseScale,
    bool RandomiseNoiseScaling,
    float2 NoiseScalingMinMax,
    
    bool RandomiseRotation, 
    float2 RandomiseRotationMinMax,

    out float4 OutputColor
){
    float VoronoiCells = 1;
    float EdgeMask = 0;
    float2 EdgeUV = UV;
    float2 TransformedUV = UV;
    GetRepetitionlessNoiseUVs(UV, NoiseAngleOffset, NoiseScale, RandomiseNoiseScaling, NoiseScalingMinMax, RandomiseRotation, RandomiseRotationMinMax, VoronoiCells, EdgeMask, EdgeUV, TransformedUV);
    
    OutputColor = SampleRepetitionlessTexture(InputTexture.tex, SS, EdgeMask, EdgeUV, TransformedUV, true);
}

#endif