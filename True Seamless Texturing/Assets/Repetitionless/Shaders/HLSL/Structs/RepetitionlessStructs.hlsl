struct RepetitionlessMaterial
{
    float2 Settings;
    float4 TilingOffset;
    
    sampler2D Albedo;
    sampler2D MetallicMap;
    sampler2D SmoothnessMap;
    sampler2D RoughnessMap;
    sampler2D NormalMap;
    sampler2D OcclussionMap;
    sampler2D EmissionMap;
    
    float4 AlbedoTint;
    float3 EmissionColor;
    
    float4 MaterialProperties1;
    float2 MaterialProperties2;

    float2 NoiseSettings;
    float4 NoiseMinMax;

    float VariationMode;
    float4 VariationSettings;
    float4 VariationNoiseSettings;
    float VariationBrightness;
    sampler2D VariationTexture;
    float4 VariationTextureTO;
};

struct RepetitionlessLayer
{
    RepetitionlessMaterial BaseMaterial;
    RepetitionlessMaterial FarMaterial;
    RepetitionlessMaterial BlendMaterial;
};