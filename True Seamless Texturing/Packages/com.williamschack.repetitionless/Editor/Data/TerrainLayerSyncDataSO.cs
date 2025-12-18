using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Data
{
    using Variables;
    using GUIUtilities;

    /// <summary>
    /// Syncs the terrain layers from the terrains to the materials<br />
    /// Only one of this asset should exist
    /// </summary>
    public class TerrainLayerSyncDataSO : ScriptableObject
    {
        // Use wrappers to allow serialization with my jank dictionary implementation
        // Bit stupid but it works

        /// <summary>
        /// List of terrain layers<br />
        /// Using a wrapper to allow serialization
        /// </summary>
        [System.Serializable] public class TerrainLayerList {
            /// <summary>
            /// The list of terrain layers
            /// </summary>
            [SerializeField] public List<TerrainLayer> Items = new List<TerrainLayer>();
        }

        /// <summary>
        /// List of materials<br />
        /// Using a wrapper to allow serialization
        /// </summary>
        [System.Serializable] public class MaterialList {
            /// <summary>
            /// The list of materials
            /// </summary>
            [SerializeField] public List<Material> Items = new List<Material>();
        }

        private const string ASSET_PATH = Constants.PACKAGE_PATH + "/Data/TerrainLayerSyncData.asset";

        /// <summary>
        /// Material to terrain layers dictionary
        /// </summary>
        public SerializableDictionary<Material, TerrainLayerList> MaterialToTerrainLayer = new SerializableDictionary<Material, TerrainLayerList>();
        /// <summary>
        /// Terrain layer to materials dictionary
        /// </summary>
        public SerializableDictionary<TerrainLayer, MaterialList> TerrainLayerToMaterial = new SerializableDictionary<TerrainLayer, MaterialList>();

        private static TerrainLayerSyncDataSO Create()
        {
            TerrainLayerSyncDataSO asset = CreateInstance<TerrainLayerSyncDataSO>();
            AssetDatabase.CreateAsset(asset, ASSET_PATH);

            return asset;
        } 

        /// <summary>
        /// Loads the sync data instance
        /// </summary>
        /// <returns>
        /// The sync data object
        /// </returns>
        public static TerrainLayerSyncDataSO Load()
        {
            TerrainLayerSyncDataSO asset = AssetDatabase.LoadAssetAtPath<TerrainLayerSyncDataSO>(ASSET_PATH);
            if (asset == null) asset = Create();

            return asset;
        }

        /// <summary>
        /// Saves this object
        /// </summary>
        public void Save()
        {
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Updates the synced material and terrain layers, overriding the layers with the new ones for this material
        /// </summary>
        /// <param name="mat">
        /// The material to update
        /// </param>
        /// <param name="layers">
        /// The new terrain layers
        /// </param>
        public void UpdateGlobalMaterialLayers(Material mat, TerrainLayer[] layers)
        {
            if (mat == null)
                return;

            List<TerrainLayer> prevTerrainLayers;

            // Update terrain layers dict
            if (MaterialToTerrainLayer.ContainsKey(mat)) {
                prevTerrainLayers = MaterialToTerrainLayer.Get(mat).Items;
                MaterialToTerrainLayer.Set(mat, new TerrainLayerList() { Items = layers.ToList() });
                
                if (MaterialToTerrainLayer.Get(mat).Items.Count == 0)
                    MaterialToTerrainLayer.Remove(mat);
            } else {
                MaterialToTerrainLayer.Set(mat, new TerrainLayerList() { Items = layers.ToList() });
                prevTerrainLayers = new List<TerrainLayer>();
            }

            // Remove unused layers
            for (int i = 0; i < prevTerrainLayers.Count; i++) {
                TerrainLayer layer = prevTerrainLayers[i];

                if (layers.Contains(layer))
                    continue;

                TerrainLayerToMaterial.Get(layer).Items.Remove(mat);
                if (TerrainLayerToMaterial.Get(layer).Items.Count == 0)
                    TerrainLayerToMaterial.Remove(layer);
            }

            // Update materials dict
            for (int i = 0; i < layers.Length; i++) {
                TerrainLayer layer = layers[i];

                if (TerrainLayerToMaterial.ContainsKey(layer)) {
                    if (!TerrainLayerToMaterial.Get(layer).Items.Contains(mat)) TerrainLayerToMaterial.Get(layer).Items.Add(mat);
                } else {
                    TerrainLayerToMaterial.Set(layer, new MaterialList { Items = new List<Material> { mat } });
                }
            }

            Save();
        }

        /// <summary>
        /// Removes a material from the lists
        /// </summary>
        /// <param name="mat">
        /// The material to remove
        /// </param>
        public void RemoveMaterial(Material mat)
        {
            if (!MaterialToTerrainLayer.ContainsKey(mat))
                return;

            List<TerrainLayer> usedTerrainLayers = MaterialToTerrainLayer.Get(mat).Items;
            MaterialToTerrainLayer.Remove(mat);

            foreach (TerrainLayer layer in usedTerrainLayers) {
                TerrainLayerToMaterial.Get(layer).Items.Remove(mat);

                if (TerrainLayerToMaterial.Get(layer).Items.Count == 0)
                    TerrainLayerToMaterial.Remove(layer);
            }

            Save();
        }

        /// <summary>
        /// Updates the material properties and texture data for a specific terrain layer
        /// </summary>
        /// <param name="terrainLayer">
        /// The terrain layer to use
        /// </param>
        /// <param name="material">
        /// Used to only update a specific material
        /// </param>
        public void UpdateLayerMaterialsData(TerrainLayer terrainLayer, Material material = null)
        {
            if (!TerrainLayerToMaterial.ContainsKey(terrainLayer))
                return;

            bool nullMaterialFound = false;

            // Update the materials data related to this terrain layer
            foreach (Material mat in TerrainLayerToMaterial.Get(terrainLayer).Items) {
                if (material != null && mat != material)
                    continue;

                if (mat == null) {
                    nullMaterialFound = true;
                    continue;
                }

                string progressBarTitle = $"Updating {mat.name}...";
                EditorUtility.DisplayProgressBar(progressBarTitle, "Setting up", 0.0f);

                // Wrapping in try to prevent infinite progress bar on an error
                try {
                    MaterialDataManager materialData = new MaterialDataManager(mat);
                    RepetitionlessMaterialDataSO materialProperties = materialData.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
                    RepetitionlessTextureDataSO  textureData        = materialData.LoadAsset<RepetitionlessTextureDataSO>(Constants.TEXTURE_DATA_FILE_NAME);

                    if (materialProperties == null || textureData == null) {
                        Debug.LogError($"Could not find properties or textures for material {mat.name}");
                        EditorUtility.ClearProgressBar();
                        continue;
                    }

                    // Setup data
                    materialProperties.SetDataManager(materialData);
                    textureData.SetupTextureDrawers(materialData);

                    // Get the layer index
                    int layerIndex = MaterialToTerrainLayer.Get(mat).Items.IndexOf(terrainLayer);
                    if (layerIndex >= materialProperties.Data.Length ||
                        layerIndex >= textureData.LayersTextureData.Length) {
                        EditorUtility.ClearProgressBar();
                        continue;
                    }

                    // Update properties
                    RepetitionlessMaterialData baseMaterialData = materialProperties.Data[layerIndex].BaseMaterialData;
                    baseMaterialData.NormalScale  = terrainLayer.normalScale;
                    baseMaterialData.TilingOffset = new Vector4(terrainLayer.tileSize.x, terrainLayer.tileSize.y, terrainLayer.tileOffset.x, terrainLayer.tileOffset.y);

                    EditorUtility.DisplayProgressBar(progressBarTitle, "Updating Textures", 0.2f);

                    // Update diffuse, normal textures
                    int arrayLayerIndex = layerIndex * Constants.MATERIALS_PER_LAYER_COUNT + 0; // Using base material
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
                        textureData.GetTextureData(layerIndex, 0, 2)[2].Texture = terrainLayer.maskMapTexture;

                        textureData.UpdatePackedTexture(layerIndex, 0, true);
                    }

                    EditorUtility.DisplayProgressBar(progressBarTitle, "Updating Properties", 0.8f);

                    // Save changed properties
                    materialProperties.UpdateAssignedTextures(mat, textureData, 0, layerIndex);

                    materialProperties.Save();
                    textureData.Save();
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }

                EditorUtility.ClearProgressBar();
            }

            if (nullMaterialFound) RemoveMaterial(null);
        }

        /// <summary>
        /// Removes any unused terrain layers for a material
        /// </summary>
        /// <param name="material">
        /// The material to update
        /// </param>
        public void RemoveUnusedLayerTextures(Material material)
        {
            MaterialDataManager materialData = new MaterialDataManager(material);
            RepetitionlessTextureDataSO textureData = materialData.LoadAsset<RepetitionlessTextureDataSO>(Constants.TEXTURE_DATA_FILE_NAME);

            // Check if the count differs
            // Only handles if textures need to be removed
            List<TerrainLayer> terrainLayers = MaterialToTerrainLayer.Get(material)?.Items;
            if (terrainLayers == null) return;

            textureData.SetupTextureDrawers(materialData);
            RemoveArrayLayer(textureData.AVTexturesDrawer,  textureData, terrainLayers.Count);
            RemoveArrayLayer(textureData.NSOTexturesDrawer, textureData, terrainLayers.Count);
            RemoveArrayLayer(textureData.EMTexturesDrawer,  textureData, terrainLayers.Count);
        }

        private void RemoveArrayLayer(TextureArrayCustomChannelsGUIDrawer arrayDrawer, RepetitionlessTextureDataSO textureData, int terrainLayersCount)
        {
            if (arrayDrawer == null || arrayDrawer.Array == null)
                return;

            int arrayDepth = arrayDrawer.Array.depth;
            if (terrainLayersCount >= arrayDepth)
                return;

            for (int i = terrainLayersCount; i < arrayDepth; i++) {
                textureData.AVTexturesDrawer.RemoveArrayLayer(i * Constants.MATERIALS_PER_LAYER_COUNT + 0);
                textureData.AVTexturesDrawer.RemoveArrayLayer(i * Constants.MATERIALS_PER_LAYER_COUNT + 1);
                textureData.AVTexturesDrawer.RemoveArrayLayer(i * Constants.MATERIALS_PER_LAYER_COUNT + 2);
            }
        }
    }
}