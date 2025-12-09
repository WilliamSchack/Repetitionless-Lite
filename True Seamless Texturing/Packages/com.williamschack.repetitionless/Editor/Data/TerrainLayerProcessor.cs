#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Data
{
    using Inspectors;
    using Variables;

    public class TerrainLayerProcessor : AssetModificationProcessor
    {
        private static TerrainLayerSyncDataSO _syncDataCache;
        private static TerrainLayerSyncDataSO _syncData {
            get {
                if (_syncDataCache == null)
                    _syncDataCache = TerrainLayerSyncDataSO.Load();

                return _syncDataCache;
            }
        }

        private static void UpdateLayerMaterialsData(TerrainLayer terrainLayer)
        {
            // Update the materials data related to this terrain layer
            foreach (Material mat in _syncData.TerrainLayerToMaterial.Get(terrainLayer).Items) {
                string progressBarTitle = $"Updating {mat.name}...";
                EditorUtility.DisplayProgressBar(progressBarTitle, "Setting up", 0.0f);

                MaterialDataManager materialData = new MaterialDataManager(mat);
                RepetitionlessMaterialDataSO materialProperties = materialData.LoadAsset<RepetitionlessMaterialDataSO>(RepetitionlessGUIBaseNEW.PROPERTIES_FILE_NAME);
                RepetitionlessTextureDataSO  textureData        = materialData.LoadAsset<RepetitionlessTextureDataSO>(RepetitionlessGUIBaseNEW.TEXTURE_DATA_FILE_NAME);

                if (materialProperties == null || textureData == null) {
                    Debug.LogError($"Could not find properties or textures for material {mat.name}");
                    EditorUtility.ClearProgressBar();
                    continue;
                }

                // Setup data
                materialProperties.SetDataManager(materialData);
                textureData.SetupTextureDrawers(materialData);

                // Get the layer index
                int layerIndex = _syncData.MaterialToTerrainLayer.Get(mat).Items.IndexOf(terrainLayer);

                // Update properties
                RepetitionlessMaterialData baseMaterialData = materialProperties.Data.BaseMaterialData;
                baseMaterialData.NormalScale  = terrainLayer.normalScale;
                baseMaterialData.TilingOffset = new Vector4(terrainLayer.tileSize.x, terrainLayer.tileSize.y, terrainLayer.tileOffset.x, terrainLayer.tileOffset.y);

                EditorUtility.DisplayProgressBar(progressBarTitle, "Updating Textures", 0.2f);

                // Update diffuse, normal textures
                int arrayLayerIndex = layerIndex * 3 + 0; // Using base material
                if(terrainLayer.diffuseTexture != textureData.GetTextureData(layerIndex, 0, 0)[0].Texture)
                    textureData.AVTexturesDrawer.UpdateTexture(terrainLayer.diffuseTexture, arrayLayerIndex, 0, true);
                if(terrainLayer.normalMapTexture != textureData.GetTextureData(layerIndex, 0, 1)[0].Texture)
                    textureData.NSOTexturesDrawer.UpdateTexture(terrainLayer.normalMapTexture, arrayLayerIndex, 0, true);

                // Update packed textures
                if(!baseMaterialData.PackedTexture ||
                    textureData.GetTextureData(layerIndex, 0, 1)[3].Texture != terrainLayer.maskMapTexture ||
                    textureData.GetTextureData(layerIndex, 0, 2)[2].Texture != terrainLayer.maskMapTexture
                ) {
                    baseMaterialData.PackedTexture = true;
                
                    textureData.GetTextureData(layerIndex, 0, 1)[3].Texture = terrainLayer.maskMapTexture;
                    textureData.GetTextureData(layerIndex, 0, 2)[2].Texture  = terrainLayer.maskMapTexture;

                    textureData.UpdatePackedTexture(layerIndex, 0, true);
                }

                EditorUtility.DisplayProgressBar(progressBarTitle, "Updating Properties", 0.8f);

                // Save changed properties
                materialProperties.UpdateAssignedTextures(mat, textureData, 0, layerIndex);

                materialProperties.Save();
                textureData.Save();

                EditorUtility.ClearProgressBar();
            }
        }

        // Need to recreate in postprocessor
        /*
        private static void OnWillCreateAsset(string path)
        {
            TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);

            Debug.Log($"Creating asset: {path} >> {terrainLayer}");

            if (terrainLayer == null || !_terrainLayerToMaterial.ContainsKey(terrainLayer))
                return;

            UpdateLayerMaterialsData(terrainLayer);
        }*/

        private static string[] OnWillSaveAssets(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++) {
                string assetPath = paths[i];
                TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(assetPath);

                Debug.Log(_syncData.TerrainLayerToMaterial.Count);

                if (terrainLayer == null || !_syncData.TerrainLayerToMaterial.ContainsKey(terrainLayer))
                    continue;

                UpdateLayerMaterialsData(terrainLayer);
            }

            return paths;
        }
    }
}
#endif