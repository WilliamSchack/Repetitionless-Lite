// This file is auto-included by Shader Graph when named correctly
// Place it in the same folder as your Shader Graph asset

#ifndef TERRAIN_INSTANCING_SETUP
#define TERRAIN_INSTANCING_SETUP

#ifdef UNITY_INSTANCING_ENABLED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/TerrainLitInput.hlsl"

// Shader Graph calls this function automatically before the vertex stage
// if it exists in scope
void TerrainInstancingSetup()
{
#ifdef PROCEDURAL_INSTANCING_ON
    // Reconstructs vertex position/normal/uv from the terrain heightmap
    // This is what makes Draw Instanced work
    TerrainInstancing(unity_ObjectToWorld, unity_WorldToObject, float4(1,1,1,1));
#endif
}

#endif
#endif