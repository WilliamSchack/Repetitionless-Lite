#if UNITY_EDITOR && MAPMAGIC2
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Variables;
using Repetitionless.Runtime.Integrations.MapMagic;

namespace Repetitionless.Editor.Integrations.MapMagic
{
    using Materials;
    using Data;

    [CustomEditor(typeof(RepetitionlessMapMagic))]
    public class RepetitionlessMapMagicEditor : UnityEditor.Editor
    {
        private RepetitionlessMapMagic _main;

        private MaterialDataManager _dataManager;
        private RepetitionlessLayeredDataSO _materialLayeredData;
        private RepetitionlessTerrainDataSO _materialTerrainData;
        private RepetitionlessTextureDataSO _materialTextureData;
        private RepetitionlessMaterialDataSO _materialProperties;

        private SerializedProperty _materialProp;
        private SerializedProperty _autoSaveProp;

        private GUIStyle _headerStyle;
        private GUIStyle _headerStyleError;
        private GUIStyle _toggleStyle;

        private bool _incorrectMaterial = false;

        private void SyncLayersToMaterial()
        {
            if (_materialTerrainData == null)
                return;

            // Update global data for terrain layer saving
            _materialTerrainData.UpdateTerrainLayers(_main.GetFirstTerrain().terrainData.terrainLayers);
        }

        // Save textures to the material
        private void UpdateMaterialTerrainLayerTextures(bool forceUpdate = false)
        {
            EditorApplication.delayCall += () => {
                if (_main.MainMaterial == null || _materialTerrainData == null)
                    return;

                // Will only update changed layers
                TerrainLayer[] terrainLayers = _main.GetFirstTerrain().terrainData.terrainLayers;
                for (int i = 0; i < terrainLayers.Length; i++)
                    _materialTerrainData.UpdateLayerMaterialData(i, forceUpdate);

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

            _dataManager = new MaterialDataManager(mat);
            _materialLayeredData = _dataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);
            if (_materialLayeredData == null) _materialLayeredData = RepetitionlessLayeredMaterialUtilities.SetupLayeredData(_dataManager);

            _materialTerrainData = _dataManager.LoadAsset<RepetitionlessTerrainDataSO>(Constants.TERRAIN_DATA_FILE_NAME);
            _materialTextureData = _dataManager.LoadAsset<RepetitionlessTextureDataSO>(Constants.TEXTURE_DATA_FILE_NAME);
            _materialProperties  = _dataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);

            _materialTextureData.OnDataChanged += UpdateMaterialTerrainTextures;
            _materialProperties.OnExternalDataChanged  += UpdateMaterialTerrainTextures;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= UpdateMaterialTerrainTextures;
            Undo.undoRedoPerformed += UpdateMaterialTerrainTextures;

            _main = (RepetitionlessMapMagic)serializedObject.targetObject;
            GetMaterialTerrainLayersData(_main.MainMaterial);

            _materialProp = serializedObject.FindProperty("_mainMaterial");
            _autoSaveProp = serializedObject.FindProperty("AutoSaveTextures");

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

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            // Cannot copy GUI.skin styles outside of ongui, make it here
            if (_toggleStyle == null) {
                _toggleStyle = new GUIStyle("button");
                _toggleStyle.fontSize = 12;
                _toggleStyle.fontStyle = FontStyle.Bold;
                _toggleStyle.alignment = TextAnchor.MiddleCenter;
            }

            // There is no callback for mapmagic terrain setting changes so check every gui update instead
            if (_main.enabled)
                _main.CheckAndUpdateMaterials();

            if (_main.MainMaterial == null) DrawNoMaterialGUI();
            else DrawAssignedMaterialGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void AssignNewMaterial(Material newMat)
        {
            if (newMat == null) {
                _incorrectMaterial = false;
                _autoSaveProp.boolValue = true;

                if (_materialTerrainData != null)
                    _materialTerrainData.ClearTerrainLayers();
                GetMaterialTerrainLayersData(null);

                _main.RemoveAllTilesMaterials();
                return;
            }

            string newShaderName = newMat.shader.name;
            
            if (newShaderName.StartsWith("Repetitionless/") && newShaderName.Contains(Constants.SHADER_MATERIAL_NAME_LAYERED)) {
                _incorrectMaterial = false;
                _autoSaveProp.boolValue = true;

                if (_materialTerrainData != null)
                    _materialTerrainData.ClearTerrainLayers();
                GetMaterialTerrainLayersData(newMat);

                _materialLayeredData.LayerMode = ELayerMode.TerrainLayers;
                _materialLayeredData.Save();

                _main.UpdateTerrainMaterials(newMat, false);

                // Assign textures after a frame so the material is properly assigned
                EditorApplication.delayCall += () => {
                    SyncLayersToMaterial();

                    // Assign after material has been initialized, will cause white light otherwise
                    EditorApplication.delayCall += () => {
                        _main.AssignNewMaterial(newMat);
                        UpdateMaterialTerrainLayerTextures(true);
                    };
                };
            } else {
                _incorrectMaterial = true;
                _materialProp.objectReferenceValue = _main.MainMaterial;
            }
        }

        private void DrawMaterialProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_materialProp);
            if (EditorGUI.EndChangeCheck()) {
                Material newMat = (Material)_materialProp.objectReferenceValue;
                AssignNewMaterial(newMat);
            }
        }

        private void DrawCreateMaterialButton()
        {
            if (!GUILayout.Button("Create New Material", GUILayout.Height(30)))
                return;

            MaterialDataObjects terrainMatObjects = RepetitionlessMaterialCreator.CreateTerrainMaterialAtCurrentFolder(false);
            AssignNewMaterial(terrainMatObjects.Material);
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
            GUILayout.Label("Assign a material to get started", _headerStyle);
            DrawIncorrectMaterialWarning();

            GUILayout.Space(10);

            DrawMaterialProperty();
            DrawCreateMaterialButton();
        }

        private void DrawAssignedMaterialGUI()
        {
            GUILayout.Space(5);

            //GUILayout.Label("Material", _headerStyle);
            DrawIncorrectMaterialWarning();

            //GUILayout.Space(10);

            DrawMaterialProperty();

            // Edit Material Button
            if (GUILayout.Button("Edit Material", GUILayout.Height(30)))
                Selection.activeObject = _main.MainMaterial;

            // Save Texture Layers Button
            //GUILayout.Space(10);

            //GUILayout.Label("Textures", _headerStyle);
            //GUILayout.Space(5);

            if (_materialTerrainData == null)
                return;

            if (!_materialTerrainData.AutoSyncLayers) {
                EditorGUILayout.HelpBox("Auto sync is disabled in the material, layers will not be auto saved", MessageType.Info);
            }

            if (GUILayout.Button(new GUIContent("Save Textures", "Manually save the data from the terrain layers to the material"), GUILayout.Height(22))) {
                _main.UpdateTerrainMaterials(_main.MainMaterial);

                // Make sure the material is set to terrain mode
                RepetitionlessLayeredMaterialUtilities.UpdateLayerMode(_dataManager, ELayerMode.TerrainLayers);

                SyncLayersToMaterial();
                UpdateMaterialTerrainLayerTextures(true);
            }

            //if (_main.Terrain.drawInstanced) {
            //    // Check for hdrp, Unity < 6.3, display error
            //}
        }
    }
}
#endif