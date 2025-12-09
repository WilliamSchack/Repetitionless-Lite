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

        private TerrainLayerSyncDataSO _syncData;

        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;

        private bool TerrainLayersEqual(TerrainLayer[] newTerrainLayers)
        {
            if (_terrainLayers == null)
                return false;

            if (_terrainLayers.Length != newTerrainLayers.Length)
                return false;

            for (int i = 0; i < _terrainLayers.Length; i++) {
                if (_terrainLayers[i] != newTerrainLayers[i])
                    return false;
            }

            return true;
        }

        private void UpdateTerrainLayers()
        {
            _terrainLayers = _terrainData.terrainLayers;
            _syncData.UpdateMaterialLayers(_main.RepetitionlessMaterial, _terrainLayers);

            if (_main.RepetitionlessMaterial == null)
                return;

            EditorApplication.delayCall += () => {
                for (int i = 0; i < _terrainData.terrainLayers.Length; i++)
                    _syncData.UpdateLayerMaterialsData(_terrainData.terrainLayers[i]);
            };
        }

        private void OnEnable()
        {
            _main = (RepetitionlessTerrain)serializedObject.targetObject;
            _syncData = TerrainLayerSyncDataSO.Load();

            _materialProp = serializedObject.FindProperty("_repetitionlessMaterial");

            _terrainData = _main.Terrain.terrainData;
            UpdateTerrainLayers();

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 14;
            _headerStyle.wordWrap = true;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.normal.textColor = Color.white;

            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 12;
            _labelStyle.wordWrap = true;
            _labelStyle.alignment = TextAnchor.MiddleCenter;
            _labelStyle.normal.textColor = Color.white;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Update global layers sync data for layer saving
            TerrainLayer[] newTerrainLayers = _terrainData.terrainLayers;
            if (!TerrainLayersEqual(newTerrainLayers))
                UpdateTerrainLayers();

            if (_main.RepetitionlessMaterial == null) {
                GUILayout.Label("Assign the material here to get started", _headerStyle);
                GUILayout.Space(10);
            }

            // Material Selection
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_materialProp);
            if (EditorGUI.EndChangeCheck()) {
                Material newMat = (Material)_materialProp.objectReferenceValue;
                if (newMat != null) {
                    string newShaderName = newMat.shader.name;

                    Debug.LogWarning("CHANGE ME TO ONLY ALLOW TERRAINS");
                    if (newShaderName.StartsWith("Repetitionless/")) {
                        _syncData.RemoveMaterial(_main.RepetitionlessMaterial);
                        _main.UpdateTerrainMaterial(newMat);
                        
                        EditorApplication.delayCall += UpdateTerrainLayers;
                    } else {
                        _materialProp.objectReferenceValue = _main.RepetitionlessMaterial;
                    }
                } else {
                    _syncData.RemoveMaterial(_main?.RepetitionlessMaterial);
                }
            }

            if (_main.RepetitionlessMaterial != null) {
                // Edit Material Button
                if (GUILayout.Button("Edit Material", GUILayout.Height(30)))
                    Selection.activeObject = _main.RepetitionlessMaterial;

                // Save Texture Layers Button
                GUILayout.Space(10);
                GUILayout.Label("Textures automatically update but click here if they have not", _labelStyle);

                if (GUILayout.Button("Save Textures", GUILayout.Height(30))) {
                    for (int i = 0; i < _terrainData.terrainLayers.Length; i++)
                        _syncData.UpdateLayerMaterialsData(_terrainData.terrainLayers[i]);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif