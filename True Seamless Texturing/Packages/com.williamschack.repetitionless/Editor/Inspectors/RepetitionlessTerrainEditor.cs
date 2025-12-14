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
        private SerializedProperty _parentTerrainProp;


        private TerrainData _terrainData;
        private TerrainLayer[] _terrainLayers;
        private Terrain[] _terrainNeighbours = new Terrain[4];

        private TerrainLayerSyncDataSO _syncData;

        private GUIStyle _headerStyle;
        private GUIStyle _headerStyleError;
        private GUIStyle _toggleStyle;

        bool _incorrectMaterial = false;

        private bool TerrainLayersUpdated()
        {
            TerrainLayer[] newTerrainLayers = _terrainData.terrainLayers;

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

        private void SetupNewTerrainNeighbour(Terrain newNeighbour)
        {
            RepetitionlessTerrain repetitionlessTerrain;
            newNeighbour.TryGetComponent<RepetitionlessTerrain>(out repetitionlessTerrain);

            // Already setup
            if (repetitionlessTerrain != null && repetitionlessTerrain.ParentTerrain == _main)
                return;

            // Create terrain component
            if (repetitionlessTerrain == null)
                repetitionlessTerrain = newNeighbour.gameObject.AddComponent<RepetitionlessTerrain>();

            RepetitionlessTerrain newParent = _main;
            if (_main.ParentTerrain != null)
                newParent = _main.ParentTerrain;
            
            repetitionlessTerrain.ParentTerrain = newParent;

            // In the case another parent is set, there is a conflict and prompt which one to use
            if (repetitionlessTerrain.ParentTerrain != null && repetitionlessTerrain.ParentTerrain != _main) {
                bool usingMainParent = EditorUtility.DisplayDialog(
                    "Repetitionless Terrain Conflict",
                    $"Conflict detected between two repetitionless terrains. Pick which one will be the new parent (Which terrain layers will be used).\n\n" +
                    $"1: {_main.gameObject.name} ({_main.MainMaterial})" +
                    $"\n2: {repetitionlessTerrain.ParentTerrain.gameObject.name} ({repetitionlessTerrain.ParentTerrain.MainMaterial})",
                    _main.gameObject.name,
                    repetitionlessTerrain.ParentTerrain.gameObject.name);
            }

            // Set terrain layers and material
            repetitionlessTerrain.Terrain.terrainData.terrainLayers = _terrainLayers;
            repetitionlessTerrain.AutoSaveTextures = false;
            repetitionlessTerrain.UpdateTerrainMaterial(_main.MainMaterial);
        }

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
                    SetupNewTerrainNeighbour(neighbour);
                    _terrainNeighbours[i] = neighbour;
                }
            }
        }

        private void SyncLayersToMaterial()
        {
            // Update global data for terrain layer saving
            _terrainLayers = _terrainData.terrainLayers;
            _syncData.UpdateGlobalMaterialLayers(_main.MainMaterial, _terrainLayers);
        }

        // Save textures to the material
        private void UpdateMaterialTerrainLayerTextures()
        {
            if (_main.MainMaterial == null)
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

            _materialProp      = serializedObject.FindProperty("_mainMaterial");
            _autoSaveProp      = serializedObject.FindProperty("AutoSaveTextures");
            _parentTerrainProp = serializedObject.FindProperty("ParentTerrain");

            _terrainData   = _main.Terrain.terrainData;
            _terrainLayers = _terrainData.terrainLayers;
            //SyncLayersToMaterial();
            //UpdateMaterialTerrainLayerTextures();

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

        public override void OnInspectorGUI()
        {   
            serializedObject.Update();

            EditorGUILayout.PropertyField(_parentTerrainProp);

            // Cannot copy GUI.skin styles outside of ongui, make it here
            if (_toggleStyle == null) {
                _toggleStyle = new GUIStyle("button");
                _toggleStyle.fontSize = 12;
                _toggleStyle.fontStyle = FontStyle.Bold;
                _toggleStyle.alignment = TextAnchor.MiddleCenter;
            }

            // Update global layers sync data for layer saving
            if (!TerrainLayersUpdated()) {
                SyncLayersToMaterial();
                if (_autoSaveProp.boolValue)
                    UpdateMaterialTerrainLayerTextures();
            }

            // Add scripts to terrain neighbours if added
            HandleUpdatedTerrainNeighbours();

            if (_main.MainMaterial == null) {
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

                        _syncData.RemoveMaterial(_main.MainMaterial);
                        _main.UpdateTerrainMaterial(newMat);

                        EditorApplication.delayCall += () => {
                            SyncLayersToMaterial();
                            UpdateMaterialTerrainLayerTextures();
                            _main.UpdateMaterialTerrainTextures();
                        };
                    } else {
                        _incorrectMaterial = true;
                        _materialProp.objectReferenceValue = _main.MainMaterial;
                    }
                } else {
                    _incorrectMaterial = false;
                    _syncData.RemoveMaterial(_main?.MainMaterial);
                }
            }

            if (_main.MainMaterial != null) {
                // Edit Material Button
                if (GUILayout.Button("Edit Material", GUILayout.Height(30)))
                    Selection.activeObject = _main.MainMaterial;

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