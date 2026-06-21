#ifndef REPETITIONLESSTERRAINDEPTHNORMALSPASS_INCLUDED
#define REPETITIONLESSTERRAINDEPTHNORMALSPASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "../Common/Main/SampleRepetitionlessDynamic.hlsl"

// DepthNormal pass
struct AttributesDepthNormal
{
    float4 positionOS : POSITION;
    half3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsDepthNormal
{
    float4 uvMainAndLM              : TEXCOORD0; // xy: control, zw: lightmap
    half4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z

    float4 clipPos                  : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsDepthNormal DepthNormalOnlyVertex(AttributesDepthNormal v)
{
    VaryingsDepthNormal o = (VaryingsDepthNormal)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    TerrainInstancing(v.positionOS, v.normalOS, v.texcoord);

    const VertexPositionInputs attributes = GetVertexPositionInputs(v.positionOS.xyz);

    o.uvMainAndLM.xy = v.texcoord;
    o.uvMainAndLM.zw = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(attributes.positionWS);
    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);

    o.normal = half4(normalInput.normalWS, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

    o.clipPos = attributes.positionCS;
    return o;
}

half4 DepthNormalOnlyFragment(VaryingsDepthNormal IN) : SV_Target0
{
    #ifdef _ALPHATEST_ON
        ClipHoles(IN.uvMainAndLM.xy);
    #endif

    // Sample material for the normals
    float4 albedo;
    float3 normalTS;
    float  metallic;
    float  smoothness;
    float  occlusion;
    float3 emission;
    SampleRepetitionless(
        input.uv, input.normalWS, input.positionWS, 0,
        albedo, normalTS, metallic, smoothness, occlusion, emission
    );

    float3 normalWS = TransformTangentToWorld(normalTS, half3x3(-IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz));
    half4 outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);

    return outNormalWS;
}

#endif