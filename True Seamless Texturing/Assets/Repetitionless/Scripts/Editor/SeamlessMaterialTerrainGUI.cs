using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextureArrayEssentials.GUIUtilities;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    public class SeamlessMaterialTerrainGUI : SeamlessMaterialGUI
    {
        //public override void OnEnable(MaterialEditor materialEditor)
        //{
        //    base.OnEnable(materialEditor);
        //
        //    // Material Properties
        //    MaterialProperty settingTogglesProp = FindProperty($"_Layer1BaseSettings");
        //
        //    // Get variables from settings prop
        //    int settingToggles = (int)settingTogglesProp.vectorValue.x;
        //    bool noiseEnabled = BooleanCompression.GetCompressedValue(settingToggles, 0);
        //    bool randomiseScaling = BooleanCompression.GetCompressedValue(settingToggles, 1);
        //    bool randomiseRotation = BooleanCompression.GetCompressedValue(settingToggles, 2);
        //    bool smoothnessEnabled = BooleanCompression.GetCompressedValue(settingToggles, 3);
        //    bool variationEnabled = BooleanCompression.GetCompressedValue(settingToggles, 4);
        //    bool packedTexture = BooleanCompression.GetCompressedValue(settingToggles, 5);
        //    bool emissionEnabled = BooleanCompression.GetCompressedValue(settingToggles, 6);
        //
        //    Debug.Log("TERRAIN ENABLED:" + " || " + noiseEnabled + " || " + randomiseScaling + " || " + randomiseRotation + " || " + smoothnessEnabled + " || " + variationEnabled + " || " + packedTexture + " || " + emissionEnabled);
        //
        //    int ahhh = (int)settingTogglesProp.vectorValue.y;
        //    Debug.Log("AHHH: " + ahhh);
        //}

        private GUIStyle _labelStyle;

        private int _currentLayer = 0;

        private string[] _layerStrings = { "1", "2", "3", "4" };

        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);

            // Initialize styles
            _labelStyle = new GUIStyle("Label");
            _labelStyle.alignment = TextAnchor.MiddleCenter;
            _labelStyle.wordWrap = true;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            DrawLayerSelectionGUI();

            DrawLayerGUI(_currentLayer);
        }

        private void DrawLayerSelectionGUI()
        {
            // Start Background
            GUIUtilities.BeginBackgroundVertical();

            // Title Label
            GUIUtilities.DrawHeaderLabelLarge($"Layer");

            // Layer Selection Toolbar
            _currentLayer = GUILayout.Toolbar(_currentLayer, _layerStrings);

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }

        private void DrawLayerGUI(int layerIndex)
        {
            
        }
    }
}
#endif