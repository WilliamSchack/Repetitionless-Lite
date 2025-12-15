#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    using GUIUtilities;
    using TextureUtilities;
    using Data;
    using Variables;

    public class RepetitionlessMaterialEditorTerrainNEW : RepetitionlessMaterialEditorBaseNEW
    {
        protected override int _maxLayers => 32;

        private List<TerrainLayer> _terrainLayers;

        private int _currentLayerIndex = 0;
        private bool _showingTerrainLayers = false;

        private GUIStyle _instanceHeaderStyle;
        private bool _isInstance = false;

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

            // Set terrain compatible tag
            _material.SetOverrideTag("TerrainCompatible", "True");

            // Load terrain layers
            TerrainLayerSyncDataSO syncData = TerrainLayerSyncDataSO.Load();
            _terrainLayers = syncData.MaterialToTerrainLayer.Get(_material)?.Items;
        }

        protected override void OnPropertiesCreated()
        {
            // Set uv space to world
            MaterialProperty uvSpaceProp = FindProperty("_UVSpace");
            uvSpaceProp.floatValue = (int)EUVSpace.World;

            // Set all tiling offset values to 100
            Vector4 defaultTilingOffset = new Vector4(100, 100, 0, 0);
            for (int i = 0; i < _materialProperties.Data.Length; i++) {
                _materialProperties.Data[i].BaseMaterialData.TilingOffset  = defaultTilingOffset;
                _materialProperties.Data[i].FarMaterialData.TilingOffset   = defaultTilingOffset;
                _materialProperties.Data[i].BlendMaterialData.TilingOffset = defaultTilingOffset;
            }
        }

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

            if (_terrainLayers == null) {
                GUILayout.Label("No terrain layers found, editing layer 1");
                GUIUtilities.EndBackgroundVertical();
                return;
            }

            _currentLayerIndex = EditorGUILayout.IntSlider("Editing Layer", _currentLayerIndex + 1, 1, _terrainLayers.Count) - 1;

            GUILayout.Space(5);

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
    }
}
#endif