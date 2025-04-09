using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TextureArrayEssentials.Compression;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    using Variables;

    public class SeamlessMaterialGUI : ShaderGUI
    {
        #region Variables
        
        internal class MaterialFoldoutState
        {
            public bool MainProperties = true;
            public bool NoiseProperties = true;
            public bool VariationProperties = true;
        }

        // Constants
        internal const int HEADER_PADDING = 4;
        internal const int SETTING_SPACING = 4;

        internal const int SCALED_TEXT_PADDING = 10;

        // Material Helpers
        internal Material _material;
        internal MaterialEditor _editor;
        internal Dictionary<string, MaterialProperty> _cachedProperties = new Dictionary<string, MaterialProperty>();

        // Foldout States, dynamically adds new materialPrefixes
        internal Dictionary<string, MaterialFoldoutState> _foldoutStates = new Dictionary<string, MaterialFoldoutState>();

        // Debug
        private int _prevDebugIndex = 0;

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
            return Screen.width <= minWidth + SCALED_TEXT_PADDING ? smallText : largeText;
        }

        private Dictionary<string, MaterialProperty> GetMaterialProperties(MaterialProperty[] properties)
        {
            Dictionary<string, MaterialProperty> cachedProperties = new Dictionary<string, MaterialProperty>();
            foreach (MaterialProperty property in properties) {
                cachedProperties.Add(property.name, property);
            }

            return cachedProperties;
        }

        // Helper to draw textures for child classes
        // Require section and texture indexes for child classes to modify individual texture drawing
        protected virtual Rect DrawTexture(int sectionIndex, int textureIndex, GUIContent content, MaterialProperty textureProperty)
        {
            // Draw texture property
            Rect lineRect = _editor.TexturePropertySingleLine(content, textureProperty);

            // Get and return rect after texture
            lineRect = MaterialEditor.GetRectAfterLabelWidth(lineRect);
            return lineRect;
        }
        #endregion

        #region GUI Calls
        public virtual void OnEnable(MaterialEditor materialEditor)
        {
            // Assign Material Helpers
            _material = (Material)materialEditor.target;
            _editor = materialEditor;
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
            GUIUtilities.BeginBackgroundVertical();
            DrawMaterialPropertiesGUI();
            GUIUtilities.EndBackgroundVertical();

            GUILayout.Space(SETTING_SPACING);
        }
        #endregion

        #region Material GUI
        // Top properties, general material settings
        protected virtual void DrawMaterialPropertiesGUI()
        {
            // Header
            GUIUtilities.DrawHeaderLabelLarge($"Material Properties");

            GUILayout.Space(4);

            // Surface Type
            MaterialProperty surfaceTypeProp = FindProperty("_SurfaceType");

            EditorGUI.BeginChangeCheck();
            ESurfaceType surfaceType = (ESurfaceType)surfaceTypeProp.floatValue;
            surfaceType = (ESurfaceType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Surface Type", surfaceType);

            if (EditorGUI.EndChangeCheck()) {
                surfaceTypeProp.floatValue = (int)surfaceType;

                switch (surfaceType) {
                    case ESurfaceType.Opaque:
                        _material.renderQueue = (int)RenderQueue.Geometry;
                        _material.SetOverrideTag("RenderType", "Opaque");
                        _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 0.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        _material.SetInt("_BUILTIN_ZWrite", 1);
                        break;
                    case ESurfaceType.Cutout:
                        _material.renderQueue = (int)RenderQueue.AlphaTest;
                        _material.SetOverrideTag("RenderType", "TransparentCutout");
                        _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 0.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        _material.SetInt("_BUILTIN_ZWrite", 1);
                        break;
                    case ESurfaceType.Transparent:
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
            _editor.LightmapEmissionProperty();
            _editor.RenderQueueField();
            _editor.DoubleSidedGIField();
        }
        #endregion

        #region Base Material
        protected virtual void DrawMaterialGUI(string materialPrefix, int sectionIndex, string headerText = "")
        {
            // Setup Foldouts
            if (!_foldoutStates.ContainsKey(materialPrefix))
                _foldoutStates.Add(materialPrefix, new MaterialFoldoutState());

            // Title Label
            if (headerText == "")
                headerText = $"{materialPrefix} Material";

            GUIUtilities.DrawHeaderLabelLarge(headerText);

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
                DrawMaterialMainProperties(materialPrefix, sectionIndex);

            // Settings
            MaterialProperty settingsProp = FindProperty($"_{materialPrefix}Settings");
            int settingToggles = (int)settingsProp.vectorValue.x;
            bool noiseEnabled = BooleanCompression.GetValue(settingToggles, 0);
            bool variationEnabled = BooleanCompression.GetValue(settingToggles, 4);

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
                    DrawMaterialVariationProperties(materialPrefix, sectionIndex);
            }
        }

        protected virtual void DrawMaterialSettingsGUI(string materialPrefix, bool showNoise = true, bool showVariation = true, bool showPT = true, bool showEmission = true, bool showSR = true)
        {
            // Material Properties
            MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}Settings");

            // Get variables from settings prop
            int settingToggles = (int)settingTogglesProp.vectorValue.x;
            bool noiseEnabled = BooleanCompression.GetValue(settingToggles, 0);
            bool randomiseScaling = BooleanCompression.GetValue(settingToggles, 1);
            bool randomiseRotation = BooleanCompression.GetValue(settingToggles, 2);
            bool smoothnessEnabled = BooleanCompression.GetValue(settingToggles, 3);
            bool variationEnabled = BooleanCompression.GetValue(settingToggles, 4);
            bool packedTexture = BooleanCompression.GetValue(settingToggles, 5);
            bool emissionEnabled = BooleanCompression.GetValue(settingToggles, 6);

            // Calculate scaled text min width
            int minScaledTextWidth = 0;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Noise")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Variation")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Packed Texture")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Emission")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Smooth")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Rough")).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Clear Tex")).x;
            if (noiseEnabled) {
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Random Scaling")).x;
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Random Rotation")).x;
            }

            EditorGUILayout.BeginHorizontal();

            // Noise Enabled
            if (showNoise) {
                string noiseEnabledStyle = noiseEnabled ? "ButtonLeft" : "Button";
                noiseEnabled = GUILayout.Toggle(noiseEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Noise", "N"), "Adds random scaling & rotation based on voronoi noise"), noiseEnabledStyle);

                if (noiseEnabled) {
                    // Noise Scaling Enabled
                    randomiseScaling = GUILayout.Toggle(randomiseScaling, new GUIContent(GetScaledText(minScaledTextWidth, "Random Scaling", "RS"), "Adds random scaling to each voronoi cell"), "ButtonMid");

                    // Randomise Rotation Enabled
                    randomiseRotation = GUILayout.Toggle(randomiseRotation, new GUIContent(GetScaledText(minScaledTextWidth, "Random Rotation", "RR"), "Adds random rotation to each voronoi cell"), "ButtonRight");
                }
            }

            // Variation toggle
            if(showVariation)
                variationEnabled = GUILayout.Toggle(variationEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Variation", "V"), "Adds random variation on top of the albedo color\n\nUsing a custom texture can cause visible tiling"), "Button");

            GUILayout.FlexibleSpace();

            // Packed Texture Toggle
            if(showPT)
                packedTexture = GUILayout.Toggle(packedTexture, new GUIContent(GetScaledText(minScaledTextWidth, "Packed Texture", "PT"), "If you are using a packed texture of multiple regular ones (Enabled is default unity material behaviour)\n\nPacked: (R: Metallic, G: Occlussion, A: Smoothness/Roughness)\n\nNon-Packed uses Red channel for each texture"), "Button");

            // Emission Toggle
            if(showEmission)
                emissionEnabled = GUILayout.Toggle(emissionEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Emission", "E"), "If Emission is enabled"), "Button");

            // Smoothness/Roughness Toggle
            if (showSR) {
                EditorGUI.BeginChangeCheck();
                float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
                srSelected = GUILayout.Toolbar((int)srSelected, new GUIContent[] { new GUIContent(GetScaledText(minScaledTextWidth, "Smooth", "S"), "Using smoothness for material (Default unity material behaviour)"), new GUIContent(GetScaledText(minScaledTextWidth, "Rough", "R"), "Uses roughness for material (1 - smoothness)") });
                if (EditorGUI.EndChangeCheck())
                    smoothnessEnabled = srSelected == 1.0f ? false : true;
            }

            //Debug.Log("CLEARNING TEXTURE2DARRAY HERE");
            //if (GUILayout.Button(new GUIContent(GetScaledText(minScaledTextWidth, "Clear Tex", "X"), "Clear the Texture2DArray holding the textures")) && EditorUtility.DisplayDialog("Clear Texture2DArray", $"Are you sure?\nYou will have to reassign all the {materialPrefix} textures\nCan be used to change the resolution of your textures", "Clear", "Cancel")) {
            //    string assetsPath = Application.dataPath;
            //    assetsPath = assetsPath.Substring(0, assetsPath.LastIndexOf("/")); // Remove "/Assets", included in filePath
            //
            //    string filePath = AssetDatabase.GetAssetPath(_editor.target);
            //    filePath = filePath.Substring(0, filePath.LastIndexOf("/"));
            //    filePath = $"{filePath}/SeamlessMaterialData/TextureArray.asset";
            //
            //    if (System.IO.File.Exists($"{assetsPath}/{filePath}")) {
            //        AssetDatabase.DeleteAsset(filePath);
            //        //for (int i = 0; i < textures.Length; i++) {
            //        //    textures[i] = null;
            //        //}
            //    }
            //}

            EditorGUILayout.EndHorizontal();

            // Enabled Settings, for the shader to determine whether to use textures or values
            // Storing inside of int instead of multiple bools so its only one variable, less to manage
            int compressedSettingToggles = BooleanCompression.CompressValues(noiseEnabled, randomiseScaling, randomiseRotation, smoothnessEnabled, variationEnabled, packedTexture, emissionEnabled);
            settingTogglesProp.vectorValue = new Vector2(compressedSettingToggles, settingTogglesProp.vectorValue.y);
        }

        protected virtual void DrawMaterialMainProperties(string materialPrefix, int sectionIndex)
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
            bool smoothnessEnabled = BooleanCompression.GetValue(settingToggles, 3);
            bool packedTexture = BooleanCompression.GetValue(settingToggles, 5);
            bool emissionEnabled = BooleanCompression.GetValue(settingToggles, 6);

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
            Rect albedoTintRect = DrawTexture(sectionIndex, 0, new GUIContent("Albedo", "Albedo (RGB), Transparency (A)"), albedoTexProp);
            _editor.ColorProperty(albedoTintRect, abledoTintProp, "");

            //_editor.TexturePropertySingleLine(new GUIContent("Albedo", "Albedo (RGB), Transparency (A)"), albedoTexProp, abledoTintProp);

            // Metallic
            Rect metallicSliderRect = DrawTexture(sectionIndex, 1, new GUIContent("Metallic", "Metallic (R), other channels are ignored"), metallicTexProp);
            if (!metallicAssigned)
                materialProperties1.x = EditorGUI.Slider(metallicSliderRect, materialProperties1.x, 0, 1);

            //materialProperties1.x = GUIUtilities.DrawTexturePropertyWithSlider(_editor, metallicTexProp, !metallicAssigned, materialProperties1.x, new GUIContent("Metallic", "Metallic (R), other channels are ignored"));

            // Smoothness/Roughness
            float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
            switch (srSelected) {
                case 0: // Smoothness
                    Rect smoothnessSliderRect = DrawTexture(sectionIndex, 2, new GUIContent("Smoothness", $"Smoothness ({(packedTexture ? 'A' : 'R')}), other channels are ignored"), smoothnessTexProp);
                    if (!smoothnessAssigned)
                        materialProperties1.y = EditorGUI.Slider(smoothnessSliderRect, materialProperties1.y, 0, 1);

                    //materialProperties1.y = GUIUtilities.DrawTexturePropertyWithSlider(_editor, smoothnessTexProp, !smoothnessAssigned, materialProperties1.y, new GUIContent("Smoothness", $"Smoothness ({(packedTexture ? 'A' : 'R')}), other channels are ignored"));

                    break;
                case 1: // Roughness
                    Rect roughnessSliderRect = DrawTexture(sectionIndex, 3, new GUIContent("Roughness", $"Roughness ({(packedTexture ? 'A' : 'R')}), other channels are ignored"), roughnessTexProp);
                    if (!roughnessAssigned)
                        materialProperties1.z = EditorGUI.Slider(roughnessSliderRect, materialProperties1.z, 0, 1);

                    //materialProperties1.z = GUIUtilities.DrawTexturePropertyWithSlider(_editor, roughnessTexProp, !roughnessAssigned, materialProperties1.z, new GUIContent("Roughness", $"Roughness ({(packedTexture ? 'A' : 'R')}), other channels are ignored"));

                    break;
            }

            // Normal Map
            Rect normalStrengthSliderRect = DrawTexture(sectionIndex, 4, new GUIContent("Normal Map"), normalTexProp);
            if (normalAssigned)
                materialProperties1.w = EditorGUI.Slider(normalStrengthSliderRect, materialProperties1.w, 0, 1);

            //materialProperties1.w = GUIUtilities.DrawTexturePropertyWithSlider(_editor, normalTexProp, normalAssigned, materialProperties1.w, new GUIContent("Normal Map"));

            // Occlussion Map
            Rect occlussionStrengthSliderRect = DrawTexture(sectionIndex, 5, new GUIContent("Occlussion", $"Occlussion ({(packedTexture ? 'G' : 'R')}), other channels are ignored"), occlussionTexProp);
            if (occlussionAssigned)
                materialProperties2.x = EditorGUI.Slider(occlussionStrengthSliderRect, materialProperties2.x, 0, 1);

            //materialProperties2.x = GUIUtilities.DrawTexturePropertyWithSlider(_editor, occlussionTexProp, occlussionAssigned, materialProperties2.x, new GUIContent("Occlussion", $"Occlussion ({(packedTexture ? 'G' : 'R')}), other channels are ignored"));

            // Emission
            if (emissionEnabled) {
                EditorGUI.BeginChangeCheck();
                Texture oldEmissionTex = emissionTexProp.textureValue;

                Rect emissionColourRect = DrawTexture(sectionIndex, 6, new GUIContent("Emission", "Emission (RGB)"), emissionTexProp);
                Color emissionColour = EditorGUI.ColorField(emissionColourRect, GUIContent.none, emissionColorProp.colorValue, true, false, true);

                //_editor.TexturePropertyWithHDRColor(new GUIContent("Emission", "Emission (RGB)"), emissionTexProp, emissionColorProp, false);
                
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

        protected virtual void DrawMaterialNoiseGUI(string materialPrefix)
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
                scalingMinMax = GUIUtilities.DrawVector2Field(scalingMinMax, new GUIContent("Noise Scaling Min Max", "(x: Min Scale, y: Max Scale)\n\nRange that each voronoi cell is randomly scaled by"));
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
                randomiseRotationMinMax = GUIUtilities.DrawVector2Field(randomiseRotationMinMax, new GUIContent("Random Rotation Min Max", "(x: Min Rotation Degrees, y: Max Rotation Degrees)\n\nRange that each voronoi cell is randomly rotated by"));
                if (EditorGUI.EndChangeCheck())
                    noiseMinMax = new Vector4(noiseMinMax.x, noiseMinMax.y, randomiseRotationMinMax.x, randomiseRotationMinMax.y);
            }

            // Assign Properties
            if (noiseSettings != oriNoiseSettings)
                noiseSettingsProp.vectorValue = noiseSettings;
            if (noiseMinMax != oriNoiseMinMax)
                noiseMinMaxProp.vectorValue = noiseMinMax;
        }

        protected virtual void DrawMaterialVariationProperties(string materialPrefix, int sectionIndex)
        {
            // Material Properties
            MaterialProperty variationModeProp = FindProperty($"_{materialPrefix}VariationMode");
            MaterialProperty variationSettingsProp = FindProperty($"_{materialPrefix}VariationSettings");
            MaterialProperty variationBrightnessProp = FindProperty($"_{materialPrefix}VariationBrightness");

            Vector4 variationSettings = variationSettingsProp.vectorValue;
            Vector4 oriVariationSettings = variationSettings;

            // Variation Mode
            EditorGUI.BeginChangeCheck();
            ETextureType variationMode = (ETextureType)variationModeProp.floatValue;
            variationMode = (ETextureType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), new GUIContent("Variation Mode", "Using a custom texture can cause visible tiling"), variationMode);
            if (EditorGUI.EndChangeCheck())
                variationModeProp.floatValue = (int)variationMode;

            // Opacity
            variationSettings.x = EditorGUI.Slider(GUIUtilities.GetLineRect(), new GUIContent("Opacity", "Transparency of the variation"), variationSettings.x, 0, 1);

            // Brightness
            EditorGUI.BeginChangeCheck();
            float variationBrightness = variationBrightnessProp.floatValue;
            variationBrightness = EditorGUI.Slider(GUIUtilities.GetLineRect(), new GUIContent("Brightness", "Intensity of the variation"), variationBrightness, 0, 1);
            if (EditorGUI.EndChangeCheck())
                variationBrightnessProp.floatValue = variationBrightness;

            // Scaling
            variationSettings.y = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Small Scale", "Scale of the small variation sample"), variationSettings.y);
            variationSettings.z = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Medium Scale", "Scale of the medium variation sample"), variationSettings.z);
            variationSettings.w = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Large Scale", "Scale of the large variation sample"), variationSettings.w);

            if (variationMode != ETextureType.CustomTexture) { // Noise
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
                noiseOffset = GUIUtilities.DrawVector2Field(noiseOffset, new GUIContent("Noise Offset"));
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
                DrawTexture(sectionIndex, 7, new GUIContent("Variation Texture", "Variation (R), other channels are ignored\n\nTexture that is drawn onto other materials, can cause visible tiling"), variationTexturesProp);
                
                //_editor.TexturePropertySingleLine(new GUIContent("Variation Texture", "Variation (R), other channels are ignored\n\nTexture that is drawn onto other materials, can cause visible tiling"), variationTexturesProp);

                // Tiling & Offset
                GUIUtilities.DrawTilingOffset(variationTextureTOProp, "Variation Scale", "Variation Offset");
            }

            // Assign Property
            if (variationSettings != oriVariationSettings)
                variationSettingsProp.vectorValue = variationSettings;
        }
        #endregion

        protected virtual void DrawBaseMaterialGUI(string propertiesPrefix = "")
        {
            GUIUtilities.BeginBackgroundVertical();

            DrawMaterialGUI($"{propertiesPrefix}Base", 0, "Base Material");

            GUIUtilities.EndBackgroundVertical();
        }

        protected virtual void DrawDistanceBlendGUI(string propertiesPrefix = "")
        {
            // Material Property
            MaterialProperty distanceBlendEnabledProp = FindProperty($"_{propertiesPrefix}DistanceBlendEnabled");

            // Start Background
            GUIUtilities.BeginBackgroundVertical();

            // Distance Blend Enabled Toggle
            bool distanceBlendEnabled = GUIUtilities.DrawMajorToggleButton(distanceBlendEnabledProp, "Distance Blending");

            // Draw distance blending settings
            if (distanceBlendEnabled) {
                // Material Properties
                MaterialProperty distanceBlendModeProp = FindProperty($"_{propertiesPrefix}DistanceBlendMode");
                MaterialProperty distanceBlendMinMaxProp = FindProperty($"_{propertiesPrefix}DistanceBlendMinMax");

                GUILayout.Space(5);

                // Distance Blend Mode
                EditorGUI.BeginChangeCheck();
                EDistanceBlendMode distanceBlendMode = (EDistanceBlendMode)distanceBlendModeProp.floatValue;
                distanceBlendMode = (EDistanceBlendMode)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), new GUIContent("Blend Mode", "Tiling & Offset: Resamples materials with defined Tiling & Offset\nMaterial: Samples far material"), distanceBlendMode);
                if (EditorGUI.EndChangeCheck())
                    distanceBlendModeProp.floatValue = (int)distanceBlendMode;

                // Distance Blend Min Max
                EditorGUI.BeginChangeCheck();
                Vector2 distanceBlendMinMax = new Vector2(distanceBlendMinMaxProp.vectorValue.x, distanceBlendMinMaxProp.vectorValue.y);
                distanceBlendMinMax = GUIUtilities.DrawVector2Field(distanceBlendMinMax, new GUIContent("Distance Blend Min Max", "(x: Min Distance, y: Max Distance)\n\nBlend distance which the material will be sampled. Materials will be blended with regular material at min, and far material at max"));
                if (EditorGUI.EndChangeCheck()) {
                    if (distanceBlendMinMax.x < 0) distanceBlendMinMax.x = 0;
                    if (distanceBlendMinMax.y < 0) distanceBlendMinMax.y = 0;

                    distanceBlendMinMaxProp.vectorValue = distanceBlendMinMax;
                }

                switch (distanceBlendMode) {
                    case EDistanceBlendMode.TilingOffset:
                        MaterialProperty tilingOffsetProp = FindProperty($"_{propertiesPrefix}FarTilingOffset");

                        // Tiling & Offset GUI
                        GUIUtilities.DrawTilingOffset(tilingOffsetProp);
                        break;
                    case EDistanceBlendMode.Material:
                        GUILayout.Space(10);

                        // Material GUI
                        DrawMaterialGUI($"{propertiesPrefix}Far", 1, "Far Material");
                        break;
                }
            }

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }

        protected virtual void DrawMaterialBlendGUI(string propertiesPrefix = "")
        {
            // Start Background
            GUIUtilities.BeginBackgroundVertical();

            // Material Property
            MaterialProperty materialBlendingSettingsProp = FindProperty($"_{propertiesPrefix}MaterialBlendSettings");

            int materialBlendingSettings = (int)materialBlendingSettingsProp.floatValue;
            bool materialBlendingEnabled = BooleanCompression.GetValue(materialBlendingSettings, 0);
            bool overrideDistanceBlend = BooleanCompression.GetValue(materialBlendingSettings, 1);
            bool overrideDistanceBlendTO = BooleanCompression.GetValue(materialBlendingSettings, 2);

            // Material Blend Enabled Toggle
            materialBlendingEnabled = GUIUtilities.DrawMajorToggleButton(materialBlendingEnabled, "Material Blending");

            if (materialBlendingEnabled) {
                // Material Properties
                MaterialProperty blendMaskTypeProp = FindProperty($"_{propertiesPrefix}BlendMaskType");
                MaterialProperty distanceBlendEnabledProp = FindProperty($"_{propertiesPrefix}DistanceBlendEnabled");

                MaterialProperty materialBlendPropertiesProp = FindProperty($"_{propertiesPrefix}MaterialBlendProperties");

                Vector2 materialBlendProperties = materialBlendPropertiesProp.vectorValue;
                Vector2 oriMaterialBlendProperties = materialBlendProperties;

                // Mask
                GUIUtilities.DrawHeaderLabelLarge("Mask");

                EditorGUI.BeginChangeCheck();
                ETextureType blendMaskType = (ETextureType)blendMaskTypeProp.floatValue;
                blendMaskType = (ETextureType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Mask Type", blendMaskType);
                if (EditorGUI.EndChangeCheck())
                    blendMaskTypeProp.floatValue = (int)blendMaskType;

                materialBlendProperties.x = EditorGUI.Slider(GUIUtilities.GetLineRect(), new GUIContent("Mask Opacity", "Opacity of the mask and in response the blend material"), materialBlendProperties.x, 0, 1);
                materialBlendProperties.y = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Mask Strength", "The higher the value, the sharper the edges and vice versa"), materialBlendProperties.y);

                if (blendMaskType != ETextureType.CustomTexture) { // Noise
                    // Material Properties
                    MaterialProperty materialBlendNoiseSettingsProp = FindProperty($"_{propertiesPrefix}MaterialBlendNoiseSettings");

                    Vector3 materialBlendNoiseSettings = materialBlendNoiseSettingsProp.vectorValue;
                    Vector3 oriMaterialBlendNoiseSettings = materialBlendNoiseSettings;

                    // Scale
                    float noiseScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", materialBlendNoiseSettings.x);

                    // Offset
                    Vector2 noiseOffset = GUIUtilities.DrawVector2Field(new Vector2(materialBlendNoiseSettings.y, materialBlendNoiseSettings.z), new GUIContent("Noise Offset"));

                    materialBlendNoiseSettings = new Vector3(noiseScale, noiseOffset.x, noiseOffset.y);

                    if (materialBlendNoiseSettings != oriMaterialBlendNoiseSettings)
                        materialBlendNoiseSettingsProp.vectorValue = materialBlendNoiseSettings;
                } else { // Custom Texture
                    // Material Properties
                    MaterialProperty blendMaskTextureProp = FindProperty($"_{propertiesPrefix}BlendMaskTexture");
                    MaterialProperty blendMaskTextureTOProp = FindProperty($"_{propertiesPrefix}BlendMaskTextureTO");

                    // Texture
                    _editor.TexturePropertySingleLine(new GUIContent("Blend Mask", "Blend Mask (R), other channels are ignored\n\nTexture that is sampled as the mask for the blend material. Color from black-white represents opacity (0-1)"), blendMaskTextureProp);

                    // Scale & Offset
                    GUIUtilities.DrawTilingOffset(blendMaskTextureTOProp);
                }

                GUILayout.Space(10);

                // Distance Blending
                bool distanceBlendEnabled = distanceBlendEnabledProp.floatValue == 1 ? true : false;

                if (distanceBlendEnabled) {
                    // Material Property
                    MaterialProperty distanceBlendModeProp = FindProperty($"_{propertiesPrefix}DistanceBlendMode");
                    EDistanceBlendMode distanceBlendMode = (EDistanceBlendMode)distanceBlendModeProp.floatValue;

                    // Calculate scaled text min width
                    int minScaledTextWidth = 0;
                    minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Override Distance Blending")).x;
                    if (overrideDistanceBlend && distanceBlendMode == EDistanceBlendMode.TilingOffset)
                        minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Override Tiling & Offset")).x;

                    // Header
                    GUIUtilities.DrawHeaderLabelLarge("Distance Blending");

                    GUILayout.BeginHorizontal();

                    // Override Distance Blending Toggle
                    string distanceBlendEnabledStyle = overrideDistanceBlend && distanceBlendMode == 0 ? "ButtonLeft" : "Button";
                    overrideDistanceBlend = GUILayout.Toggle(overrideDistanceBlend, new GUIContent(GetScaledText(minScaledTextWidth, "Override Distance Blending", "ODB"), "Draws the blend material on top of the far material"), distanceBlendEnabledStyle);

                    // Override Tiling & Offset Options
                    bool endedHorizontal = false;
                    if (overrideDistanceBlend && distanceBlendMode == EDistanceBlendMode.TilingOffset) {
                        // Override Tiling & Offset Toggle
                        overrideDistanceBlendTO = GUILayout.Toggle(overrideDistanceBlendTO, new GUIContent(GetScaledText(minScaledTextWidth, "Override Tiling & Offset", "OTO"), "Uses defined Tiling & Offset rather than distance blend Tiling & Offset"), "ButtonRight");

                        endedHorizontal = true;
                        GUILayout.EndHorizontal();

                        // Override Tiling & Offset
                        if (overrideDistanceBlendTO) {
                            MaterialProperty blendMaskDistanceTOProp = FindProperty($"_{propertiesPrefix}BlendMaskDistanceTO");

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
                DrawMaterialGUI($"{propertiesPrefix}Blend", 2, "Blend Material");
            }

            // Save compressed material blend settings
            int compressedMaterialBlendSettings = BooleanCompression.CompressValues(materialBlendingEnabled, overrideDistanceBlend, overrideDistanceBlendTO);
            materialBlendingSettingsProp.floatValue = compressedMaterialBlendSettings;

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }

        protected virtual void DrawDebugGUI()
        {
            // Start Background
            GUIUtilities.BeginBackgroundVertical();

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
            GUIUtilities.EndBackgroundVertical();
        }
    }
}
#endif