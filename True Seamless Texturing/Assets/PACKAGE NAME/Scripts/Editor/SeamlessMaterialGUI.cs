using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using SeamlessMaterial.Editor;
using SeamlessMaterial.Compression;
using SeamlessMaterial.Variables;

#if UNITY_EDITOR
using UnityEditor;

public class SeamlessMaterialGUI : ShaderGUI
{
    #region Variables
    // Constants
    private const int HEADER_PADDING = 4;
    private const int SETTING_SPACING = 4;

    private const int DEFAULT_DISABLED_SETTING_BACKGROUND_HEIGHT = 33;
    private const int DEFAULT_PROPERTIES_BACKGROUND_HEIGHT = 50;
    private const int DEFAULT_BASE_BACKGROUND_HEIGHT = 260;
    private const int DEFAULT_DISTANCE_BLEND_BACKGROUND_HEIGHT = 330;
    private const int DEFAULT_MATERIAL_BLEND_BACKGROUND_HEIGHT = 0;
    private const int DEFAULT_DEBUG_BACKGROUND_HEIGHT = 150;

    private const int SMALL_TEXT_MAX_WIDTH = 510;

    // Material Helpers
    private Material _material;
    private MaterialEditor _editor;
    private Dictionary<string, MaterialProperty> _cachedProperties = new Dictionary<string, MaterialProperty>();

    // Debug
    private int _prevDebugIndex = 0;

    // Background Heights
    // Rough solution as it only works properly on second call of OnGUI but its better then estimating and fiddling around with the height
    // Drawing a box after calculating height of area would draw ontop of other fields, this will draw behind
    private float _propertiesBackgroundHeight;
    private float _baseBackgroundHeight;
    private float _distanceBlendBackgroundHeight;
    private float _materialBlendBackgroundHeight;
    private float _debugBackgroundHeight;

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;
    #endregion

    #region Helpers
    private MaterialProperty FindProperty(string name)
    {
        return _cachedProperties[name];
    }

    private string GetScaledText(string largeText, string smallText)
    {
        return Screen.width <= SMALL_TEXT_MAX_WIDTH ? smallText : largeText;
    }
    #endregion

    #region Inspector Calls
    private void OnEnable(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Assign Material Helpers
        _material = (Material)materialEditor.target;
        _editor = materialEditor;

        // Make Terrain Compatable (removes warning)
        // Note: Will still have warning complaining about tangeont geometry but it still works fine
        //_material.SetOverrideTag("TerrainCompatible", "True");
        // CREATE SEPERATE SHADER FOR TERRAIN
        // REQUIRES TEXTURE PAINTING AND THOSE SHENANIGANS

        // Setup Initial Background Heights
        MaterialProperty distanceBlendingEnabledProp = FindProperty($"_DistanceBlendingEnabled");
        MaterialProperty debuggingIndexProp = FindProperty("_DebuggingIndex");

        bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;
        bool debuggingEnabled = debuggingIndexProp.floatValue != -1 ? true : false;

        _propertiesBackgroundHeight = DEFAULT_PROPERTIES_BACKGROUND_HEIGHT;
        _baseBackgroundHeight = DEFAULT_BASE_BACKGROUND_HEIGHT;
        _distanceBlendBackgroundHeight = distanceBlendingEnabled ? DEFAULT_DISTANCE_BLEND_BACKGROUND_HEIGHT : DEFAULT_DISABLED_SETTING_BACKGROUND_HEIGHT;
        _materialBlendBackgroundHeight = DEFAULT_MATERIAL_BLEND_BACKGROUND_HEIGHT;
        _debugBackgroundHeight = debuggingEnabled ? DEFAULT_DEBUG_BACKGROUND_HEIGHT : DEFAULT_DISABLED_SETTING_BACKGROUND_HEIGHT;
    }

    private Dictionary<string, MaterialProperty> GetMaterialProperties(MaterialProperty[] properties)
    {
        Dictionary<string, MaterialProperty> cachedProperties = new Dictionary<string, MaterialProperty>();
        foreach (MaterialProperty property in properties) {
            cachedProperties.Add(property.name, property);
        }

        return cachedProperties;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Cache properties into dict, can change each call, faster than FindProperty (loops through all properties each call)
        _cachedProperties = GetMaterialProperties(properties);

        // OnEnable if first call
        if (_firstSetup) {
            OnEnable(materialEditor, properties);
            _firstSetup = false;
        }

        // Make Vectors One Line
        EditorGUIUtility.wideMode = true;

        GUILayout.Space(HEADER_PADDING);

        // Material Properties
        DrawMaterialPropertiesGUI();

        GUILayout.Space(SETTING_SPACING);

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

    #region Material Sections GUI
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
        if (EditorGUI.EndChangeCheck())
            surfaceTypeProp.floatValue = (int)surfaceType;

        if (EditorGUI.EndChangeCheck()) {
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
        MaterialProperty distanceBlendingEnabledProp = FindProperty($"_DistanceBlendingEnabled");

        // Start Background
        float backgroundStartingYPos = GUIUtilities.StartBackground(_distanceBlendBackgroundHeight);

        // Distance Blend Enabled Toggle
        bool distanceBlendingEnabled = GUIUtilities.DrawMajorToggleButton(distanceBlendingEnabledProp, "Distance Blending");

        // Draw distance blending settings
        if (distanceBlendingEnabled) {
            // Material Properties
            MaterialProperty distanceBlendModeProp   = FindProperty($"_DistanceBlendMode");
            MaterialProperty distanceBlendMinMaxProp = FindProperty($"_DistanceBlendMinMax");

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
        MaterialProperty materialBlendingSettingsProp = FindProperty($"_MaterialBlendSettings");

        int materialBlendingSettings = (int)materialBlendingSettingsProp.floatValue;
        bool materialBlendingEnabled    = BooleanCompression.GetCompressedValue(materialBlendingSettings, 0);
        bool overrideDistanceBlending   = BooleanCompression.GetCompressedValue(materialBlendingSettings, 1);
        bool overrideDistanceBlendingTO = BooleanCompression.GetCompressedValue(materialBlendingSettings, 2);

        // Material Blend Enabled Toggle
        materialBlendingEnabled = GUIUtilities.DrawMajorToggleButton(materialBlendingEnabled, "Material Blending");

        if (materialBlendingEnabled) {
            // Material Properties
            MaterialProperty blendMaskTypeProp           = FindProperty($"_BlendMaskType");
            MaterialProperty distanceBlendingEnabledProp = FindProperty($"_DistanceBlendingEnabled");

            MaterialProperty materialBlendPropertiesProp = FindProperty($"_MaterialBlendProperties");

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
                MaterialProperty materialBlendNoiseSettingsProp = FindProperty($"_MaterialBlendNoiseSettings");

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
                MaterialProperty blendMaskTextureProp   = FindProperty($"_BlendMaskTexture");
                MaterialProperty blendMaskTextureTOProp = FindProperty($"_BlendMaskTextureTO");

                // Texture
                _editor.TexturePropertySingleLine(new GUIContent("Blend Mask"), blendMaskTextureProp);

                // Scale & Offset
                GUIUtilities.DrawTilingOffset(blendMaskTextureTOProp);
            }

            GUILayout.Space(10);

            // Distance Blending
            bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;

            if (distanceBlendingEnabled) {
                MaterialProperty distanceBlendingModeProp = FindProperty($"_DistanceBlendMode");
                DistanceBlendMode distanceBlendingMode = (DistanceBlendMode)distanceBlendingModeProp.floatValue;

                GUIUtilities.DrawHeaderLabelLarge("Distance Blending");

                GUILayout.BeginHorizontal();

                string distanceBlendingEnabledStyle = overrideDistanceBlending && distanceBlendingMode == 0 ? "ButtonLeft" : "Button";
                overrideDistanceBlending = GUILayout.Toggle(overrideDistanceBlending, GetScaledText("Override Distance Blending", "ODB"), distanceBlendingEnabledStyle);

                bool endedHorizontal = false;
                if (overrideDistanceBlending && distanceBlendingMode == DistanceBlendMode.TilingOffset) {
                    overrideDistanceBlendingTO = GUILayout.Toggle(overrideDistanceBlendingTO, GetScaledText("Override Tiling & Offset", "OTO"), "ButtonRight");

                    endedHorizontal = true;
                    GUILayout.EndHorizontal();

                    if (overrideDistanceBlendingTO) {
                        MaterialProperty blendMaskDistanceTOProp = FindProperty($"_BlendMaskDistanceTO");

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

    #region Material GUI Functions
    private void DrawMaterialSettingsGUI(string materialPrefix)
    {
        // Material Properties
        MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}Settings");

        EditorGUILayout.BeginHorizontal();

        // Get variables from settings prop
        int settingToggles = (int)settingTogglesProp.vectorValue.x;
        bool noiseEnabled      = BooleanCompression.GetCompressedValue(settingToggles, 0);
        bool randomiseScaling  = BooleanCompression.GetCompressedValue(settingToggles, 1);
        bool randomiseRotation = BooleanCompression.GetCompressedValue(settingToggles, 2);
        bool smoothnessEnabled = BooleanCompression.GetCompressedValue(settingToggles, 3);
        bool variationEnabled  = BooleanCompression.GetCompressedValue(settingToggles, 4);

        // Noise Enabled
        string noiseEnabledStyle = noiseEnabled ? "ButtonLeft" : "Button";
        noiseEnabled = GUILayout.Toggle(noiseEnabled, GetScaledText("Noise", "N"), noiseEnabledStyle);

        if (noiseEnabled) {
            // Noise Scaling Enabled
            randomiseScaling = GUILayout.Toggle(randomiseScaling, GetScaledText("Random Scaling", "RS"), "ButtonMid");

            // Randomise Rotation Enabled
            randomiseRotation = GUILayout.Toggle(randomiseRotation, GetScaledText("Random Rotation", "RR"), "ButtonRight");
        }

        // Variation toggle
        variationEnabled = GUILayout.Toggle(variationEnabled, GetScaledText("Variation", "V"), "Button");

        GUILayout.FlexibleSpace();

        // Smoothness/Roughness Toggle
        EditorGUI.BeginChangeCheck();
        float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
        srSelected = GUILayout.Toolbar((int)srSelected, new string[] { GetScaledText("Smooth", "S"), GetScaledText("Rough", "R") });
        if (EditorGUI.EndChangeCheck())
            smoothnessEnabled = srSelected == 1.0f ? false : true;

        EditorGUILayout.EndHorizontal();

        // Enabled Settings, for the shader to determine whether to use textures or values
        // Storing inside of int instead of multiple bools so its only one variable, less to manage
        int compressedSettingToggles = BooleanCompression.CompressValues(noiseEnabled, randomiseScaling, randomiseRotation, smoothnessEnabled, variationEnabled);
        settingTogglesProp.vectorValue = new Vector2(compressedSettingToggles, settingTogglesProp.vectorValue.y);
    }

    private void DrawMaterialGUI(string materialPrefix)
    {
        // Title Label
        GUIUtilities.DrawHeaderLabelLarge($"{materialPrefix} Material");

        DrawMaterialSettingsGUI(materialPrefix);

        GUIUtilities.DrawHeaderLabelSmall("Main Properties");

        // Material Properties
        MaterialProperty surfaceTypeProp = FindProperty("_SurfaceType");

        MaterialProperty settingsProp            = FindProperty($"_{materialPrefix}Settings");
        MaterialProperty tilingOffsetProp        = FindProperty($"_{materialPrefix}TilingOffset");

        MaterialProperty albedoTexProp           = FindProperty($"_{materialPrefix}Albedo");
        MaterialProperty metallicTexProp         = FindProperty($"_{materialPrefix}MetallicMap");
        MaterialProperty smoothnessTexProp       = FindProperty($"_{materialPrefix}SmoothnessMap");
        MaterialProperty roughnessTexProp        = FindProperty($"_{materialPrefix}RoughnessMap");
        MaterialProperty normalTexProp           = FindProperty($"_{materialPrefix}NormalMap");
        MaterialProperty occlussionTexProp       = FindProperty($"_{materialPrefix}OcclussionMap");
        MaterialProperty emissionTexProp         = FindProperty($"_{materialPrefix}EmissionMap");

        MaterialProperty abledoTintProp          = FindProperty($"_{materialPrefix}AlbedoTint");
        MaterialProperty emissionColorProp       = FindProperty($"_{materialPrefix}EmissionColor");

        MaterialProperty materialProperties1Prop = FindProperty($"_{materialPrefix}MaterialProperties1");
        MaterialProperty materialProperties2Prop = FindProperty($"_{materialPrefix}MaterialProperties2");

        Vector4 materialProperties1 = materialProperties1Prop.vectorValue;
        Vector2 materialProperties2 = materialProperties2Prop.vectorValue;
        Vector4 oriMaterialProperties1 = materialProperties1;
        Vector2 oriMaterialProperties2 = materialProperties2;

        // Get variables from settings prop
        int settingToggles = (int)settingsProp.vectorValue.x;
        bool noiseEnabled      = BooleanCompression.GetCompressedValue(settingToggles, 0);
        bool smoothnessEnabled = BooleanCompression.GetCompressedValue(settingToggles, 3);
        bool variationEnabled  = BooleanCompression.GetCompressedValue(settingToggles, 4);

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

        // Draw Noise Properties
        if (noiseEnabled) DrawMaterialNoiseGUI(materialPrefix);

        // Draw Variation properties
        if (variationEnabled) DrawMaterialVariationProperties(materialPrefix);

        // Assign Properties
        if (materialProperties1 != oriMaterialProperties1)
            materialProperties1Prop.vectorValue = materialProperties1;
        if (materialProperties2 != oriMaterialProperties2)
            materialProperties2Prop.vectorValue = materialProperties2;
    }

    private void DrawMaterialNoiseGUI(string materialPrefix)
    {
        GUILayout.Space(5);

        GUIUtilities.DrawHeaderLabelSmall("Noise Properties");

        // Material Properties
        MaterialProperty settingsProp      = FindProperty($"_{materialPrefix}Settings");

        MaterialProperty noiseSettingsProp = FindProperty($"_{materialPrefix}NoiseSettings");
        MaterialProperty noiseMinMaxProp   = FindProperty($"_{materialPrefix}NoiseMinMax");

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
        GUILayout.Space(5);

        GUIUtilities.DrawHeaderLabelSmall("Variation Properties");

        // Material Properties
        MaterialProperty variationModeProp       = FindProperty($"_{materialPrefix}VariationMode");
        MaterialProperty variationSettingsProp   = FindProperty($"_{materialPrefix}VariationSettings");
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
            MaterialProperty variationTexturesProp  = FindProperty($"_{materialPrefix}VariationTexture");
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
#endif