#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    using Data;
    
    [CustomEditor(typeof(RepetitionlessTerrain))]
    public class RepetitionlessTerrainEditor : Editor
    {
        private RepetitionlessTerrain _main;

        private SerializedProperty _materialProp;

        private TerrainData _terrainData;
        private TerrainLayer[] _terrainLayers;

        TerrainLayerSyncDataSO _syncData;

        private void UpdateTerrainLayers()
        {
            _terrainLayers = _terrainData.terrainLayers;

            _syncData.UpdateMaterialLayers(_main.RepetitionlessMaterial, _terrainLayers);
        }

        private void OnEnable()
        {
            _main = (RepetitionlessTerrain)serializedObject.targetObject;
            _syncData = TerrainLayerSyncDataSO.Load();

            _materialProp = serializedObject.FindProperty("_repetitionlessMaterial");

            _terrainData = _main.Terrain.terrainData;
            UpdateTerrainLayers();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Check if a terrain layer was added or removed
            TerrainLayer[] currentTerrainLayers = _terrainData.terrainLayers;
            if (currentTerrainLayers.Length != _terrainLayers.Length)
                UpdateTerrainLayers();

            // Material Selection
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_materialProp);
            if (EditorGUI.EndChangeCheck()) {
                Material newMat = (Material)_materialProp.objectReferenceValue;
                string newShaderName = newMat.shader.name;

                Debug.LogWarning("CHANGE ME TO ONLY ALLOW TERRAINS");
                if (newShaderName.StartsWith("Repetitionless/")) {
                    _syncData.RemoveMaterial(_main.RepetitionlessMaterial);
                    _main.UpdateTerrainMaterial(newMat);
                } else {
                    _materialProp.objectReferenceValue = _main.RepetitionlessMaterial;
                }
            }

            GUILayout.Space(10);

            // Edit Material Button
            if (_main.RepetitionlessMaterial != null &&
                GUILayout.Button("Edit Material", GUILayout.Height(30))) {
                Selection.activeObject = _main.RepetitionlessMaterial;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif