#ifndef SAMPLETEXTUREWITHEDGE_INCLUDED
#define SAMPLETEXTUREWITHEDGE_INCLUDED

float4 BlendOverwrite(float4 Base, float4 Blend, float Opacity)
{
    return lerp(Base, Blend, Opacity);
}

float3 UnpackNormalmap(float4 PackedNormal)
{
    PackedNormal.x *= PackedNormal.w;

    float3 normal;
    normal.xy = PackedNormal.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}
float4 SampleTexture(UnityTexture2D Texture, UnitySamplerState SS, float EdgeMask, float2 EdgeUV, float2 TransformedUV, bool NormalMap = false)
{
    float4 baseTextureColor = SAMPLE_TEXTURE2D(Texture, SS, TransformedUV);
    float4 edgeTextureColor = SAMPLE_TEXTURE2D(Texture, SS, EdgeUV);

    if (NormalMap) {
        baseTextureColor.rgb = UnpackNormalmap(baseTextureColor);
        edgeTextureColor.rgb = UnpackNormalmap(edgeTextureColor);
    }
    
    return BlendOverwrite(baseTextureColor, edgeTextureColor, EdgeMask);
}

void SampleSeamlessMaterial_float(    
    float EdgeMask, float2 EdgeUV, float2 TransformedUV, UnitySamplerState SS, UnityTexture2D Albedo, UnityTexture2D NormalMap, UnityTexture2D MetallicMap, bool SmoothnessEnabled, UnityTexture2D SmoothnessMap, UnityTexture2D RoughnessMap, bool EmissionEnabled, UnityTexture2D EmissionMap,
    out float4 AbledoColor, out float4 NormalColor, out float4 MetallicColor, out float4 SmoothnessColor, out float4 EmissionColor)
{
    // Albedo
    AbledoColor = SampleTexture(Albedo, SS, EdgeMask, EdgeUV, TransformedUV);
    
    // Normal Map
    NormalColor = SampleTexture(NormalMap, SS, EdgeMask, EdgeUV, TransformedUV, true);
    
    // Metallic
    MetallicColor = SampleTexture(MetallicMap, SS, EdgeMask, EdgeUV, TransformedUV);
    
    // Smoothness / Roughness
    if (SmoothnessEnabled) SmoothnessColor = SampleTexture(SmoothnessMap, SS, EdgeMask, EdgeUV, TransformedUV);
    else SmoothnessColor = 1 - SampleTexture(RoughnessMap, SS, EdgeMask, EdgeUV, TransformedUV);
    
    // Emission
    EmissionColor = 0;
    if (EmissionEnabled) EmissionColor = SampleTexture(EmissionMap, SS, EdgeMask, EdgeUV, TransformedUV);
    
}

#endif // SAMPLETEXTUREWITHEDGE_INCLUDED