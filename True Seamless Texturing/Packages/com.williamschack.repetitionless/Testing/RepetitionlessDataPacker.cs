using UnityEngine;
using Repetitionless.Variables;
using Repetitionless.Compression;

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
        bool[] targetBools = BooleanCompression.GetValues((int)target, values.Length);
        for (int i = 0; i < values.Length; i++) {
            if (targetBools[i] == values[i])
                continue;

            target = BooleanCompression.AddValue((int)target, i, values[i]);
            return true;
        }

        return false;
    }

    // Returns the changed variable index in the struct
    public static int UpdateCompressedMaterialDataSingle(ref RepetitionlessMaterialDataCompressed compressedData, RepetitionlessMaterialData newData)
    {
        if (UpdateBoolsIfChanged(ref compressedData.Settings1.x,
            newData.NoiseEnabled,
            newData.RandomiseNoiseScaling,
            newData.RandomiseRotation,
            newData.SmoothnessEnabled,
            newData.VariationEnabled,
            newData.PackedTexture,
            newData.EmissionEnabled
        )) return 0;


        if (UpdateBoolsIfChanged(ref compressedData.Settings1.y,
            newData.AlbedoAssigned,
            newData.MetallicAssigned,
            newData.SmoothnessAssigned,
            newData.NormalAssigned,
            newData.OcclussionAssigned,
            newData.EmissionAssigned,
            newData.VarationAssigned
        )) return 0;

        if (UpdateGenericIfChanged<float>(ref compressedData.Settings1.z, newData.Metallic))            return 0;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings1.w, newData.SmoothnessRoughness)) return 0;

        if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.x, newData.NormalScale))        return 1;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.y, newData.OcclussionStrength)) return 1;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.z, newData.AlphaClipping))      return 1;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings2.w, newData.NoiseAngleOffset))   return 1;

        if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.x, newData.NoiseScale))          return 2;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.y, (int)newData.VariationMode))  return 2;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.z, newData.VariationOpacity))    return 2;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings3.w, newData.VariationBrightness)) return 2;
        
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.x, newData.VariationSmallScale))    return 3;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.y, newData.VariationMediumScale))   return 3;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.z, newData.VariationLargeScale))    return 3;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings4.w, newData.VariationNoiseStrength)) return 3;

        if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.x, newData.NoiseScalingMinMax.x))           return 4;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.y, newData.NoiseScalingMinMax.y))           return 4;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.z, newData.NoiseRandomiseRotationMinMax.x)) return 4;
        if (UpdateGenericIfChanged<float>(ref compressedData.Settings5.w, newData.NoiseRandomiseRotationMinMax.y)) return 4;

        if (UpdateColourIfChanged(ref compressedData.AlbedoTint, newData.AlbedoTint))         return 5;
        if (UpdateColourIfChanged(ref compressedData.EmissionColour, newData.EmissionColour)) return 6;

        if (UpdateGenericIfChanged<Vector4>(ref compressedData.TilingOffset, newData.TilingOffset)) return 7;

        if (newData.VariationMode == ETextureType.CustomTexture) {
            if (UpdateGenericIfChanged<Vector4>(ref compressedData.VariationTO, newData.VariationTextureTO)) return 8;
        } else {
            // All of these should be updated, return if changed afterwards
            bool updatedAny = false;

            if (UpdateGenericIfChanged<float>(ref compressedData.VariationTO.x, newData.VariationNoiseScale)) updatedAny = true;
            compressedData.VariationTO.y = 0;
            if (UpdateGenericIfChanged<float>(ref compressedData.VariationTO.z, newData.VariationNoiseOffset.x)) updatedAny = true;
            if (UpdateGenericIfChanged<float>(ref compressedData.VariationTO.w, newData.VariationNoiseOffset.y)) updatedAny = true;

            if (updatedAny)
                return 8;
        }

        return -1;
    }

    // Returns the changed variable index in the struct, incrementing for each material
    public static int UpdateCompressedLayerDataSingle(ref RepetitionlessLayerDataCompressed compressedData, RepetitionlessLayerData newData)
    {
        int baseMaterialChangedIndex = UpdateCompressedMaterialDataSingle(ref compressedData.BaseMaterialData, newData.BaseMaterialData);
        if (baseMaterialChangedIndex != -1) return baseMaterialChangedIndex;

        int farMaterialChangedIndex = UpdateCompressedMaterialDataSingle(ref compressedData.FarMaterialData, newData.FarMaterialData);
        if (farMaterialChangedIndex != -1) return farMaterialChangedIndex + COMPRESSED_MATERIAL_VARIABLES_COUNT;

        int blendMaterialChangedIndex = UpdateCompressedMaterialDataSingle(ref compressedData.BlendMaterialData, newData.BlendMaterialData);
        if (blendMaterialChangedIndex != -1) return blendMaterialChangedIndex + COMPRESSED_MATERIAL_VARIABLES_COUNT * 2;

        int startingChangedIndex = COMPRESSED_MATERIAL_VARIABLES_COUNT * 3;

        if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.x, newData.DistanceBlendEnabled ? 1 : 0)) return startingChangedIndex + 0;
        if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.y, (int)newData.DistanceBlendMode))       return startingChangedIndex + 0;
        if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.z, newData.DistanceBlendMinMax.x))        return startingChangedIndex + 0;
        if (UpdateGenericIfChanged<float>(ref compressedData.DistanceBlendSettings.w, newData.DistanceBlendMinMax.y))        return startingChangedIndex + 0;

        if (UpdateGenericIfChanged<Vector4>(ref compressedData.BlendMaskDistanceTO, newData.BlendMaskDistanceTO)) return startingChangedIndex + 1;

        if (UpdateBoolsIfChanged(ref compressedData.MaterialBlendSettings.x,
            newData.MaterialBlendEnabled,
            newData.OverrideDistanceBlend,
            newData.OverrideDistanceBlendTO
        )) return startingChangedIndex + 2;

        if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendSettings.y, (int)newData.BlendMaskType)) return startingChangedIndex + 2;
        if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendSettings.z, newData.BlendMaskOpacity))   return startingChangedIndex + 2;
        if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendSettings.w, newData.BlendMaskStrength))  return startingChangedIndex + 2;

        if (newData.BlendMaskType == ETextureType.CustomTexture) {
            if (UpdateGenericIfChanged<Vector4>(ref compressedData.MaterialBlendMaskTO, newData.BlendMaskTextureTO)) return startingChangedIndex + 3;
        } else {
            // All of these should be updated, return if changed afterwards
            bool updatedAny = false;

            if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendMaskTO.x, newData.BlendMaskNoiseScale)) updatedAny = true;
            compressedData.MaterialBlendMaskTO.y = 0;
            if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendMaskTO.z, newData.BlendMaskNoiseOffset.x)) updatedAny = true;
            if (UpdateGenericIfChanged<float>(ref compressedData.MaterialBlendMaskTO.w, newData.BlendMaskNoiseOffset.y)) updatedAny = true;

            if (updatedAny)
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

        Color? baseMaterialFieldColour = GetMaterialFieldColour(compressedData.BaseMaterialData, materialFieldIndex);
        if (baseMaterialFieldColour.HasValue) return baseMaterialFieldColour;

        Color? farMaterialFieldColour = GetMaterialFieldColour(compressedData.FarMaterialData, materialFieldIndex);
        if (farMaterialFieldColour.HasValue) return farMaterialFieldColour;

        Color? blendMaterialFieldColour = GetMaterialFieldColour(compressedData.BlendMaterialData, materialFieldIndex);
        if (blendMaterialFieldColour.HasValue) return blendMaterialFieldColour;

        switch (materialFieldIndex) {
            case 0: return Vector4ToColour(compressedData.DistanceBlendSettings);
            case 1: return Vector4ToColour(compressedData.BlendMaskDistanceTO);
            case 2: return Vector4ToColour(compressedData.MaterialBlendSettings);
            case 3: return Vector4ToColour(compressedData.MaterialBlendMaskTO);
        }

        return null;
    }
}