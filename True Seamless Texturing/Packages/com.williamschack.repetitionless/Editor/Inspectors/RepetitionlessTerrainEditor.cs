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
        private SerializedProperty _autoSaveProp;


        private TerrainData _terrainData;
        private TerrainLayer[] _terrainLayers;

        private TerrainLayerSyncDataSO _syncData;

        private GUIStyle _headerStyle;
        private GUIStyle _headerStyleError;
        private GUIStyle _toggleStyle;

        bool _incorrectMaterial = false;

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

        private void SyncLayersToMaterial()
        {
            // Update global data for terrain layer saving
            _terrainLayers = _terrainData.terrainLayers;
            _syncData.UpdateGlobalMaterialLayers(_main.RepetitionlessMaterial, _terrainLayers);
        }

        // Save textures to the material
        private void UpdateMaterialTerrainLayerTextures()
        {
            if (_main.RepetitionlessMaterial == null)
                return;

            EditorApplication.delayCall += () => {
                // Will only update changed layers
                for (int i = 0; i < _terrainData.terrainLayers.Length; i++)
                    _syncData.UpdateLayerMaterialsData(_terrainData.terrainLayers[i]);
            };
        }

        private void OnEnable()
        {
            _main = (RepetitionlessTerrain)serializedObject.targetObject;
            _syncData = TerrainLayerSyncDataSO.Load();

            _materialProp = serializedObject.FindProperty("_repetitionlessMaterial");
            _autoSaveProp = serializedObject.FindProperty("_autoSaveTextures");

            _terrainData = _main.Terrain.terrainData;
            _terrainLayers = _terrainData.terrainLayers;
            //SyncLayersToMaterial();
            //UpdateMaterialTerrainLayerTextures();

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 14;
            _headerStyle.wordWrap = true;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.normal.textColor = Color.white;

            _headerStyleError = new GUIStyle(_headerStyle);
            _headerStyleError.normal.textColor = new Color(1, 0.4f, 0.4f);
        }

        public override void OnInspectorGUI()
        {   
            serializedObject.Update();

            // Cannot copy GUI.skin styles outside of ongui, make it here
            if (_toggleStyle == null) {
                _toggleStyle = new GUIStyle("button");
                _toggleStyle.fontSize = 12;
                _toggleStyle.fontStyle = FontStyle.Bold;
                _toggleStyle.alignment = TextAnchor.MiddleCenter;
            }

            // Update global layers sync data for layer saving
            TerrainLayer[] newTerrainLayers = _terrainData.terrainLayers;
            if (!TerrainLayersEqual(newTerrainLayers)) {
                SyncLayersToMaterial();
                if (_autoSaveProp.boolValue)
                    UpdateMaterialTerrainLayerTextures();
            }

            if (_main.RepetitionlessMaterial == null) {
                if (_incorrectMaterial) GUILayout.Label("Only Repetitionless terrain materials are accepted", _headerStyleError);
                else GUILayout.Label("Assign a material here to get started", _headerStyle);
            } else {
                GUILayout.Label("Material", _headerStyle);
            }

            // Material Selection
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_materialProp);
            if (EditorGUI.EndChangeCheck()) {
                Material newMat = (Material)_materialProp.objectReferenceValue;
                if (newMat != null) {
                    string newShaderName = newMat.shader.name;

                    Debug.LogWarning("CHANGE ME TO ONLY ALLOW TERRAINS");
                    if (newShaderName.StartsWith("Repetitionless/")) {
                        _incorrectMaterial = false;

                        _syncData.RemoveMaterial(_main.RepetitionlessMaterial);
                        _main.UpdateTerrainMaterial(newMat);

                        EditorApplication.delayCall += () => {
                            SyncLayersToMaterial();
                            UpdateMaterialTerrainLayerTextures();
                            _main.UpdateMaterialTerrainTextures();
                        };
                    } else {
                        _incorrectMaterial = true;
                        _materialProp.objectReferenceValue = _main.RepetitionlessMaterial;
                    }
                } else {
                    _incorrectMaterial = false;
                    _syncData.RemoveMaterial(_main?.RepetitionlessMaterial);
                }
            }

            if (_main.RepetitionlessMaterial != null) {
                // Edit Material Button
                if (GUILayout.Button("Edit Material", GUILayout.Height(30)))
                    Selection.activeObject = _main.RepetitionlessMaterial;

                // Save Texture Layers Button
                GUILayout.Space(10);

                GUILayout.Label("Textures", _headerStyle);
                GUILayout.Space(5);

                Color prevBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _autoSaveProp.boolValue ? Color.green : Color.red;

                _autoSaveProp.boolValue = GUILayout.Toggle(_autoSaveProp.boolValue, "Auto Save Textures", _toggleStyle, GUILayout.Height(30));

                GUI.backgroundColor = prevBackgroundColor;

                if (GUILayout.Button(new GUIContent("Save Textures", "Manually save the data from the terrain layers to the material"), GUILayout.Height(30))) {
                    UpdateMaterialTerrainLayerTextures();
                    _main.UpdateMaterialTerrainTextures();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif