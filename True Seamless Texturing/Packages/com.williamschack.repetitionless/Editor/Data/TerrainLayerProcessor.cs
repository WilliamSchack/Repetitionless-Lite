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
        private static Dictionary<Material, List<TerrainLayer>> _terrainLayers = new Dictionary<Material, List<TerrainLayer>>();
        private static Dictionary<TerrainLayer, List<Material>> _materials     = new Dictionary<TerrainLayer, List<Material>>();

        private static void DebugDicts()
        {
            // Debug
            Debug.Log("MATERIAL > LAYER");
            foreach (KeyValuePair<Material, List<TerrainLayer>> p in _terrainLayers) {
                string debugString = $"{p.Key} => {{ ";
                foreach (TerrainLayer l in p.Value) {
                    debugString += $"{l.name} "; 
                }
                debugString += "}";

                Debug.Log(debugString);
            }

            Debug.Log("LAYER > MATERIAL");
            foreach (KeyValuePair<TerrainLayer, List<Material>> p in _materials) {
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
            if (_terrainLayers.ContainsKey(mat)) {
                prevTerrainLayers = _terrainLayers[mat];
                _terrainLayers[mat] = layers.ToList();

                if (_terrainLayers[mat].Count == 0)
                    _terrainLayers.Remove(mat);
            } else {
                _terrainLayers.Add(mat, layers.ToList());
                prevTerrainLayers = new List<TerrainLayer>();
            }

            // Remove unused layers
            for (int i = 0; i < prevTerrainLayers.Count; i++) {
                TerrainLayer layer = prevTerrainLayers[i];

                if (layers.Contains(layer))
                    continue;

                _materials[layer].Remove(mat);
                if (_materials[layer].Count == 0)
                    _materials.Remove(layer);
            }

            // Update materials dict
            for (int i = 0; i < layers.Length; i++) {
                TerrainLayer layer = layers[i];

                if (_materials.ContainsKey(layer)) {
                    if (!_materials[layer].Contains(mat)) _materials[layer].Add(mat);
                } else {
                    _materials.Add(layer, new List<Material> { mat });
                }
            }

            // DEBUG
            DebugDicts();
        }

        public static void RemoveMaterial(Material mat)
        {
            if (!_terrainLayers.ContainsKey(mat))
                return;

            List<TerrainLayer> usedTerrainLayers = _terrainLayers[mat];
            _terrainLayers.Remove(mat);

            foreach (TerrainLayer layer in usedTerrainLayers) {
                _materials[layer].Remove(mat);
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

                if (terrainLayer == null || !_materials.ContainsKey(terrainLayer))
                    continue;

                // Update the materials data related to this terrain layer

                foreach (Material mat in _materials[terrainLayer]) {
                    MaterialDataManager materialData = new MaterialDataManager(mat);
                    RepetitionlessTextureDataSO textureData = materialData.LoadAsset<RepetitionlessTextureDataSO>(RepetitionlessGUIBaseNEW.TEXTURE_DATA_FILE_NAME);

                    Debug.LogWarning("GET WHICH LAYER THIS IS");

                    int layer = 0;
                    Debug.Log("UPDATING TEXTURES: " + textureData);
                    ref RepetitionlessTextureDataSO.MaterialTextureData baseTextureData = ref textureData.LayersTextureData[layer].BaseMaterialTextures;
                    baseTextureData.AVTextures[0].Texture  = terrainLayer.diffuseTexture;
                    baseTextureData.NSOTextures[0].Texture = terrainLayer.normalMapTexture;
                    baseTextureData.NSOTextures[3].Texture = terrainLayer.maskMapTexture;
                    baseTextureData.EMTextures[2].Texture  = terrainLayer.maskMapTexture;

                }
            }

            return paths;
        }
    }
}
#endif