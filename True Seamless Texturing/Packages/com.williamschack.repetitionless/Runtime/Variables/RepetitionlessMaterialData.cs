using UnityEngine;

namespace Repetitionless.Variables
{
    [System.Serializable]
    public class RepetitionlessMaterialData
    {
        // Features
        public bool NoiseEnabled = true;
        public bool RandomiseNoiseScaling = true;
        public bool RandomiseNoiseRotation = true;
        public bool SmoothnessEnabled = true;
        public bool VariationEnabled = false;
        public bool PackedTexture = false;
        public bool EmissionEnabled = false;

        // Assigned Textures
        public bool AlbedoAssigned = false;
        public bool MetallicAssigned = false;
        public bool SmoothnessAssigned = false;
        public bool NormalAssigned = false;
        public bool OcclussionAssigned = false;
        public bool EmissionAssigned = false;
        public bool VariationAssigned = false;
        public bool PackedTextureAssigned = false;

        // Material Properties
        public Color AlbedoTint = Color.white;
        public Color EmissionColour = Color.black;

        public Vector4 TilingOffset = new Vector4(1, 1, 0, 0);

        public float Metallic = 0;
        public float SmoothnessRoughness = 0.5f;
        public float NormalScale = 1.0f;
        public float OcclussionStrength = 1.0f;
        public float AlphaClipping = 0.5f;

        // Noise Settings
        public float NoiseAngleOffset = 2.0f;
        public float NoiseScale = 5.0f;
        public Vector2 NoiseScalingMinMax = new Vector2(0.8f, 1.2f);
        public Vector2 NoiseRandomiseRotationMinMax = new Vector2(0, 360);

        // Variation Settings
        public ETextureType VariationMode = ETextureType.CustomTexture;
        public float VariationOpacity = 0.5f;
        public float VariationBrightness = 0.3f;
        public float VariationSmallScale = 2.0f;
        public float VariationMediumScale = 1.0f;
        public float VariationLargeScale = 0.5f;
        public float VariationNoiseStrength = 0.4f;
        public float VariationNoiseScale = 100.0f;
        public Vector2 VariationNoiseOffset = Vector2.zero;
        public Vector4 VariationTextureTO = new Vector4(5, 5, 0, 0);
    }

    [System.Serializable]
    public struct RepetitionlessMaterialDataCompressed
    {
        // x: Compressed Bools
        //   0 > NoiseEnabled
        //   1 > RandomiseNoiseScaling
        //   2 > RandomiseNoiseRotation
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
        //   7 > PackedTextureAssigned
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
}