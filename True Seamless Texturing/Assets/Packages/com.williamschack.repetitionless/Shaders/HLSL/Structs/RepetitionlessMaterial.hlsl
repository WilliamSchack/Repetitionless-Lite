#ifndef REPETITIONLESSMATERIAL_INCLUDED
#define REPETITIONLESSMATERIAL_INCLUDED

#include "RepetitionlessMaterialData.hlsl"

struct RepetitionlessMaterial
{
    UnityTexture2D Albedo;
    UnityTexture2D MetallicMap;
    UnityTexture2D SmoothnessMap;
    UnityTexture2D RoughnessMap;
    UnityTexture2D NormalMap;
    UnityTexture2D OcclussionMap;
    UnityTexture2D EmissionMap;

    RepetitionlessMaterialData Data;
};

#endif