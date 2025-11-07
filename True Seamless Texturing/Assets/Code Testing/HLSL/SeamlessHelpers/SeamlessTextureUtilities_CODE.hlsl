#ifndef SEAMLESSTEXTUREUTILITIES_CODE_INCLUDED
#define SEAMLESSTEXTUREUTILITIES_CODE_INCLUDED

#include "../Utilities/TextureUtilities.hlsl"
#include "../TextureArrayEssentials/TextureArrayUtilities_CODE.hlsl" // CHANGE TO PROPER PATH WHEN CLEANING UP

// Sample from Regular Texture
float4 SampleSeamlessTexture(sampler2D Texture, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool NoiseEnabled, bool NormalMap = false, float NormalStrength = 1.0)
{
    // Only sample required textures if noise disabled
    if (!NoiseEnabled) {
        float4 baseTextureColor = tex2D(Texture, TransformedUV);
        
        if (NormalMap)
            baseTextureColor.rgb = UnpackNormalMap(baseTextureColor, NormalStrength);
        
        return baseTextureColor;
    }
    
    float4 baseTextureColor = tex2D(Texture, TransformedUV);
    float4 edgeTextureColor = tex2D(Texture, EdgeUV);

    if (NormalMap) {
        baseTextureColor.rgb = UnpackNormalMap(baseTextureColor, NormalStrength);
        edgeTextureColor.rgb = UnpackNormalMap(edgeTextureColor, NormalStrength);
    }
    
    return lerp(baseTextureColor, edgeTextureColor, EdgeMask);
}

// Sample from Texture Array
//float4 SampleSeamlessArrayTexture(UnityTexture2DArray TextureArray, int AssignedTextures, int ConstantIndex, SamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool NoiseEnabled)
//{
//    // Skip normal maps, cannot use them in an array
//    
//    // Only sample required textures if noise disabled
//    if (!NoiseEnabled)
//    {
//        float4 baseTextureColor = 1;
//        SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, TransformedUV, 0, SS, baseTextureColor);
//        
//        return baseTextureColor;
//    }
//    
//    float4 baseTextureColor = 1;
//    float4 edgeTextureColor = 1;
//    SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, TransformedUV, 0, SS, baseTextureColor);
//    SampleArrayAtConstantIndex_float(TextureArray, AssignedTextures, ConstantIndex, EdgeUV, 0, SS, edgeTextureColor);
//    
//    return lerp(baseTextureColor, edgeTextureColor, EdgeMask);
//}

#endif