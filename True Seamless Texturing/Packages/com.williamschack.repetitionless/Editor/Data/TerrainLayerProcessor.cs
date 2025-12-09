#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Data
{
    using Inspectors;

    public class TerrainLayerProcessor : AssetModificationProcessor
    {
        private static Dictionary<Material, List<TerrainLayer>> _materialToTerrainLayer = new Dictionary<Material, List<TerrainLayer>>();
        private static Dictionary<TerrainLayer, List<Material>> _terrainLayerToMaterial = new Dictionary<TerrainLayer, List<Material>>();

        private static void DebugDicts()
        {
            // Debug
            Debug.Log("MATERIAL > LAYER");
            foreach (KeyValuePair<Material, List<TerrainLayer>> p in _materialToTerrainLayer) {
                string debugString = $"{p.Key} => {{ ";
                foreach (TerrainLayer l in p.Value) {
                    debugString += $"{l.name} "; 
                }
                debugString += "}";

                Debug.Log(debugString);
            }

            Debug.Log("LAYER > MATERIAL");
            foreach (KeyValuePair<TerrainLayer, List<Material>> p in _terrainLayerToMaterial) {
                string debugString = $"{p.Key} => {{ ";
                foreach (Material m in p.Value) {
                    debugString += $"{m.name} "; 
                }
                debugString += "}";

                Debug.Log(debugString);
            }
        }

        public static void UpdateMaterialLayers(Material mat, TerrainLayer[] layers)
        {
            List<TerrainLayer> prevTerrainLayers;

            // Update terrain layers dict
            if (_materialToTerrainLayer.ContainsKey(mat)) {
                prevTerrainLayers = _materialToTerrainLayer[mat];
                _materialToTerrainLayer[mat] = layers.ToList();

                if (_materialToTerrainLayer[mat].Count == 0)
                    _materialToTerrainLayer.Remove(mat);
            } else {
                _materialToTerrainLayer.Add(mat, layers.ToList());
                prevTerrainLayers = new List<TerrainLayer>();
            }

            // Remove unused layers
            for (int i = 0; i < prevTerrainLayers.Count; i++) {
                TerrainLayer layer = prevTerrainLayers[i];

                if (layers.Contains(layer))
                    continue;

                _terrainLayerToMaterial[layer].Remove(mat);
                if (_terrainLayerToMaterial[layer].Count == 0)
                    _terrainLayerToMaterial.Remove(layer);
            }

            // Update materials dict
            for (int i = 0; i < layers.Length; i++) {
                TerrainLayer layer = layers[i];

                if (_terrainLayerToMaterial.ContainsKey(layer)) {
                    if (!_terrainLayerToMaterial[layer].Contains(mat)) _terrainLayerToMaterial[layer].Add(mat);
                } else {
                    _terrainLayerToMaterial.Add(layer, new List<Material> { mat });
                }
            }

            // DEBUG
            //DebugDicts();
        }

        public static void RemoveMaterial(Material mat)
        {
            if (!_materialToTerrainLayer.ContainsKey(mat))
                return;

            List<TerrainLayer> usedTerrainLayers = _materialToTerrainLayer[mat];
            _materialToTerrainLayer.Remove(mat);

            foreach (TerrainLayer layer in usedTerrainLayers) {
                _terrainLayerToMaterial[layer].Remove(mat);
            }

            // DEBUG
            //DebugDicts();
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++) {
                string assetPath = paths[i];
                TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(assetPath);
                Debug.Log($"Saved: {terrainLayer}");

                if (terrainLayer == null || !_terrainLayerToMaterial.ContainsKey(terrainLayer))
                    continue;

                // Update the materials data related to this terrain layer

                foreach (Material mat in _terrainLayerToMaterial[terrainLayer]) {
                    MaterialDataManager materialData = new MaterialDataManager(mat);
                    RepetitionlessMaterialDataSO materialProperties = materialData.LoadAsset<RepetitionlessMaterialDataSO>(RepetitionlessGUIBaseNEW.PROPERTIES_FILE_NAME);
                    RepetitionlessTextureDataSO  textureData        = materialData.LoadAsset<RepetitionlessTextureDataSO>(RepetitionlessGUIBaseNEW.TEXTURE_DATA_FILE_NAME);

                    if (materialProperties == null || textureData == null) {
                        Debug.LogError($"Could not find properties or textures for material {mat.name}");
                        continue;
                    }

                    // Setup data
                    materialProperties.SetDataManager(materialData);
                    textureData.SetupTextureDrawers(materialData);

                    // Get the layer index
                    int layerIndex = _materialToTerrainLayer[mat].IndexOf(terrainLayer);

                    // Update properties
                    materialProperties.Data.BaseMaterialData.NormalScale  = terrainLayer.normalScale;
                    materialProperties.Data.BaseMaterialData.TilingOffset = new Vector4(terrainLayer.tileSize.x, terrainLayer.tileSize.y, terrainLayer.tileOffset.x, terrainLayer.tileOffset.y);

                    // Update diffuse, normal textures
                    int arrayLayerIndex = layerIndex * 3 + 0; // Using base material
                    textureData.AVTexturesDrawer.UpdateTexture(terrainLayer.diffuseTexture, arrayLayerIndex, 0, true);
                    textureData.NSOTexturesDrawer.UpdateTexture(terrainLayer.normalMapTexture, arrayLayerIndex, 0, true);

                    // Update packed textures
                    materialProperties.Data.BaseMaterialData.PackedTexture = true;

                    ref RepetitionlessTextureDataSO.MaterialTextureData baseTextureData = ref textureData.LayersTextureData[layerIndex].BaseMaterialTextures;
                    baseTextureData.NSOTextures[3].Texture = terrainLayer.maskMapTexture;
                    baseTextureData.EMTextures[2].Texture  = terrainLayer.maskMapTexture;

                    textureData.UpdatePackedTexture(layerIndex, 0, true);

                    // Save changed properties
                    materialProperties.UpdateAssignedTextures(mat, textureData, 0, layerIndex);

                    materialProperties.Save();
                    textureData.Save();
                }
            }

            return paths;
        }
    }
}
#endif