using Repetitionless.Variables;
using UnityEngine;

public struct RepetitionlessMaterialData
{
    // Features
    public bool NoiseEnabled;
    public bool RandomiseNoiseScaling;
    public bool RandomiseRotation;
    public bool SmoothnessEnabled;
    public bool VariationEnabled;
    public bool PackedTexture;
    public bool EmissionEnabled;

    // Assigned Textures
    public bool AlbedoAssigned;
    public bool MetallicAssigned;
    public bool SmoothnessAssigned;
    public bool NormalAssigned;
    public bool OcclussionAssigned;
    public bool EmissionAssigned;
    public bool VarationAssigned;

    // Material Properties
    public Color AlbedoTint;
    public Color EmissionColour;

    public Vector4 TilingOffset;

    public float Metallic;
    public float SmoothnessRoughness;
    public float NormalScale;
    public float OcclussionStrength;
    public float AlphaClipping;

    // Noise Settings
    public float NoiseAngleOffset;
    public float NoiseScale;
    public Vector2 NoiseScalingMinMax;
    public Vector2 NoiseRandomiseRotationMinMax;

    // Variation Settings
    public ETextureType VariationMode;
    public float VariationOpacity;
    public float VariationBrightness;
    public float VariationSmallScale;
    public float VariationMediumScale;
    public float VariationLargeScale;
    public float VariationNoiseStrength;
    public float VariationNoiseScale;
    public Vector2 VariationNoiseOffset;
    public Vector4 VariationTextureTO;

    
}

public struct RepetitionlessMaterialDataCompressed
{
    // x: Compressed Bools
    //   0 > NoiseEnabled
    //   1 > RandomiseNoiseScaling
    //   2 > RandomiseRotation
    //   3 > SmoothnessEnabled
    //   4 > VariationEnabled
    //   5 > PackedTexture
    //   6 > EmissionEnabled
    // y: Compressed Bools
    //   0 > AlbedoAssigned
    //   1 > MetallicAssigned
    //   2 > SmoothnessAssigned
    //   3 > NormalAssigned
    //   4 > OcclussionAssigned
    //   5 > EmissionAssigned
    //   6 > VarationAssigned
    // z: Metallic
    // w: Smoothness/Roughness
    public Vector4 Settings1;

    // x: NormalScale
    // y: OcclussionStrength
    // z: AlphaClipping
    // w: NoiseAngleOffset
    public Vector4 Settings2;

    // x: NoiseScale
    // y: VariationMode
    // z: VariationOpacity
    // w: VariationBrightness
    public Vector4 Settings3;

    // x: VariationSmallScale
    // y: VariationMediumScale
    // z: VariationLargeScale
    // w: VariationNoiseStrength
    public Vector4 Settings4;

    // xy: NoiseScalingMinMax
    // zw: NoiseRandomiseRotationMinMax
    public Vector4 Settings5;

    public Vector3 AlbedoTint;
    public Vector3 EmissionColour;

    public Vector4 TilingOffset;

    // IF USING NOISE:
    // x: VariationNoiseScale
    // zw: VariationNoiseOffset
    // IF USING TEXTURE:
    // xyzw: VariationTextureTO
    public Vector4 VariationTO;
}