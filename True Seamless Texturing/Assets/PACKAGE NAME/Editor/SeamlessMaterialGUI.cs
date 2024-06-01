using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.VisualScripting;

#if UNITY_EDITOR
public class SeamlessMaterialGUI : ShaderGUI
{
    // REPLACE REGIONS WITH CLASSES

    #region Variables
    // Constants
    private const float LINE_HEIGHT = 18.0f;
    private const float LINE_SPACING = 2.0f;

    private const int HEADER_PADDING = 4;
    private const int SETTING_SPACING = 4;

    private const int GROUP_SIDE_PADDING = 8;
    private const int GROUP_TOP_PADDING = 4;
    private const int GROUP_BOTTOM_PADDING = 4;

    private const int DEFAULT_DISABLED_SETTING_BACKGROUND_HEIGHT = 33;
    private const int DEFAULT_PROPERTIES_BACKGROUND_HEIGHT = 50;
    private const int DEFAULT_BASE_BACKGROUND_HEIGHT = 260;
    private const int DEFAULT_DISTANCE_BLEND_BACKGROUND_HEIGHT = 330;
    private const int DEFAULT_MATERIAL_BLEND_BACKGROUND_HEIGHT = 0;
    private const int DEFAULT_DEBUG_BACKGROUND_HEIGHT = 150;

    private const int BACKGROUND_CORNER_RADIUS = 10;

    private const int SMALL_TEXT_MAX_WIDTH = 450;

    // Material Helpers
    private Material _material;
    private MaterialEditor _editor;
    private MaterialProperty[] _properties;

    // GUI Styles
    private GUIStyle _boldHeaderStyle;
    private GUIStyle _majorToggleButton;

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;

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
    #endregion

    #region Helpers
    private MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, _properties);
    }

    private Rect GetLineRect(float ?heightOverride = null)
    {
        if (heightOverride != null)
            return EditorGUILayout.GetControlRect(true, heightOverride.Value, EditorStyles.layerMaskField);
        else
            return EditorGUILayout.GetControlRect(true, LINE_HEIGHT + LINE_SPACING, EditorStyles.layerMaskField);
    }

    private string GetScaledText(string largeText, string smallText)
    {
        return Screen.width <= SMALL_TEXT_MAX_WIDTH ? smallText : largeText;
    }

    private void DrawHeaderLabel(string text)
    {
        GUIContent textGUIContent = new GUIContent(text);
        float height = _boldHeaderStyle.CalcHeight(textGUIContent, Screen.width);
        Rect rect = GetLineRect(height);
        GUI.Label(rect, textGUIContent, _boldHeaderStyle);
    }

    private bool DrawMajorToggleButton(MaterialProperty property, string label)
    {
        EditorGUI.BeginChangeCheck();
        bool enabled = property.floatValue == 1 ? true : false;

        Color prevBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = enabled ? Color.green : Color.red;

        enabled = GUILayout.Toggle(enabled, label, _majorToggleButton);
        if (EditorGUI.EndChangeCheck())
            property.floatValue = enabled ? 1.0f : 0.0f;

        GUI.backgroundColor = prevBackgroundColor;

        return enabled;
    }

    private bool DrawMajorToggleButton(bool enabled, string label)
    {
        Color prevBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = enabled ? Color.green : Color.red;
        enabled = GUILayout.Toggle(enabled, label, _majorToggleButton);
        GUI.backgroundColor = prevBackgroundColor;

        return enabled;
    }

    private float DrawTextureWithSlider(MaterialProperty textureProperty, bool sliderCondition, float sliderValue, string label)
    {
        Rect rect = GetLineRect();
        _editor.TexturePropertyMiniThumbnail(rect, textureProperty, label, "");
        if (sliderCondition) {
            sliderValue = EditorGUI.Slider(MaterialEditor.GetRectAfterLabelWidth(rect), sliderValue, 0, 1);
        }

        return sliderValue;
    }

    private Vector2 DrawVector2Field(Vector2 value, string label)
    {
        return EditorGUI.Vector2Field(GetLineRect(), label, value);
    }

    private void DrawTilingOffset(MaterialProperty tilingOffsetProperty)
    {
        Vector4 tilingOffset = tilingOffsetProperty.vectorValue;
        Vector4 oriTilingOffset = tilingOffset;

        EditorGUI.BeginChangeCheck();
        Vector2 scale = new Vector2(tilingOffset.x, tilingOffset.y);
        scale = EditorGUI.Vector2Field(GetLineRect(), "Scale", scale);
        if (EditorGUI.EndChangeCheck())
            tilingOffset = new Vector4(scale.x, scale.y, tilingOffset.z, tilingOffset.w);

        EditorGUI.BeginChangeCheck();
        Vector2 offset = new Vector2(tilingOffset.z, tilingOffset.w);
        offset = EditorGUI.Vector2Field(GetLineRect(), "Offset", offset);
        if (EditorGUI.EndChangeCheck())
            tilingOffset = new Vector4(tilingOffset.x, tilingOffset.y, offset.x, offset.y);

        if (tilingOffset != oriTilingOffset)
            tilingOffsetProperty.vectorValue = tilingOffset;
    }

    private void DrawTilingOffset(MaterialProperty tilingProperty, MaterialProperty offsetProperty)
    {
        // Scale & Offset, manually setting scaleOffset and rect because using Albedo Tiling & Offset doesn't like the noise applied to the UV
        Vector2 scaleVal = tilingProperty.vectorValue;
        Vector2 offsetVal = offsetProperty.vectorValue;
        Vector4 scaleOffset = new Vector4(scaleVal.x, scaleVal.y, offsetVal.x, offsetVal.y);
        Rect rect = GetLineRect();
        rect.height *= 2;

        EditorGUI.BeginChangeCheck();
        scaleOffset = MaterialEditor.TextureScaleOffsetProperty(rect, scaleOffset, false);
        if (EditorGUI.EndChangeCheck()) {
            tilingProperty.vectorValue = new Vector2(scaleOffset.x, scaleOffset.y);
            offsetProperty.vectorValue = new Vector2(scaleOffset.z, scaleOffset.w);
        }
    }

    private float StartBackground(float backgroundHeight)
    {
        GUILayout.Space(GROUP_TOP_PADDING);

        // Background, get height from previous OnGUI call
        Rect borderRect = GetLineRect(0);
        float startingYPosition = borderRect.position.y;
        borderRect.position += new Vector2(0, -GROUP_TOP_PADDING);
        borderRect.height = backgroundHeight + GROUP_TOP_PADDING + GROUP_BOTTOM_PADDING;
        GUI.DrawTexture(borderRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0, 0, 0, 0.2f), 0, BACKGROUND_CORNER_RADIUS);

        // Start Padding
        GUILayout.BeginHorizontal();
        GUILayout.Space(GROUP_SIDE_PADDING);
        GUILayout.BeginVertical();

        return startingYPosition;
    }

    private float EndBackground(float startingYPos)
    {
        // End Padding
        GUILayout.EndVertical();
        GUILayout.Space(GROUP_SIDE_PADDING);
        GUILayout.EndHorizontal();

        // Set background height for next OnGUI call
        Rect endRect = GetLineRect(0);
        float heightDifference = endRect.position.y - startingYPos;

        GUILayout.Space(GROUP_BOTTOM_PADDING);

        return heightDifference;
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

        // Setup GUI Styles
        _boldHeaderStyle = new GUIStyle("label");
        _boldHeaderStyle.fontStyle = FontStyle.Bold;
        _boldHeaderStyle.fontSize = 16;
        _boldHeaderStyle.alignment = TextAnchor.MiddleCenter;

        _majorToggleButton = new GUIStyle("button");
        _majorToggleButton.fontStyle = FontStyle.Bold;
        _majorToggleButton.fontSize = 18;
        _majorToggleButton.alignment = TextAnchor.MiddleCenter;

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

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Properties can change between calls
        _properties = properties;

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
        float backgroundStartingYPos = StartBackground(_propertiesBackgroundHeight);

        // Header
        DrawHeaderLabel($"Material Properties");

        GUILayout.Space(4);

        // Surface Type
        EditorGUI.BeginChangeCheck();
        MaterialProperty surfaceTypeProp = FindProperty("_SURFACETYPE");
        _editor.ShaderProperty(surfaceTypeProp, "Surface Type");
        int surfaceType = (int)surfaceTypeProp.floatValue;

        if (EditorGUI.EndChangeCheck()) {
            switch (surfaceType) {
                case 0: // Opaque
                    _material.renderQueue = (int)RenderQueue.Geometry;
                    _material.SetOverrideTag("RenderType", "Opaque");
                    _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                    _material.SetFloat("_BUILTIN_Surface", 0.0f);
                    _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                    _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                    _material.SetInt("_BUILTIN_ZWrite", 1);
                    break;
                case 1: // Cutout
                    _material.renderQueue = (int)RenderQueue.AlphaTest;
                    _material.SetOverrideTag("RenderType", "TransparentCutout");
                    _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                    _material.SetFloat("_BUILTIN_Surface", 0.0f);
                    _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                    _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                    _material.SetInt("_BUILTIN_ZWrite", 1);
                    break;
                case 2: // Transparent
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
        float heightDiff = EndBackground(backgroundStartingYPos);
        if (heightDiff > 0)
            _propertiesBackgroundHeight = heightDiff;
    }

    private void DrawBaseMaterialGUI()
    {
        float backgroundStartingYPos = StartBackground(_baseBackgroundHeight);

        DrawMaterialGUI("Base");

        float heightDiff = EndBackground(backgroundStartingYPos);
        if (heightDiff > 0)
            _baseBackgroundHeight = heightDiff;
    }

    private void DrawDistanceBlendGUI()
    {
        // Material Properties
        MaterialProperty distanceBlendingEnabledProp = FindProperty($"_DistanceBlendingEnabled");

        // Start Background
        float backgroundStartingYPos = StartBackground(_distanceBlendBackgroundHeight);

        // Distance Blend Enabled Toggle
        bool distanceBlendingEnabled = DrawMajorToggleButton(distanceBlendingEnabledProp, "Distance Blending");

        // Draw distance blending settings
        if (distanceBlendingEnabled) {
            // Material Properties
            MaterialProperty distanceBlendingModeProp = FindProperty($"_DISTANCEBLENDMODE");
            MaterialProperty distanceBlendMinMaxProp = FindProperty($"_DistanceBlendMinMax");

            GUILayout.Space(5);

            // Distance Blend Mode
            _editor.ShaderProperty(distanceBlendingModeProp, "Blend Mode");
            int distanceBlendingMode = (int)distanceBlendingModeProp.floatValue;

            // Distance Blend Min Max
            EditorGUI.BeginChangeCheck();
            Vector2 distanceBlendMinMax = new Vector2(distanceBlendMinMaxProp.vectorValue.x, distanceBlendMinMaxProp.vectorValue.y);
            distanceBlendMinMax = EditorGUI.Vector2Field(GetLineRect(), "Distance Blend Min Max", distanceBlendMinMax);
            if (EditorGUI.EndChangeCheck()) {
                if (distanceBlendMinMax.x < 0) distanceBlendMinMax.x = 0;
                if (distanceBlendMinMax.y < 0) distanceBlendMinMax.y = 0;

                distanceBlendMinMaxProp.vectorValue = distanceBlendMinMax;
            }

            switch (distanceBlendingMode) {
                case 0: // Tiling & Offset
                    MaterialProperty tilingOffsetProp = FindProperty("_FarTilingOffset");

                    // Tiling & Offset GUI
                    DrawTilingOffset(tilingOffsetProp);
                    break;
                case 1: // Material
                    GUILayout.Space(10);

                    // Material GUI
                    DrawMaterialGUI("Far"); 
                    break;
            }
        }

        // End Background
        float heightDiff = EndBackground(backgroundStartingYPos);
        if (heightDiff > 0)
            _distanceBlendBackgroundHeight = heightDiff;
    }

    private void DrawMaterialBlendGUI()
    {
        // Start Background
        float backgroundStartingYPos = StartBackground(_materialBlendBackgroundHeight);

        // Material Property
        MaterialProperty materialBlendingSettingsProp = FindProperty($"_MaterialBlendSettings");

        int materialBlendingSettings = (int)materialBlendingSettingsProp.floatValue;
        bool materialBlendingEnabled    = (materialBlendingSettings & 1) != 0;
        bool overrideDistanceBlending   = (materialBlendingSettings & 2) != 0;
        bool overrideDistanceBlendingTO = (materialBlendingSettings & 4) != 0;

        // Material Blend Enabled Toggle
        materialBlendingEnabled = DrawMajorToggleButton(materialBlendingEnabled, "Material Blending");

        if (materialBlendingEnabled) {
            // Material Properties
            MaterialProperty blendMaskTypeProp = FindProperty($"_BLENDMASKTYPE");
            MaterialProperty distanceBlendingEnabledProp = FindProperty($"_DistanceBlendingEnabled");

            MaterialProperty materialBlendPropertiesProp = FindProperty($"_MaterialBlendProperties");

            Vector2 materialBlendProperties = materialBlendPropertiesProp.vectorValue;
            Vector2 oriMaterialBlendProperties = materialBlendProperties;

            // Mask
            DrawHeaderLabel("Mask");

            _editor.ShaderProperty(blendMaskTypeProp, "Mask Type");
            int blendMaskType = (int)blendMaskTypeProp.floatValue;

            materialBlendProperties.x = EditorGUI.Slider(GetLineRect(), "Mask Opacity", materialBlendProperties.x, 0, 1);
            materialBlendProperties.y = EditorGUI.FloatField(GetLineRect(), "Mask Strength", materialBlendProperties.y);

            if (blendMaskType < 2) { // Noise
                // Material Properties
                MaterialProperty materialBlendNoiseSettingsProp = FindProperty($"_MaterialBlendNoiseSettings");

                Vector3 materialBlendNoiseSettings = materialBlendNoiseSettingsProp.vectorValue;
                Vector3 oriMaterialBlendNoiseSettings = materialBlendNoiseSettings;

                // Scale
                float noiseScale = EditorGUI.FloatField(GetLineRect(), "Noise Scale", materialBlendNoiseSettings.x);

                // Offset
                Vector2 noiseOffset = DrawVector2Field(new Vector2(materialBlendNoiseSettings.y, materialBlendNoiseSettings.z), "Noise Offset");

                materialBlendNoiseSettings = new Vector3(noiseScale, noiseOffset.x, noiseOffset.y);

                if (materialBlendNoiseSettings != oriMaterialBlendNoiseSettings)
                    materialBlendNoiseSettingsProp.vectorValue = materialBlendNoiseSettings;
            } else { // Custom Texture
                // Material Properties
                MaterialProperty blendMaskTextureProp = FindProperty($"_BlendMaskTexture");
                MaterialProperty blendMaskTextureTOProp = FindProperty($"_MaterialBlendTilingOffset");

                // Texture
                _editor.TexturePropertySingleLine(new GUIContent("Blend Mask"), blendMaskTextureProp);

                // Scale & Offset
                DrawTilingOffset(blendMaskTextureTOProp);
            }

            GUILayout.Space(10);

            // Distance Blending
            bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;

            if (distanceBlendingEnabled) {
                MaterialProperty distanceBlendingModeProp = FindProperty($"_DISTANCEBLENDMODE");
                int distanceBlendingMode = (int)distanceBlendingModeProp.floatValue;

                DrawHeaderLabel("Distance Blending");

                GUILayout.BeginHorizontal();

                string distanceBlendingEnabledStyle = overrideDistanceBlending && distanceBlendingMode == 0 ? "ButtonLeft" : "Button";
                overrideDistanceBlending = GUILayout.Toggle(overrideDistanceBlending, GetScaledText("Override Distance Blending", "ODB"), distanceBlendingEnabledStyle);

                bool endedHorizontal = false;
                if (overrideDistanceBlending && distanceBlendingMode == 0) {
                    overrideDistanceBlendingTO = GUILayout.Toggle(overrideDistanceBlendingTO, GetScaledText("Override Tiling & Offset", "OTO"), "ButtonRight");

                    endedHorizontal = true;
                    GUILayout.EndHorizontal();

                    if (overrideDistanceBlendingTO) {
                        MaterialProperty blendMaskDistanceTOProp = FindProperty($"_BlendMaskDistanceTO");

                        DrawTilingOffset(blendMaskDistanceTOProp);
                    }
                }

                if(!endedHorizontal)
                    GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }

            if (materialBlendProperties != oriMaterialBlendProperties)
                materialBlendPropertiesProp.vectorValue = materialBlendProperties;

            // Material
            DrawMaterialGUI("Blend");
        }
        
        // Save compressed material blend settings
        int compressedMaterialBlendSettings = (materialBlendingEnabled ? 1 : 0) | (overrideDistanceBlending ? 2 : 0) | (overrideDistanceBlendingTO ? 4 : 0);
        materialBlendingSettingsProp.floatValue = compressedMaterialBlendSettings;

        // End Background
        float heightDiff = EndBackground(backgroundStartingYPos);
        if (heightDiff > 0)
            _materialBlendBackgroundHeight = heightDiff;
    }

    private void DrawDebugGUI()
    {
        // Start Background
        float backgroundStartingYPos = StartBackground(_debugBackgroundHeight);

        // Material Property
        MaterialProperty debuggingIndexProp = FindProperty("_DebuggingIndex");

        // Debug Toggle
        bool prevDebugging = debuggingIndexProp.floatValue != -1;
        bool debugging = DrawMajorToggleButton(prevDebugging, "Debug");

        if (debugging) {
            GUILayout.Space(5);

            // If just started debugging, get previous debugging index
            if (!prevDebugging) {
                debuggingIndexProp.floatValue = _prevDebugIndex;
            }

            // Title Label
            DrawHeaderLabel("Debug Texture");

            // Selection Grid
            string[] debugValues = new string[] {
                "Voronoi Cells",
                "Edge Mask",
                "Distance Mask",
                "Blend Material Mask"
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
        float heightDiff = EndBackground(backgroundStartingYPos);
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
        bool noiseEnabled = (settingToggles & 1) != 0;
        bool randomiseScaling = (settingToggles & 2) != 0;
        bool randomiseRotation = (settingToggles & 4) != 0;
        bool smoothnessEnabled = (settingToggles & 8) != 0;

        // Noise Enabled
        string noiseEnabledStyle = noiseEnabled ? "ButtonLeft" : "Button";
        noiseEnabled = GUILayout.Toggle(noiseEnabled, GetScaledText("Noise", "N"), noiseEnabledStyle);

        if (noiseEnabled) {
            // Noise Scaling Enabled
            randomiseScaling = GUILayout.Toggle(randomiseScaling, GetScaledText("Random Scaling", "RS"), "ButtonMid");

            // Randomise Rotation Enabled
            randomiseRotation = GUILayout.Toggle(randomiseRotation, GetScaledText("Random Rotation", "RR"), "ButtonRight");
        }

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
        int compressedSettingToggles = (noiseEnabled ? 1 : 0) | (randomiseScaling ? 2 : 0) | (randomiseRotation ? 4 : 0) | (smoothnessEnabled ? 8 : 0);
        settingTogglesProp.vectorValue = new Vector2(compressedSettingToggles, settingTogglesProp.vectorValue.y);
    }

    private void DrawMaterialGUI(string materialPrefix)
    {
        // Title Label
        DrawHeaderLabel($"{materialPrefix} Material");

        DrawMaterialSettingsGUI(materialPrefix);

        // Material Properties
        MaterialProperty surfaceTypeProp = FindProperty("_SURFACETYPE");

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
        bool noiseEnabled = (settingToggles & 1) != 0;
        bool smoothnessEnabled = (settingToggles & 8) != 0;

        // Assigned Textures, for the shader to determine whether to use textures or values
        // Storing inside of int instead of multiple bools so its only one variable, less to manage
        bool metallicAssigned = metallicTexProp.textureValue != null;     // (bits & 1)  != 0
        bool smoothnessAssigned = smoothnessTexProp.textureValue != null; // (bits & 2)  != 0
        bool roughnessAssigned = roughnessTexProp.textureValue != null;   // (bits & 4)  != 0
        bool normalAssigned = normalTexProp.textureValue != null;         // (bits & 8)  != 0
        bool occlussionAssigned = occlussionTexProp.textureValue != null; // (bits & 16) != 0
        bool emissionAssigned = emissionTexProp.textureValue != null;     // (bits & 32) != 0
        int compressedAssignedTextures = (metallicAssigned ? 1 : 0) | (smoothnessAssigned ? 2 : 0) | (roughnessAssigned ? 4 : 0) | (normalAssigned ? 8 : 0) | (occlussionAssigned ? 16 : 0) | (emissionAssigned ? 32 : 0);
        settingsProp.vectorValue = new Vector2(settingToggles, compressedAssignedTextures);

        // Albedo
        _editor.TexturePropertySingleLine(new GUIContent("Albedo"), albedoTexProp, abledoTintProp);

        // Metallic
        materialProperties1.x = DrawTextureWithSlider(metallicTexProp, !metallicAssigned, materialProperties1.x, "Metallic");

        // Smoothness/Roughness
        float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
        switch (srSelected) {
            case 0: // Smoothness
                materialProperties1.y = DrawTextureWithSlider(smoothnessTexProp, !smoothnessAssigned, materialProperties1.y, "Smoothness");

                break;
            case 1: // Roughness
                materialProperties1.z = DrawTextureWithSlider(roughnessTexProp, !roughnessAssigned, materialProperties1.z, "Roughness");

                break;
        }

        // Normal Map
        materialProperties1.w = DrawTextureWithSlider(normalTexProp, normalAssigned, materialProperties1.w, "Normal Map");

        // Occlussion Map
        materialProperties2.x = DrawTextureWithSlider(occlussionTexProp, occlussionAssigned, materialProperties2.x, "Occlussion");

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
            alphaClippingValue = EditorGUI.Slider(GetLineRect(), "Alpha Clipping", alphaClippingValue, 0, 1);
            if (EditorGUI.EndChangeCheck())
                materialProperties2.y = alphaClippingValue;
        }

        // Draw noise properties
        if(noiseEnabled) DrawMaterialNoiseGUI(materialPrefix);

        // Scale & Offset
        DrawTilingOffset(tilingOffsetProp);

        // Assign Properties
        if (materialProperties1 != oriMaterialProperties1)
            materialProperties1Prop.vectorValue = materialProperties1;
        if(materialProperties2 != oriMaterialProperties2)
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
        EditorGUI.BeginChangeCheck();
        float noiseAngleOffsetValue = noiseSettings.x;
        noiseAngleOffsetValue = EditorGUI.FloatField(GetLineRect(), "Noise Angle Offset", noiseAngleOffsetValue);
        if (EditorGUI.EndChangeCheck())
            noiseSettings.x = noiseAngleOffsetValue;

        // Scale Randomising
        if (randomiseNoiseScaling) {
            // Scale
            EditorGUI.BeginChangeCheck();
            float noiseScaleValue = noiseSettings.y;
            noiseScaleValue = EditorGUI.FloatField(GetLineRect(), "Noise Scale", noiseScaleValue);
            if (EditorGUI.EndChangeCheck())
                noiseSettings.y = noiseScaleValue;

            // Scaling Min Max
            EditorGUI.BeginChangeCheck();
            Vector2 scalingMinMax = new Vector2(noiseMinMax.x, noiseMinMax.y);
            scalingMinMax = EditorGUI.Vector2Field(GetLineRect(), "Noise Scaling Min Max", scalingMinMax);
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
            randomiseRotationMinMax = EditorGUI.Vector2Field(GetLineRect(), "Random Rotation Min Max", randomiseRotationMinMax);
            if (EditorGUI.EndChangeCheck())
                noiseMinMax = new Vector4(noiseMinMax.x, noiseMinMax.y, randomiseRotationMinMax.x, randomiseRotationMinMax.y);
        }

        // Assign Properties
        if (noiseSettings != oriNoiseSettings)
            noiseSettingsProp.vectorValue = noiseSettings;
        if (noiseMinMax != oriNoiseMinMax)
            noiseMinMaxProp.vectorValue = noiseMinMax;
    }
    #endregion
}

#endif