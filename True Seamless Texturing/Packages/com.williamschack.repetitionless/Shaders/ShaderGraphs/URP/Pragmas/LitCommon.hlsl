#ifndef REPETITIONLESSPRAGMASLITCOMMON_INCLUDED
#define REPETITIONLESSPRAGMASLITCOMMON_INCLUDED

#include_with_pragmas "Common.hlsl"

#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION

#pragma multi_compile _ USE_LEGACY_LIGHTMAPS
#pragma multi_compile _ LOD_FADE_CROSSFADE

#pragma multi_compile_instancing
#pragma instancing_options renderinglayer

#endif