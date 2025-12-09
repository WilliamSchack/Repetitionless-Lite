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

        private void UpdateTerrainLayers()
        {
            _terrainLayers = _terrainData.terrainLayers;

            TerrainLayerProcessor.UpdateMaterialLayers(_main.RepetitionlessMaterial, _terrainLayers);
        }

        private void OnEnable()
        {
            _main = (RepetitionlessTerrain)serializedObject.targetObject;

            _materialProp = serializedObject.FindProperty("_repetitionlessMaterial");

            _terrainData = _main.Terrain.terrainData;
            UpdateTerrainLayers();
        }

        private void OnDisable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Check if amount of terrain layers has changed
            TerrainLayer[] currentTerrainLayers = _terrainData.terrainLayers;
            if (currentTerrainLayers.Length != _terrainLayers.Length) {
                Debug.Log("Added / Removed terrain layer");
                UpdateTerrainLayers();
            }

            // Material Selection
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_materialProp);
            if (EditorGUI.EndChangeCheck()) {
                Material newMat = (Material)_materialProp.objectReferenceValue;
                string newShaderName = newMat.shader.name;

                Debug.LogWarning("CHANGE ME TO ONLY ALLOW TERRAINS");
                if (newShaderName.StartsWith("Repetitionless/")) {
                    TerrainLayerProcessor.RemoveMaterial(_main.RepetitionlessMaterial);
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