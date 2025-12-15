using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Repetitionless.Data
{
    using Inspectors;
    using Variables;

    public class TerrainLayerSyncDataSO : ScriptableObject
    {
        private const string ASSET_PATH = "Packages/com.williamschack.repetitionless/Data/TerrainLayerSyncData.asset";

        // Use wrappers to allow serialization
        // Bit stupid but it works
        [System.Serializable] public class TerrainLayerList { [SerializeField] public List<TerrainLayer> Items = new List<TerrainLayer>(); }
        [System.Serializable] public class MaterialList     { [SerializeField] public List<Material> Items = new List<Material>(); }

        public SerializableDictionary<Material, TerrainLayerList> MaterialToTerrainLayer = new SerializableDictionary<Material, TerrainLayerList>();
        public SerializableDictionary<TerrainLayer, MaterialList> TerrainLayerToMaterial = new SerializableDictionary<TerrainLayer, MaterialList>();

    #if UNITY_EDITOR
        private static TerrainLayerSyncDataSO Create()
        {
            TerrainLayerSyncDataSO asset = CreateInstance<TerrainLayerSyncDataSO>();
            AssetDatabase.CreateAsset(asset, ASSET_PATH);

            return asset;
        } 

        public static TerrainLayerSyncDataSO Load()
        {
            TerrainLayerSyncDataSO asset = AssetDatabase.LoadAssetAtPath<TerrainLayerSyncDataSO>(ASSET_PATH);
            if (asset == null) asset = Create();

            return asset;
        }
    #endif

        public void Save()
        {
    #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
    #endif
        }

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

                Debug.Log("UPDATING DATA");

                string progressBarTitle = $"Updating {mat.name}...";
                EditorUtility.DisplayProgressBar(progressBarTitle, "Setting up", 0.0f);

                // Wrapping in try to prevent infinite progress bar on an error
                try {
                    MaterialDataManager materialData = new MaterialDataManager(mat);
                    RepetitionlessMaterialDataSO materialProperties = materialData.LoadAsset<RepetitionlessMaterialDataSO>(RepetitionlessMaterialEditorBaseNEW.PROPERTIES_FILE_NAME);
                    RepetitionlessTextureDataSO  textureData        = materialData.LoadAsset<RepetitionlessTextureDataSO>(RepetitionlessMaterialEditorBaseNEW.TEXTURE_DATA_FILE_NAME);

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

        public void RemoveUnusedLayerTextures(Material material)
        {
            MaterialDataManager materialData = new MaterialDataManager(material);
            RepetitionlessTextureDataSO textureData = materialData.LoadAsset<RepetitionlessTextureDataSO>(RepetitionlessMaterialEditorBaseNEW.TEXTURE_DATA_FILE_NAME);

            // Check if the count differs
            // Only handles if textures need to be removed
            List<TerrainLayer> terrainLayers = MaterialToTerrainLayer.Get(material)?.Items;
            if (terrainLayers == null) return;

            textureData.SetupTextureDrawers(materialData);
            bool avHasArray  = textureData.AVTexturesDrawer.Array  == null;
            bool nsoHasArray = textureData.NSOTexturesDrawer.Array == null;
            bool emHasArray  = textureData.EMTexturesDrawer.Array  == null;

            if (!avHasArray && !nsoHasArray && !emHasArray) return;

            // AV Array
            int arrayDepth = textureData.AVTexturesDrawer.Array.depth;
            if (terrainLayers.Count >= arrayDepth)
                return;

            for (int i = terrainLayers.Count; i < arrayDepth; i++) {
                Debug.Log("REMOVING: " + i);
                textureData.AVTexturesDrawer.RemoveArrayLayer(i*3+0);
                textureData.AVTexturesDrawer.RemoveArrayLayer(i*3+1);
                textureData.AVTexturesDrawer.RemoveArrayLayer(i*3+2);

                //textureData.LayersTextureData[i].BaseMaterialTextures.AVTextures[0].Texture = null;
                //textureData.LayersTextureData[i].BaseMaterialTextures.AVTextures[1].Texture = null;
            }

            Debug.Log(textureData.LayersTextureData[1].BaseMaterialTextures.AVTextures[0].Texture);

            // NSOArray

            // EMArray
        }
    }
}