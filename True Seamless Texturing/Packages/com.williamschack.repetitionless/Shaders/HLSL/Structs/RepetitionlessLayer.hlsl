#ifndef REPETITIONLESSLAYER_INCLUDED
#define REPETITIONLESSLAYER_INCLUDED

#include "RepetitionlessLayerData.hlsl"
#include "RepetitionlessMaterial.hlsl"

struct RepetitionlessLayer
{
    RepetitionlessMaterial BaseMaterial;
    RepetitionlessMaterial FarMaterial;
    RepetitionlessMaterial BlendMaterial;

    RepetitionlessLayerData Data;
};

#endif