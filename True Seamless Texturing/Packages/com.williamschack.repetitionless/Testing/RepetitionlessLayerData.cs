using UnityEngine;
using Repetitionless.Variables;

public struct RepetitionlessLayerData
{
    // Materials
    public RepetitionlessMaterialData BaseMaterialData;
    public RepetitionlessMaterialData FarMaterialData;
    public RepetitionlessMaterialData BlendMaterialData;

    // Distance Blend Settings
    public bool DistanceBlendEnabled;
    public EDistanceBlendMode DistanceBlendMode;
    public Vector2 DistanceBlendMinMax;

    // Blend Material Settings
    public bool MaterialBlendEnabled;
    public bool OverrideDistanceBlend;
    public bool OverrideDistanceBlendTO;
    public ETextureType BlendMaskType;
    public Vector4 BlendMaskDistanceTO;
    public float BlendMaskOpacity;
    public float BlendMaskStrength;
    public float BlendMaskNoiseScale;
    public Vector2 BlendMaskNoiseOffset;
    public Vector4 BlendMaskTextureTO; // Not sure if its being used look into it
}

public struct RepetitionlessLayerDataCompressed
{
    public RepetitionlessMaterialDataCompressed BaseMaterialData;
    public RepetitionlessMaterialDataCompressed FarMaterialData;
    public RepetitionlessMaterialDataCompressed BlendMaterialData;

    // x: DistanceBlendEnabled
    // y: DistanceBlendMode
    // zw: DistanceBlendMinMax
    public Vector4 DistanceBlendSettings;

    public Vector4 BlendMaskDistanceTO;

    // x: Compressed Bools
    //   0 > MaterialBlendEnabled
    //   1 > OverrideDistanceBlend
    //   2 > OverrideDistanceBlendTO
    // y: BlendMaskType
    // z: BlendMaskOpacity
    // w: BlendMaskStrength
    public Vector4 MaterialBlendSettings;

    // IF USING NOISE MASK:
    // x: BlendMaskNoiseScale
    // zw: BlendMaskNoiseOffset
    // IF USING TEXTURE MASK:
    // xyzw: BlendMaskTextureTO
    public Vector4 MaterialBlendMaskTO;
}