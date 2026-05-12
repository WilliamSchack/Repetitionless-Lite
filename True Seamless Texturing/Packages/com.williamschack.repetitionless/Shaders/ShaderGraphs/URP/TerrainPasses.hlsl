#ifndef REPETITIONLESSTERRAINPASSES_INCLUDED
#define REPETITIONLESSTERRAINPASSES_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
#include "../../HLSL/Main/SampleRepetitionlessTerrain.hlsl"

// Structs
struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    half4 colour      : COLOR;
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
    float4 shadowCoord : TEXCOORD6;
#endif
#if defined(DYNAMICLIGHTMAP_ON)
    float2 dynamicLightmapUV : TEXCOORD7;
#endif
#ifdef USE_APV_PROBE_OCCLUSION
    float4 probeOcclusion : TEXCOORD8;
#endif

    half4 colour  : COLOR;
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_OUTPUT_STEREO
};

struct VaryingsLean
{
    float4 clipPos  : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

// Helpers
void InitializeInputData(Varyings IN, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = IN.positionWS;
    inputData.positionCS = IN.clipPos;

    // Convert normals from tangent to world space
    inputData.tangentToWorld = half3x3(-IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
    inputData.normalWS = normalize(inputData.normalWS);
        
    inputData.viewDirectionWS = half3(IN.normal.w, IN.tangent.w, IN.bitangent.w);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = IN.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
    
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(IN.positionWS, 1.0), IN.fogFactorAndVertexLight.x);
    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
#else
    inputData.fogCoord = InitializeInputDataFog(float4(IN.positionWS, 1.0), IN.fogFactor);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);

#if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
        inputData.dynamicLightmapUV = IN.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
        inputData.staticLightmapUV = IN.uvMainAndLM.zw;
    #else
        inputData.vertexSH = 0;
    #endif
    #if defined(USE_APV_PROBE_OCCLUSION)
        inputData.probeOcclusion = IN.probeOcclusion;
    #endif
#endif
}

void InitializeBakedGIData(Varyings IN, inout InputData inputData)
{
    half3 SH = 0;

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(IN.uvMainAndLM.zw, IN.dynamicLightmapUV, SH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(IN.uvMainAndLM.zw);
#elif !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    inputData.bakedGI = SAMPLE_GI(SH,
        GetAbsolutePositionWS(inputData.positionWS),
        inputData.normalWS,
        inputData.viewDirectionWS,
        inputData.positionCS.xy,
        IN.probeOcclusion,
        inputData.shadowMask);
#else
    inputData.bakedGI = SAMPLE_GI(IN.uvMainAndLM.zw, SH, inputData.normalWS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(IN.uvMainAndLM.zw);
#endif
}

// Lit
Varyings Vert(Attributes v)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    TerrainInstancing(v.positionOS, v.normalOS, v.texcoord);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);

    o.uvMainAndLM.xy = v.texcoord;
    o.uvMainAndLM.zw = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;

#if defined(DYNAMICLIGHTMAP_ON)
    o.dynamicLightmapUV = v.texcoord * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);

    o.normal = half4(normalInput.normalWS, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);

    half fogFactor = 0;
#if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    o.fogFactorAndVertexLight.x = fogFactor;
    o.fogFactorAndVertexLight.yzw = VertexLighting(vertexInput.positionWS, o.normal.xyz);
#else
    o.fogFactor = fogFactor;
#endif

    o.positionWS = vertexInput.positionWS;
    o.clipPos = vertexInput.positionCS;
    o.colour = v.colour;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    o.shadowCoord = GetShadowCoord(vertexInput);
#endif

    return o;
}

#ifdef TERRAIN_GBUFFER
GBufferFragOutput Frag(Varyings IN)
#else
half4 Frag(Varyings IN) : SV_TARGET
#endif
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

    float2 uv = IN.uvMainAndLM.xy;

    // Main function
    float4 albedo;
    float3 normalTS;
    float  metallic;
    float  smoothness;
    float  occlusion;
    float3 emission;
    SampleRepetitionlessTerrain(
        sampler_TrilinearRepeat,
        uv,
        IN.normal,
        IN.positionWS,
        _WorldSpaceCameraPos,
        (int)_SurfaceTypeSetting,
        (int)_UVSpace,
        (int)_VertexColourBlendMode,
        (int)_DebuggingIndex,
        IN.colour,

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

    InputData inputData;
    InitializeInputData(IN, normalTS, inputData);

#if defined(_DBUFFER)
    half3 specular = half3(0.0h, 0.0h, 0.0h);
    ApplyDecal(IN.clipPos,
        albedo,
        specular,
        inputData.normalWS,
        metallic,
        occlusion,
        smoothness);
#endif

    InitializeBakedGIData(IN, inputData);

#ifdef TERRAIN_GBUFFER
    BRDFData brdfData;
    InitializeBRDFData(albedo, metallic, half3(0.0h, 0.0h, 0.0h), smoothness, albedo.a, brdfData);

    half4 colour;
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, inputData.shadowMask);
    colour.rgb = GlobalIllumination(brdfData, (BRDFData)0, 0, inputData.bakedGI, occlusion, inputData.positionWS,
                                   inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV);
    colour.a = albedo.a;
    colour.rgb *= colour.a;
    brdfData.albedo.rgb *= albedo.a;
    brdfData.diffuse.rgb *= albedo.a;
    brdfData.specular.rgb *= albedo.a;
    brdfData.reflectivity *= albedo.a;
    inputData.normalWS = inputData.normalWS * albedo.a;
    smoothness *= albedo.a;

    return PackGBuffersBRDFData(brdfData, inputData, smoothness, colour.rgb, occlusion);
#else
    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo      = albedo.rgb;
    surfaceData.metallic    = metallic;
    surfaceData.smoothness  = smoothness;
    surfaceData.occlusion   = occlusion;
    surfaceData.emission    = emission;
    surfaceData.alpha       = 1;

    half4 colour = UniversalFragmentPBR(inputData, surfaceData);
    colour.rgb = MixFog(colour.rgb, inputData.fogCoord);
#endif

    return colour;
}

// Shadow
float3 _LightDirection;
float3 _LightPosition;

VaryingsLean ShadowPassVert(Attributes v)
{
    VaryingsLean o = (VaryingsLean)0;
    UNITY_SETUP_INSTANCE_ID(v);
    TerrainInstancing(v.positionOS, v.normalOS, v.texcoord);

    float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
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

half4 ShadowPassFrag(VaryingsLean IN) : SV_TARGET
{
#ifdef _ALPHATEST_ON
    ClipHoles(IN.texcoord);
#endif
    return 0;
}

// DepthOnly
VaryingsLean DepthOnlyVert(Attributes v)
{
    VaryingsLean o = (VaryingsLean)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    TerrainInstancing(v.positionOS, v.normalOS, v.texcoord);

    o.clipPos = TransformObjectToHClip(v.positionOS.xyz);
    o.texcoord = v.texcoord;
    return o;
}

half4 DepthOnlyFrag(VaryingsLean IN) : SV_TARGET
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