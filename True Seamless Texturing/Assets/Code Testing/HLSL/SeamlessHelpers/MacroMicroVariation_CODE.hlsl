#ifndef MACROMICROVARIATION_INCLUDED
#define MACROMICROVARIATION_INCLUDED

// NOTE:
// Tiling & Offset must be done along with size scaling, would love to support pre-scaled UVs but it would break the offset otherwise
// eg. If a UV has been offset before inputting to this function, the offset will be multiplied by the Small, Medium, and Large scales

#include "../Noise/Keijiro/SimplexNoise2D.hlsl"
#include "../Noise/Keijiro/ClassicNoise2D.hlsl"

// Samples a given texture and turns it into a variation multiplier
float MacroMicroVariationTexture(float SmallScale, float MediumScale, float LargeScale, float VariationBrightness, sampler2D Texture, float2 UV, float2 Tiling = float2(1, 1), float2 Offset = float2(0, 0))
{
    // Get UVs
    float2 smallUV = UV * Tiling * SmallScale + Offset;
    float2 mediumUV = UV * Tiling * MediumScale + Offset;
    float2 largeUV = UV * Tiling * LargeScale + Offset;
    
    // Sample Texture
    float smallColor = tex2D(Texture, smallUV).r;
    float mediumColor = tex2D(Texture, mediumUV).r;
    float largeColor = tex2D(Texture, largeUV).r;
    
    // Add Brightness
    smallColor += VariationBrightness;
    mediumColor += VariationBrightness;
    largeColor += VariationBrightness;
        
    return smallColor * mediumColor * largeColor;
}

// Samples perlin noise and turns it into a variation multiplier
float MacroMicroVariationPerlinNoise(float SmallScale, float MediumScale, float LargeScale, float VariationBrightness, float NoiseStrength, float2 UV, float NoiseScale = 1, float2 NoiseOffset = float2(0, 0))
{
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
float MacroMicroVariationSimplexNoise(float SmallScale, float MediumScale, float LargeScale, float VariationBrightness, float NoiseStrength, float2 UV, float NoiseScale = 1, float2 NoiseOffset = float2(0, 0))
{
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

#endif