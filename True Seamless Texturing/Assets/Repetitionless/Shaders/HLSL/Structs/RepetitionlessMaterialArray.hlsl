#ifndef REPETITIONLESSARRAYMATERIAL_INCLUDED
#define REPETITIONLESSARRAYMATERIAL_INCLUDED

#include "RepetitionlessMaterialData.hlsl"

struct RepetitionlessMaterialArray
{
    UnityTexture2DArray Textures;
    UnityTexture2D NormalMap;
    int ArrayAssignedTextures;

    RepetitionlessMaterialData Data;
};

#endif