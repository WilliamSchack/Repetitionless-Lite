#ifndef MACROMICROVARIATION_INCLUDED
#define MACROMICROVARIATION_INCLUDED

// NOTE:
// Tiling & Offset must be done along with size scaling, would love to support pre-scaled UVs but it would break the offset otherwise
// eg. If a UV has been offset before inputting to this function, the offset will be multiplied by the Small, Medium, and Large scales

#include "../Noise/Keijiro/SimplexNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"

// Samples a given texture and turns it into a variation multiplier
float MacroMicroVariationTexture(
    float SmallScale,
    float MediumScale,
    float LargeScale,

    float VariationBrightness,
    UnityTexture2D Texture,
    SamplerState SS,

    float2 UV,
    float2 Tiling = float2(1, 1),
    float2 Offset = float2(0, 0)
){
    // Get UVs
    float2 smallUV = UV * Tiling * SmallScale + Offset;
    float2 mediumUV = UV * Tiling * MediumScale + Offset;
    float2 largeUV = UV * Tiling * LargeScale + Offset;
    
    // Sample Texture
    float smallColor = SAMPLE_TEXTURE2D(Texture, SS, smallUV).r;
    float mediumColor = SAMPLE_TEXTURE2D(Texture, SS, mediumUV).r;
    float largeColor = SAMPLE_TEXTURE2D(Texture, SS, largeUV).r;
    
    // Add Brightness
    smallColor += VariationBrightness;
    mediumColor += VariationBrightness;
    largeColor += VariationBrightness;
        
    return smallColor * mediumColor * largeColor;
}

// Samples perlin noise and turns it into a variation multiplier
float MacroMicroVariationPerlinNoise(
    float SmallScale,
    float MediumScale,
    float LargeScale,

    float VariationBrightness,

    float NoiseStrength,
    float2 UV,
    float NoiseScale = 1,
    float2 NoiseOffset = float2(0, 0)
){
    // Get UVs
    float2 smallUV = UV * NoiseScale * SmallScale + NoiseOffset;
    float2 mediumUV = UV * NoiseScale * MediumScale + NoiseOffset;
    float2 largeUV = UV * NoiseScale * LargeScale + NoiseOffset;
    
    // Sample Noise
    float smallColor = ClassicNoise(smallUV) * 2 * NoiseStrength;
    float mediumColor = ClassicNoise(mediumUV) * 2 * NoiseStrength;
    float largeColor = ClassicNoise(largeUV) * 2 * NoiseStrength;
    
    // Remap to more suitable size
    smallColor = lerp(0.75, 1, smallColor);
    mediumColor = lerp(0.75, 1, mediumColor);
    largeColor = lerp(0.75, 1, largeColor);
    
    // Add Brightness
    smallColor += VariationBrightness;
    mediumColor += VariationBrightness;
    largeColor += VariationBrightness;
        
    return smallColor * mediumColor * largeColor;
}

// Samples simplex noise and turns it into a variation multiplier
float MacroMicroVariationSimplexNoise(
    float SmallScale,
    float MediumScale,
    float LargeScale,

    float VariationBrightness,

    float NoiseStrength,
    float2 UV,
    float NoiseScale = 1,
    float2 NoiseOffset = float2(0, 0)
){
    // Get UVs
    float2 smallUV = UV * NoiseScale * SmallScale + NoiseOffset;
    float2 mediumUV = UV * NoiseScale * MediumScale + NoiseOffset;
    float2 largeUV = UV * NoiseScale * LargeScale + NoiseOffset;
    
    // Sample Noise
    float smallColor = SimplexNoise(smallUV) * 2 * NoiseStrength;
    float mediumColor = SimplexNoise(mediumUV) * 2 * NoiseStrength;
    float largeColor = SimplexNoise(largeUV) * 2 * NoiseStrength;
    
    // Remap to more suitable size
    smallColor = lerp(0.75, 1, smallColor);
    mediumColor = lerp(0.75, 1, mediumColor);
    largeColor = lerp(0.75, 1, largeColor);
    
    // Add Brightness
    smallColor += VariationBrightness;
    mediumColor += VariationBrightness;
    largeColor += VariationBrightness;
        
    return smallColor * mediumColor * largeColor;
}

// Outputs a variation colour based on an inputted texture
void MacroMicroVariationTexture_float(
    float4 InputColor,

    float SmallScale,
    float MediumScale,
    float LargeScale,

    float VariationBrightness,
    float VariationOpacity,
    UnityTexture2D VariationTexture,

    SamplerState SS,
    float2 UV,
    float2 Tiling,
    float2 Offset,

    out float4 OutputColor
){
    VariationOpacity = clamp(VariationOpacity, 0, 1);
    VariationBrightness = clamp(VariationBrightness, 0, 1);
    
    float variationMultiplier = MacroMicroVariationTexture(SmallScale, MediumScale, LargeScale, VariationBrightness, VariationTexture, SS, UV, Tiling, Offset);
    OutputColor = lerp(InputColor, variationMultiplier * InputColor, VariationOpacity);
}

// Outputs a variation colour based on perlin noise
void MacroMicroVariationPerlinNoise_float(
    float4 InputColor,
    
    float SmallScale,
    float MediumScale,
    float LargeScale,

    float VariationBrightness,
    float VariationOpacity,
    
    float NoiseStrength,
    float NoiseScale,
    float2 NoiseOffset,
    float2 UV,
    
    out float4 OutputColor
){
    VariationOpacity = clamp(VariationOpacity, 0, 1);
    VariationBrightness = clamp(VariationBrightness, 0, 1);
    
    float variationMultiplier = MacroMicroVariationPerlinNoise(SmallScale, MediumScale, LargeScale, VariationBrightness, NoiseStrength, UV, NoiseScale, NoiseOffset);
    OutputColor = lerp(InputColor, variationMultiplier * InputColor, VariationOpacity);
}

// Outputs a variation colour based on simplex noise
void MacroMicroVariationSimplexNoise_float(
    float4 InputColor,

    float SmallScale,
    float MediumScale,
    float LargeScale,
    
    float VariationBrightness,
    float VariationOpacity,
    
    float NoiseStrength,
    float NoiseScale,
    float2 NoiseOffset,
    float2 UV,
    
    out float4 OutputColor
){
    VariationOpacity = clamp(VariationOpacity, 0, 1);
    VariationBrightness = clamp(VariationBrightness, 0, 1);
    
    float variationMultiplier = MacroMicroVariationSimplexNoise(SmallScale, MediumScale, LargeScale, VariationBrightness, NoiseStrength, UV, NoiseScale, NoiseOffset);
    OutputColor = lerp(InputColor, variationMultiplier * InputColor, VariationOpacity);
}

#endif