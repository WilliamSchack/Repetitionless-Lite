#ifndef SEAMLESSTEXTUREUTILITIES_INCLUDED
#define SEAMLESSTEXTUREUTILITIES_INCLUDED

#include "../Utilities/TextureUtilities.hlsl"
#include "../TextureArrayEssentials/TextureArrayUtilities.hlsl"

// Sample from Regular Texture
float4 SampleSeamlessTexture(UnityTexture2D Texture, SamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool SampleEdge, bool NormalMap = false, float NormalStrength = 1.0)
{
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

// Sample from Texture Array
// Normal maps do not work properly in an array, dont allow them
float4 SampleSeamlessArrayTexture(UnityTexture2DArray TextureArray, int AssignedTextures, int ConstantIndex, SamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool SampleEdge)
{
    float4 baseTextureColor = 1;
    
    // Only sample base material if visible
    if (!SampleEdge || (SampleEdge && EdgeMask != 1))
        SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, TransformedUV, 0, SS, baseTextureColor);

    if (SampleEdge) {
        float4 edgeTextureColor = 1;
        SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, EdgeUV, 0, SS, edgeTextureColor);
        baseTextureColor = lerp(baseTextureColor, edgeTextureColor, EdgeMask);
    }

    return baseTextureColor;
}

#endif