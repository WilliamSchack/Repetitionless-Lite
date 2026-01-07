#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Inspectors
{
    using GUIUtilities;
    using TextureUtilities;
    using Data;

    /// <summary>
    /// The editor for the terrain repetitionless material
    /// </summary>
    public class RepetitionlessMaterialEditorTerrain : RepetitionlessMaterialEditorBase
    {
        /// <summary>
        /// The max amount of layers for the material
        /// </summary>
        protected override int _maxLayers => 32;

        private RepetitionlessTerrainDataSO _materialTerrainData;

        private List<TerrainLayer> _terrainLayers => _materialTerrainData.TerrainLayers;

        private int _currentLayerIndex = 0;
        private bool _showingTerrainLayers = false;

        private GUIStyle _instanceHeaderStyle;
        private bool _isInstance = false;

        /// <summary>
        /// Called when the inspector is first opened
        /// </summary>
        /// <param name="materialEditor">
        /// The material editor being used
        /// </param>
        public override void OnEnable(MaterialEditor materialEditor)
        {
            // Check if this is an instance, disables gui pretty much
            // We need the data folder which is gotten from next to the editor
            _material = (Material)materialEditor.target;
            _isInstance = !AssetDatabase.Contains(_material);
            if (_isInstance) {
                _instanceHeaderStyle = new GUIStyle(GUIUtilities.BoldHeaderLargeStyle);
                _instanceHeaderStyle.wordWrap = true;

                return;
            }

            base.OnEnable(materialEditor);

            // Get terrain layer data SO
            if (_dataManager.AssetExists(Constants.TERRAIN_DATA_FILE_NAME))
                _materialTerrainData = _dataManager.LoadAsset<RepetitionlessTerrainDataSO>(Constants.TERRAIN_DATA_FILE_NAME);
            else {
                _materialTerrainData = ScriptableObject.CreateInstance<RepetitionlessTerrainDataSO>();
                _dataManager.CreateAsset(_materialTerrainData, Constants.TERRAIN_DATA_FILE_NAME);

                _materialTerrainData.Save();
                AssetDatabase.SaveAssetIfDirty(_materialTerrainData);
            }

            // Set terrain compatible tag
            _material.SetOverrideTag("TerrainCompatible", "True");
        }

        /// <summary>
        /// Called when the material properties are first created
        /// </summary>
        protected override void OnPropertiesCreated()
        {
            // Set uv space to world
            MaterialProperty uvSpaceProp = FindProperty("_UVSpace");
            uvSpaceProp.floatValue = (int)EUVSpace.World;

            // Set all tiling offset values to 100
            Vector4 defaultTilingOffset = new Vector4(100, 100, 0, 0);
            for (int i = 0; i < _materialProperties.Data.Length; i++) {
                _materialProperties.Data[i].BaseMaterialData.TilingOffset  = defaultTilingOffset;
                _materialProperties.Data[i].FarMaterialData.TilingOffset   = defaultTilingOffset / 2;
                _materialProperties.Data[i].BlendMaterialData.TilingOffset = defaultTilingOffset;
            }
        }

        /// <summary>
        /// Base OnGUI function
        /// </summary>
        /// <param name="materialEditor">
        /// The material editor being used
        /// </param>
        /// <param name="properties">
        /// The material properties
        /// </param>
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (_isInstance) {
                GUILayout.Label("Cannot view the material from an instance, select the main material to view and edit.", _instanceHeaderStyle);
                return;
            }

            base.OnGUI(materialEditor, properties);

            // If OnEnable was just called, prevents errors
            if (_isInstance) return;

            DrawTerrainSettings();

            GUILayout.Space(SETTING_PADDING);

            // Base Material
            DrawBaseMaterialGUI(_currentLayerIndex);

            GUILayout.Space(SETTING_PADDING);

            // Distance Blend Material
            DrawDistanceBlendGUI(_currentLayerIndex);

            GUILayout.Space(SETTING_PADDING);

            // Material Blend
            DrawMaterialBlendGUI(_currentLayerIndex);

            GUILayout.Space(SETTING_PADDING);

            // Footer Settings
            DrawDebugGUI();
        }

        private void DrawTerrainSettings()
        {
            GUIUtilities.BeginBackgroundVertical();

            GUIUtilities.DrawHeaderLabelLarge("Terrain");

            if (_terrainLayers == null || _terrainLayers.Count == 0) {
                GUILayout.Label("No terrain layers found, editing layer 1");
                GUIUtilities.EndBackgroundVertical();
                return;
            }

            _currentLayerIndex = EditorGUILayout.IntSlider("Editing Layer", _currentLayerIndex + 1, 1, _terrainLayers.Count) - 1;

            GUI.enabled = false;
            EditorGUILayout.ObjectField(new GUIContent("Terrain Layer", "The terrain layer that is being used for these fields. Modifying these fields that are in the layer will also update that layers fields."), _terrainLayers[_currentLayerIndex], typeof(TerrainLayer), false);
            GUI.enabled = true;

            GUILayout.Space(10);

            GUIUtilities.BeginBackgroundVertical();
            _showingTerrainLayers = GUIUtilities.DrawFoldout(_showingTerrainLayers, "Terrain Layers");
            if (_showingTerrainLayers) {
                GUILayout.Label("These can be changed in the terrain");

                GUI.enabled = false;
                for (int i = 0; i < _terrainLayers.Count; i++) {
                    EditorGUILayout.ObjectField($"Terrain Layer {i}", _terrainLayers[i], typeof(TerrainLayer), false);
                }
                GUI.enabled = true;
            }
            GUIUtilities.EndBackgroundVertical();
            GUIUtilities.EndBackgroundVertical();
        }

        /// <summary>
        /// Used to draw all the texture fields
        /// </summary>
        /// <param name="layerIndex">
        /// The layer which the texture will be drawn
        /// </param>
        /// <param name="sectionIndex">
        /// The section that this texture is in
        /// </param>
        /// <param name="textureIndex">
        /// The index of the texture in this section
        /// </param>
        /// <param name="content">
        /// The GUIContent to use in the field
        /// </param>
        /// <returns>
        /// The rect that the texture field is using
        /// </returns>
        protected override Rect DrawTexture(int layerIndex, int sectionIndex, int textureIndex, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            Rect rect = base.DrawTexture(layerIndex, sectionIndex, textureIndex, content);
            if (!EditorGUI.EndChangeCheck() || sectionIndex != 0) return rect;
    
            // Update terrain layer
            TerrainLayer terrainLayer = _terrainLayers[layerIndex];

            switch (textureIndex) {
                case 0: { // Albedo
                    ref TexturePacker.TextureData[] textureData = ref _textureData.GetTextureData(layerIndex, sectionIndex, 0);
                    terrainLayer.diffuseTexture = textureData[0].Texture;
                    break;
                } case 1: { // Packed texture
                    ref TexturePacker.TextureData[] textureData = ref _textureData.GetTextureData(layerIndex, sectionIndex, 1);
                    RepetitionlessMaterialData currentData = GetMaterialData(layerIndex, sectionIndex);
                    if (!currentData.PackedTexture) break;

                    terrainLayer.maskMapTexture = textureData[3].Texture;
                    break;
                } case 3: { // Normal
                    ref TexturePacker.TextureData[] textureData = ref _textureData.GetTextureData(layerIndex, sectionIndex, 1);
                    terrainLayer.normalMapTexture = textureData[0].Texture;
                    break;
                }
            }

            EditorUtility.SetDirty(terrainLayer);

            return rect;
        }

        /// <summary>
        /// Saves the material property if changed in the action<br />
        /// <b>Each gui function modifying the material properties should be using this function</b>
        /// </summary>
        /// <param name="layerIndex">
        /// The layer index of this property
        /// </param>
        /// <param name="drawPropertyAction">
        /// The draw function
        /// </param>
        protected override void DrawProperty(int layerIndex, Action drawPropertyAction)
        {
            // Check for terrain layer properties
            RepetitionlessMaterialData baseData = GetMaterialData(layerIndex, 0);
            float prevMetallic       = baseData.Metallic;
            float prevSmoothness     = baseData.SmoothnessRoughness;
            Vector4 prevTilingOffset = baseData.TilingOffset;

            EditorGUI.BeginChangeCheck();
            base.DrawProperty(layerIndex, drawPropertyAction);
            if (!EditorGUI.EndChangeCheck()) return;

            if (layerIndex >= _terrainLayers.Count)
                return;

            // If any terrain layer properties changed, update them
            TerrainLayer terrainLayer = _terrainLayers[layerIndex];
            if (prevMetallic != baseData.Metallic)
                terrainLayer.metallic = baseData.Metallic;
                
            if (prevSmoothness != baseData.SmoothnessRoughness)
                terrainLayer.smoothness = baseData.SmoothnessRoughness;

            if (prevTilingOffset != baseData.TilingOffset) {
                terrainLayer.tileSize   = new Vector2(baseData.TilingOffset.x, baseData.TilingOffset.y);
                terrainLayer.tileOffset = new Vector2(baseData.TilingOffset.z, baseData.TilingOffset.w);
            }
        }
    }
}
#endif