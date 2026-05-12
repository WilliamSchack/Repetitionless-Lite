#ifndef REPETITIONLESSPASSES_INCLUDED
#define REPETITIONLESSPASSES_INCLUDED

// Structs
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uvMainAndLM : TEXCOORD0; // xy: control, zw: lightmap
    half4 normal       : TEXCOORD1;
    half4 tangent      : TEXCOORD2;
    half4 bitangent    : TEXCOORD3;
    
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight : TEXCOORD4; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor : TEXCOORD4;
#endif
    float3 positionWS : TEXCOORD5;
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord : TEXCOORD5;
#endif
#if defined(DYNAMICLIGHTMAP_ON)
    float2 dynamicLightmapUV : TEXCOORD6;
#endif
#ifdef USE_APV_PROBE_OCCLUSION
    float4 probeOcclusion : TEXCOORD7;
#endif

    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
}

#endif