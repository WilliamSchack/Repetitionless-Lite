#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Materials
{
    using Data;

    internal class RepetitionlessTerrainMaterialUtilities
    {
        private const string UV_SPACE_PROP_NAME = "_UVSpace";

        public static void SetupProperties(Material mat, RepetitionlessMaterialDataSO materialProperties)
        {
            // Set uv space to world
            mat.SetFloat(UV_SPACE_PROP_NAME, (int)EUVSpace.World);

            // Update default global tiling offset
            materialProperties.SetGlobalTilingOffset(Constants.DEFAULT_TILING_OFFSET_TERRAIN);

            // Set terrain compatible tag
            mat.SetOverrideTag("TerrainCompatible", "True");
        }

        public static RepetitionlessLayeredDataSO SetupLayeredData(MaterialDataManager dataManager)
        {
            if (dataManager.AssetExists(Constants.LAYERED_DATA_FILE_NAME))
                return dataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            RepetitionlessLayeredDataSO data = ScriptableObject.CreateInstance<RepetitionlessLayeredDataSO>();
            dataManager.CreateAsset(data, Constants.LAYERED_DATA_FILE_NAME);

            // If terrain data exists, set to terrain mode (for previous versions)
            if (dataManager.AssetExists(Constants.TERRAIN_DATA_FILE_NAME))
                data.LayerMode = EControlMode.TerrainLayers;

            // Otherwise set to control textures
            else
                data.LayerMode = EControlMode.ControlTextures;

            data.Save();
            AssetDatabase.SaveAssetIfDirty(data);

            return data;
        }

        public static RepetitionlessTerrainDataSO SetupTerrainData(MaterialDataManager dataManager)
        {
            if (dataManager.AssetExists(Constants.TERRAIN_DATA_FILE_NAME))
                return dataManager.LoadAsset<RepetitionlessTerrainDataSO>(Constants.TERRAIN_DATA_FILE_NAME);

            RepetitionlessTerrainDataSO data = ScriptableObject.CreateInstance<RepetitionlessTerrainDataSO>();
            dataManager.CreateAsset(data, Constants.TERRAIN_DATA_FILE_NAME);

            data.Save();
            AssetDatabase.SaveAssetIfDirty(data);

            return data;
        }
    }
}
#endif