using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Repetitionless.Data
{
    using Variables;

    public class TerrainLayerSyncDataSO : ScriptableObject
    {
        // Use wrappers to allow serialization
        // Bit stupid but it works
        [System.Serializable] public class TerrainLayerList { [SerializeField] public List<TerrainLayer> Items = new List<TerrainLayer>(); }
        [System.Serializable] public class MaterialList     { [SerializeField] public List<Material> Items = new List<Material>(); }

        private const string ASSET_PATH = "Packages/com.williamschack.repetitionless/Data/TerrainLayerSyncData.asset";

        [SerializeField] public SerializableDictionary<Material, TerrainLayerList> MaterialToTerrainLayer = new SerializableDictionary<Material, TerrainLayerList>();
        [SerializeField] public SerializableDictionary<TerrainLayer, MaterialList> TerrainLayerToMaterial = new SerializableDictionary<TerrainLayer, MaterialList>();

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
            AssetDatabase.SaveAssetIfDirty(this);
            AssetDatabase.Refresh();
    #endif
        }

        public void UpdateMaterialLayers(Material mat, TerrainLayer[] layers)
        {
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
            }

            Save();
        }
    }
}