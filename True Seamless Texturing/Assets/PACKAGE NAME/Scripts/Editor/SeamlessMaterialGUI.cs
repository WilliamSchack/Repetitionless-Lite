using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    using Compression;
    using Variables;

    public class SeamlessMaterialGUI : ShaderGUI
    {
        #region Variables
        // Custom Class
        private class MaterialFoldoutState
        {
            public bool MainProperties = true;
            public bool NoiseProperties = true;
            public bool VariationProperties = true;
        }

        // Constants
        internal const int HEADER_PADDING = 4;
        internal const int SETTING_SPACING = 4;

        internal const int BACKGROUND_HEIGHT_DISABLED_SETTING = 29;
        internal const int BACKGROUND_HEIGHT_PROPERTIES = 96;

        internal const int BACKGROUND_HEIGHT_HEADERSETTINGS = 47;

        internal const int BACKGROUND_HEIGHT_COLLAPSED_FOLDOUT = 24;
        internal const int BACKGROUND_HEIGHT_MAIN_FOLDOUT = 200;
        internal const int BACKGROUND_HEIGHT_NOISE_FOLDOUT = 112;
        internal const int BACKGROUND_HEIGHT_VARIATION_FOLDOUT = 222;

        internal const int SCALED_TEXT_PADDING = 5;

        // Material Helpers
        internal Material _material;
        internal MaterialEditor _editor;
        internal Dictionary<string, MaterialProperty> _cachedProperties = new Dictionary<string, MaterialProperty>();

        // Background Heights
        // Rough solution as it only works properly on second call of OnGUI but its better then estimating and fiddling around with the height
        // Drawing a box after calculating height of area would draw ontop of other fields, this will draw behind
        internal float _propertiesBackgroundHeight;

        // Foldout States, dynamically adds new materialPrefixes
        private Dictionary<string, MaterialFoldoutState> _foldoutStates = new Dictionary<string, MaterialFoldoutState>();

        // ShaderGUI doesnt have an OnEnable function, using this instead
        private bool _firstSetup = true;
        #endregion

        #region Helpers
        internal MaterialProperty FindProperty(string name)
        {
            return _cachedProperties[name];
        }

        internal string GetScaledText(int minWidth, string largeText, string smallText)
        {
            // Using screen width so it is accurate in both layout and repaint events
            float areaWidth = Screen.width - GUIUtilities.BACKGROUND_SIDE_PADDING * 6 - GUIUtilities.SIDE_EMPTY_SPACE_WIDTH; // Multiplying side padding by 2 would make sense but only 6 is accurate for some reason idk
            return areaWidth <= minWidth + SCALED_TEXT_PADDING ? smallText : largeText;
        }

        private Dictionary<string, MaterialProperty> GetMaterialProperties(MaterialProperty[] properties)
        {
            Dictionary<string, MaterialProperty> cachedProperties = new Dictionary<string, MaterialProperty>();
            foreach (MaterialProperty property in properties) {
                cachedProperties.Add(property.name, property);
            }

            return cachedProperties;
        }
        #endregion

        #region GUI Calls
        public virtual void OnEnable(MaterialEditor materialEditor)
        {
            // Assign Material Helpers
            _material = (Material)materialEditor.target;
            _editor = materialEditor;

            // Make Terrain Compatable (removes warning)
            // Note: Will still have warning complaining about tangeont geometry but it still works fine
            //_material.SetOverrideTag("TerrainCompatible", "True");
            // CREATE SEPERATE SHADER FOR TERRAIN
            // REQUIRES TEXTURE PAINTING AND THOSE SHENANIGANS

            _propertiesBackgroundHeight = BACKGROUND_HEIGHT_PROPERTIES;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Cache properties into dict, can change each call, faster than FindProperty (loops through all properties each call)
            _cachedProperties = GetMaterialProperties(properties);
            
            // OnEnable if first call
            if (_firstSetup) {
                OnEnable(materialEditor);
                _firstSetup = false;
            }
            
            // Make Vectors One Line
            EditorGUIUtility.wideMode = true;
            
            GUILayout.Space(HEADER_PADDING);
            
            // Material Properties
            DrawMaterialPropertiesGUI();
            
            GUILayout.Space(SETTING_SPACING);
        }
        #endregion

        #region Material GUI
        private void DrawMaterialPropertiesGUI()
        {
            // Start Background
            float backgroundStartingYPos = GUIUtilities.StartBackground(_propertiesBackgroundHeight);

            // Header
            GUIUtilities.DrawHeaderLabelLarge($"Material Properties");

            GUILayout.Space(4);

            // Surface Type
            MaterialProperty surfaceTypeProp = FindProperty("_SurfaceType");

            EditorGUI.BeginChangeCheck();
            SurfaceType surfaceType = (SurfaceType)surfaceTypeProp.floatValue;
            surfaceType = (SurfaceType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Surface Type", surfaceType);

            if (EditorGUI.EndChangeCheck()) {
                surfaceTypeProp.floatValue = (int)surfaceType;

                switch (surfaceType) {
                    case SurfaceType.Opaque:
                        _material.renderQueue = (int)RenderQueue.Geometry;
                        _material.SetOverrideTag("RenderType", "Opaque");
                        _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 0.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        _material.SetInt("_BUILTIN_ZWrite", 1);
                        break;
                    case SurfaceType.Cutout:
                        _material.renderQueue = (int)RenderQueue.AlphaTest;
                        _material.SetOverrideTag("RenderType", "TransparentCutout");
                        _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 0.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        _material.SetInt("_BUILTIN_ZWrite", 1);
                        break;
                    case SurfaceType.Transparent:
                        _material.renderQueue = (int)RenderQueue.Transparent;
                        _material.SetOverrideTag("RenderType", "Transparent");
                        _material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 1.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.SrcAlpha);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        _material.SetInt("_BUILTIN_ZWrite", 0);
                        break;
                }
            }

            // Advanced Options
            _editor.RenderQueueField();
            _editor.DoubleSidedGIField();

            // End Background
            float heightDiff = GUIUtilities.EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _propertiesBackgroundHeight = heightDiff;
        }

        internal void DrawMaterialGUI(string materialPrefix)
        {
            // Setup Foldouts
            if (!_foldoutStates.ContainsKey(materialPrefix))
                _foldoutStates.Add(materialPrefix, new MaterialFoldoutState());

            // Title Label
            GUIUtilities.DrawHeaderLabelLarge($"{materialPrefix} Material");

            // Draw Settings Toggles
            DrawMaterialSettingsGUI(materialPrefix);

            // Draw Main Properties Foldout
            EditorGUI.BeginChangeCheck();
            bool mainPropertiesFoldout = _foldoutStates[materialPrefix].MainProperties;
            mainPropertiesFoldout = GUIUtilities.DrawFoldout(mainPropertiesFoldout, "Main Properties");
            if (EditorGUI.EndChangeCheck())
                _foldoutStates[materialPrefix].MainProperties = mainPropertiesFoldout;

            // Draw Main Properties
            if (mainPropertiesFoldout)
                DrawMaterialMainProperties(materialPrefix);

            // Settings
            MaterialProperty settingsProp = FindProperty($"_{materialPrefix}Settings");
            int settingToggles = (int)settingsProp.vectorValue.x;
            bool noiseEnabled = BooleanCompression.GetCompressedValue(settingToggles, 0);
            bool variationEnabled = BooleanCompression.GetCompressedValue(settingToggles, 4);

            // Draw Noise Properties
            if (noiseEnabled) {
                // Foldout
                EditorGUI.BeginChangeCheck();
                bool noisePropertiesFoldout = _foldoutStates[materialPrefix].NoiseProperties;
                noisePropertiesFoldout = GUIUtilities.DrawFoldout(noisePropertiesFoldout, "Noise Properties");
                if (EditorGUI.EndChangeCheck())
                    _foldoutStates[materialPrefix].NoiseProperties = noisePropertiesFoldout;

                // Properties
                if (noisePropertiesFoldout)
                    DrawMaterialNoiseGUI(materialPrefix);
            }

            // Draw Variation properties
            if (variationEnabled) {
                // Foldout
                EditorGUI.BeginChangeCheck();
                bool variationPropertiesFoldout = _foldoutStates[materialPrefix].VariationProperties;
                variationPropertiesFoldout = GUIUtilities.DrawFoldout(variationPropertiesFoldout, "Variation Properties");
                if (EditorGUI.EndChangeCheck())
                    _foldoutStates[materialPrefix].VariationProperties = variationPropertiesFoldout;

                // Properties
                if (variationPropertiesFoldout)
                    DrawMaterialVariationProperties(materialPrefix);
            }
        }

        private void DrawMaterialSettingsGUI(string materialPrefix)
        {
            // Material Properties
            MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}Settings");

            // Get variables from settings prop
            int settingToggles = (int)settingTogglesProp.vectorValue.x;
            bool noiseEnabled = BooleanCompression.GetCompressedValue(settingToggles, 0);
            bool randomiseScaling = BooleanCompression.GetCompressedValue(settingToggles, 1);
            bool randomiseRotation = BooleanCompression.GetCompressedValue(settingToggles, 2);
            bool smoothnessEnabled = BooleanCompression.GetCompressedValue(settingToggles, 3);
            bool variationEnabled = BooleanCompression.GetCompressedValue(settingToggles, 4);

            // Calculate scaled text min width
            int minScaledTextWidth = 0;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Noise")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Variation")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Smooth")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Rough")).x;
            if (noiseEnabled) {
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Random Scaling")).x;
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Random Rotation")).x;
            }

            EditorGUILayout.BeginHorizontal();

            // Noise Enabled
            string noiseEnabledStyle = noiseEnabled ? "ButtonLeft" : "Button";
            noiseEnabled = GUILayout.Toggle(noiseEnabled, GetScaledText(minScaledTextWidth, "Noise", "N"), noiseEnabledStyle);

            if (noiseEnabled) {
                // Noise Scaling Enabled
                randomiseScaling = GUILayout.Toggle(randomiseScaling, GetScaledText(minScaledTextWidth, "Random Scaling", "RS"), "ButtonMid");

                // Randomise Rotation Enabled
                randomiseRotation = GUILayout.Toggle(randomiseRotation, GetScaledText(minScaledTextWidth, "Random Rotation", "RR"), "ButtonRight");
            }

            // Variation toggle
            variationEnabled = GUILayout.Toggle(variationEnabled, GetScaledText(minScaledTextWidth, "Variation", "V"), "Button");

            GUILayout.FlexibleSpace();

            // Smoothness/Roughness Toggle
            EditorGUI.BeginChangeCheck();
            float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
            srSelected = GUILayout.Toolbar((int)srSelected, new string[] { GetScaledText(minScaledTextWidth, "Smooth", "S"), GetScaledText(minScaledTextWidth, "Rough", "R") });
            if (EditorGUI.EndChangeCheck())
                smoothnessEnabled = srSelected == 1.0f ? false : true;

            EditorGUILayout.EndHorizontal();

            // Enabled Settings, for the shader to determine whether to use textures or values
            // Storing inside of int instead of multiple bools so its only one variable, less to manage
            int compressedSettingToggles = BooleanCompression.CompressValues(noiseEnabled, randomiseScaling, randomiseRotation, smoothnessEnabled, variationEnabled);
            settingTogglesProp.vectorValue = new Vector2(compressedSettingToggles, settingTogglesProp.vectorValue.y);
        }

        private void DrawMaterialMainProperties(string materialPrefix)
        {
            // Material Properties
            MaterialProperty surfaceTypeProp = FindProperty("_SurfaceType");

            MaterialProperty settingsProp = FindProperty($"_{materialPrefix}Settings");
            MaterialProperty tilingOffsetProp = FindProperty($"_{materialPrefix}TilingOffset");

            MaterialProperty albedoTexProp = FindProperty($"_{materialPrefix}Albedo");
            MaterialProperty metallicTexProp = FindProperty($"_{materialPrefix}MetallicMap");
            MaterialProperty smoothnessTexProp = FindProperty($"_{materialPrefix}SmoothnessMap");
            MaterialProperty roughnessTexProp = FindProperty($"_{materialPrefix}RoughnessMap");
            MaterialProperty normalTexProp = FindProperty($"_{materialPrefix}NormalMap");
            MaterialProperty occlussionTexProp = FindProperty($"_{materialPrefix}OcclussionMap");
            MaterialProperty emissionTexProp = FindProperty($"_{materialPrefix}EmissionMap");

            MaterialProperty abledoTintProp = FindProperty($"_{materialPrefix}AlbedoTint");
            MaterialProperty emissionColorProp = FindProperty($"_{materialPrefix}EmissionColor");

            MaterialProperty materialProperties1Prop = FindProperty($"_{materialPrefix}MaterialProperties1");
            MaterialProperty materialProperties2Prop = FindProperty($"_{materialPrefix}MaterialProperties2");

            Vector4 materialProperties1 = materialProperties1Prop.vectorValue;
            Vector2 materialProperties2 = materialProperties2Prop.vectorValue;
            Vector4 oriMaterialProperties1 = materialProperties1;
            Vector2 oriMaterialProperties2 = materialProperties2;

            // Get variables from settings prop
            int settingToggles = (int)settingsProp.vectorValue.x;
            bool smoothnessEnabled = BooleanCompression.GetCompressedValue(settingToggles, 3);

            // Assigned Textures, for the shader to determine whether to use textures or values
            // Storing inside of int instead of multiple bools so its only one variable, less to manage
            bool metallicAssigned = metallicTexProp.textureValue != null;     // (bits & 1)  != 0
            bool smoothnessAssigned = smoothnessTexProp.textureValue != null; // (bits & 2)  != 0
            bool roughnessAssigned = roughnessTexProp.textureValue != null;   // (bits & 4)  != 0
            bool normalAssigned = normalTexProp.textureValue != null;         // (bits & 8)  != 0
            bool occlussionAssigned = occlussionTexProp.textureValue != null; // (bits & 16) != 0
            bool emissionAssigned = emissionTexProp.textureValue != null;     // (bits & 32) != 0
            int compressedAssignedTextures = BooleanCompression.CompressValues(metallicAssigned, smoothnessAssigned, roughnessAssigned, normalAssigned, occlussionAssigned, emissionAssigned);
            settingsProp.vectorValue = new Vector2(settingToggles, compressedAssignedTextures);

            // Albedo
            _editor.TexturePropertySingleLine(new GUIContent("Albedo"), albedoTexProp, abledoTintProp);

            // Metallic
            materialProperties1.x = GUIUtilities.DrawTextureWithSlider(_editor, metallicTexProp, !metallicAssigned, materialProperties1.x, "Metallic");

            // Smoothness/Roughness
            float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
            switch (srSelected) {
                case 0: // Smoothness
                    materialProperties1.y = GUIUtilities.DrawTextureWithSlider(_editor, smoothnessTexProp, !smoothnessAssigned, materialProperties1.y, "Smoothness");

                    break;
                case 1: // Roughness
                    materialProperties1.z = GUIUtilities.DrawTextureWithSlider(_editor, roughnessTexProp, !roughnessAssigned, materialProperties1.z, "Roughness");

                    break;
            }

            // Normal Map
            materialProperties1.w = GUIUtilities.DrawTextureWithSlider(_editor, normalTexProp, normalAssigned, materialProperties1.w, "Normal Map");

            // Occlussion Map
            materialProperties2.x = GUIUtilities.DrawTextureWithSlider(_editor, occlussionTexProp, occlussionAssigned, materialProperties2.x, "Occlussion");

            // Emission
            EditorGUI.BeginChangeCheck();
            Texture oldEmissionTex = emissionTexProp.textureValue;
            _editor.TexturePropertyWithHDRColor(new GUIContent("Emission"), emissionTexProp, emissionColorProp, false);
            // Change color to white if currently black when setting texture
            if (EditorGUI.EndChangeCheck() && oldEmissionTex != emissionTexProp.textureValue) {
                Color blackColor = new Color(0, 0, 0, emissionTexProp.colorValue.a);
                if (emissionColorProp.colorValue == blackColor && emissionTexProp.textureValue != null) {
                    emissionColorProp.colorValue = Color.white;
                }
            }

            // Alpha Clipping
            if (surfaceTypeProp.floatValue == 1.0f) {
                EditorGUI.BeginChangeCheck();
                float alphaClippingValue = materialProperties2.y;
                alphaClippingValue = EditorGUI.Slider(GUIUtilities.GetLineRect(), "Alpha Clipping", alphaClippingValue, 0, 1);
                if (EditorGUI.EndChangeCheck())
                    materialProperties2.y = alphaClippingValue;
            }

            // Scale & Offset
            GUIUtilities.DrawTilingOffset(tilingOffsetProp);

            // Assign Properties
            if (materialProperties1 != oriMaterialProperties1)
                materialProperties1Prop.vectorValue = materialProperties1;
            if (materialProperties2 != oriMaterialProperties2)
                materialProperties2Prop.vectorValue = materialProperties2;
        }

        private void DrawMaterialNoiseGUI(string materialPrefix)
        {
            // Material Properties
            MaterialProperty settingsProp = FindProperty($"_{materialPrefix}Settings");

            MaterialProperty noiseSettingsProp = FindProperty($"_{materialPrefix}NoiseSettings");
            MaterialProperty noiseMinMaxProp = FindProperty($"_{materialPrefix}NoiseMinMax");

            Vector2 noiseSettings = noiseSettingsProp.vectorValue;
            Vector4 noiseMinMax = noiseMinMaxProp.vectorValue;
            Vector2 oriNoiseSettings = noiseSettings;
            Vector4 oriNoiseMinMax = noiseMinMax;

            // Get variables from settings prop
            int settingToggles = (int)settingsProp.vectorValue.x;
            bool randomiseNoiseScaling = (settingToggles & 2) != 0;
            bool randomiseRotation = (settingToggles & 4) != 0;

            // Angle Offset
            noiseSettings.x = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Angle Offset", noiseSettings.x);

            // Scale Randomising
            if (randomiseNoiseScaling) {
                // Scale
                noiseSettings.y = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", noiseSettings.y);

                // Scaling Min Max
                EditorGUI.BeginChangeCheck();
                Vector2 scalingMinMax = new Vector2(noiseMinMax.x, noiseMinMax.y);
                scalingMinMax = GUIUtilities.DrawVector2Field(scalingMinMax, "Noise Scaling Min Max");
                if (EditorGUI.EndChangeCheck()) {
                    if (scalingMinMax.x < 0) scalingMinMax.x = 0;
                    if (scalingMinMax.y < 0) scalingMinMax.y = 0;

                    noiseMinMax = new Vector4(scalingMinMax.x, scalingMinMax.y, noiseMinMax.z, noiseMinMax.w);
                }
            }

            // Rotation Randomising
            if (randomiseRotation) {
                EditorGUI.BeginChangeCheck();
                Vector2 randomiseRotationMinMax = new Vector2(noiseMinMax.z, noiseMinMax.w);
                randomiseRotationMinMax = GUIUtilities.DrawVector2Field(randomiseRotationMinMax, "Random Rotation Min Max");
                if (EditorGUI.EndChangeCheck())
                    noiseMinMax = new Vector4(noiseMinMax.x, noiseMinMax.y, randomiseRotationMinMax.x, randomiseRotationMinMax.y);
            }

            // Assign Properties
            if (noiseSettings != oriNoiseSettings)
                noiseSettingsProp.vectorValue = noiseSettings;
            if (noiseMinMax != oriNoiseMinMax)
                noiseMinMaxProp.vectorValue = noiseMinMax;
        }

        private void DrawMaterialVariationProperties(string materialPrefix)
        {
            // Material Properties
            MaterialProperty variationModeProp = FindProperty($"_{materialPrefix}VariationMode");
            MaterialProperty variationSettingsProp = FindProperty($"_{materialPrefix}VariationSettings");
            MaterialProperty variationBrightnessProp = FindProperty($"_{materialPrefix}VariationBrightness");

            Vector4 variationSettings = variationSettingsProp.vectorValue;
            Vector4 oriVariationSettings = variationSettings;

            // Variation Mode
            EditorGUI.BeginChangeCheck();
            TextureType variationMode = (TextureType)variationModeProp.floatValue;
            variationMode = (TextureType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Variation Mode", variationMode);
            if (EditorGUI.EndChangeCheck())
                variationModeProp.floatValue = (int)variationMode;

            // Opacity
            variationSettings.x = EditorGUI.Slider(GUIUtilities.GetLineRect(), "Opacity", variationSettings.x, 0, 1);

            // Brightness
            EditorGUI.BeginChangeCheck();
            float variationBrightness = variationBrightnessProp.floatValue;
            variationBrightness = EditorGUI.Slider(GUIUtilities.GetLineRect(), "Brightness", variationBrightness, 0, 1);
            if (EditorGUI.EndChangeCheck())
                variationBrightnessProp.floatValue = variationBrightness;

            // Scaling
            variationSettings.y = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Small Scale", variationSettings.y);
            variationSettings.z = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Medium Scale", variationSettings.z);
            variationSettings.w = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Large Scale", variationSettings.w);

            if (variationMode != TextureType.CustomTexture) { // Noise
                // Material Property
                MaterialProperty variationNoiseSettingsProp = FindProperty($"_{materialPrefix}VariationNoiseSettings");

                Vector4 variationNoiseSettings = variationNoiseSettingsProp.vectorValue;
                Vector4 oriVariationNoiseSettings = variationNoiseSettings;

                // Strength
                variationNoiseSettings.x = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Strength", variationNoiseSettings.x);

                // Scale
                variationNoiseSettings.y = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", variationNoiseSettings.y);

                // Offset
                EditorGUI.BeginChangeCheck();
                Vector2 noiseOffset = new Vector2(variationNoiseSettings.z, variationNoiseSettings.w);
                noiseOffset = GUIUtilities.DrawVector2Field(noiseOffset, "Noise Offset");
                if (EditorGUI.EndChangeCheck())
                    variationNoiseSettings = new Vector4(variationNoiseSettings.x, variationNoiseSettings.y, noiseOffset.x, noiseOffset.y);

                // Assign Property
                if (variationNoiseSettings != oriVariationNoiseSettings)
                    variationNoiseSettingsProp.vectorValue = variationNoiseSettings;
            } else {
                // Material Properties
                MaterialProperty variationTexturesProp = FindProperty($"_{materialPrefix}VariationTexture");
                MaterialProperty variationTextureTOProp = FindProperty($"_{materialPrefix}VariationTextureTO");

                // Texture
                _editor.TexturePropertySingleLine(new GUIContent("Variation Teture"), variationTexturesProp);

                // Tiling & Offset
                GUIUtilities.DrawTilingOffset(variationTextureTOProp, "Variation Scale", "Variation Offset");
            }

            // Assign Property
            if (variationSettings != oriVariationSettings)
                variationSettingsProp.vectorValue = variationSettings;
        }
        #endregion
    }
}
#endif