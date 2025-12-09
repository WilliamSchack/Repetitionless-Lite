#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Data
{
    public class TerrainLayerPostprocessor : AssetPostprocessor
    {
        private static TerrainLayerSyncDataSO _syncDataCache;
        private static TerrainLayerSyncDataSO _syncData {
            get {
                if (_syncDataCache == null)
                    _syncDataCache = TerrainLayerSyncDataSO.Load();

                return _syncDataCache;
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string str in importedAssets)
            {
                TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(str);

                if (terrainLayer == null || !_syncData.TerrainLayerToMaterial.ContainsKey(terrainLayer))
                    continue;

                _syncData.UpdateLayerMaterialsData(terrainLayer);
            }
        }
    }
}
#endif