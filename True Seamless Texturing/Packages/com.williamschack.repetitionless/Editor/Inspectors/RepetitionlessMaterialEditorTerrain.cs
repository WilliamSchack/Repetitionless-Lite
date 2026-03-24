#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Inspectors
{
    using Materials;
    using GUIUtilities;
    using TextureUtilities;
    using Data;

    /// <summary>
    /// The editor for the terrain repetitionless material
    /// </summary>
    public class RepetitionlessMaterialEditorTerrain : RepetitionlessMaterialEditorBase
    {
        private static readonly Vector4 DEFAULT_GLOBAL_TILING_OFFSET = new Vector4(100, 100, 0, 0);

        /// <summary>
        /// The max amount of layers for the material
        /// </summary>
        protected override int _maxLayers => Constants.MAX_LAYERS_TERRAIN;

        private RepetitionlessTerrainDataSO _materialTerrainData;

        private List<TerrainLayer> _terrainLayers => _materialTerrainData.TerrainLayers;

        private int _currentLayerIndex = 0;
        private bool _showingTerrainLayers = false;

        private GUIStyle _instanceHeaderStyle;
        private bool _isInstance = false;

        private GUIStyle _toggleSyncStyle;

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
            _materialTerrainData = RepetitionlessTerrainMaterialUtilities.SetupData(_dataManager);

            // Set terrain compatible tag
            _material.SetOverrideTag("TerrainCompatible", "True");

            // GUIStyles
            _toggleSyncStyle = new GUIStyle(GUIUtilities.MajorToggleButtonStyle);
            _toggleSyncStyle.fontSize = 12;
        }

        /// <summary>
        /// Called when the material properties are first created
        /// </summary>
        /// <param name="materialProperties">
        /// The material properties that were just created
        /// </param>
        protected override void OnPropertiesCreated(RepetitionlessMaterialDataSO materialProperties)
        {
            RepetitionlessTerrainMaterialUtilities.SetupProperties(_material, materialProperties);
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

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button(new GUIContent("Load From Layer", "Loads the textures and settings from the current terrain layer"))) {
                EditorApplication.delayCall += () => { _materialTerrainData.UpdateLayerMaterialData(_terrainLayers[_currentLayerIndex], true); };
            }

            if (GUILayout.Button(new GUIContent("Save To Layer", "Saves the textures and settings in the base material to the current terrain layer"))) {
                EditorApplication.delayCall += () => { SaveMaterialToLayer(_currentLayerIndex); };
            }

            GUILayout.EndHorizontal();

            Color prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = _materialTerrainData.AutoSyncLayers ? Color.green : Color.red;

            EditorGUI.BeginChangeCheck();
            _materialTerrainData.AutoSyncLayers = GUILayout.Toggle(_materialTerrainData.AutoSyncLayers, new GUIContent("Auto Sync Layers", "Toggles if textures and settings are automatically saved and loaded to and from the terrain layers"), _toggleSyncStyle);
            if (EditorGUI.EndChangeCheck()) {
                _materialTerrainData.Save();
                AssetDatabase.SaveAssetIfDirty(_materialTerrainData);
            }
            
            GUI.backgroundColor = prevBackgroundColor;

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
            if (!_materialTerrainData.AutoSyncLayers)
                return rect;

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
            float prevNormal               = baseData.NormalScale;
            float prevMetallic             = baseData.Metallic;
            float prevSmoothness           = baseData.SmoothnessRoughness;
            Vector4 prevTilingOffset       = baseData.TilingOffset;
            Vector4 prevGlobalTilingOffset = _materialProperties.GlobalTilingOffset;

            EditorGUI.BeginChangeCheck();
            base.DrawProperty(layerIndex, drawPropertyAction);
            if (!EditorGUI.EndChangeCheck()) return;

            if (layerIndex >= _terrainLayers.Count)
                return;

            if (prevGlobalTilingOffset != _materialProperties.GlobalTilingOffset) {
                // Update for all layers, by default only updates layer 0
                for (int i = 1; i < _maxLayers; i++) {
                    UpdateMaterialPropertiesTexture(i);
                }
            }

            // If any terrain layer properties changed, update them
            if (!_materialTerrainData.AutoSyncLayers)
                return;

            TerrainLayer terrainLayer = _terrainLayers[layerIndex];

            if (prevNormal != baseData.NormalScale)
                terrainLayer.normalScale = baseData.NormalScale;

            if (prevMetallic != baseData.Metallic)
                terrainLayer.metallic = baseData.Metallic;
                
            if (prevSmoothness != baseData.SmoothnessRoughness)
                terrainLayer.smoothness = baseData.SmoothnessRoughness;

            if (prevTilingOffset != baseData.TilingOffset) {
                terrainLayer.tileSize   = new Vector2(baseData.TilingOffset.x, baseData.TilingOffset.y);
                terrainLayer.tileOffset = new Vector2(baseData.TilingOffset.z, baseData.TilingOffset.w);
            }
        }

        private void SaveMaterialToLayer(int layerIndex)
        {
            TerrainLayer terrainLayer = _terrainLayers[layerIndex];
            if (terrainLayer == null) return;

            RepetitionlessMaterialData baseData = GetMaterialData(layerIndex, 0);

            // Textures
            ref TexturePacker.TextureData[] AVTextureData = ref _textureData.GetTextureData(layerIndex, 0, 0);
            terrainLayer.diffuseTexture = AVTextureData[0].Texture;

            ref TexturePacker.TextureData[] NSOTextureData = ref _textureData.GetTextureData(layerIndex, 0, 1);
            terrainLayer.normalMapTexture = NSOTextureData[0].Texture;

            if (baseData.PackedTexture)
                terrainLayer.maskMapTexture = NSOTextureData[3].Texture;

            // Properties
            terrainLayer.normalScale = baseData.NormalScale;
            terrainLayer.metallic = baseData.Metallic;
            terrainLayer.smoothness = baseData.SmoothnessRoughness;
            terrainLayer.tileSize   = new Vector2(baseData.TilingOffset.x, baseData.TilingOffset.y);
            terrainLayer.tileOffset = new Vector2(baseData.TilingOffset.z, baseData.TilingOffset.w);
        }
    }
}
#endif