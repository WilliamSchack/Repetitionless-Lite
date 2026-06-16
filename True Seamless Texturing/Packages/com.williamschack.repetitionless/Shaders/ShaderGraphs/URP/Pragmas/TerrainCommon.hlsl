#ifndef REPETITIONLESSPRAGMASTERRAINCOMMON_INCLUDED
#define REPETITIONLESSPRAGMASTERRAINCOMMON_INCLUDED

#include_with_pragmas "Common.hlsl"

#pragma multi_compile_instancing
#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

#if USE_DYNAMIC_BRANCH_FOG_KEYWORD && SHADER_API_VULKAN && SHADER_API_MOBILE
    #define SKIP_SHADOWS_LIGHT_INDEX_CHECK 1
#endif

#endif