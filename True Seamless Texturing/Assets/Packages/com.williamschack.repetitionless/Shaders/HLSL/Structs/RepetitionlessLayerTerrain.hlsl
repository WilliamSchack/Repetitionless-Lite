#ifndef REPETITIONLESSTERRAINLAYER_INCLUDED
#define REPETITIONLESSTERRAINLAYER_INCLUDED

#include "RepetitionlessLayerData.hlsl"
#include "RepetitionlessMaterialArray.hlsl"
#include "RepetitionlessMaterial.hlsl"

struct RepetitionlessLayerTerrain
{
    RepetitionlessMaterial BaseMaterial;
    RepetitionlessMaterialArray FarMaterial;
    RepetitionlessMaterialArray BlendMaterial;

    RepetitionlessLayerData Data;
};

#endif