using System.Collections.Generic;
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
        // Debug
        private int _prevDebugIndex = 0;
        #endregion

        //// Textures
        //private Dictionary<string, KeyValuePair<Texture2D[], bool[]>> _textures = new Dictionary<string, KeyValuePair<Texture2D[], bool[]>> {
        //    { "Base", new KeyValuePair<Texture2D[], bool[]> (new Texture2D[8], new bool[8]) },
        //    { "Far", new KeyValuePair<Texture2D[], bool[]> (new Texture2D[8], new bool[8]) },
        //    { "Blend", new KeyValuePair<Texture2D[], bool[]> (new Texture2D[9], new bool[9]) }
        //};
        //
        //#region Utilities
        //private void LoadTextureGroup(string textureGroup)
        //{
        //    KeyValuePair<Texture2D[], bool[]> texturesKeyValuePair = _textures[textureGroup];
        //
        //    // Get the array storing the textures
        //    string arrayPath = AssetDatabase.GetAssetPath(FindProperty($"_{textureGroup}Textures").textureValue);
        //    Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(arrayPath, typeof(Texture2DArray));
        //    if (array != null) {
        //        // Read the textures from the array
        //        Texture2D[] arrayTextures = Texture2DArrayUtilities.GetTextures(array);
        //
        //        // Get which textures are assigned
        //        int compressedAssignedTextures = (int)FindProperty($"_{textureGroup}AssignedTextures").floatValue;
        //        bool[] currentAssignedTextures = BooleanCompression.GetCompressedValues(compressedAssignedTextures, texturesKeyValuePair.Key.Length);
        //
        //        // Figure out which texture in the array goes to which texture here
        //        Texture2D[] textures = texturesKeyValuePair.Key;
        //
        //        int currentIndex = 0;
        //        for (int i = 0; i < currentAssignedTextures.Length; i++) {
        //            if (currentAssignedTextures[i]) {
        //                textures[i] = arrayTextures[currentIndex];
        //                currentIndex++;
        //            }
        //        }
        //
        //        texturesKeyValuePair = new KeyValuePair<Texture2D[], bool[]>(textures, currentAssignedTextures);
        //    }
        //
        //    _textures[textureGroup] = texturesKeyValuePair;
        //}
        //#endregion

        #region GUI Calls
        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);
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
        private void DrawMaterialGUI(string materialPrefix)
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
            bool packedTexture = BooleanCompression.GetCompressedValue(settingToggles, 5);
            bool emissionEnabled = BooleanCompression.GetCompressedValue(settingToggles, 6);

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
            string noiseEnabledStyle = noiseEnabled ? "ButtonLeft" : "Button";
            noiseEnabled = GUILayout.Toggle(noiseEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Noise", "N"), "Adds random scaling & rotation based on voronoi noise"), noiseEnabledStyle);

            if (noiseEnabled) {
                // Noise Scaling Enabled
                randomiseScaling = GUILayout.Toggle(randomiseScaling, new GUIContent(GetScaledText(minScaledTextWidth, "Random Scaling", "RS"), "Adds random scaling to each voronoi cell"), "ButtonMid");

                // Randomise Rotation Enabled
                randomiseRotation = GUILayout.Toggle(randomiseRotation, new GUIContent(GetScaledText(minScaledTextWidth, "Random Rotation", "RR"), "Adds random rotation to each voronoi cell"), "ButtonRight");
            }

            // Variation toggle
            variationEnabled = GUILayout.Toggle(variationEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Variation", "V"), "Adds random variation on top of the albedo color\n\nUsing a custom texture can cause visible tiling"), "Button");

            GUILayout.FlexibleSpace();

            // Packed Texture Toggle
            packedTexture = GUILayout.Toggle(packedTexture, new GUIContent(GetScaledText(minScaledTextWidth, "Packed Texture", "PT"), "If you are using a packed texture of multiple regular ones (Enabled is default unity material behaviour)\n\nPacked: (R: Metallic, G: Occlussion, A: Smoothness/Roughness)\n\nNon-Packed uses Red channel for each texture"), "Button");

            // Emission Toggle
            emissionEnabled = GUILayout.Toggle(emissionEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Emission", "E"), "If Emission is enabled"), "Button");

            // Smoothness/Roughness Toggle
            EditorGUI.BeginChangeCheck();
            float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
            srSelected = GUILayout.Toolbar((int)srSelected, new GUIContent[] { new GUIContent(GetScaledText(minScaledTextWidth, "Smooth", "S"), "Using smoothness for material (Default unity material behaviour)"), new GUIContent(GetScaledText(minScaledTextWidth, "Rough", "R"), "Uses roughness for material (1 - smoothness)") });
            if (EditorGUI.EndChangeCheck())
                smoothnessEnabled = srSelected == 1.0f ? false : true;

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
            bool packedTexture = BooleanCompression.GetCompressedValue(settingToggles, 5);
            bool emissionEnabled = BooleanCompression.GetCompressedValue(settingToggles, 6);

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
            _editor.TexturePropertySingleLine(new GUIContent("Albedo", "Albedo (RGB), Transparency (A)"), albedoTexProp, abledoTintProp);

            // Metallic
            materialProperties1.x = GUIUtilities.DrawTexturePropertyWithSlider(_editor, metallicTexProp, !metallicAssigned, materialProperties1.x, new GUIContent("Metallic", "Metallic (R), other channels are ignored"));

            // Smoothness/Roughness
            float srSelected = smoothnessEnabled ? 0.0f : 1.0f; // On GUI S=0,R=1, flip the value
            switch (srSelected) {
                case 0: // Smoothness
                    materialProperties1.y = GUIUtilities.DrawTexturePropertyWithSlider(_editor, smoothnessTexProp, !smoothnessAssigned, materialProperties1.y, new GUIContent("Smoothness", $"Smoothness ({(packedTexture ? 'A' : 'R')}), other channels are ignored"));

                    break;
                case 1: // Roughness
                    materialProperties1.z = GUIUtilities.DrawTexturePropertyWithSlider(_editor, roughnessTexProp, !roughnessAssigned, materialProperties1.z, new GUIContent("Roughness", $"Roughness ({(packedTexture ? 'A' : 'R')}), other channels are ignored"));

                    break;
            }

            // Normal Map
            materialProperties1.w = GUIUtilities.DrawTexturePropertyWithSlider(_editor, normalTexProp, normalAssigned, materialProperties1.w, new GUIContent("Normal Map"));

            // Occlussion Map
            materialProperties2.x = GUIUtilities.DrawTexturePropertyWithSlider(_editor, occlussionTexProp, occlussionAssigned, materialProperties2.x, new GUIContent("Occlussion", $"Occlussion ({(packedTexture ? 'G' : 'R')}), other channels are ignored"));

            // Emission
            if (emissionEnabled) {
                EditorGUI.BeginChangeCheck();
                Texture oldEmissionTex = emissionTexProp.textureValue;
                _editor.TexturePropertyWithHDRColor(new GUIContent("Emission", "Emission (RGB)"), emissionTexProp, emissionColorProp, false);
                // Change color to white if currently black when setting texture
                if (EditorGUI.EndChangeCheck() && oldEmissionTex != emissionTexProp.textureValue) {
                    Color blackColor = new Color(0, 0, 0, emissionTexProp.colorValue.a);
                    if (emissionColorProp.colorValue == blackColor && emissionTexProp.textureValue != null) {
                        emissionColorProp.colorValue = Color.white;
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
                _editor.TexturePropertySingleLine(new GUIContent("Variation Teture", "Variation (R), other channels are ignored\n\nTexture that is drawn onto other materials, can cause visible tiling"), variationTexturesProp);

                // Tiling & Offset
                GUIUtilities.DrawTilingOffset(variationTextureTOProp, "Variation Scale", "Variation Offset");
            }

            // Assign Property
            if (variationSettings != oriVariationSettings)
                variationSettingsProp.vectorValue = variationSettings;
        }

        private void DrawBaseMaterialGUI()
        {
            GUIUtilities.BeginBackgroundVertical();

            DrawMaterialGUI("Base");

            GUIUtilities.EndBackgroundVertical();
        }

        private void DrawDistanceBlendGUI()
        {
            // Material Property
            MaterialProperty distanceBlendEnabledProp = FindProperty("_DistanceBlendEnabled");

            // Start Background
            GUIUtilities.BeginBackgroundVertical();

            // Distance Blend Enabled Toggle
            bool distanceBlendEnabled = GUIUtilities.DrawMajorToggleButton(distanceBlendEnabledProp, "Distance Blending");

            // Draw distance blending settings
            if (distanceBlendEnabled) {
                // Material Properties
                MaterialProperty distanceBlendModeProp = FindProperty("_DistanceBlendMode");
                MaterialProperty distanceBlendMinMaxProp = FindProperty("_DistanceBlendMinMax");

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
                        MaterialProperty tilingOffsetProp = FindProperty("_FarTilingOffset");

                        // Tiling & Offset GUI
                        GUIUtilities.DrawTilingOffset(tilingOffsetProp);
                        break;
                    case EDistanceBlendMode.Material:
                        GUILayout.Space(10);

                        // Material GUI
                        DrawMaterialGUI("Far");
                        break;
                }
            }

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }

        private void DrawMaterialBlendGUI()
        {
            // Start Background
            GUIUtilities.BeginBackgroundVertical();

            // Material Property
            MaterialProperty materialBlendingSettingsProp = FindProperty("_MaterialBlendSettings");

            int materialBlendingSettings = (int)materialBlendingSettingsProp.floatValue;
            bool materialBlendingEnabled = BooleanCompression.GetCompressedValue(materialBlendingSettings, 0);
            bool overrideDistanceBlend = BooleanCompression.GetCompressedValue(materialBlendingSettings, 1);
            bool overrideDistanceBlendTO = BooleanCompression.GetCompressedValue(materialBlendingSettings, 2);

            // Material Blend Enabled Toggle
            materialBlendingEnabled = GUIUtilities.DrawMajorToggleButton(materialBlendingEnabled, "Material Blending");

            if (materialBlendingEnabled) {
                // Material Properties
                MaterialProperty blendMaskTypeProp = FindProperty("_BlendMaskType");
                MaterialProperty distanceBlendEnabledProp = FindProperty("_DistanceBlendEnabled");

                MaterialProperty materialBlendPropertiesProp = FindProperty("_MaterialBlendProperties");

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
                    MaterialProperty materialBlendNoiseSettingsProp = FindProperty("_MaterialBlendNoiseSettings");

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
                    MaterialProperty blendMaskTextureProp = FindProperty("_BlendMaskTexture");
                    MaterialProperty blendMaskTextureTOProp = FindProperty("_BlendMaskTextureTO");

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
                    MaterialProperty distanceBlendModeProp = FindProperty("_DistanceBlendMode");
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
            int compressedMaterialBlendSettings = BooleanCompression.CompressValues(materialBlendingEnabled, overrideDistanceBlend, overrideDistanceBlendTO);
            materialBlendingSettingsProp.floatValue = compressedMaterialBlendSettings;

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }

        private void DrawDebugGUI()
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
        #endregion

    }
}
#endif