#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    using GUIUtilities;
    using Data;

    public class RepetitionlessMaterialEditorTerrainNEW : RepetitionlessMaterialEditorBaseNEW
    {
        protected override int _maxLayers => 32;

        private List<TerrainLayer> _terrainLayers;

        private int _currentLayerIndex = 0;
        private bool _showingTerrainLayers = false;

        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);

            // Load terrain layers
            TerrainLayerSyncDataSO syncData = TerrainLayerSyncDataSO.Load();
            _terrainLayers = syncData.MaterialToTerrainLayer.Get(_material).Items;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

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
    }
}
#endif