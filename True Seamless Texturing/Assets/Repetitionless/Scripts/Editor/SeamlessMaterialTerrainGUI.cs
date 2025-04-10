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
        private class TerrainLayerTextureDrawers
        {
            public TextureArrayGUIDrawer FarTexturesDrawer;
            public TextureArrayGUIDrawer BlendTexturesDrawer;
        }

        private GUIStyle _labelStyle;

        private int _currentLayer = 0;

        private string[] _layerStrings = { "1", "2", "3", "4" };
        private List<TerrainLayerTextureDrawers> _textureDrawers;

        private TextureArrayGUIDrawer GetTextureDrawer(int sectionIndex)
        {
            // Get texture array drawer based on current selected layer and section, far if section is 1, blend if 2
            TerrainLayerTextureDrawers textureDrawers = _textureDrawers[_currentLayer];
            return sectionIndex == 1 ? textureDrawers.FarTexturesDrawer : textureDrawers.BlendTexturesDrawer;
        }

        protected override int HandleAssignedTextures(string materialPrefix, int sectionIndex, MaterialProperty settingsProp)
        {
            // If the base material, return true for each texture as that information cannot be got, textures in the Texture Layer
            if (sectionIndex == 0) {
                // Emission is the only texture assigned in the inspected, handle that properly
                MaterialProperty emissionTexProp = FindProperty($"_{materialPrefix}EmissionMap");

                return BooleanCompression.CompressValues(true, true, true, true, true, emissionTexProp.textureValue != null);
            }

            // Material Properties
            MaterialProperty normalTexProp = FindProperty($"_{materialPrefix}NormalMap");

            // Get texture array drawer
            TextureArrayGUIDrawer textureDrawer = GetTextureDrawer(sectionIndex);

            // Compress Assigned Textures
            bool metallicAssigned = textureDrawer.TextureAssignedAt(1);
            bool smoothnessAssigned = textureDrawer.TextureAssignedAt(2);
            bool roughnessAssigned = textureDrawer.TextureAssignedAt(3);
            bool normalAssigned = normalTexProp.textureValue != null;
            bool occlussionAssigned = textureDrawer.TextureAssignedAt(4);
            bool emissionAssigned = textureDrawer.TextureAssignedAt(5);

            int compressedAssignedTextures = BooleanCompression.CompressValues(metallicAssigned, smoothnessAssigned, roughnessAssigned, normalAssigned, occlussionAssigned, emissionAssigned);
            return compressedAssignedTextures;
        }

        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);

            // Initialize styles
            _labelStyle = new GUIStyle("Label");
            _labelStyle.alignment = TextAnchor.MiddleCenter;
            _labelStyle.wordWrap = true;

            // Setup Texture Array Drawers
            _textureDrawers = new List<TerrainLayerTextureDrawers>();
            for (int i = 0; i < _layerStrings.Length; i++) {
                TerrainLayerTextureDrawers textureData = new TerrainLayerTextureDrawers();

                // Get properties
                // Have specific assigned textures for the texture arrays as the main one includes other textures as well
                // Combine the two in the HandleAssignedTextures function above
                MaterialProperty farTexturesProp = FindProperty($"_Layer{i + 1}FarTextures");
                MaterialProperty farAssignedTexturesProp = FindProperty($"_Layer{i + 1}FarArrayAssignedTextures");

                MaterialProperty blendTexturesProp = FindProperty($"_Layer{i + 1}BlendTextures");
                MaterialProperty blendAssignedTexturesProp = FindProperty($"_Layer{i + 1}BlendArrayAssignedTextures");

                textureData.FarTexturesDrawer = new TextureArrayGUIDrawer(farTexturesProp, farAssignedTexturesProp, 6, $"Layer{i + 1}FarTextures");
                textureData.BlendTexturesDrawer = new TextureArrayGUIDrawer(blendTexturesProp, blendAssignedTexturesProp, 6, $"Layer{i + 1}BlendTextures");

                _textureDrawers.Add(textureData);
            }
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Layer Selection
            DrawLayerSelectionGUI();

            GUILayout.Space(SETTING_SPACING);

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

            MaterialProperty emissionColorProp = FindProperty($"_{materialPrefix}EmissionColor");

            int settingToggles = (int)settingsProp.vectorValue.x;
            int compressedAssignedTextures = (int)settingsProp.vectorValue.y;
            bool emissionEnabled = BooleanCompression.GetValue(settingToggles, 6);

            // Emission
            if (emissionEnabled) {
                bool prevEmissionAssigned = BooleanCompression.GetValue(compressedAssignedTextures, 4);

                // Change emission colour to white if texture assigned and texture is black
                EditorGUI.BeginChangeCheck();
                Rect emissionColourRect = DrawTexture(sectionIndex, 6, new GUIContent("Emission", "Emission (RGB)"), $"_{materialPrefix}EmissionMap");
                Color emissionColour = EditorGUI.ColorField(emissionColourRect, GUIContent.none, emissionColorProp.colorValue, true, false, true);
                if (EditorGUI.EndChangeCheck()) {
                    // Rehandle assigned textures since the function can be changed in child classes
                    int afterAssignedTextures = HandleAssignedTextures(materialPrefix, sectionIndex, settingsProp);
                    bool afterEmissionAssigned = BooleanCompression.GetValue(afterAssignedTextures, 5);

                    // Update emission colour
                    emissionColorProp.colorValue = emissionColour;

                    // If texture just assigned and colour is black, change colour to white
                    if (afterEmissionAssigned && !prevEmissionAssigned) {
                        Color blackColor = new Color(0, 0, 0, emissionColorProp.colorValue.a);
                        if (emissionColorProp.colorValue == blackColor) {
                            emissionColorProp.colorValue = Color.white;
                        }
                    }
                }
            }
        }

        protected override Rect DrawTexture(int sectionIndex, int textureIndex, GUIContent content, string texturePropertyName)
        {
            // If the base material, normal map, or variation texture, dont use the texture arrays so draw the field normally
            if(sectionIndex == 0 || textureIndex == 4 || textureIndex == 7)
                return base.DrawTexture(sectionIndex, textureIndex, content, texturePropertyName);

            // If greater than the normal map, go back an index (skip the normal map)
            if (textureIndex >= 4)
                textureIndex--;

            // Get texture array drawer
            TextureArrayGUIDrawer textureDrawer = GetTextureDrawer(sectionIndex);

            // Draw texture
            Rect lineRect = GUIUtilities.GetLineRect();
            textureDrawer.DrawTexture(lineRect, textureIndex, content);

            // Return rect after texture field
            lineRect = MaterialEditor.GetRectAfterLabelWidth(lineRect);
            return lineRect;
        }
    }
}
#endif