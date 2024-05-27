using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

#if UNITY_EDITOR
public class SeamlessMaterialGUI : ShaderGUI
{
    // REPLACE REGIONS WITH CLASSES

    #region Variables
    // Constants
    private const int HEADER_PADDING = 4;
    private const int SETTING_SPACING = 4;

    private const int GROUP_SIDE_PADDING = 8;
    private const int GROUP_TOP_PADDING = 4;
    private const int GROUP_BOTTOM_PADDING = 4;

    private const int DEFAULT_DISABLED_SETTING_BACKGROUND_HEIGHT = 33;
    private const int DEFAULT_PROPERTIES_BACKGROUND_HEIGHT = 50;
    private const int DEFAULT_BASE_BACKGROUND_HEIGHT = 260;
    private const int DEFAULT_DISTANCE_BLEND_BACKGROUND_HEIGHT = 330;
    private const int DEFAULT_DEBUG_BACKGROUND_HEIGHT = 150;

    private const int BACKGROUND_CORNER_RADIUS = 10;

    private const float LINE_HEIGHT = 18.0f;
    private const float LINE_SPACING = 1.5f;

    // Material Helpers
    private Material _material;
    private MaterialEditor _editor;
    private MaterialProperty[] _properties;

    // GUI Styles
    private GUIStyle _boldHeaderStyle;
    private GUIStyle _majorToggleButton;

    // Background Heights
    // Rough solution as it only works properly on second call of OnGUI but its better then estimating and fiddling around with the height
    // Drawing a box after calculating height of area would draw ontop of other fields, this will draw behind
    private float _propertiesBackgroundHeight;
    private float _baseBackgroundHeight;
    private float _distanceBlendBackgroundHeight;
    private float _debugBackgroundHeight;

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;
    #endregion

    #region Helpers
    private MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, _properties);
    }

    private void DrawHeaderLabel(string text)
    {
        GUIContent textGUIContent = new GUIContent(text);
        float height = _boldHeaderStyle.CalcHeight(textGUIContent, Screen.width);
        Rect rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
        GUI.Label(rect, textGUIContent, _boldHeaderStyle);
    }

    private float StartBackground(float backgroundHeight)
    {
        GUILayout.Space(GROUP_TOP_PADDING);

        // Background, get height from previous OnGUI call
        Rect borderRect = EditorGUILayout.GetControlRect(false, 0.0f, EditorStyles.layerMaskField);
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
        Rect endRect = EditorGUILayout.GetControlRect(false, 0.0f, EditorStyles.layerMaskField);
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
        _properties = properties;

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
        _debugBackgroundHeight = debuggingEnabled ? DEFAULT_DEBUG_BACKGROUND_HEIGHT : DEFAULT_DISABLED_SETTING_BACKGROUND_HEIGHT;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
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

        // Distance Blend Material
        GUILayout.Space(SETTING_SPACING);

        DrawDistanceBlendGUI();

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

                    _material.SetFloat("_BUILTIN_Surface", 0.0f);
                    _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                    _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                    _material.SetInt("_BUILTIN_ZWrite", 1);
                    break;
                case 1: // Cutout
                    _material.renderQueue = (int)RenderQueue.AlphaTest;
                    _material.SetOverrideTag("RenderType", "TransparentCutout");

                    _material.SetFloat("_BUILTIN_Surface", 0.0f);
                    _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                    _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                    _material.SetInt("_BUILTIN_ZWrite", 1);
                    break;
                case 2: // Transparent
                    _material.renderQueue = (int)RenderQueue.Transparent;
                    _material.SetOverrideTag("RenderType", "Transparent");

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
        MaterialProperty distanceBlendingModeProp = FindProperty($"_DISTANCEBLENDMODE");
        MaterialProperty distanceBlendMinMaxProp = FindProperty($"_DistanceBlendMinMax");

        // Start Background
        float backgroundStartingYPos = StartBackground(_distanceBlendBackgroundHeight);

        // Distance Blend Enabled Toggle
        EditorGUI.BeginChangeCheck();
        bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;

        Color prevBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = distanceBlendingEnabled ? Color.green : Color.red;

        distanceBlendingEnabled = GUILayout.Toggle(distanceBlendingEnabled, "Distance Blending", _majorToggleButton);
        if (EditorGUI.EndChangeCheck())
            distanceBlendingEnabledProp.floatValue = distanceBlendingEnabled ? 1.0f : 0.0f;

        GUI.backgroundColor = prevBackgroundColor;

        // Draw distance blending settings
        if (distanceBlendingEnabled) {
            GUILayout.Space(5);

            // Distance Blend Mode
            _editor.ShaderProperty(distanceBlendingModeProp, "Blend Mode");
            int distanceBlendingMode = (int)distanceBlendingModeProp.floatValue;

            // Distance Blend Min Max
            EditorGUI.BeginChangeCheck();
            Vector2 distanceBlendMinMax = new Vector2(distanceBlendMinMaxProp.vectorValue.x, distanceBlendMinMaxProp.vectorValue.y);
            Rect rect = EditorGUILayout.GetControlRect(true, LINE_HEIGHT + LINE_SPACING, EditorStyles.layerMaskField);
            distanceBlendMinMax = EditorGUI.Vector2Field(rect, "Distance Blend Min Max", distanceBlendMinMax);
            if (EditorGUI.EndChangeCheck()) {
                if (distanceBlendMinMax.x < 0) distanceBlendMinMax.x = 0;
                if (distanceBlendMinMax.y < 0) distanceBlendMinMax.y = 0;

                distanceBlendMinMaxProp.vectorValue = distanceBlendMinMax;
            }

            switch (distanceBlendingMode) {
                case 0: // Tiling & Offset
                    MaterialProperty scaleProp = FindProperty($"_FarScale");
                    MaterialProperty offsetProp = FindProperty($"_FarOffset");

                    // Tiling & Offset GUI
                    Vector2 scaleVal = scaleProp.vectorValue;
                    Vector2 offsetVal = offsetProp.vectorValue;
                    Vector4 scaleOffset = new Vector4(scaleVal.x, scaleVal.y, offsetVal.x, offsetVal.y);
                    Rect scaleOffsetRect = EditorGUILayout.GetControlRect(true, 2 * (LINE_HEIGHT + LINE_SPACING), EditorStyles.layerMaskField);

                    EditorGUI.BeginChangeCheck();
                    scaleOffset = MaterialEditor.TextureScaleOffsetProperty(scaleOffsetRect, scaleOffset, false);
                    if (EditorGUI.EndChangeCheck()) {
                        scaleProp.vectorValue = new Vector2(scaleOffset.x, scaleOffset.y);
                        offsetProp.vectorValue = new Vector2(scaleOffset.z, scaleOffset.w);
                    }

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

    private void DrawDebugGUI()
    {
        // Start Background
        float backgroundStartingYPos = StartBackground(_debugBackgroundHeight);

        // Material Property
        MaterialProperty debuggingIndexProp = FindProperty("_DebuggingIndex");

        // Debug Toggle
        Color prevBackgroundColor = GUI.backgroundColor;
        bool prevDebugging = debuggingIndexProp.floatValue != -1;
        GUI.backgroundColor = prevDebugging ? Color.green : Color.red;
        bool debugging = GUILayout.Toggle(prevDebugging, "Debug", _majorToggleButton);
        GUI.backgroundColor = prevBackgroundColor;

        if (debugging) {
            GUILayout.Space(5);

            // If just started debugging, set debugging index
            if (!prevDebugging) {
                debuggingIndexProp.floatValue = 0;
            }

            // Title Label
            DrawHeaderLabel("Debug Texture");

            // Selection Grid
            string[] debugValues = new string[] {
                "Voronoi Cells",
                "Edge Mask",
                "Distance Mask",
                "Transformed UV"
            };

            EditorGUI.BeginChangeCheck();
            float debuggingIndex = debuggingIndexProp.floatValue;
            debuggingIndex = GUILayout.SelectionGrid((int)debuggingIndex, debugValues, 1);
            if (EditorGUI.EndChangeCheck())
                debuggingIndexProp.floatValue = debuggingIndex;
        } else if (debuggingIndexProp.floatValue != -1) {
            debuggingIndexProp.floatValue = -1;
        }

        // End Background
        float heightDiff = EndBackground(backgroundStartingYPos);
        if (heightDiff > 0)
            _debugBackgroundHeight = heightDiff;
    }
    #endregion

    #region Material GUI Functions
    private void DrawMaterialGUI(string materialPrefix)
    {
        // Title Label
        DrawHeaderLabel($"{materialPrefix} Material");

        DrawMaterialSettingsGUI(materialPrefix);

        // Material Properties
        MaterialProperty surfaceTypeProp = FindProperty("_SURFACETYPE");

        MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}SettingToggles");
        MaterialProperty assignedTexturesProp = FindProperty($"_{materialPrefix}AssignedTextures");

        MaterialProperty albedoTexProp = FindProperty($"_{materialPrefix}Albedo");
        MaterialProperty abledoTintProp = FindProperty($"_{materialPrefix}AlbedoTint");

        MaterialProperty metallicTexProp = FindProperty($"_{materialPrefix}MetallicMap");
        MaterialProperty metallicProp = FindProperty($"_{materialPrefix}Metallic");

        MaterialProperty smoothnessTexProp = FindProperty($"_{materialPrefix}SmoothnessMap");
        MaterialProperty smoothnessProp = FindProperty($"_{materialPrefix}Smoothness");
        MaterialProperty roughnessTexProp = FindProperty($"_{materialPrefix}RoughnessMap");
        MaterialProperty roughnessProp = FindProperty($"_{materialPrefix}Roughness");

        MaterialProperty normalTexProp = FindProperty($"_{materialPrefix}NormalMap");
        MaterialProperty normalScaleProp = FindProperty($"_{materialPrefix}NormalScale");

        MaterialProperty occlussionTexProp = FindProperty($"_{materialPrefix}OcclussionMap");
        MaterialProperty occlussionStrengthProp = FindProperty($"_{materialPrefix}OcclussionStrength");

        MaterialProperty emissionTexProp = FindProperty($"_{materialPrefix}EmissionMap");
        MaterialProperty emissionColorProp = FindProperty($"_{materialPrefix}EmissionColor");

        MaterialProperty alphaClippingProp = FindProperty($"_{materialPrefix}AlphaClipping");

        MaterialProperty scaleProp = FindProperty($"_{materialPrefix}Scale");
        MaterialProperty offsetProp = FindProperty($"_{materialPrefix}Offset");

        // Get variables from settings prop
        int settingToggles = (int)settingTogglesProp.floatValue;
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
        assignedTexturesProp.floatValue = compressedAssignedTextures;

        // Albedo
        _editor.TexturePropertySingleLine(new GUIContent("Albedo"), albedoTexProp, abledoTintProp);

        // Metallic
        _editor.TexturePropertySingleLine(new GUIContent("Metallic"), metallicTexProp, metallicAssigned ? null : metallicProp);

        // Smoothness/Roughness
        float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
        switch (srSelected) {
            case 0: // Smoothness
                _editor.TexturePropertySingleLine(new GUIContent("Smoothness"), smoothnessTexProp, smoothnessAssigned ? null : smoothnessProp);
                break;
            case 1: // Roughness
                _editor.TexturePropertySingleLine(new GUIContent("Roughness"), roughnessTexProp, roughnessAssigned ? null : roughnessProp);
                break;
        }

        // Normal Map
        _editor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalTexProp, normalAssigned ? normalScaleProp : null);

        // Occlussion Map
        _editor.TexturePropertySingleLine(new GUIContent("Occlussion"), occlussionTexProp, occlussionAssigned ? occlussionStrengthProp : null);

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
            _editor.RangeProperty(alphaClippingProp, "Alpha Clipping");
        }

        // Draw noise settings near end, makes things more clean
        if(noiseEnabled) DrawMaterialNoiseGUI(materialPrefix);

        // Scale & Offset, manually setting scaleOffset and rect because using Albedo Tiling & Offset doesn't like the noise applied to the UV
        Vector2 scaleVal = scaleProp.vectorValue;
        Vector2 offsetVal = offsetProp.vectorValue;
        Vector4 scaleOffset = new Vector4(scaleVal.x, scaleVal.y, offsetVal.x, offsetVal.y);
        Rect rect = EditorGUILayout.GetControlRect(true, 2 * (LINE_HEIGHT + LINE_SPACING), EditorStyles.layerMaskField);

        EditorGUI.BeginChangeCheck();
        scaleOffset = MaterialEditor.TextureScaleOffsetProperty(rect, scaleOffset, false);
        if (EditorGUI.EndChangeCheck()) {
            scaleProp.vectorValue = new Vector2(scaleOffset.x, scaleOffset.y);
            offsetProp.vectorValue = new Vector2(scaleOffset.z, scaleOffset.w);
        }
    }

    private void DrawMaterialSettingsGUI(string materialPrefix)
    {
        // Material Properties
        MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}SettingToggles");

        EditorGUILayout.BeginHorizontal();

        // Get variables from settings prop
        int settingToggles = (int)settingTogglesProp.floatValue;
        bool noiseEnabled = (settingToggles & 1) != 0;
        bool randomiseNoiseScaling = (settingToggles & 2) != 0;
        bool randomiseRotation = (settingToggles & 4) != 0;
        bool smoothnessEnabled = (settingToggles & 8) != 0;

        // Noise Enabled
        string noiseEnabledStyle = noiseEnabled ? "ButtonLeft" : "Button";
        noiseEnabled = GUILayout.Toggle(noiseEnabled, "Noise", noiseEnabledStyle);

        if (noiseEnabled) {
            // Noise Scaling Enabled
            randomiseNoiseScaling = GUILayout.Toggle(randomiseNoiseScaling, "Random Scaling", "ButtonMid");

            // Randomise Rotation Enabled
            randomiseRotation = GUILayout.Toggle(randomiseRotation, "Random Rotation", "ButtonRight");
        }

        GUILayout.FlexibleSpace();

        // Smoothness/Roughness Toggle
        EditorGUI.BeginChangeCheck();
        float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
        srSelected = GUILayout.Toolbar((int)srSelected, new string[] { "Smooth", "Rough" });
        if (EditorGUI.EndChangeCheck())
            smoothnessEnabled = srSelected == 1.0f ? false : true;

        EditorGUILayout.EndHorizontal();

        // Enabled Settings, for the shader to determine whether to use textures or values
        // Storing inside of int instead of multiple bools so its only one variable, less to manage
        int compressedSettingToggles = (noiseEnabled ? 1 : 0) | (randomiseNoiseScaling ? 2 : 0) | (randomiseRotation ? 4 : 0) | (smoothnessEnabled ? 8 : 0);
        settingTogglesProp.floatValue = compressedSettingToggles;
    }

    private void DrawMaterialNoiseGUI(string materialPrefix)
    {
        // Material Properties
        MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}SettingToggles");

        MaterialProperty noiseAngleOffsetProp = FindProperty($"_{materialPrefix}NoiseAngleOffset");
        MaterialProperty noiseScaleProp = FindProperty($"_{materialPrefix}NoiseScale");

        MaterialProperty noiseScalingMinMaxProp = FindProperty($"_{materialPrefix}NoiseScalingMinMax");

        MaterialProperty randomiseRotationMinMaxProp = FindProperty($"_{materialPrefix}RandomiseRotationMinMax");

        // Get variables from settings prop
        int settingToggles = (int)settingTogglesProp.floatValue;
        bool randomiseNoiseScaling = (settingToggles & 2) != 0;
        bool randomiseRotation = (settingToggles & 4) != 0;

        // Voronoi Noise
        _editor.FloatProperty(noiseAngleOffsetProp, "Noise Angle Offset");
        _editor.FloatProperty(noiseScaleProp, "Noise Scale");

        // Scale Randomising
        if (randomiseNoiseScaling) {
            EditorGUI.BeginChangeCheck();
            Vector2 scalingMinMax = new Vector2(noiseScalingMinMaxProp.vectorValue.x, noiseScalingMinMaxProp.vectorValue.y);
            Rect rect = EditorGUILayout.GetControlRect(true, LINE_HEIGHT + LINE_SPACING, EditorStyles.layerMaskField);
            scalingMinMax = EditorGUI.Vector2Field(rect, "Noise Scaling Min Max", scalingMinMax);
            if (EditorGUI.EndChangeCheck()) {
                if (scalingMinMax.x < 0) scalingMinMax.x = 0;
                if (scalingMinMax.y < 0) scalingMinMax.y = 0;

                noiseScalingMinMaxProp.vectorValue = scalingMinMax;
            }
        }

        // Rotation Randomising
        if (randomiseRotation) {
            EditorGUI.BeginChangeCheck();
            Vector2 randomiseRotationMinMax = new Vector2(randomiseRotationMinMaxProp.vectorValue.x, randomiseRotationMinMaxProp.vectorValue.y);
            Rect rect = EditorGUILayout.GetControlRect(true, LINE_HEIGHT + LINE_SPACING, EditorStyles.layerMaskField);
            randomiseRotationMinMax = EditorGUI.Vector2Field(rect, "Random Rotation Min Max", randomiseRotationMinMax);
            if (EditorGUI.EndChangeCheck())
                randomiseRotationMinMaxProp.vectorValue = randomiseRotationMinMax;
        }
    }
    #endregion
}

#endif