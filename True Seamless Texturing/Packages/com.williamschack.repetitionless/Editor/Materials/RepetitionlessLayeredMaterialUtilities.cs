#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Materials
{
    using Data;

    internal class RepetitionlessLayeredMaterialUtilities
    {
        private const string UV_SPACE_PROP_NAME = "_UVSpace";

        public static void SetupProperties(Material mat, RepetitionlessMaterialDataSO materialProperties)
        {
            // Set uv space to world
            mat.SetFloat(UV_SPACE_PROP_NAME, (int)EUVSpace.World);

            // Update default global tiling offset
            materialProperties.SetGlobalTilingOffset(Constants.DEFAULT_TILING_OFFSET_TERRAIN);
        }

        public static void UpdateLayerMode(MaterialDataManager dataManager, ELayerMode layerMode)
        {
            string newShader = "";
            switch (layerMode) {
                case ELayerMode.TerrainLayers:
                    newShader = Constants.SHADER_MATERIAL_NAME_LAYERED_TERRAIN;
                    break;
                case ELayerMode.ControlTextures:
                    newShader = Constants.SHADER_MATERIAL_NAME_LAYERED_LIT;
                    break;
                default:
                    return;
            }

            string shaderName = dataManager.Material.shader.name;
            string rp = shaderName.Split("/")[1];
            string newShaderName = $"{Constants.SHADER_FOLDER}{rp}/{newShader}";
            
            dataManager.Material.shader = Shader.Find(newShaderName);

            // Create terrain data if required
            if (layerMode == ELayerMode.TerrainLayers)
                SetupTerrainData(dataManager);
        }

        public static RepetitionlessLayeredDataSO SetupLayeredData(MaterialDataManager dataManager)
        {
            if (dataManager.AssetExists(Constants.LAYERED_DATA_FILE_NAME))
                return dataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            RepetitionlessLayeredDataSO data = ScriptableObject.CreateInstance<RepetitionlessLayeredDataSO>();
            dataManager.CreateAsset(data, Constants.LAYERED_DATA_FILE_NAME);

            // Setup the textures
            data.Init();

            // Update layer mode based on shader name
            string shaderName = dataManager.Material.shader.name;
            if (shaderName.Contains(Constants.SHADER_MATERIAL_NAME_LAYERED_LIT)) {
                data.LayerMode = ELayerMode.ControlTextures;
            } else {
                data.LayerMode = ELayerMode.TerrainLayers;
                SetupTerrainData(dataManager);
            }

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