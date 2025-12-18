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

                _syncData.UpdateLayerMaterialsData(terrainLayer);
            }

            return paths;
        }
    }
}
#endif