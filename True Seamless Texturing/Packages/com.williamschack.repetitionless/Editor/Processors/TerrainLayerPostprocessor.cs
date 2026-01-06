#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Editor.Processors
{
    using Data;

    public class TerrainLayerPostprocessor : AssetModificationProcessor
    {
        private static TerrainLayerSyncDataSO _syncDataCache;
        private static TerrainLayerSyncDataSO _syncData {
            get {
                if (_syncDataCache == null)
                    _syncDataCache = TerrainLayerSyncDataSO.Load();

                return _syncDataCache;
            }
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths) {
                TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);

                if (terrainLayer == null || !_syncData.TerrainLayerToMaterial.ContainsKey(terrainLayer))
                    continue;

                UpdateAllRepetitionlessDataUsingLayer(terrainLayer);
            }

            return paths;
        }

        private static void UpdateAllRepetitionlessDataUsingLayer(TerrainLayer layer)
        {
            string[] layerSOGuids = AssetDatabase.FindAssets("t:RepetitionlessTerrainLayersSO", new string[] { "Assets" });

            foreach (string guid in layerSOGuids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == "") continue;

                RepetitionlessTerrainLayersSO so = AssetDatabase.LoadAssetAtPath<RepetitionlessTerrainLayersSO>(path);
                if (so == null || so.TerrainLayers == null) continue;

                // Update data
                if (so.TerrainLayers.Contains(layer))
                    so.UpdateLayerMaterialData(layer);
            }
        }
    }
}
#endif