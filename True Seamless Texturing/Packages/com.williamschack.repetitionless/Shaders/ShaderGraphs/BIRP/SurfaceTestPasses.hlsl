#ifndef REPETITIONLESSSURFACETESTPASSES_INCLUDED
#define REPETITIONLESSSURFACETESTPASSES_INCLUDED

#include "../../HLSL/Main/SampleRepetitionlessDynamic.hlsl"

struct Input
{
    float2 uv_PropertiesTexture; // uv
    float3 worldPos;
    float3 worldNormal;
    float4 colour : COLOR;

};

void surf (Input input, inout SurfaceOutputStandard o)
{
    float2 uv = input.uv_PropertiesTexture;

    float4 albedo;
    float3 normalTS;
    float  metallic;
    float  smoothness;
    float  occlusion;
    float3 emission;
    SampleRepetitionless(
        uv, input.worldNormal, input.worldPos, input.colour,
        albedo, normalTS, metallic, smoothness, occlusion, emission
    );

    o.Albedo = albedo.rgb;
    o.Normal = normalTS;
    o.Metallic = metallic;
    o.Smoothness = smoothness;
    o.Occlussion = occlussion;
    o.Emission = emission;
    o.Alpha = albedo.a;
}

#endif