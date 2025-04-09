using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextureArrayEssentials.GUIUtilities;
using TextureArrayEssentials.Compression;

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

            // Layer Selection
            DrawLayerSelectionGUI();

            // Layer
            DrawLayerGUI(_currentLayer);

            GUILayout.Space(SETTING_SPACING);

            // Footer Settings
            DrawDebugGUI();
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
            // Get property prefix
            string layerPropertyPrefix = $"Layer{layerIndex + 1}";

            // Base Material
            DrawBaseMaterialGUI(layerPropertyPrefix);

            GUILayout.Space(SETTING_SPACING);

            // Distance Blend Material
            DrawDistanceBlendGUI(layerPropertyPrefix);

            GUILayout.Space(SETTING_SPACING);

            // Material Blend
            DrawMaterialBlendGUI(layerPropertyPrefix);
        }

        protected override void DrawMaterialSettingsGUI(string materialPrefix, bool showNoise = true, bool showVariation = true, bool showPT = true, bool showEmission = true, bool showSR = true)
        {
            // For the base material remove Packed Texture and Smoothness/Roughness settings
            if (materialPrefix.Contains("Base")) {
                base.DrawMaterialSettingsGUI(materialPrefix, true, true, false, true, false);
                return;
            }

            base.DrawMaterialSettingsGUI(materialPrefix, showNoise, showVariation, showPT, showEmission, showSR);
        }

        protected override void DrawMaterialMainProperties(string materialPrefix, int sectionIndex)
        {
            // Show regular gui for all gui main properties but the base
            if (!materialPrefix.Contains("Base")) {
                base.DrawMaterialMainProperties(materialPrefix, sectionIndex);
                return;
            }

            // For the base material, only show emission property as textures are assigned in the terrain layer

            // Draw help box
            EditorGUILayout.HelpBox("Main material properties are assigned in the terrain layers", MessageType.Info);

            // Get properties
            MaterialProperty settingsProp = FindProperty($"_{materialPrefix}Settings");

            MaterialProperty emissionTexProp = FindProperty($"_{materialPrefix}EmissionMap");
            MaterialProperty emissionColorProp = FindProperty($"_{materialPrefix}EmissionColor");

            int settingToggles = (int)settingsProp.vectorValue.x;
            bool emissionEnabled = BooleanCompression.GetValue(settingToggles, 6);

            // Emission
            if (emissionEnabled) {
                EditorGUI.BeginChangeCheck();
                Texture oldEmissionTex = emissionTexProp.textureValue;

                Rect emissionColourRect = DrawTexture(sectionIndex, 6, new GUIContent("Emission", "Emission (RGB)"), emissionTexProp);
                Color emissionColour = EditorGUI.ColorField(emissionColourRect, GUIContent.none, emissionColorProp.colorValue, true, false, true);

                if (EditorGUI.EndChangeCheck()) {
                    // Update emission colour
                    emissionColorProp.colorValue = emissionColour;

                    // Change color to white if currently black when setting texture
                    if (oldEmissionTex != emissionTexProp.textureValue) {
                        Color blackColor = new Color(0, 0, 0, emissionTexProp.colorValue.a);
                        if (emissionColorProp.colorValue == blackColor && emissionTexProp.textureValue != null) {
                            emissionColorProp.colorValue = Color.white;
                        }
                    }
                }
            }
        }

        protected override Rect DrawTexture(int sectionIndex, int textureIndex, GUIContent content, MaterialProperty textureProperty)
        {
            return base.DrawTexture(sectionIndex, textureIndex, content, textureProperty);
        }
    }
}
#endif