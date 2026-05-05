#ifndef TERRAIN_INSTANCING_PRAGMAS
#define TERRAIN_INSTANCING_PRAGMAS

#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forcemaxcount:3

#ifndef SHADERGRAPH_PREVIEW
    #include "TerrainInstanceBuffer.hlsl"
#endif

void TerrainInstancingSetup_float(float3 In, out float3 Out)
{
    Out = In;
}

#endif