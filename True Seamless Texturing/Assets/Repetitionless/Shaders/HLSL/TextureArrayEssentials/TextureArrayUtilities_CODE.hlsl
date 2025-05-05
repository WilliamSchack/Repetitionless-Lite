// ---------------------------------------------------
// | Script created by me taken from my other asset: |
// | Texture Array Essentials (https://u3d.as/3s4d)  |
// ---------------------------------------------------

// CHANGE THE NAME OF THIS FILE WHEN COMPLETE

#ifndef TEXTUREARRAYUTILITIES_CODE_INCLUDED
#define TEXTUREARRAYUTILITIES_CODE_INCLUDED

#include "../Utilities/BooleanCompression.hlsl"

int GetIndexInArray(int TexturesAssignedCompressed, int Index)
{
    // Get the index of the texture in the array
    float arrayIndex = 0;
    for (int i = 0; i < Index; i++)
    {
        bool assigned = GetCompressedValue(TexturesAssignedCompressed, i);
        
        if (assigned)
        {
            arrayIndex++;
        }
    }

    return arrayIndex;
}

//void SampleArrayAtConstantIndex(2DArray TextureArray, int TexturesAssignedCompressed, int Index, float2 UV, float4 UnassignedColor, SamplerState SS, out float4 Out)
//{
//    // Only output texture if it is assigned in inspector
//    bool indexExists = GetCompressedValue(TexturesAssignedCompressed, Index);
//    if (!indexExists) {
//        Out = UnassignedColor;
//        return;
//    }
//    
//    // Get the index of the texture in the array
//    float arrayIndex = GetIndexInArray(TexturesAssignedCompressed, Index);
//    
//    // Sample the array at the index found previously
//    Out = UNITY_SAMPLE_TEX2DARRAY(TextureArray, float3(UV.x, UV.y, arrayIndex));
//}

#endif