#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime;

namespace Repetitionless.Editor.Inspectors
{
    using Data;
    
    /// <summary>
    /// The editor for the repetitionless terrain component
    /// </summary>
    [CustomEditor(typeof(RepetitionlessTerrain))]
    public class RepetitionlessTerrainEditor : UnityEditor.Editor
    {
        private RepetitionlessTerrain _main;

        private SerializedProperty _materialProp;
        private SerializedProperty _autoSaveProp;
        private SerializedProperty _parentTerrainProp;


        private TerrainData _terrainData;
        private TerrainLayer[] _terrainLayers;
        private Terrain[] _terrainNeighbours = new Terrain[4];

        private RepetitionlessTerrainDataSO _materialTerrainData;
        private RepetitionlessTextureDataSO _materialTextureData;
        private RepetitionlessMaterialDataSO _materialProperties;

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
            if (_terrainData == null)
                return;

            // Update global data for terrain layer saving
            _materialTerrainData.UpdateTerrainLayers(_terrainData.terrainLayers);

            // Update layers
            _terrainLayers = _terrainData.terrainLayers;
        }

        // Save textures to the material
        private void UpdateMaterialTerrainLayerTextures(bool forceUpdate = false)
        {
            EditorApplication.delayCall += () => {
                if (_main.MainMaterial == null || _terrainData == null)
                    return;

                // Will only update changed layers
                for (int i = 0; i < _terrainData.terrainLayers.Length; i++)
                    _materialTerrainData.UpdateLayerMaterialData(_terrainData.terrainLayers[i], forceUpdate);

                _main.UpdateMaterialTerrainTextures();
            };
        }

        private void UpdateMaterialTerrainTextures()
        {
            _main.UpdateMaterialTerrainTextures();
        }

        private void GetMaterialTerrainLayersData(Material mat)
        {
            if (_materialTextureData != null) _materialTextureData.OnDataChanged -= UpdateMaterialTerrainTextures;
            if (_materialProperties != null)  _materialProperties.OnExternalDataChanged  -= UpdateMaterialTerrainTextures;

            if (mat == null) {
                _materialTerrainData = null;
                _materialTextureData = null;
                _materialProperties  = null;
                return;
            }

            MaterialDataManager dataManager = new MaterialDataManager(mat);
            _materialTerrainData = dataManager.LoadAsset<RepetitionlessTerrainDataSO>(Constants.TERRAIN_DATA_FILE_NAME);
            _materialTextureData = dataManager.LoadAsset<RepetitionlessTextureDataSO>(Constants.TEXTURE_DATA_FILE_NAME);
            _materialProperties  = dataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);

            _materialTextureData.OnDataChanged += UpdateMaterialTerrainTextures;
            _materialProperties.OnExternalDataChanged  += UpdateMaterialTerrainTextures;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= UpdateMaterialTerrainTextures;
            Undo.undoRedoPerformed += UpdateMaterialTerrainTextures;

            _main = (RepetitionlessTerrain)serializedObject.targetObject;
            GetMaterialTerrainLayersData(_main.MainMaterial);

            _materialProp      = serializedObject.FindProperty("_mainMaterial");
            _autoSaveProp      = serializedObject.FindProperty("AutoSaveTextures");
            _parentTerrainProp = serializedObject.FindProperty("ParentTerrain");

            if (_main.Terrain.terrainData != null) {
                _terrainData   = _main.Terrain.terrainData;
                _terrainLayers = _terrainData.terrainLayers;
            }

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
            Undo.undoRedoPerformed -= UpdateMaterialTerrainTextures;
        }

        /// <summary>
        /// Base OnGUI function
        /// </summary>
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
                bool updatingLayers = true;
                if (_main.ParentTerrain != null)
                    updatingLayers = EditorUtility.DisplayDialog("Updating Terrain Layers", "This will remove the parent from this terrain, do you want to continue?", "Continue", "Cancel");

                if (updatingLayers) {
                    _parentTerrainProp.objectReferenceValue = null;

                    SyncLayersToMaterial();
                    if (_autoSaveProp.boolValue && _materialTerrainData.AutoSyncLayers)
                        UpdateMaterialTerrainLayerTextures();

                    _main.OnTerrainLayersChanged?.Invoke(_terrainData.terrainLayers);
                } else {
                    // Set terrain layers back to previous
                    _terrainData.terrainLayers = _terrainLayers;
                }
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

                        if (_materialTerrainData != null)
                            _materialTerrainData.ClearTerrainLayers();
                        GetMaterialTerrainLayersData(newMat);

                        _main.UpdateTerrainMaterial(newMat, false);

                        // Assign textures after a frame so the material is properly assigned
                        EditorApplication.delayCall += () => {
                            UpdateMaterialTerrainLayerTextures(true);
                            SyncLayersToMaterial();

                            // Assign after material has been initialized, will cause white light otherwise
                            EditorApplication.delayCall += _main.AssignMaterialInstance;
                        };
                    } else {
                        _incorrectMaterial = true;
                        _materialProp.objectReferenceValue = _main.MainMaterial;
                    }
                } else {
                    _incorrectMaterial = false;
                    _parentTerrainProp.objectReferenceValue = null;
                    _autoSaveProp.boolValue = true;

                    if (_materialTerrainData != null)
                        _materialTerrainData.ClearTerrainLayers();
                    GetMaterialTerrainLayersData(null);
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
                    // Cannot assign this terrain as the parent
                    if (newTerrain == _main) {
                        _parentTerrainProp.objectReferenceValue = _main.ParentTerrain;
                        return;
                    }

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

            if (_terrainLayers.Length > 32) {
                EditorGUILayout.HelpBox("Over 32 terrain layers are assigned. Only 32 are supported, any extras will appear white.", MessageType.Warning);
                GUILayout.Space(5);
            }

            // Terrain Parent
            if (_settingUpParent) _settingUpParent = false;
            DrawParentProperty();

            if (_parentTerrainProp.objectReferenceValue != null) {
                EditorGUILayout.HelpBox("This terrain is taking the material and terrain layers from the parent terrain assigned above. Change the terrain layers, material, or remove the parent to use this terrains textures", MessageType.Info);
            } else {
                GUILayout.Space(10);

                if (!_materialTerrainData.AutoSyncLayers) {
                    EditorGUILayout.HelpBox("Auto sync is disabled in the material, layers will not be auto saved", MessageType.Info);
                    GUI.enabled = false;
                }

                Color prevBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _materialTerrainData.AutoSyncLayers && _autoSaveProp.boolValue ? Color.green : Color.red;
                _autoSaveProp.boolValue = GUILayout.Toggle(_autoSaveProp.boolValue, "Auto Save Textures", _toggleStyle, GUILayout.Height(30));
                GUI.backgroundColor = prevBackgroundColor;

                if (!_materialTerrainData.AutoSyncLayers)
                    GUI.enabled = true;

                if (GUILayout.Button(new GUIContent("Save Textures", "Manually save the data from the terrain layers to the material"), GUILayout.Height(30))) {
                    // Incase the material was changed to something different
                    if (_main.Terrain.materialTemplate != _main.MaterialInstance)
                        _main.UpdateTerrainMaterial(_main.MainMaterial);

                    SyncLayersToMaterial();
                    UpdateMaterialTerrainLayerTextures(true);
                }
            }
        }
    }
}
#endif