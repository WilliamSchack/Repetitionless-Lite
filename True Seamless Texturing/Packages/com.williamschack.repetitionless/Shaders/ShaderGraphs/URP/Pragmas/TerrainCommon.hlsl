#ifndef REPETITIONLESSPRAGMASTERRAINCOMMON_INCLUDED
#define REPETITIONLESSPRAGMASTERRAINCOMMON_INCLUDED

// URP Keywords 
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
#pragma multi_compile _ _CLUSTER_LIGHT_LOOP // Might not be supported pre 6.1: _FORWARD_PLUS
#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
#pragma multi_compile_fragment _ _SCREEN_SPACE_REFLECTION
#pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

// Unity defined
#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
#pragma multi_compile _ SHADOWS_SHADOWMASK
#pragma multi_compile _ DIRLIGHTMAP_COMBINED
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile _ DYNAMICLIGHTMAP_ON
#pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
#pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

// GPU Instancing
#pragma multi_compile_instancing
#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

#if USE_DYNAMIC_BRANCH_FOG_KEYWORD && SHADER_API_VULKAN && SHADER_API_MOBILE
    #define SKIP_SHADOWS_LIGHT_INDEX_CHECK 1
#endif

#endif