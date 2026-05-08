// THIS WILL BE REWRITTEN
// https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl

#ifndef REPETITIONLESS_TERRAIN_PASSES_INCLUDED
#define REPETITIONLESS_TERRAIN_PASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "TerrainInstancing.hlsl"

// Your properties
CBUFFER_START(UnityPerMaterial)
    float _SurfaceTypeSetting;
    float _UVSpace;
    float _VertexColourBlendMode;
    half  _DebuggingIndex;
    float _LayersCount;
    float4 _NoiseTexture_TexelSize;
    float4 _Control0_TexelSize;
    float4 _Control1_TexelSize;
    float4 _Control2_TexelSize;
    float4 _Control3_TexelSize;
    float4 _Control4_TexelSize;
    float4 _Control5_TexelSize;
    float4 _Control6_TexelSize;
    float4 _Control7_TexelSize;
    float4 _PropertiesTexture_TexelSize;
    float4 _AssignedTexturesTexture_TexelSize;
    float4 _TerrainHoles_TexelSize;
CBUFFER_END

TEXTURE2D(_TerrainHoles); SAMPLER(sampler_TerrainHoles);
TEXTURE2D(_Control0); SAMPLER(sampler_Control0);
TEXTURE2D(_Control1); SAMPLER(sampler_Control1);
TEXTURE2D(_Control2); SAMPLER(sampler_Control2);
TEXTURE2D(_Control3); SAMPLER(sampler_Control3);
TEXTURE2D(_Control4); SAMPLER(sampler_Control4);
TEXTURE2D(_Control5); SAMPLER(sampler_Control5);
TEXTURE2D(_Control6); SAMPLER(sampler_Control6);
TEXTURE2D(_Control7); SAMPLER(sampler_Control7);
TEXTURE2D(_NoiseTexture);           SAMPLER(sampler_NoiseTexture);
TEXTURE2D(_PropertiesTexture);      SAMPLER(sampler_PropertiesTexture);
TEXTURE2D(_AssignedTexturesTexture); SAMPLER(sampler_AssignedTexturesTexture);
TEXTURE2D_ARRAY(_AVTextures);       SAMPLER(sampler_AVTextures);
TEXTURE2D_ARRAY(_NSOTextures);      SAMPLER(sampler_NSOTextures);
TEXTURE2D_ARRAY(_EMTextures);       SAMPLER(sampler_EMTextures);
TEXTURE2D_ARRAY(_BMTextures);       SAMPLER(sampler_BMTextures);

#include "../../HLSL/Main/SampleRepetitionlessTerrain.hlsl"

// Reuse URP terrain structs
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uvMainAndLM  : TEXCOORD0;
    half4  normal       : TEXCOORD1;
    half4  tangent      : TEXCOORD2;
    half4  bitangent    : TEXCOORD3;
    float3 positionWS   : TEXCOORD4;
    half3  vertexSH     : TEXCOORD5;
    half   fogFactor    : TEXCOORD6;
    float4 color        : TEXCOORD7;
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord : TEXCOORD8;
    #endif
    #if defined(DYNAMICLIGHTMAP_ON)
        float2 dynamicLightmapUV : TEXCOORD9;
    #endif
    #ifdef USE_APV_PROBE_OCCLUSION
        float4 probeOcclusion : TEXCOORD10;
    #endif
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings RepetitionlessTerrainVert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // This is the key call that makes Draw Instanced work
    TerrainInstancing(v.positionOS, v.normalOS, v.texcoord);

    VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
    o.positionWS    = posInputs.positionWS;
    o.clipPos       = posInputs.positionCS;
    o.uvMainAndLM.xy = v.texcoord;
    o.uvMainAndLM.zw = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
    #if defined(DYNAMICLIGHTMAP_ON)
        o.dynamicLightmapUV = v.texcoord * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    // Normal
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);
    o.normal = half4(normalInput.normalWS, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

    OUTPUT_SH4(posInputs.positionWS, o.normal.xyz, GetWorldSpaceNormalizeViewDir(posInputs.positionWS), o.vertexSH, o.probeOcclusion);

    #if !defined(_FOG_FRAGMENT)
        o.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        o.shadowCoord = TransformWorldToShadowCoord(posInputs.positionWS);
    #endif

    // Pass vertex color as white since terrain has none
    o.color = float4(1, 1, 1, 1);

    return o;
}

half4 RepetitionlessTerrainFrag(Varyings IN) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

    float2 uv = IN.uvMainAndLM.xy;
    
    float4 albedo;
    float3 normalTS;
    float  metallic;
    float  smoothness;
    float  occlusion;
    float3 emission;

    SampleRepetitionlessTerrain(
        sampler_TrilinearRepeat,
        uv,
        //normalize(IN.normal),
        IN.normal,
        IN.positionWS,
        _WorldSpaceCameraPos,
        (int)_SurfaceTypeSetting,
        (int)_UVSpace,
        (int)_VertexColourBlendMode,
        (int)_DebuggingIndex,
        IN.color,
        (int)_LayersCount,

        _PropertiesTexture,
        _AssignedTexturesTexture,

        _AVTextures,
        _NSOTextures,
        _EMTextures,
        _BMTextures,

        _NoiseTexture,

        albedo, normalTS, metallic, smoothness, occlusion, emission
    );

    // Build InputData for URP lighting
    InputData inputData = (InputData)0;
    inputData.positionWS        = IN.positionWS;
    inputData.positionCS        = IN.clipPos;

    inputData.viewDirectionWS = half3(IN.normal.w, IN.tangent.w, IN.bitangent.w); 
    //inputData.viewDirectionWS   = GetWorldSpaceNormalizeViewDir(IN.positionWS);

    // Convert normal to world space
    inputData.tangentToWorld = half3x3(-IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);

    inputData.shadowCoord = 
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            IN.shadowCoord;
        #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
            TransformWorldToShadowCoord(IN.positionWS);
        #else
            float4(0,0,0,0);
        #endif
    inputData.fogCoord          = InitializeInputDataFog(float4(IN.positionWS, 1.0), IN.fogFactor);
    inputData.bakedGI           = SAMPLE_GI(IN.uvMainAndLM.zw, IN.vertexSH, inputData.normalWS);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
    inputData.shadowMask        = SAMPLE_SHADOWMASK(IN.uvMainAndLM.zw);

    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo      = albedo.rgb;
    surfaceData.metallic    = metallic;
    surfaceData.smoothness  = smoothness;
    surfaceData.occlusion   = occlusion;
    surfaceData.emission    = emission;
    surfaceData.alpha       = 1;

    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    return color;
}

// Shadow Pass

float3 _LightDirection;
float3 _LightPosition;

struct AttributesLean
{
    float4 position     : POSITION;
    float3 normalOS       : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsLean
{
    float4 clipPos      : SV_POSITION;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

void ClipHoles(float2 uv)
{
    float hole = SAMPLE_TEXTURE2D(_TerrainHoles, sampler_TerrainHoles, uv).r;
    clip(hole < 0.0005f ? -1 : 1);
}

VaryingsLean ShadowPassVertex(AttributesLean v)
{
    VaryingsLean o = (VaryingsLean)0;
    UNITY_SETUP_INSTANCE_ID(v);
    TerrainInstancing(v.position, v.normalOS, v.texcoord);

    float3 positionWS = TransformObjectToWorld(v.position.xyz);
    float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

#if UNITY_REVERSED_Z
    clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
#else
    clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
#endif

    o.clipPos = clipPos;

    o.texcoord = v.texcoord;

    return o;
}

half4 ShadowPassFragment(VaryingsLean IN) : SV_TARGET
{
#ifdef _ALPHATEST_ON
    ClipHoles(IN.texcoord);
#endif
    return 0;
}

// Depth pass

VaryingsLean DepthOnlyVertex(AttributesLean v)
{
    VaryingsLean o = (VaryingsLean)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    TerrainInstancing(v.position, v.normalOS);
    o.clipPos = TransformObjectToHClip(v.position.xyz);
    o.texcoord = v.texcoord;
    return o;
}

half4 DepthOnlyFragment(VaryingsLean IN) : SV_TARGET
{
#ifdef _ALPHATEST_ON
    ClipHoles(IN.texcoord);
#endif
#ifdef SCENESELECTIONPASS
    // We use depth prepass for scene selection in the editor, this code allow to output the outline correctly
    return half4(_ObjectId, _PassValue, 1.0, 1.0);
#endif
    return IN.clipPos.z;
}

#endif