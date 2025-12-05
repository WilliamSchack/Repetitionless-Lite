using UnityEngine;

namespace Repetitionless.Data
{
    using Variables;
    using Compression;
    using Unity.VisualScripting.YamlDotNet.Core.Tokens;

    public static class RepetitionlessDataPacker
    {
        public const int COMPRESSED_MATERIAL_VARIABLES_COUNT = 9;
        public const int COMPRESSED_LAYER_VARIABLES_COUNT = COMPRESSED_MATERIAL_VARIABLES_COUNT * 3 + 4;

        public static Color Vector4ToColour(Vector4 vector)
        {
            return new Color(vector.x, vector.y, vector.z, vector.w);
        }

        public static Color Vector3ToColour(Vector3 vector)
        {
            return new Color(vector.x, vector.y, vector.z);
        }

        // Returns if the value was changed
        private static bool UpdateGenericIfChanged<T>(ref T target, T value)
        {
            if (target.Equals(value))
                return false;

            target = value;
            return true;
        }

        // Returns if the value was changed
        private static bool UpdateColourIfChanged(ref Vector3 target, Color value)
        {
            if (target.x == value.r && target.y == value.g && target.y == value.b)
                return false;

            target = new Vector3(value.r, value.g, value.b);
            return true;
        }

        // Returns if the value was changed
        private static bool UpdateBoolsIfChanged(ref float target, params bool[] values)
        {
            bool valueChanged = false;

            bool[] targetBools = BooleanCompression.GetValues((int)target, values.Length);
            for (int i = 0; i < values.Length; i++) {
                if (targetBools[i] == values[i])
                    continue;

                target = BooleanCompression.AddValue((int)target, i, values[i]);
                valueChanged = true;
            }

            return valueChanged;
        }

        // Returns the changed variable index in the struct
        // By default returns on the first changed value
        public static int UpdateCompressedMaterialData(ref RepetitionlessMaterialDataCompressed compressedData, RepetitionlessMaterialData newData, bool updateAll = false)
        {
            if (UpdateBoolsIfChanged(ref compressedData.Settings1.x,
                newData.NoiseEnabled,
                newData.RandomiseNoiseScaling,
                newData.RandomiseNoiseRotation,
                newData.SmoothnessEnabled,
                newData.VariationEnabled,
                newData.PackedTexture,
                newData.EmissionEnabled
            ) && !updateAll) return 0;


            if (UpdateBoolsIfChanged(ref compressedData.Settings1.y,
                newData.AlbedoAssigned,
                newData.MetallicAssigned,
                newData.SmoothnessAssigned,
                newData.NormalAssigned,
                newData.OcclussionAssigned,
                newData.EmissionAssigned,
                newData.VariationAssigned
            ) && !updateAll) return 0;

            if (UpdateGenericIfChanged<float>(ref compressedData.Settings1.z, newData.Metallic) && !updateAll)            return 0;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings1.w, newData.SmoothnessRoughness) && !updateAll) return 0;

            if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.x, newData.NormalScale) && !updateAll)        return 1;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.y, newData.OcclussionStrength) && !updateAll) return 1;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.z, newData.AlphaClipping) && !updateAll)      return 1;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.w, newData.NoiseAngleOffset) && !updateAll)   return 1;

            if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.x, newData.NoiseScale) && !updateAll)          return 2;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.y, (int)newData.VariationMode) && !updateAll)  return 2;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.z, newData.VariationOpacity) && !updateAll)    return 2;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.w, newData.VariationBrightness) && !updateAll) return 2;
            
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.x, newData.VariationSmallScale) && !updateAll)    return 3;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.y, newData.VariationMediumScale) && !updateAll)   return 3;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.z, newData.VariationLargeScale) && !updateAll)    return 3;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.w, newData.VariationNoiseStrength) && !updateAll) return 3;

            if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.x, newData.NoiseScalingMinMax.x) && !updateAll)           return 4;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.y, newData.NoiseScalingMinMax.y) && !updateAll)           return 4;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.z, newData.NoiseRandomiseRotationMinMax.x) && !updateAll) return 4;
            if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.w, newData.NoiseRandomiseRotationMinMax.y) && !updateAll) return 4;

            if (UpdateColourIfChanged(ref compressedData.AlbedoTint, newData.AlbedoTint) && !updateAll)         return 5;
            if (UpdateColourIfChanged(ref compressedData.EmissionColour, newData.EmissionColour) && !updateAll) return 6;

            if (UpdateGenericIfChanged<Vector4>(ref compressedData.TilingOffset, newData.TilingOffset) && !updateAll) return 7;

            if (newData.VariationMode == ETextureType.CustomTexture) {
                if (UpdateGenericIfChanged<Vector4>(ref compressedData.VariationTO, newData.VariationTextureTO) && !updateAll) return 8;
            } else {
                // All of these should be updated, return if changed afterwards
                bool updatedAny = false;

                if (UpdateGenericIfChanged<float>(ref compressedData.VariationTO.x, newData.VariationNoiseScale)) updatedAny = true;
                compressedData.VariationTO.y = 0;
                if (UpdateGenericIfChanged<float>(ref compressedData.VariationTO.z, newData.VariationNoiseOffset.x)) updatedAny = true;
                if (UpdateGenericIfChanged<float>(ref compressedData.VariationTO.w, newData.VariationNoiseOffset.y)) updatedAny = true;

                if (updatedAny && !updateAll)
                    return 8;
            }

            return -1;
        }

        // Returns the changed variable index in the struct, incrementing for each material
        // By default returns on the first changed value
        public static int UpdateCompressedLayerData(ref RepetitionlessLayerDataCompressed compressedData, RepetitionlessLayerData newData, bool updateAll = false)
        {
            int baseMaterialChangedIndex = UpdateCompressedMaterialData(ref compressedData.BaseMaterialData, newData.BaseMaterialData, updateAll);
            if (baseMaterialChangedIndex != -1 && !updateAll) return baseMaterialChangedIndex;

            int farMaterialChangedIndex = UpdateCompressedMaterialData(ref compressedData.FarMaterialData, newData.FarMaterialData, updateAll);
            if (farMaterialChangedIndex != -1 && !updateAll) return farMaterialChangedIndex + COMPRESSED_MATERIAL_VARIABLES_COUNT;

            int blendMaterialChangedIndex = UpdateCompressedMaterialData(ref compressedData.BlendMaterialData, newData.BlendMaterialData, updateAll);
            if (blendMaterialChangedIndex != -1 && !updateAll) return blendMaterialChangedIndex + COMPRESSED_MATERIAL_VARIABLES_COUNT * 2;

            int startingChangedIndex = COMPRESSED_MATERIAL_VARIABLES_COUNT * 3;

            if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.x, newData.DistanceBlendEnabled ? 1.0f : 0.0f) && !updateAll) return startingChangedIndex + 0;
            if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.y, (int)newData.DistanceBlendMode) && !updateAll)             return startingChangedIndex + 0;
            if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.z, newData.DistanceBlendMinMax.x) && !updateAll)              return startingChangedIndex + 0;
            if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.w, newData.DistanceBlendMinMax.y) && !updateAll)              return startingChangedIndex + 0;

            if (UpdateGenericIfChanged<Vector4>(ref compressedData.BlendMaskDistanceTO, newData.BlendMaskDistanceTO) && !updateAll) return startingChangedIndex + 1;

            if (UpdateBoolsIfChanged(ref compressedData.MaterialBlendSettings.x,
                newData.MaterialBlendEnabled,
                newData.OverrideDistanceBlend,
                newData.OverrideDistanceBlendTO
            ) && !updateAll) return startingChangedIndex + 2;

            if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendSettings.y, (int)newData.BlendMaskType) && !updateAll) return startingChangedIndex + 2;
            if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendSettings.z, newData.BlendMaskOpacity) && !updateAll)   return startingChangedIndex + 2;
            if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendSettings.w, newData.BlendMaskStrength) && !updateAll)  return startingChangedIndex + 2;

            if (newData.BlendMaskType == ETextureType.CustomTexture) {
                if (UpdateGenericIfChanged<Vector4>(ref compressedData.MaterialBlendMaskTO, newData.BlendMaskTextureTO) && !updateAll) return startingChangedIndex + 3;
            } else {
                // All of these should be updated, return if changed afterwards
                bool updatedAny = false;

                if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendMaskTO.x, newData.BlendMaskNoiseScale)) updatedAny = true;
                compressedData.MaterialBlendMaskTO.y = 0;
                if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendMaskTO.z, newData.BlendMaskNoiseOffset.x)) updatedAny = true;
                if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendMaskTO.w, newData.BlendMaskNoiseOffset.y)) updatedAny = true;

                if (updatedAny && !updateAll)
                    return startingChangedIndex + 3;
            }

            return -1;
        }

        public static Color? GetMaterialFieldColour(RepetitionlessMaterialDataCompressed compressedData, int compressedFieldIndex)
        {
            switch(compressedFieldIndex) {
                case 0: return Vector4ToColour(compressedData.Settings1);
                case 1: return Vector4ToColour(compressedData.Settings2);
                case 2: return Vector4ToColour(compressedData.Settings3);
                case 3: return Vector4ToColour(compressedData.Settings4);
                case 4: return Vector4ToColour(compressedData.Settings5);
                case 5: return Vector3ToColour(compressedData.AlbedoTint);
                case 6: return Vector3ToColour(compressedData.EmissionColour);
                case 7: return Vector4ToColour(compressedData.TilingOffset);
                case 8: return Vector4ToColour(compressedData.VariationTO);
            }

            return null;
        }

        public static Color? GetLayerFieldColour(RepetitionlessLayerDataCompressed compressedData, int compressedFieldIndex)
        {
            int materialFieldIndex = compressedFieldIndex % COMPRESSED_MATERIAL_VARIABLES_COUNT;

            if (compressedFieldIndex < COMPRESSED_MATERIAL_VARIABLES_COUNT)
                return GetMaterialFieldColour(compressedData.BaseMaterialData, materialFieldIndex);

            if (compressedFieldIndex < COMPRESSED_MATERIAL_VARIABLES_COUNT * 2)
                return GetMaterialFieldColour(compressedData.FarMaterialData, materialFieldIndex);

            if (compressedFieldIndex < COMPRESSED_MATERIAL_VARIABLES_COUNT * 3)
                return GetMaterialFieldColour(compressedData.BlendMaterialData, materialFieldIndex);

            switch (materialFieldIndex) {
                case 0: return Vector4ToColour(compressedData.DistanceBlendSettings);
                case 1: return Vector4ToColour(compressedData.BlendMaskDistanceTO);
                case 2: return Vector4ToColour(compressedData.MaterialBlendSettings);
                case 3: return Vector4ToColour(compressedData.MaterialBlendMaskTO);
            }

            return null;
        }
    }
}