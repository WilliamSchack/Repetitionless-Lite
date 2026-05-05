#ifndef TERRAIN_INSTANCE_BUFFER_DEFINED
#define TERRAIN_INSTANCE_BUFFER_DEFINED

#ifdef UNITY_INSTANCING_ENABLED
    TEXTURE2D(_TerrainHeightmapTexture);
    TEXTURE2D(_TerrainNormalmapTexture);
    float4 _TerrainHeightmapScale;
    float4 _TerrainHeightmapRecipSize;

    UNITY_INSTANCING_BUFFER_START(Terrain)
        UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData)
    UNITY_INSTANCING_BUFFER_END(Terrain)
#endif

#endif