#ifndef SEAMLESSTEXTUREUTILITIES_INCLUDED
#define SEAMLESSTEXTUREUTILITIES_INCLUDED

#include "../Utilities/TextureUtilities.hlsl"

float4 SampleSeamlessTexture(UnityTexture2D Texture, SamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool NoiseEnabled, bool NormalMap = false, float NormalStrength = 1.0) // SamplerState
{
    // Only sample required textures is noise disabled
    if (!NoiseEnabled) {
        float4 baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
        
        if (NormalMap)
            baseTextureColor.rgb = UnpackNormalmap(baseTextureColor, NormalStrength);
        
        return baseTextureColor;
    }
    
    float4 baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
    float4 edgeTextureColor = SAMPLE_TEXTURE2D(Texture, SS, EdgeUV);

    if (NormalMap) {
        baseTextureColor.rgb = UnpackNormalmap(baseTextureColor, NormalStrength);
        edgeTextureColor.rgb = UnpackNormalmap(edgeTextureColor, NormalStrength);
    }
    
    return lerp(baseTextureColor, edgeTextureColor, EdgeMask);
}

float4 SampleSeamlessTexture(UnityTexture2D Texture, UnitySamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool NoiseEnabled, bool NormalMap = false, float NormalStrength = 1.0) // UnitySamplerState
{
    // Only sample required textures is noise disabled
    if (!NoiseEnabled) {
        float4 baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
        
        if (NormalMap)
            baseTextureColor.rgb = UnpackNormalmap(baseTextureColor, NormalStrength);
        
        return SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
    }
    
    float4 baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
    float4 edgeTextureColor = SAMPLE_TEXTURE2D(Texture, SS, EdgeUV);

    if (NormalMap)
    {
        baseTextureColor.rgb = UnpackNormalmap(baseTextureColor, NormalStrength);
        edgeTextureColor.rgb = UnpackNormalmap(edgeTextureColor, NormalStrength);
    }
    
    return lerp(baseTextureColor, edgeTextureColor, EdgeMask);
}

#endif