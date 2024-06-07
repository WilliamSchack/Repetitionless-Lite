using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    using Compression;
    using Variables;

    public class SeamlessMaterialMasterGUI : SeamlessMaterialGUI
    {
        #region Variables
        // Constants
        private const int BACKGROUND_HEIGHT_DISTANCE_BLEND_TO = 128;
        private const int BACKGROUND_HEIGHT_DISTANCE_BLEND_MATERIAL = 84;

        private const int BACKGROUND_HEIGHT_MATERIAL_BLEND_NOISE = 179;
        private const int BACKGROUND_HEIGHT_MATERIAL_BLEND_TEXTURE = 201;
        private const int BACKGROUND_HEIGHT_MATERIAL_BLEND_DB_TO_DISABLED = 57;
        private const int BACKGROUND_HEIGHT_MATERIAL_BLEND_DB_TO_ENABLED = 101;

        private const int BACKGROUND_HEIGHT_DEBUG = 163;

        // Debug
        private int _prevDebugIndex = 0;

        // Background Heights
        private float _baseBackgroundHeight;
        private float _distanceBlendBackgroundHeight;
        private float _materialBlendBackgroundHeight;
        private float _debugBackgroundHeight;
        #endregion

        #region Setup
        private void SetupInitialBackgroundHeights()
        {
            // Base Material
            MaterialProperty baseSettingTogglesProp = FindProperty("_BaseSettings");
            int baseSettingToggles = (int)baseSettingTogglesProp.vectorValue.x;
            bool baseNoiseEnabled = BooleanCompression.GetCompressedValue(baseSettingToggles, 0);
            bool baseVariationEnabled = BooleanCompression.GetCompressedValue(baseSettingToggles, 4);

            _baseBackgroundHeight = BACKGROUND_HEIGHT_HEADERSETTINGS + BACKGROUND_HEIGHT_MAIN_FOLDOUT;
            if (baseNoiseEnabled) _baseBackgroundHeight += BACKGROUND_HEIGHT_NOISE_FOLDOUT;
            if (baseVariationEnabled) _baseBackgroundHeight += BACKGROUND_HEIGHT_VARIATION_FOLDOUT;
            _baseBackgroundHeight -= GUIUtilities.BACKGROUND_BOTTOM_PADDING;

            // Distance Blend
            MaterialProperty distanceBlendingEnabledProp = FindProperty("_DistanceBlendingEnabled");
            MaterialProperty distanceBlendModeProp = FindProperty("_DistanceBlendMode");

            bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;
            DistanceBlendMode distanceBlendMode = (DistanceBlendMode)distanceBlendModeProp.floatValue;

            if (distanceBlendingEnabled) {
                if (distanceBlendMode == DistanceBlendMode.TilingOffset) {
                    // Tiling & Offset Settings
                    _distanceBlendBackgroundHeight = BACKGROUND_HEIGHT_DISTANCE_BLEND_TO;
                } else {
                    // Main Material
                    MaterialProperty farSettingTogglesProp = FindProperty("_FarSettings");
                    int farSettingToggles = (int)farSettingTogglesProp.vectorValue.x;
                    bool farNoiseEnabled = BooleanCompression.GetCompressedValue(farSettingToggles, 0);
                    bool farVariationEnabled = BooleanCompression.GetCompressedValue(farSettingToggles, 4);

                    _distanceBlendBackgroundHeight = BACKGROUND_HEIGHT_DISTANCE_BLEND_MATERIAL + BACKGROUND_HEIGHT_HEADERSETTINGS + BACKGROUND_HEIGHT_MAIN_FOLDOUT;
                    if (farNoiseEnabled) _distanceBlendBackgroundHeight += BACKGROUND_HEIGHT_NOISE_FOLDOUT;
                    if (farVariationEnabled) _distanceBlendBackgroundHeight += BACKGROUND_HEIGHT_VARIATION_FOLDOUT;
                }

                _distanceBlendBackgroundHeight -= GUIUtilities.BACKGROUND_BOTTOM_PADDING;
            } else {
                _distanceBlendBackgroundHeight = BACKGROUND_HEIGHT_DISABLED_SETTING;
            }

            // Material Blend
            MaterialProperty materialBlendingSettingsProp = FindProperty("_MaterialBlendSettings");

            int materialBlendingSettings = (int)materialBlendingSettingsProp.floatValue;
            bool materialBlendingEnabled = BooleanCompression.GetCompressedValue(materialBlendingSettings, 0);

            if (materialBlendingEnabled) {
                // Mask Settings
                MaterialProperty blendMaskTypeProp = FindProperty("_BlendMaskType");
                TextureType blendMaskType = (TextureType)blendMaskTypeProp.floatValue;
                _materialBlendBackgroundHeight = blendMaskType == TextureType.CustomTexture ? BACKGROUND_HEIGHT_MATERIAL_BLEND_TEXTURE : BACKGROUND_HEIGHT_MATERIAL_BLEND_NOISE;
                _materialBlendBackgroundHeight -= GUIUtilities.BACKGROUND_BOTTOM_PADDING;

                // Distance Blend Settings
                if (distanceBlendingEnabled) {
                    bool overrideDistanceBlending = BooleanCompression.GetCompressedValue(materialBlendingSettings, 1);
                    bool overrideDistanceBlendingTO = BooleanCompression.GetCompressedValue(materialBlendingSettings, 2);
                    _materialBlendBackgroundHeight += overrideDistanceBlending && overrideDistanceBlendingTO ? BACKGROUND_HEIGHT_MATERIAL_BLEND_DB_TO_ENABLED : BACKGROUND_HEIGHT_MATERIAL_BLEND_DB_TO_DISABLED;
                    _materialBlendBackgroundHeight -= GUIUtilities.BACKGROUND_BOTTOM_PADDING;
                }

                // Main Material
                MaterialProperty blendSettingTogglesProp = FindProperty("_BlendSettings");
                int blendSettingToggles = (int)blendSettingTogglesProp.vectorValue.x;
                bool blendNoiseEnabled = BooleanCompression.GetCompressedValue(blendSettingToggles, 0);
                bool blendVariationEnabled = BooleanCompression.GetCompressedValue(blendSettingToggles, 4);

                _materialBlendBackgroundHeight += BACKGROUND_HEIGHT_HEADERSETTINGS + BACKGROUND_HEIGHT_MAIN_FOLDOUT;
                if (blendNoiseEnabled) _materialBlendBackgroundHeight += BACKGROUND_HEIGHT_NOISE_FOLDOUT;
                if (blendVariationEnabled) _materialBlendBackgroundHeight += BACKGROUND_HEIGHT_VARIATION_FOLDOUT;

                _materialBlendBackgroundHeight -= GUIUtilities.BACKGROUND_BOTTOM_PADDING;
            } else {
                _materialBlendBackgroundHeight = BACKGROUND_HEIGHT_DISABLED_SETTING;
            }

            // Debug
            MaterialProperty debuggingIndexProp = FindProperty("_DebuggingIndex");
            bool debuggingEnabled = debuggingIndexProp.floatValue != -1 ? true : false;

            _debugBackgroundHeight = debuggingEnabled ? BACKGROUND_HEIGHT_DEBUG : BACKGROUND_HEIGHT_DISABLED_SETTING;
        }
        #endregion

        #region GUI Calls
        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);

            SetupInitialBackgroundHeights();
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Base Material
            DrawBaseMaterialGUI();

            GUILayout.Space(SETTING_SPACING);

            // Distance Blend Material
            DrawDistanceBlendGUI();

            GUILayout.Space(SETTING_SPACING);

            // Material Blend
            DrawMaterialBlendGUI();

            GUILayout.Space(SETTING_SPACING);

            // Footer Settings
            DrawDebugGUI();
        }
        #endregion

        #region Material GUI
        private void DrawBaseMaterialGUI()
        {
            float backgroundStartingYPos = GUIUtilities.StartBackground(_baseBackgroundHeight);

            DrawMaterialGUI("Base");

            float heightDiff = GUIUtilities.EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _baseBackgroundHeight = heightDiff;
        }

        private void DrawDistanceBlendGUI()
        {
            // Material Property
            MaterialProperty distanceBlendingEnabledProp = FindProperty("_DistanceBlendingEnabled");

            // Start Background
            float backgroundStartingYPos = GUIUtilities.StartBackground(_distanceBlendBackgroundHeight);

            // Distance Blend Enabled Toggle
            bool distanceBlendingEnabled = GUIUtilities.DrawMajorToggleButton(distanceBlendingEnabledProp, "Distance Blending");

            // Draw distance blending settings
            if (distanceBlendingEnabled) {
                // Material Properties
                MaterialProperty distanceBlendModeProp = FindProperty("_DistanceBlendMode");
                MaterialProperty distanceBlendMinMaxProp = FindProperty("_DistanceBlendMinMax");

                GUILayout.Space(5);

                // Distance Blend Mode
                EditorGUI.BeginChangeCheck();
                DistanceBlendMode distanceBlendMode = (DistanceBlendMode)distanceBlendModeProp.floatValue;
                distanceBlendMode = (DistanceBlendMode)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Blend Mode", distanceBlendMode);
                if (EditorGUI.EndChangeCheck())
                    distanceBlendModeProp.floatValue = (int)distanceBlendMode;

                // Distance Blend Min Max
                EditorGUI.BeginChangeCheck();
                Vector2 distanceBlendMinMax = new Vector2(distanceBlendMinMaxProp.vectorValue.x, distanceBlendMinMaxProp.vectorValue.y);
                distanceBlendMinMax = GUIUtilities.DrawVector2Field(distanceBlendMinMax, "Distance Blend Min Max");
                if (EditorGUI.EndChangeCheck()) {
                    if (distanceBlendMinMax.x < 0) distanceBlendMinMax.x = 0;
                    if (distanceBlendMinMax.y < 0) distanceBlendMinMax.y = 0;

                    distanceBlendMinMaxProp.vectorValue = distanceBlendMinMax;
                }

                switch (distanceBlendMode) {
                    case DistanceBlendMode.TilingOffset:
                        MaterialProperty tilingOffsetProp = FindProperty("_FarTilingOffset");

                        // Tiling & Offset GUI
                        GUIUtilities.DrawTilingOffset(tilingOffsetProp);
                        break;
                    case DistanceBlendMode.Material:
                        GUILayout.Space(10);

                        // Material GUI
                        DrawMaterialGUI("Far");
                        break;
                }
            }

            // End Background
            float heightDiff = GUIUtilities.EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _distanceBlendBackgroundHeight = heightDiff;
        }

        private void DrawMaterialBlendGUI()
        {
            // Start Background
            float backgroundStartingYPos = GUIUtilities.StartBackground(_materialBlendBackgroundHeight);

            // Material Property
            MaterialProperty materialBlendingSettingsProp = FindProperty("_MaterialBlendSettings");

            int materialBlendingSettings = (int)materialBlendingSettingsProp.floatValue;
            bool materialBlendingEnabled = BooleanCompression.GetCompressedValue(materialBlendingSettings, 0);
            bool overrideDistanceBlending = BooleanCompression.GetCompressedValue(materialBlendingSettings, 1);
            bool overrideDistanceBlendingTO = BooleanCompression.GetCompressedValue(materialBlendingSettings, 2);

            // Material Blend Enabled Toggle
            materialBlendingEnabled = GUIUtilities.DrawMajorToggleButton(materialBlendingEnabled, "Material Blending");

            if (materialBlendingEnabled) {
                // Material Properties
                MaterialProperty blendMaskTypeProp = FindProperty("_BlendMaskType");
                MaterialProperty distanceBlendingEnabledProp = FindProperty("_DistanceBlendingEnabled");

                MaterialProperty materialBlendPropertiesProp = FindProperty("_MaterialBlendProperties");

                Vector2 materialBlendProperties = materialBlendPropertiesProp.vectorValue;
                Vector2 oriMaterialBlendProperties = materialBlendProperties;

                // Mask
                GUIUtilities.DrawHeaderLabelLarge("Mask");

                EditorGUI.BeginChangeCheck();
                TextureType blendMaskType = (TextureType)blendMaskTypeProp.floatValue;
                blendMaskType = (TextureType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Mask Type", blendMaskType);
                if (EditorGUI.EndChangeCheck())
                    blendMaskTypeProp.floatValue = (int)blendMaskType;

                materialBlendProperties.x = EditorGUI.Slider(GUIUtilities.GetLineRect(), "Mask Opacity", materialBlendProperties.x, 0, 1);
                materialBlendProperties.y = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Mask Strength", materialBlendProperties.y);

                if (blendMaskType != TextureType.CustomTexture) { // Noise
                                                                  // Material Properties
                    MaterialProperty materialBlendNoiseSettingsProp = FindProperty("_MaterialBlendNoiseSettings");

                    Vector3 materialBlendNoiseSettings = materialBlendNoiseSettingsProp.vectorValue;
                    Vector3 oriMaterialBlendNoiseSettings = materialBlendNoiseSettings;

                    // Scale
                    float noiseScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", materialBlendNoiseSettings.x);

                    // Offset
                    Vector2 noiseOffset = GUIUtilities.DrawVector2Field(new Vector2(materialBlendNoiseSettings.y, materialBlendNoiseSettings.z), "Noise Offset");

                    materialBlendNoiseSettings = new Vector3(noiseScale, noiseOffset.x, noiseOffset.y);

                    if (materialBlendNoiseSettings != oriMaterialBlendNoiseSettings)
                        materialBlendNoiseSettingsProp.vectorValue = materialBlendNoiseSettings;
                } else { // Custom Texture
                         // Material Properties
                    MaterialProperty blendMaskTextureProp = FindProperty("_BlendMaskTexture");
                    MaterialProperty blendMaskTextureTOProp = FindProperty("_BlendMaskTextureTO");

                    // Texture
                    _editor.TexturePropertySingleLine(new GUIContent("Blend Mask"), blendMaskTextureProp);

                    // Scale & Offset
                    GUIUtilities.DrawTilingOffset(blendMaskTextureTOProp);
                }

                GUILayout.Space(10);

                // Distance Blending
                bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;

                if (distanceBlendingEnabled) {
                    // Material Property
                    MaterialProperty distanceBlendingModeProp = FindProperty("_DistanceBlendMode");
                    DistanceBlendMode distanceBlendingMode = (DistanceBlendMode)distanceBlendingModeProp.floatValue;

                    // Calculate scaled text min width
                    int minScaledTextWidth = 0;
                    minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Override Distance Blending")).x;
                    if (overrideDistanceBlending && distanceBlendingMode == DistanceBlendMode.TilingOffset)
                        minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Override Tiling & Offset")).x;

                    // Header
                    GUIUtilities.DrawHeaderLabelLarge("Distance Blending");

                    GUILayout.BeginHorizontal();

                    // Override Distance Blending Toggle
                    string distanceBlendingEnabledStyle = overrideDistanceBlending && distanceBlendingMode == 0 ? "ButtonLeft" : "Button";
                    overrideDistanceBlending = GUILayout.Toggle(overrideDistanceBlending, GetScaledText(minScaledTextWidth, "Override Distance Blending", "ODB"), distanceBlendingEnabledStyle); // TEMP 0 VAL

                    // Override Tiling & Offset Options
                    bool endedHorizontal = false;
                    if (overrideDistanceBlending && distanceBlendingMode == DistanceBlendMode.TilingOffset) {
                        // Override Tiling & Offset Toggle
                        overrideDistanceBlendingTO = GUILayout.Toggle(overrideDistanceBlendingTO, GetScaledText(minScaledTextWidth, "Override Tiling & Offset", "OTO"), "ButtonRight"); // TEMP 0 VAL

                        endedHorizontal = true;
                        GUILayout.EndHorizontal();

                        // Override Tiling & Offset
                        if (overrideDistanceBlendingTO) {
                            MaterialProperty blendMaskDistanceTOProp = FindProperty("_BlendMaskDistanceTO");

                            GUIUtilities.DrawTilingOffset(blendMaskDistanceTOProp);
                        }
                    }

                    if (!endedHorizontal)
                        GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                }

                if (materialBlendProperties != oriMaterialBlendProperties)
                    materialBlendPropertiesProp.vectorValue = materialBlendProperties;

                // Material
                DrawMaterialGUI("Blend");
            }

            // Save compressed material blend settings
            int compressedMaterialBlendSettings = BooleanCompression.CompressValues(materialBlendingEnabled, overrideDistanceBlending, overrideDistanceBlendingTO);
            materialBlendingSettingsProp.floatValue = compressedMaterialBlendSettings;

            // End Background
            float heightDiff = GUIUtilities.EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _materialBlendBackgroundHeight = heightDiff;
        }

        private void DrawDebugGUI()
        {
            // Start Background
            float backgroundStartingYPos = GUIUtilities.StartBackground(_debugBackgroundHeight);

            // Material Property
            MaterialProperty debuggingIndexProp = FindProperty("_DebuggingIndex");

            // Debug Toggle
            bool prevDebugging = debuggingIndexProp.floatValue != -1;
            bool debugging = GUIUtilities.DrawMajorToggleButton(prevDebugging, "Debug");

            if (debugging) {
                GUILayout.Space(5);

                // If just started debugging, get previous debugging index
                if (!prevDebugging) {
                    debuggingIndexProp.floatValue = _prevDebugIndex;
                }

                // Title Label
                GUIUtilities.DrawHeaderLabelLarge("Debug Texture");

                // Selection Grid
                string[] debugValues = new string[] {
                "Voronoi Cells",
                "Edge Mask",
                "Distance Mask",
                "Blend Material Mask",
                "Variation Multiplier"
            };

                EditorGUI.BeginChangeCheck();
                float debuggingIndex = debuggingIndexProp.floatValue;
                debuggingIndex = GUILayout.SelectionGrid((int)debuggingIndex, debugValues, 1);
                if (EditorGUI.EndChangeCheck())
                    debuggingIndexProp.floatValue = debuggingIndex;
            } else if (debuggingIndexProp.floatValue != -1) {
                _prevDebugIndex = (int)debuggingIndexProp.floatValue;
                debuggingIndexProp.floatValue = -1;
            }

            // End Background
            float heightDiff = GUIUtilities.EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _debugBackgroundHeight = heightDiff;
        }
        #endregion

    }
}
#endif