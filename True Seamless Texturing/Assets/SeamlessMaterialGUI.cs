using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class SeamlessMaterialGUI : ShaderGUI
{
    const int GROUP_SIDE_PADDING = 10;
    const int GROUP_TOP_PADDING = 2;
    const int GROUP_BOTTOM_PADDING = 4;

    const float LINE_HEIGHT = 18.0f;
    const float LINE_SPACING = 1.5f;

    MaterialEditor _editor;
    MaterialProperty[] _properties;

    GUIStyle _boldHeaderStyle;
    GUIStyle _majorToggleButton;

    bool _debugging = false;

    // Background Heights
    // Rough solution as it only works properly on second call of OnGUI but its better then estimating and fiddling around with the height
    // Drawing a box after calculating height of area would draw ontop of other fields, this will draw behind
    float _baseBackgroundHeight = 260;
    float _distanceBlendBackgroundHeight = 306;
    float _debugBackgroundHeight = 306;

    private MaterialProperty FindProperty(string name)
    {
        return FindProperty(name, _properties);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //base.OnGUI(materialEditor, properties);

        EditorGUIUtility.wideMode = true;

        _editor = materialEditor;
        _properties = properties;

        // GUI Styles
        _boldHeaderStyle = new GUIStyle("label");
        _boldHeaderStyle.fontStyle = FontStyle.Bold;
        _boldHeaderStyle.fontSize = 16;
        _boldHeaderStyle.alignment = TextAnchor.MiddleCenter;

        _majorToggleButton = new GUIStyle("button");
        _majorToggleButton.fontStyle = FontStyle.Bold;
        _majorToggleButton.fontSize = 18;
        _majorToggleButton.alignment = TextAnchor.MiddleCenter;

        GUILayout.Space(10);

        // Base Material
        DrawBaseMaterialGUI();

        // Distance Blend Material
        GUILayout.Space(20);

        DrawDistanceBlendGUI();

        GUILayout.Space(20);

        // Footer Settings
        DrawDebugGUI();
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
        MaterialProperty distanceBlendMinMaxProp = FindProperty($"_DistanceBlendMinMax");

        // Distance Blend Enabled Toggle
        EditorGUI.BeginChangeCheck();
        bool distanceBlendingEnabled = distanceBlendingEnabledProp.floatValue == 1 ? true : false;

        Color prevBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = distanceBlendingEnabled ? Color.green : Color.red;

        distanceBlendingEnabled = GUILayout.Toggle(distanceBlendingEnabled, "Distance Blending", _majorToggleButton);
        if (EditorGUI.EndChangeCheck())
            distanceBlendingEnabledProp.floatValue = distanceBlendingEnabled ? 1.0f : 0.0f;
        GUI.backgroundColor = prevBackgroundColor;

        if (distanceBlendingEnabled) {
            GUILayout.Space(5);

            float backgroundStartingYPos = StartBackground(_distanceBlendBackgroundHeight);

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

            GUILayout.Space(10);

            // Material GUI
            DrawMaterialGUI("Far");

            float heightDiff = EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _distanceBlendBackgroundHeight = heightDiff;
        }
    }

    private void DrawMaterialGUI(string materialPrefix)
    {
        // Title Label
        DrawHeaderLabel($"{materialPrefix} Material");

        DrawMaterialSettingsGUI(materialPrefix);
        DrawMaterialNoiseGUI(materialPrefix);

        // Material Properties
        MaterialProperty assignedTexturesProp = FindProperty($"_{materialPrefix}AssignedTextures");

        MaterialProperty albedoTexProp = FindProperty($"_{materialPrefix}Albedo");
        MaterialProperty abledoTintProp = FindProperty($"_{materialPrefix}AlbedoTint");

        MaterialProperty metallicTexProp = FindProperty($"_{materialPrefix}MetallicMap");
        MaterialProperty metallicProp = FindProperty($"_{materialPrefix}Metallic");

        MaterialProperty smoothnessEnabledProp = FindProperty($"_{materialPrefix}SmoothnessEnabled");
        MaterialProperty smoothnessTexProp = FindProperty($"_{materialPrefix}SmoothnessMap");
        MaterialProperty smoothnessProp = FindProperty($"_{materialPrefix}Smoothness");
        MaterialProperty roughnessTexProp = FindProperty($"_{materialPrefix}RoughnessMap");
        MaterialProperty roughnessProp = FindProperty($"_{materialPrefix}Roughness");

        MaterialProperty normalTexProp = FindProperty($"_{materialPrefix}NormalMap");
        MaterialProperty normalScaleProp = FindProperty($"_{materialPrefix}NormalScale");

        MaterialProperty emissionTexProp = FindProperty($"_{materialPrefix}EmissionMap");
        MaterialProperty emissionColorProp = FindProperty($"_{materialPrefix}EmissionColor");

        MaterialProperty scaleProp = FindProperty($"_{materialPrefix}Scale");
        MaterialProperty offsetProp = FindProperty($"_{materialPrefix}Offset");

        // Assigned Textures, for the shader to determine whether to use textures or values
        // Storing inside of int instead of multiple bools so its only one variable, less to manage
        bool metallicAssigned = metallicTexProp.textureValue != null;     // (bits & 1)  != 0
        bool smoothnessAssigned = smoothnessTexProp.textureValue != null; // (bits & 2)  != 0
        bool roughnessAssigned = roughnessTexProp.textureValue != null;   // (bits & 4)  != 0
        bool normalAssigned = normalTexProp.textureValue != null;         // (bits & 8)  != 0
        bool emissionAssigned = emissionTexProp.textureValue != null;     // (bits & 16) != 0
        int compressedassignedTextures = (metallicAssigned ? 1 : 0) | (smoothnessAssigned ? 2 : 0) | (roughnessAssigned ? 4 : 0) | (normalAssigned ? 8 : 0) | (emissionAssigned ? 16 : 0);
        assignedTexturesProp.floatValue = compressedassignedTextures;

        // Albedo
        _editor.TexturePropertySingleLine(new GUIContent("Albedo"), albedoTexProp, abledoTintProp);

        // Metallic
        _editor.TexturePropertySingleLine(new GUIContent("Metallic"), metallicTexProp, metallicAssigned ? null : metallicProp);

        // Smoothness/Roughness
        float srSelected = smoothnessEnabledProp.floatValue == 1 ? 0 : 1; // On GUI S=0,R=1, flip the value
        switch (srSelected) {
            case 0: // Smoothness
                _editor.TexturePropertySingleLine(new GUIContent("Smoothness"), smoothnessTexProp, smoothnessAssigned ? null : smoothnessProp);
                break;
            case 1: // Roughness
                _editor.TexturePropertySingleLine(new GUIContent("Roughness"), roughnessTexProp, roughnessAssigned ? null : roughnessProp);
                break;
        }

        // Normal Map
        _editor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalTexProp, normalScaleProp);

        // Emission
        EditorGUI.BeginChangeCheck();
        Texture oldEmissionTex = emissionTexProp.textureValue;
        _editor.TexturePropertyWithHDRColor(new GUIContent("Emission"), emissionTexProp, emissionColorProp, false);
        // Change color to white if currently black when setting texture
        if(EditorGUI.EndChangeCheck() && oldEmissionTex != emissionTexProp.textureValue) {
            Color blackColor = new Color(0, 0, 0, emissionTexProp.colorValue.a);
            if(emissionColorProp.colorValue == blackColor && emissionTexProp.textureValue != null) {
                emissionColorProp.colorValue = Color.white;
            }
        }

        // Scale & Offset, manually setting scaleOffset and rect because using Albedo Tiling & Offset doesn't like the noise applied to the UV
        Vector2 scaleVal = scaleProp.vectorValue;
        Vector2 offsetVal = offsetProp.vectorValue;
        Vector4 scaleOffset = new Vector4(scaleVal.x, scaleVal.y, offsetVal.x, offsetVal.y);
        Rect rect = EditorGUILayout.GetControlRect(true, 2 * (LINE_HEIGHT + LINE_SPACING), EditorStyles.layerMaskField);

        EditorGUI.BeginChangeCheck();
        scaleOffset = MaterialEditor.TextureScaleOffsetProperty(rect, scaleOffset, false);
        if(EditorGUI.EndChangeCheck()) {
            scaleProp.vectorValue = new Vector2(scaleOffset.x, scaleOffset.y);
            offsetProp.vectorValue = new Vector2(scaleOffset.z, scaleOffset.w);
        }
    }

    private void DrawMaterialSettingsGUI(string materialPrefix)
    {
        // Material Properties
        MaterialProperty randomiseNoiseScalingProp = FindProperty($"_{materialPrefix}RandomiseNoiseScaling");

        MaterialProperty smoothnessEnabledProp = FindProperty($"_{materialPrefix}SmoothnessEnabled");

        GUILayout.BeginHorizontal();

        // Noise Scaling Enabled
        EditorGUI.BeginChangeCheck();
        bool randomiseNoiseScaling = randomiseNoiseScalingProp.floatValue == 1 ? true : false;
        randomiseNoiseScaling = GUILayout.Toggle(randomiseNoiseScaling, "Random Noise Scaling", "Button");
        if (EditorGUI.EndChangeCheck())
            randomiseNoiseScalingProp.floatValue = randomiseNoiseScaling ? 1.0f : 0.0f;

        // Smoothness/Roughness Toggle
        EditorGUI.BeginChangeCheck();
        float srSelected = smoothnessEnabledProp.floatValue == 1.0f ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
        srSelected = GUILayout.Toolbar((int)srSelected, new string[] { "Smooth", "Rough" });
        if (EditorGUI.EndChangeCheck())
            smoothnessEnabledProp.floatValue = srSelected == 1.0f ? 0.0f : 1.0f;

        GUILayout.EndHorizontal();
    }

    private void DrawMaterialNoiseGUI(string materialPrefix)
    {
        // Material Properties
        MaterialProperty noiseAngleOffsetProp = FindProperty($"_{materialPrefix}NoiseAngleOffset");
        MaterialProperty noiseScaleProp = FindProperty($"_{materialPrefix}NoiseScale");

        MaterialProperty randomiseNoiseScalingProp = FindProperty($"_{materialPrefix}RandomiseNoiseScaling");
        MaterialProperty noiseScalingMinMaxProp = FindProperty($"_{materialPrefix}NoiseScalingMinMax");

        // Voronoi Noise
        _editor.FloatProperty(noiseAngleOffsetProp, "Noise Angle Offset");
        _editor.FloatProperty(noiseScaleProp, "Noise Scale");

        // Scale Randomising
        if(randomiseNoiseScalingProp.floatValue == 1.0f) {
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
    }

    private void DrawDebugGUI()
    {
        // Debug Toggle
        Color prevBackgroundColor = GUI.backgroundColor;
        bool prevDebugging = _debugging;
        GUI.backgroundColor = _debugging ? Color.green : Color.red;
        _debugging = GUILayout.Toggle(_debugging, "Debug", _majorToggleButton);
        GUI.backgroundColor = prevBackgroundColor;

        GUILayout.Space(5);

        // Material Property
        MaterialProperty debuggingIndexProp = FindProperty($"_DebuggingIndex");

        if (_debugging) {
            // If just started debugging, set debugging index
            if (!prevDebugging) {
                debuggingIndexProp.floatValue = 0;
            }

            // Start Background
            float backgroundStartingYPos = StartBackground(_debugBackgroundHeight);

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

            // End Background
            float heightDiff = EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _debugBackgroundHeight = heightDiff;

            Debug.Log(heightDiff);
        } else if (debuggingIndexProp.floatValue != -1) {
            debuggingIndexProp.floatValue = -1;
        }
    }

    private float StartBackground(float backgroundHeight)
    {
        // Background, get height from previous OnGUI call
        Rect borderRect = EditorGUILayout.GetControlRect(false, 0.0f, EditorStyles.layerMaskField);
        float startingYPosition = borderRect.position.y;
        borderRect.position += new Vector2(0, -GROUP_TOP_PADDING);
        borderRect.height = backgroundHeight + GROUP_TOP_PADDING + GROUP_BOTTOM_PADDING;
        EditorGUI.DrawRect(borderRect, new Color(0, 0, 0, 0.2f));

        // Start Padding
        GUILayout.BeginHorizontal();
        GUILayout.Space(GROUP_SIDE_PADDING);
        GUILayout.BeginVertical();
        GUILayout.Space(3);

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

        return heightDifference;
    }

    private void DrawHeaderLabel(string text)
    {
        float height = _boldHeaderStyle.CalcHeight(new GUIContent("Debug Texture"), Screen.width);
        Rect rect = EditorGUILayout.GetControlRect(true, height + 5, EditorStyles.layerMaskField);
        GUI.Label(rect, new GUIContent(text), _boldHeaderStyle);
    }
}

#endif