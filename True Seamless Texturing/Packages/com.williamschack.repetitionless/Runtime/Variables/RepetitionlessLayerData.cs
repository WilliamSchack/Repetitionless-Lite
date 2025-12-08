using UnityEngine;

namespace Repetitionless.Variables
{
    [System.Serializable]
    public class RepetitionlessLayerData
    {
        // Materials
        [SerializeField] public RepetitionlessMaterialData BaseMaterialData = new RepetitionlessMaterialData();
        [SerializeField] public RepetitionlessMaterialData FarMaterialData = new RepetitionlessMaterialData();
        [SerializeField] public RepetitionlessMaterialData BlendMaterialData = new RepetitionlessMaterialData();

        // Distance Blend Settings
        public bool DistanceBlendEnabled = false;
        public EDistanceBlendMode DistanceBlendMode = EDistanceBlendMode.TilingOffset;
        public Vector2 DistanceBlendMinMax = new Vector2(100, 150);

        // Blend Material Settings
        public bool MaterialBlendEnabled = false;
        public bool BlendMaskAssigned = false;
        public bool OverrideDistanceBlend = true;
        public bool OverrideDistanceBlendTO = true;
        public ETextureType BlendMaskType = ETextureType.PerlinNoise;
        public Vector4 BlendMaskDistanceTO = new Vector4(1, 1, 0, 0);
        public float BlendMaskOpacity = 1.0f;
        public float BlendMaskStrength = 1.0f;
        public float BlendMaskNoiseScale = 10.0f;
        public Vector2 BlendMaskNoiseOffset = Vector3.zero;
        public Vector4 BlendMaskTextureTO = new Vector4(1, 1, 0, 0);
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
        //   1 > BlendMaskAssigned
        //   2 > OverrideDistanceBlend
        //   3 > OverrideDistanceBlendTO
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
}