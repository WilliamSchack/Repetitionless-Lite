#ifndef REPETITIONLESSTEXTUREUTILITIES_INCLUDED
#define REPETITIONLESSTEXTUREUTILITIES_INCLUDED

#include "../Utilities/TextureUtilities.hlsl"
#include "../TextureArrayEssentials/TextureArrayUtilities.hlsl"

// Samples the base and edge colour if required and lerps them together
// Uses a regular texture
float4 SampleRepetitionlessTexture(
    UnityTexture2D Texture,
    SamplerState SS,
    
    float EdgeMask,
    float2 EdgeUV,
    float2 TransformedUV,
    bool SampleEdge,

    bool NormalMap = false,
    float NormalStrength = 1.0
){
    float4 baseTextureColor = 1;

    // Only sample base material if visible
    if (!SampleEdge || (SampleEdge && EdgeMask != 1))
        baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);

    if (NormalMap) baseTextureColor.rgb = UnpackNormalMap(baseTextureColor, NormalStrength);

    if (SampleEdge) {
        float4 edgeTextureColor = SAMPLE_TEXTURE2D(Texture, SS, EdgeUV);
        if (NormalMap) edgeTextureColor.rgb = UnpackNormalMap(edgeTextureColor, NormalStrength);
        baseTextureColor = lerp(baseTextureColor, edgeTextureColor, EdgeMask);
    }

    return baseTextureColor;
}

// Samples the base and edge colour if required and lerps them together
// Uses a texture array
// Normal maps do not work properly in an array, dont allow them
float4 SampleRepetitionlessArrayTexture(
    UnityTexture2DArray TextureArray,
    int AssignedTextures,
    int ConstantIndex,
    SamplerState SS,

    float EdgeMask,
    float2 EdgeUV,
    float2 TransformedUV,
    bool SampleEdge
){
    float4 baseTextureColor = 1;
    
    // Only sample base material if visible
    if (!SampleEdge || (SampleEdge && EdgeMask != 1))
        SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, TransformedUV, 1, SS, baseTextureColor);

    if (SampleEdge) {
        float4 edgeTextureColor = 1;
        SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, EdgeUV, 1, SS, edgeTextureColor);
        baseTextureColor = lerp(baseTextureColor, edgeTextureColor, EdgeMask);
    }

    return baseTextureColor;
}

#endif