#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    using Helpers;
    using Data;
    
    [CustomEditor(typeof(RepetitionlessTerrain))]
    public class RepetitionlessTerrainEditor : Editor
    {
        private RepetitionlessTerrain _main;

        private SerializedProperty _materialProp;
        private SerializedProperty _autoSaveProp;
        private SerializedProperty _parentTerrainProp;


        private TerrainData _terrainData;
        private TerrainLayer[] _terrainLayers;
        private Terrain[] _terrainNeighbours = new Terrain[4];

        private TerrainLayerSyncDataSO _syncData;

        private GUIStyle _headerStyle;
        private GUIStyle _headerStyleError;
        private GUIStyle _toggleStyle;

        private bool _incorrectMaterial = false;
        private bool _settingUpParent = false;

        private void HandleUpdatedTerrainNeighbours()
        {
            Terrain[] newTerrainNeighbours = {
                _main.Terrain.leftNeighbor,
                _main.Terrain.rightNeighbor,
                _main.Terrain.topNeighbor,
                _main.Terrain.bottomNeighbor
            };

            for (int i = 0; i < newTerrainNeighbours.Length; i++) {
                Terrain neighbour = newTerrainNeighbours[i];
                if(_terrainNeighbours[i] != neighbour) {
                    _main.SetupNewTerrainNeighbour(neighbour);
                    _terrainNeighbours[i] = neighbour;
                }
            }
        }

        private bool TerrainLayersUpdated()
        {
            if (_terrainData == null)
                return false;

            TerrainLayer[] newTerrainLayers = _terrainData.terrainLayers;

            if (_terrainLayers == null)
                return true;

            if (_terrainLayers.Length != newTerrainLayers.Length)
                return true;

            for (int i = 0; i < _terrainLayers.Length; i++) {
                if (_terrainLayers[i] != newTerrainLayers[i])
                    return true;
            }

            return false;
        }

        private void SyncLayersToMaterial()
        {
            // Update global data for terrain layer saving
            _syncData.UpdateGlobalMaterialLayers(_main.MainMaterial, _terrainData.terrainLayers);

            // If any terrain layers added have the default tiling, change it to the repetitionless default tiling
            Vector2 defaultTerrainLayerTiling   = new Vector2(2, 2);
            Vector2 defaultRepetitionlessTiling = new Vector2(100, 100);

            TerrainLayer[] addedTerrainLayers   = _terrainData.terrainLayers.Except(_terrainLayers).ToArray();
            for (int i = 0; i < addedTerrainLayers.Length; i++) {
                TerrainLayer terrainLayer = addedTerrainLayers[i];
                if (terrainLayer.tileSize == defaultTerrainLayerTiling)
                    terrainLayer.tileSize = defaultRepetitionlessTiling;
            }

            // Update layers
            _terrainLayers = _terrainData.terrainLayers;
        }

        // Save textures to the material
        private void UpdateMaterialTerrainLayerTextures()
        {
            if (_main.MainMaterial == null)
                return;

            EditorApplication.delayCall += () => {
                // Will only update changed layers
                for (int i = 0; i < _terrainData.terrainLayers.Length; i++)
                    _syncData.UpdateLayerMaterialsData(_terrainData.terrainLayers[i], _main.MainMaterial);

                _syncData.RemoveUnusedLayerTextures(_main.MainMaterial);
                _main.UpdateMaterialTerrainTextures();
            };
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            _main = (RepetitionlessTerrain)serializedObject.targetObject;
            _syncData = TerrainLayerSyncDataSO.Load();

            _materialProp      = serializedObject.FindProperty("_mainMaterial");
            _autoSaveProp      = serializedObject.FindProperty("AutoSaveTextures");
            _parentTerrainProp = serializedObject.FindProperty("ParentTerrain");

            _terrainData   = _main.Terrain.terrainData;
            _terrainLayers = _terrainData.terrainLayers;

            _terrainNeighbours[0] = _main.Terrain.leftNeighbor;
            _terrainNeighbours[1] = _main.Terrain.rightNeighbor;
            _terrainNeighbours[2] = _main.Terrain.topNeighbor;
            _terrainNeighbours[3] = _main.Terrain.bottomNeighbor;

            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 14;
            _headerStyle.wordWrap = true;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.normal.textColor = Color.white;

            _headerStyleError = new GUIStyle(_headerStyle);
            _headerStyleError.normal.textColor = new Color(1, 0.4f, 0.4f);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        // Check in the editor since we only care when we are editing this terrain
        private void OnUndoRedo()
        {
            // Hole texture tends to break on undo, may aswell update other textures aswell incase anything happens
            _main.UpdateMaterialTerrainTextures();
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
            if (TerrainLayersUpdated() && !_settingUpParent) {
                if (_main.ParentTerrain != null)
                    _parentTerrainProp.objectReferenceValue = null;

                SyncLayersToMaterial();
                if (_autoSaveProp.boolValue)
                    UpdateMaterialTerrainLayerTextures();

                _main.OnTerrainLayersChanged?.Invoke(_terrainData.terrainLayers);
            }

            // Add scripts to terrain neighbours if added
            HandleUpdatedTerrainNeighbours();

            if (_main.MainMaterial == null) DrawNoMaterialGUI();
            else DrawAssignedMaterialGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMaterialProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_materialProp);
            if (EditorGUI.EndChangeCheck()) {
                Material newMat = (Material)_materialProp.objectReferenceValue;
                if (newMat != null) {
                    string newShaderName = newMat.shader.name;
                    
                    if (newShaderName.StartsWith("Repetitionless/") && newShaderName.Contains("Terrain")) {
                        _incorrectMaterial = false;
                        _parentTerrainProp.objectReferenceValue = null;
                        _autoSaveProp.boolValue = true;

                        _syncData.RemoveMaterial(_main.MainMaterial);
                        _main.UpdateTerrainMaterial(newMat);

                        UpdateMaterialTerrainLayerTextures();
                        EditorApplication.delayCall += SyncLayersToMaterial;
                    } else {
                        _incorrectMaterial = true;
                        _materialProp.objectReferenceValue = _main.MainMaterial;
                    }
                } else {
                    _incorrectMaterial = false;
                    _parentTerrainProp.objectReferenceValue = null;
                    _autoSaveProp.boolValue = true;

                    _syncData.RemoveMaterial(_main?.MainMaterial);
                }
            }
        }

        private void DrawParentProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_parentTerrainProp);
            if (EditorGUI.EndChangeCheck()) {
                RepetitionlessTerrain newTerrain = (RepetitionlessTerrain)_parentTerrainProp.objectReferenceValue;
                if (newTerrain != null) {
                    bool assigning = true;
                    if (_materialProp.objectReferenceValue != null)
                        assigning = EditorUtility.DisplayDialog("Assigning terrain parent", "This will overwrite the current material and terrain layers with the ones in the assigned terrain, do you want to continue?", "Continue", "Cancel");
                    
                    if (!assigning) {
                        _parentTerrainProp.objectReferenceValue = _main.ParentTerrain;
                    } else {
                        _settingUpParent = true;
                        newTerrain.SetupNewTerrainNeighbour(_main.Terrain);
                        _main.UpdateParentCallback(newTerrain);

                        _materialProp.objectReferenceValue = _main.MainMaterial;
                        EditorApplication.delayCall += _main.UpdateMaterialTerrainTextures;
                    }
                } else {
                    _autoSaveProp.boolValue = true;
                }
            }
        }

        private void DrawIncorrectMaterialWarning()
        {
            if (!_incorrectMaterial)
                return;
            
            EditorGUILayout.HelpBox("Only Repetitionless terrain materials are accepted", MessageType.Warning);
            if (GUILayout.Button("Dismiss")) _incorrectMaterial = false;
        }

        private void DrawNoMaterialGUI()
        {
            GUILayout.Label("Assign a material or parent terrain to get started", _headerStyle);
            DrawIncorrectMaterialWarning();

            GUILayout.Space(10);

            DrawMaterialProperty();
            EditorGUILayout.HelpBox("If you want this terrain to use its own set of textures", MessageType.Info);

            GUILayout.Space(10);

            DrawParentProperty();
            EditorGUILayout.HelpBox("If you want to take the material and textures from another terrain\n(Should be used if using the same material as another terrain)", MessageType.Info);
        }

        private void DrawAssignedMaterialGUI()
        {
            GUILayout.Label("Material", _headerStyle);
            DrawIncorrectMaterialWarning();

            GUILayout.Space(10);

            DrawMaterialProperty();

            // Edit Material Button
            if (GUILayout.Button("Edit Material", GUILayout.Height(30)))
                Selection.activeObject = _main.MainMaterial;

            // Save Texture Layers Button
            GUILayout.Space(10);

            GUILayout.Label("Textures", _headerStyle);
            GUILayout.Space(5);

            // Terrain Parent
            if (_settingUpParent) _settingUpParent = false;
            DrawParentProperty();

            if (_parentTerrainProp.objectReferenceValue != null) {
                EditorGUILayout.HelpBox("This terrain is taking the material and terrain layers from the parent terrain assigned above. Change the terrain layers, material, or remove the parent to use this terrains textures", MessageType.Info);
            } else {
                GUILayout.Space(10);
                
                Color prevBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _autoSaveProp.boolValue ? Color.green : Color.red;

                _autoSaveProp.boolValue = GUILayout.Toggle(_autoSaveProp.boolValue, "Auto Save Textures", _toggleStyle, GUILayout.Height(30));

                GUI.backgroundColor = prevBackgroundColor;

                if (GUILayout.Button(new GUIContent("Save Textures", "Manually save the data from the terrain layers to the material"), GUILayout.Height(30))) {
                    // Incase the material was changed to something different
                    if (_main.Terrain.materialTemplate != _main.MaterialInstance)
                        _main.UpdateTerrainMaterial(_main.MainMaterial);

                    SyncLayersToMaterial();
                    UpdateMaterialTerrainLayerTextures();
                }
            }
        }
    }
}
#endif