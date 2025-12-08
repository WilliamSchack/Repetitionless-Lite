#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    using GUIUtilities;
    using TextureUtilities;
    using CustomWindows;
    using Compression;
    using Variables;
    using Data;

    /// <summary>
    /// Base class for creating the Master/Terrain repetitionless inspector windows<br />
    /// This assumes individual textures which is now unused in the current materials<br />
    /// To use the current packed texture arrays, use RepetitionlessPackedArrayGUIBase
    /// </summary>
    public class RepetitionlessGUIBaseNEW : ShaderGUI
    {
        #region Variables
        
        /// <summary>
        /// Holds the state for each property foldout in a material section
        /// </summary>
        protected class MaterialFoldoutState
        {
            /// <summary>
            /// Foldout state for the main properties
            /// </summary>
            public bool MainProperties = true;

            /// <summary>
            /// Foldout state for the noise properties
            /// </summary>
            public bool NoiseProperties = true;

            /// <summary>
            /// Foldout state for the variation properties
            /// </summary>
            public bool VariationProperties = true;
        }

        protected struct TextureDrawerDetails
        {
            public TextureArrayCustomChannelsGUIDrawer TextureDrawer;
            public int ChannelIndex;

            public TextureDrawerDetails(TextureArrayCustomChannelsGUIDrawer textureDrawer, int channelIndex)
            {
                TextureDrawer = textureDrawer;
                ChannelIndex = channelIndex;
            }
        }

        // Constants

        /// <summary>
        /// Amount of padding at the top of the inspector
        /// </summary>
        protected const int HEADER_PADDING = 4;

        /// <summary>
        /// Amount of padding between sections
        /// </summary>
        protected const int SETTING_PADDING = 4;

        /// <summary>
        /// Buffer ontop of minWidth for GetScaledText
        /// </summary>
        protected const int SCALED_TEXT_PADDING = 10;

        protected const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";
        protected const string PROPERTIES_HANDLER_FILE_NAME = "Properties.asset";

        private const string PROGRESS_BAR_TITLE = "Updating Material";

        private const string DEFAULT_VARIATION_TEXTURE_NAME = "repetitionless_VariationTexture_2048";

        // Overridable

        protected virtual int _materialCount => 3;

        // Texture drawers

        protected TextureArrayCustomChannelsGUIDrawer _avTexturesDrawer;

        protected TextureArrayCustomChannelsGUIDrawer _nsoTexturesDrawer;

        protected TextureArrayCustomChannelsGUIDrawer _emTexturesDrawer;

        // Data
        protected MaterialDataManager _dataManager;
        protected RepetitionlessTextureDataSO _textureData;
        protected RepetitionlessMaterialDataSO _materialProperties;

        // Array Settings Button
        private GUIContent _settingsIconContent;

        // Material Helpers

        /// <summary>
        /// The material being edited
        /// </summary>
        protected Material _material;

        /// <summary>
        /// The material editor being used
        /// </summary>
        protected MaterialEditor _editor;

        /// <summary>
        /// <b>Use FindProperty for getting properties</b><br />
        /// Contains all the material properties<br />
        /// </summary>
        protected Dictionary<string, MaterialProperty> _cachedProperties = new Dictionary<string, MaterialProperty>();

        // Foldout States, dynamically adds new materialPrefixes

        /// <summary>
        /// Contains the current states for all foldouts<br />
        /// Keys are the material property prefix for that section
        /// </summary>
        protected Dictionary<string, MaterialFoldoutState> _foldoutStates = new Dictionary<string, MaterialFoldoutState>();

        // Debug
        private int _prevDebugIndex = 0;

        // ShaderGUI doesnt have an OnEnable function, using this instead
        private bool _firstSetup = true;
        #endregion

        #region Helpers
        /// <summary>
        /// Gets a property from the cached properties
        /// </summary>
        /// <param name="name">
        /// The name of the material property
        /// </param>
        /// <returns>
        /// The material property requested
        /// </returns>
        protected MaterialProperty FindProperty(string name)
        {
            return _cachedProperties[name];
        }

        /// <summary>
        /// Dynamically returns a text if the window width is within the given minWidth 
        /// </summary>
        /// <param name="minWidth">
        /// The minimum width for the window width to be to show the large text
        /// </param>
        /// <param name="largeText">
        /// The text to return if the window width is greater than minWidth
        /// </param>
        /// <param name="smallText">
        /// The text to return if the window width is less than minWidth
        /// </param>
        /// <returns>
        /// The scaled text
        /// </returns>
        protected string GetScaledText(int minWidth, string largeText, string smallText)
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

        private string GetMaterialPrefix(int sectionIndex)
        {
            switch (sectionIndex) {
                case 0: return "Base";
                case 1: return "Far";
                case 2: return "Blend";
            }

            return "Base";
        }

        private int GetSectionIndex(string materialPrefix)
        {
            switch (materialPrefix) {
                case "Base":  return 0;
                case "Far":   return 1;
                case "Blend": return 2;
            }
            
            return 0;
        }

        protected RepetitionlessLayerData GetLayerData()
        {
            return _materialProperties.Data;
        }

        protected RepetitionlessMaterialData GetMaterialData(int sectionIndex)
        {
            RepetitionlessMaterialData currentData = _materialProperties.Data.BaseMaterialData;
            switch (sectionIndex) {
              //case 0: currentData = _materialProperties.Data.BaseMaterialData;  break;
                case 1: currentData = _materialProperties.Data.FarMaterialData;   break;
                case 2: currentData = _materialProperties.Data.BlendMaterialData; break; 
            }

            return currentData;
        }

        protected RepetitionlessMaterialData GetMaterialData(string materialPrefix)
        {
            int sectionIndex = GetSectionIndex(materialPrefix);
            return GetMaterialData(sectionIndex);
        }

        protected virtual void UpdateMaterialPropertiesTexture(int layerIndex = 0)
        {
            MaterialProperty textureProperty = FindProperty("_PropertiesTexture");
            _materialProperties.UpdateMaterialTexture(textureProperty, layerIndex);
        }

        // Each gui function modifying the material properties should be using this function
        // Must be called to properly save the properties
        protected virtual void DrawProperty(System.Action drawPropertyAction, int layerIndex = 0)
        {
            if (drawPropertyAction == null)
                return;

            EditorGUI.BeginChangeCheck();
            drawPropertyAction();
            if (EditorGUI.EndChangeCheck())
                UpdateMaterialPropertiesTexture();
        }

        private void UpdateVariationTexture(int sectionIndex, ETextureType prevVariationMode)
        {
            RepetitionlessMaterialData currentData = GetMaterialData(sectionIndex);

            ref RepetitionlessTextureDataSO.MaterialTextureData textureData = ref _textureData.MaterialsTextureData[sectionIndex];

            // If enabling texture, add it to the array
            if (currentData.VariationMode == ETextureType.CustomTexture) {
                textureData.AVTextures[1].Disabled = false;

                // Set the texture to the default variation if none assigned
                Texture2D texture = GetArrayLayerTextureData(0, sectionIndex)[1].Texture;
                if (texture == null) {
                    texture = Resources.Load<Texture2D>(DEFAULT_VARIATION_TEXTURE_NAME);
                }

                bool textureAdded = _avTexturesDrawer.UpdateTexture(texture, sectionIndex, 1, true).Item2;
                if (!textureAdded)
                    textureData.AVTextures[1].Texture = null;

                string materialPrefix = GetMaterialPrefix(sectionIndex);
                HandleAssignedTextures(materialPrefix, sectionIndex);
                SaveTextureData();
            }

            // If was texture, remove it from the array
            else if (prevVariationMode == ETextureType.CustomTexture) {
                textureData.AVTextures[1].Disabled = true;
                SaveTextureData();

                if (textureData.AVTextures[1].Texture != null)
                    _avTexturesDrawer.UpdateTexture(GetArrayLayerTextureData(0, sectionIndex)[1].Texture, sectionIndex, 1, true);
            }
        }

        /// <summary>
        /// Used to draw all the texture fields<br />
        /// Can be overrided to change how textures are drawn
        /// </summary>
        /// <param name="sectionIndex">
        /// The section that this texture is in
        /// </param>
        /// <param name="textureIndex">
        /// The index of the texture in this section
        /// </param>
        /// <param name="content">
        /// The GUIContent to use in the field
        /// </param>
        /// <param name="texturePropertyName">
        /// The texture material property name
        /// </param>
        /// <returns>
        /// The rect that the texture field is using
        /// </returns>
        protected virtual Rect DrawTexture(int sectionIndex, int textureIndex, GUIContent content, string texturePropertyName)
        {
            Rect lineRect = GUIUtilities.GetLineRect();

            RepetitionlessMaterialData currentData = GetMaterialData(sectionIndex);

            TextureDrawerDetails textureDrawerDetails = GetTextureDrawerDetails(textureIndex, currentData.PackedTexture);

            EditorGUI.BeginChangeCheck();
            textureDrawerDetails.TextureDrawer.DrawTexture(lineRect, sectionIndex, textureDrawerDetails.ChannelIndex, content);

            if (EditorGUI.EndChangeCheck()) {
                // Rehandle assigned textures and update the properties
                string materialPrefix = GetMaterialPrefix(sectionIndex);
                HandleAssignedTextures(materialPrefix, sectionIndex);

                // If packed texture was changed, manually update texture in emission array aswell
                if (currentData.PackedTexture && textureIndex == 1)
                    _emTexturesDrawer.UpdateTexture(GetArrayLayerTextureData(1, sectionIndex)[3].Texture, 0, 2, true);
            }

            // Return rect after texture field
            lineRect = MaterialEditor.GetRectAfterLabelWidth(lineRect);
            return lineRect;
        }

        /// <summary>
        /// Handles assigned textures that the shader uses to determine whether to use textures or values<br />
        /// Can be overrided to change how the assigned textures are set
        /// </summary>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="sectionIndex">
        /// The section that this texture is in
        /// </param>
        /// <param name="settingsProp">
        /// The settings material property for this material section
        /// </param>
        /// <returns>
        /// The compressed assigned textures
        /// </returns>
        protected virtual void HandleAssignedTextures(string materialPrefix, int sectionIndex, int layerIndex = 0)
        {
            RepetitionlessTextureDataSO.MaterialTextureData materialTextureData = _textureData.MaterialsTextureData[sectionIndex];

            RepetitionlessMaterialData currentData = GetMaterialData(sectionIndex);
            bool packedTextureAssigned = currentData.PackedTexture ? materialTextureData.NSOTextures[3].Texture != null : false;

            currentData.AlbedoAssigned     = materialTextureData.AVTextures[0].Texture != null;
            currentData.MetallicAssigned   = packedTextureAssigned ? true : materialTextureData.EMTextures[1].Texture != null;
            currentData.SmoothnessAssigned = packedTextureAssigned ? true : materialTextureData.NSOTextures[1].Texture != null;
            currentData.NormalAssigned     = materialTextureData.NSOTextures[0].Texture != null;
            currentData.OcclussionAssigned = packedTextureAssigned ? true : materialTextureData.NSOTextures[2].Texture != null;
            currentData.EmissionAssigned   = materialTextureData.EMTextures[0].Texture != null;
            currentData.VariationAssigned  = materialTextureData.AVTextures[1].Texture != null;

            UpdateMaterialPropertiesTexture();
        }

        protected virtual void SaveSO(ScriptableObject so)
        {
            // Only set it to dirty and let unity handle the 
            // Refreshing the asset database every update is slow
            EditorUtility.SetDirty(so);
        }
        
        protected virtual void SaveTextureData() { SaveSO(_textureData); }

        protected virtual void SaveMaterialProperties() { SaveSO(_materialProperties); }

        protected TextureDrawerDetails GetTextureDrawerDetails(int textureIndex, bool packedTexture)
        {
            if (packedTexture && textureIndex == 1) {
                return new TextureDrawerDetails(_nsoTexturesDrawer, 3); // Packed Texture
            }

            switch (textureIndex) {
                case 0: return new TextureDrawerDetails(_avTexturesDrawer, 0);  // Albedo
                case 1: return new TextureDrawerDetails(_emTexturesDrawer, 1);  // Metallic
                case 2: return new TextureDrawerDetails(_nsoTexturesDrawer, 1); // Smoothness/Roughness
                case 3: return new TextureDrawerDetails(_nsoTexturesDrawer, 0); // Normal
                case 4: return new TextureDrawerDetails(_nsoTexturesDrawer, 2); // Occlussion
                case 5: return new TextureDrawerDetails(_emTexturesDrawer, 0);  // Emission
                case 6: return new TextureDrawerDetails(_avTexturesDrawer, 1);  // Variation
            }

            return new TextureDrawerDetails(null, 0);
        }

        private TexturePacker.TextureData[] GetArrayLayerTextureData(int materialIndex, int layerIndex)
        {
            RepetitionlessTextureDataSO.MaterialTextureData materialData = _textureData.MaterialsTextureData[layerIndex];
            
            switch(materialIndex) {
                case 0: return materialData.AVTextures;
                case 1: return materialData.NSOTextures;
                case 2: return materialData.EMTextures;
            }

            return null;
        }

        #endregion

        #region GUI Calls
        /// <summary>
        /// Called when the inspector is first opened
        /// </summary>
        /// <param name="materialEditor">
        /// The material editor being used
        /// </param>
        public virtual void OnEnable(MaterialEditor materialEditor)
        {
            // Assign Material Helpers
            _material = (Material)materialEditor.target;
            _editor = materialEditor;

            // Initialize styles
            _settingsIconContent = EditorGUIUtility.IconContent("Settings");
            _settingsIconContent.tooltip = "Texture Array Settings";

            // Material Properties
            MaterialProperty albedoVTexturesProp = FindProperty("_AVTextures");
            MaterialProperty normalSOTexturesProp = FindProperty("_NSOTextures");
            MaterialProperty emissionMTexturesProp = FindProperty("_EMTextures");
            MaterialProperty assignedAlbedoVTexturesProp = FindProperty("_AssignedAVTextures");
            MaterialProperty assignedNormalSOTexturesProp = FindProperty("_AssignedNSOTextures");
            MaterialProperty assignedEmissionMTexturesProp = FindProperty("_AssignedEMTextures");

            // Setup data
            Material material = (Material)albedoVTexturesProp.targets[0];
            _dataManager = new MaterialDataManager(material);

            bool progressBarUsed = false;

            if (_dataManager.AssetExists(TEXTURE_DATA_FILE_NAME)) {
                _textureData = _dataManager.LoadAsset<RepetitionlessTextureDataSO>(TEXTURE_DATA_FILE_NAME);
            } else {
                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Creating Texture Data", 0.0f);
                progressBarUsed = true;

                _textureData = ScriptableObject.CreateInstance<RepetitionlessTextureDataSO>();
                _dataManager.CreateAsset(_textureData, TEXTURE_DATA_FILE_NAME);
                _textureData.Init(_materialCount);

                SaveTextureData();
                AssetDatabase.SaveAssetIfDirty(_textureData);
            }

            if (_dataManager.AssetExists(PROPERTIES_HANDLER_FILE_NAME)) {
                _materialProperties = _dataManager.LoadAsset<RepetitionlessMaterialDataSO>(PROPERTIES_HANDLER_FILE_NAME);
                _materialProperties.SetDataManager(_dataManager);
            } else {
                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Creating Properties", 0.3f);
                progressBarUsed = true;

                _materialProperties = ScriptableObject.CreateInstance<RepetitionlessMaterialDataSO>();
                _dataManager.CreateAsset(_materialProperties, PROPERTIES_HANDLER_FILE_NAME);
                _materialProperties.Init(1);
                _materialProperties.SetDataManager(_dataManager);
                SaveMaterialProperties();
                AssetDatabase.SaveAssetIfDirty(_materialProperties);

                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Writing Properties", 0.8f);
                UpdateMaterialPropertiesTexture(0);
            }

            if (progressBarUsed)
                EditorUtility.ClearProgressBar();


            // Texture Drawers
            _avTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(0, i); }, SaveTextureData, RepetitionlessTextureDataSO.DEFAULT_AV_COLOUR, albedoVTexturesProp, assignedAlbedoVTexturesProp, _materialCount);
            _nsoTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(1, i); }, SaveTextureData, RepetitionlessTextureDataSO.DEFAULT_NSO_COLOUR, normalSOTexturesProp, assignedNormalSOTexturesProp, _materialCount);
            _emTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(2, i); }, SaveTextureData, RepetitionlessTextureDataSO.DEFAULT_EM_COLOUR, emissionMTexturesProp, assignedEmissionMTexturesProp, _materialCount);

            _avTexturesDrawer.TextureFormat = TextureFormat.BC7;
            _nsoTexturesDrawer.TextureFormat = TextureFormat.BC7;
            _nsoTexturesDrawer.ArrayLinear = true;
            _emTexturesDrawer.TextureFormat = TextureFormat.BC7;
        }

        /// <summary>
        /// Base OnGUI function
        /// </summary>
        /// <param name="materialEditor">
        /// The material editor being used
        /// </param>
        /// <param name="properties">
        /// The material properties
        /// </param>
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

            GUILayout.Space(SETTING_PADDING);
        }
        #endregion

        #region Material GUI
        /// <summary>
        /// Draws the general material settings at the top of the inspector
        /// </summary>
        protected virtual void DrawMaterialPropertiesGUI()
        {
            // Header
            GUIUtilities.DrawHeaderLabelLarge($"Material Properties");

            GUILayout.Space(4);

            // Surface Type
            MaterialProperty surfaceTypeProp = FindProperty("_SurfaceTypeSetting");

            EditorGUI.BeginChangeCheck();
            ESurfaceType surfaceType = (ESurfaceType)surfaceTypeProp.floatValue;
            surfaceType = (ESurfaceType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Surface Type", surfaceType);

            if (EditorGUI.EndChangeCheck()) {
                // In HDRP the keywords are different so check for it
                // We can assume HDRP is being used if they are modifying this material
                bool usingHDRP = false;
                RenderPipelineAsset currentRpAsset = GraphicsSettings.currentRenderPipeline;
                if (currentRpAsset != null) usingHDRP = currentRpAsset.GetType().ToString().Contains("HDRenderPipelineAsset");

                surfaceTypeProp.floatValue = (int)surfaceType;
                switch (surfaceType) {
                    case ESurfaceType.Opaque:
                        _material.renderQueue = (int)RenderQueue.Geometry;
                        _material.SetOverrideTag("RenderType", "Opaque");

                        if (usingHDRP) {
                            _material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            _material.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                            _material.SetInt("_SurfaceType", 0);
                            _material.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                            _material.SetInt("_AlphaDstBlend", (int)BlendMode.Zero);
                            _material.SetInt("_AlphaCutoffEnable", 0);
                            _material.SetInt("_ZWrite", 1);
                        } else {
                            _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                            _material.SetInt("_BUILTIN_Surface", 0);
                            _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                            _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                            _material.SetInt("_BUILTIN_ZWrite", 1);
                        }
                        break;
                    case ESurfaceType.Cutout:
                        _material.renderQueue = (int)RenderQueue.AlphaTest;
                        _material.SetOverrideTag("RenderType", "TransparentCutout");

                        if (usingHDRP) {
                            _material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

                            _material.SetInt("_SurfaceType", 0);
                            _material.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                            _material.SetInt("_AlphaDstBlend", (int)BlendMode.Zero);
                            _material.SetInt("_AlphaCutoffEnable", 1);
                            _material.SetInt("_ZWrite", 1);
                        } else {
                            _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
                            _material.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                            _material.SetInt("_BUILTIN_Surface", 0);
                            _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                            _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                            _material.SetInt("_BUILTIN_ZWrite", 1); 
                        }
                        break;
                    case ESurfaceType.Transparent:
                        _material.renderQueue = (int)RenderQueue.Transparent;
                        _material.SetOverrideTag("RenderType", "Transparent");

                        if (usingHDRP) {
                            _material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            _material.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                            _material.SetInt("_SurfaceType", 1);
                            _material.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                            _material.SetInt("_AlphaDstBlend", (int)BlendMode.OneMinusSrcAlpha);
                            _material.SetInt("_AlphaCutoffEnable", 0);
                            _material.SetInt("_ZWrite", 0);
                        } else {
                            _material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                            _material.SetInt("_BUILTIN_Surface", 1);
                            _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.SrcAlpha);
                            _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                            _material.SetInt("_BUILTIN_ZWrite", 0);
                        }
                        break;
                }
            }

            // Advanced Options
            _editor.LightmapEmissionProperty();
            _editor.RenderQueueField();
            _editor.DoubleSidedGIField();
        }
        #endregion

        #region Base Material GUI Sections
        /// <summary>
        /// Draws a material section GUI
        /// </summary>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="sectionIndex">
        /// The material section index
        /// </param>
        /// <param name="headerText">
        /// The header label text for the section
        /// </param>
        protected virtual void DrawMaterialGUI(string materialPrefix, int sectionIndex, string headerText = "")
        {
            RepetitionlessMaterialData currentData = GetMaterialData(sectionIndex);

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

            // Draw Noise Properties
            if (currentData.NoiseEnabled) {
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
            if (currentData.VariationEnabled) {
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

        /// <summary>
        /// Draws the settings at the top of each material section
        /// </summary>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="showNoise">
        /// Toggles if the noise settings are enabled
        /// </param>
        /// <param name="showVariation">
        /// Toggles if the variation setting is enabled
        /// </param>
        /// <param name="showPT">
        /// Toggles if the packed texture setting is enabled
        /// </param>
        /// <param name="showEmission">
        /// Toggles if the emission setting is enabled
        /// </param>
        /// <param name="showSR">
        /// Toggles if the smoothness/roughness toggle setting is enabled
        /// </param>
        /// <param name="extraWidth">
        /// Any extra width required for the whole section<br />
        /// Used to increase the required width for the labels to expand
        /// </param>
        protected virtual void DrawMaterialSettingsGUI(string materialPrefix, bool showNoise = true, bool showVariation = true, bool showPT = true, bool showEmission = true, bool showSR = true, int extraWidth = 0)
        {
            RepetitionlessMaterialData currentData = GetMaterialData(materialPrefix);

            // Calculate scaled text min width
            int minScaledTextWidth = 0;
            if (showNoise)     minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Noise")).x;
            if (showVariation) minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Variation")).x;
            if (showPT)        minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Packed Texture")).x;
            if (showEmission)  minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Emission")).x;
            if (showSR) {
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Smooth")).x;
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Rough")).x;
            }
            if (currentData.NoiseEnabled) {
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Random Scaling")).x;
                minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Random Rotation")).x;
            }
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(_settingsIconContent).x;
            minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("-----------")).x; // Filler space
            minScaledTextWidth += extraWidth;

            EditorGUILayout.BeginHorizontal();

            // Draw Settings
            DrawLeftMaterialSettingsGUI(currentData, materialPrefix, minScaledTextWidth, showNoise, showVariation);
            GUILayout.FlexibleSpace();
            DrawRightMaterialSettingsGUI(currentData, materialPrefix, minScaledTextWidth, showPT, showEmission, showSR);

            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Draws the left section of the material settings
        /// </summary>
        /// <param name="compressedValues">
        /// The compressed setting values to modify
        /// </param>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="settingToggles">
        /// The compressed settings toggles
        /// </param>
        /// <param name="minScaledTextWidth">
        /// The minimum width required for labels to expand
        /// </param>
        /// <param name="showNoise">
        /// Toggles if the noise settings are enabled
        /// </param>
        /// <param name="showVariation">
        /// Toggles if the variation setting is enabled
        /// </param>
        /// <returns>
        /// The modified compressed setting values
        /// </returns>
        protected virtual void DrawLeftMaterialSettingsGUI(RepetitionlessMaterialData currentData, string materialPrefix, int minScaledTextWidth, bool showNoise = true, bool showVariation = true)
        {
            // Noise Enabled
            if (showNoise) {
                string noiseEnabledStyle = currentData.NoiseEnabled ? "ButtonLeft" : "Button";
                DrawProperty(() => currentData.NoiseEnabled = GUILayout.Toggle(currentData.NoiseEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Noise", "N"), "Adds random scaling & rotation based on voronoi noise"), noiseEnabledStyle));

                if (currentData.NoiseEnabled) {
                    // Noise Scaling Enabled
                    DrawProperty(() => currentData.RandomiseNoiseScaling = GUILayout.Toggle(currentData.RandomiseNoiseScaling, new GUIContent(GetScaledText(minScaledTextWidth, "Random Scaling", "RS"), "Adds random scaling to each voronoi cell"), "ButtonMid"));

                    // Randomise Rotation Enabled
                    DrawProperty(() => currentData.RandomiseNoiseRotation = GUILayout.Toggle(currentData.RandomiseNoiseRotation, new GUIContent(GetScaledText(minScaledTextWidth, "Random Rotation", "RR"), "Adds random rotation to each voronoi cell"), "ButtonRight"));
                }
            }

            // Variation toggle
            if (showVariation) {
                EditorGUI.BeginChangeCheck();
                DrawProperty(() => currentData.VariationEnabled = GUILayout.Toggle(currentData.VariationEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Variation", "V"), "Adds random variation on top of the albedo color\n\nUsing a custom texture can cause visible tiling"), "Button"));
                if (EditorGUI.EndChangeCheck() && currentData.VariationMode == ETextureType.CustomTexture)
                    UpdateVariationTexture(GetSectionIndex(materialPrefix), ETextureType.PerlinNoise);
            }
        }

        /// <summary>
        /// Draws the right section of the material settings
        /// </summary>
        /// <param name="compressedValues">
        /// The compressed setting values to modify
        /// </param>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="settingToggles">
        /// The compressed settings toggles
        /// </param>
        /// <param name="minScaledTextWidth">
        /// The minimum width required for labels to expand
        /// </param>
        /// <param name="showPT">
        /// Toggles if the packed texture setting is enabled
        /// </param>
        /// <param name="showEmission">
        /// Toggles if the emission setting is enabled
        /// </param>
        /// <param name="showSR">
        /// Toggles if the smoothness/roughness toggle setting is enabled
        /// </param>
        /// <returns>
        /// The modified compressed setting values
        /// </returns>
        protected virtual void DrawRightMaterialSettingsGUI(RepetitionlessMaterialData currentData, string materialPrefix, int minScaledTextWidth, bool showPT = true, bool showEmission = true, bool showSR = true)
        {
            // Packed Texture Toggle
            if (showPT) {
                bool prevPackedTexture = currentData.PackedTexture;
                DrawProperty(() => currentData.PackedTexture = GUILayout.Toggle(currentData.PackedTexture, new GUIContent(GetScaledText(minScaledTextWidth, "Packed Texture", "PT"), "If you are using a packed texture of multiple regular ones (Better for performance)\nR: Metallic\nG: Occlussion\nA: Smoothness/Roughness"), "Button"));

                // If packed texture was changed, update the texture data
                if (prevPackedTexture != currentData.PackedTexture) {
                    int sectionIndex = GetSectionIndex(materialPrefix);

                    _textureData.SetPackedTextureEnabled(sectionIndex, currentData.PackedTexture);
                    SaveTextureData();

                    // Repack the textures
                    if (currentData.PackedTexture) {
                        // Use packed texture
                        _nsoTexturesDrawer.UpdateTexture(_textureData.MaterialsTextureData[sectionIndex].NSOTextures[3].Texture, sectionIndex, 3, true);
                        _emTexturesDrawer.UpdateTexture(_textureData.MaterialsTextureData[sectionIndex].EMTextures[2].Texture, sectionIndex, 2, true);
                    } else {
                        // Use regular textures
                        _nsoTexturesDrawer.UpdateTexture(_textureData.MaterialsTextureData[sectionIndex].NSOTextures[1].Texture, sectionIndex, 1, true);
                        _nsoTexturesDrawer.UpdateTexture(_textureData.MaterialsTextureData[sectionIndex].NSOTextures[2].Texture, sectionIndex, 2, true);
                        _emTexturesDrawer.UpdateTexture(_textureData.MaterialsTextureData[sectionIndex].EMTextures[1].Texture, sectionIndex, 1, true);
                    }
                }
            }

            // Emission Toggle
            if (showEmission)
                DrawProperty(() => currentData.EmissionEnabled = GUILayout.Toggle(currentData.EmissionEnabled, new GUIContent(GetScaledText(minScaledTextWidth, "Emission", "E"), "If Emission is enabled"), "Button"));

            // Smoothness/Roughness Toggle
            if (showSR) {
                // S=0,R=1, flip the value
                DrawProperty(() => currentData.SmoothnessEnabled = GUILayout.Toolbar(currentData.SmoothnessEnabled ? 0 : 1, new GUIContent[] { new GUIContent(GetScaledText(minScaledTextWidth, "Smooth", "S"), "Using smoothness for material\n(Default unity material behaviour)"), new GUIContent(GetScaledText(minScaledTextWidth, "Rough", "R"), "Uses roughness for material (1 - smoothness)") }) == 0 ? true : false);
            }

            // Array settings button
            if (GUILayout.Button(_settingsIconContent)) {
                // Get the texture array for this material
                int sectionIndex = materialPrefix.Contains("Far") ? 1 : 2;
                TextureDrawerDetails textureDrawerDetails = GetTextureDrawerDetails(sectionIndex, currentData.PackedTexture);

                if (textureDrawerDetails.TextureDrawer.Array != null) {
                    ConfigureArrayWindowLimited.ShowWindow(textureDrawerDetails.TextureDrawer.Array, $"{materialPrefix} Array", (Texture2DArray newArray) => {
                        textureDrawerDetails.TextureDrawer.UpdateArray(newArray);
                    });
                } else
                    Debug.LogWarning($"{materialPrefix} has no textures assigned to modify...");
            }
        }

        /// <summary>
        /// Draws the main properties in a material section
        /// </summary>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="sectionIndex">
        /// The material section index
        /// </param>
        protected virtual void DrawMaterialMainProperties(string materialPrefix, int sectionIndex)
        {
            RepetitionlessMaterialData currentData = GetMaterialData(materialPrefix);

            // Albedo
            Rect albedoTintRect = DrawTexture(sectionIndex, 0, new GUIContent("Albedo", "Albedo (RGB), Transparency (A)"), $"_{materialPrefix}Albedo");
            DrawProperty(() => currentData.AlbedoTint = EditorGUI.ColorField(albedoTintRect, currentData.AlbedoTint));

            // Normal Map
            Rect normalStrengthSliderRect = DrawTexture(sectionIndex, 3, new GUIContent("Normal Map"), $"_{materialPrefix}NormalMap");
            if (currentData.NormalAssigned)
                DrawProperty(() => currentData.NormalScale = EditorGUI.FloatField(normalStrengthSliderRect, currentData.NormalScale));

            if (currentData.PackedTexture) {
                // Use metallic as packed texture
                DrawTexture(sectionIndex, 1, new GUIContent("Packed Texture", $"Smoothness/Roughness can be toggled above.\nIf your material is darker with this enabled, untick \"sRGB\" in the texture import settings\n\nR: Metallic\nG: Occlussion\nA: {(currentData.SmoothnessEnabled ? "Smoothness" : "Roughness")}"), $"_{materialPrefix}MetallicMap");

                // Occlussion slider
                // Get rects seperately to make slider same width as others
                Rect occlussionStrengthLabelRect = GUIUtilities.GetLineRect();
                Rect occlussionStrengthSliderRect = MaterialEditor.GetRectAfterLabelWidth(occlussionStrengthLabelRect);
                
                // Push out label to be inline with Packed Texture
                occlussionStrengthLabelRect.x += 30;
                occlussionStrengthLabelRect.width -= 30;

                EditorGUI.LabelField(occlussionStrengthLabelRect, "Occlussion Strength");
                DrawProperty(() => currentData.OcclussionStrength = EditorGUI.Slider(occlussionStrengthSliderRect, currentData.OcclussionStrength, 0, 1));
            } else {
                // Metallic
                Rect metallicSliderRect = DrawTexture(sectionIndex, 1, new GUIContent("Metallic", "Metallic (R), other channels are ignored"), $"_{materialPrefix}MetallicMap");
                if (!currentData.MetallicAssigned)
                    DrawProperty(() => currentData.Metallic = EditorGUI.Slider(metallicSliderRect, currentData.Metallic, 0, 1));

                // Smoothness/Roughness
                Rect smoothnessSliderRect = DrawTexture(sectionIndex, 2, new GUIContent(currentData.SmoothnessEnabled ? "Smoothness" : "Roughness"), $"_{materialPrefix}SmoothnessMap");
                DrawProperty(() => currentData.SmoothnessRoughness = EditorGUI.Slider(smoothnessSliderRect, currentData.SmoothnessRoughness, 0, 1));

                // Occlussion Map
                Rect occlussionStrengthSliderRect = DrawTexture(sectionIndex, 4, new GUIContent("Occlussion"), $"_{materialPrefix}OcclussionMap");
                if (currentData.OcclussionAssigned)
                    DrawProperty(() => currentData.OcclussionStrength = EditorGUI.Slider(occlussionStrengthSliderRect, currentData.OcclussionStrength, 0, 1));
            }

            // Emission
            if (currentData.EmissionEnabled) {
                bool prevEmissionAssigned = currentData.EmissionAssigned;

                // Change emission colour to white if texture assigned and texture is black
                EditorGUI.BeginChangeCheck();
                Rect emissionColourRect = DrawTexture(sectionIndex, 5, new GUIContent("Emission", "Emission (RGB)"), $"_{materialPrefix}EmissionMap");
                DrawProperty(() => currentData.EmissionColour = EditorGUI.ColorField(emissionColourRect, GUIContent.none, currentData.EmissionColour, true, false, true));
                if (EditorGUI.EndChangeCheck()) {
                    // Rehandle assigned textures since the function can be changed in child classes
                    HandleAssignedTextures(materialPrefix, sectionIndex);

                    // If texture just assigned and colour is black, change colour to white
                    if (currentData.EmissionAssigned && !prevEmissionAssigned) {
                        Color blackColor = new Color(0, 0, 0, currentData.EmissionColour.a);
                        if (currentData.EmissionColour == blackColor) {
                            currentData.EmissionColour = Color.white;
                        }
                    }
                }
            }

            // Alpha Clipping
            MaterialProperty surfaceTypeProp = FindProperty("_SurfaceTypeSetting");
            if (surfaceTypeProp.floatValue == 1.0f)
                DrawProperty(() => currentData.AlphaClipping = EditorGUI.Slider(GUIUtilities.GetLineRect(), "Alpha Clipping", currentData.AlphaClipping, 0, 1));

            // Scale & Offset
            DrawProperty(() => currentData.TilingOffset = GUIUtilities.DrawTilingOffset(currentData.TilingOffset));
        }

        /// <summary>
        /// Draws the noise properties in a material section
        /// </summary>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        protected virtual void DrawMaterialNoiseGUI(string materialPrefix)
        {
            RepetitionlessMaterialData currentData = GetMaterialData(materialPrefix);

            // Angle Offset
            DrawProperty(() => currentData.NoiseAngleOffset = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Angle Offset", currentData.NoiseAngleOffset));

            // Scale Randomising
            if (currentData.RandomiseNoiseScaling) {
                // Scale
                DrawProperty(() => currentData.NoiseScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", currentData.NoiseScale));

                // Scaling Min Max
                EditorGUI.BeginChangeCheck();
                DrawProperty(() => currentData.NoiseScalingMinMax = GUIUtilities.DrawVector2Field(currentData.NoiseScalingMinMax, new GUIContent("Noise Scaling Min Max", "(x: Min Scale, y: Max Scale)\n\nRange that each voronoi cell is randomly scaled by")));
                if (EditorGUI.EndChangeCheck()) {
                    Debug.Log("OUTSIDE END CHANGE CHECK");
                    if (currentData.NoiseScalingMinMax.x < 0) currentData.NoiseScalingMinMax.x = 0;
                    if (currentData.NoiseScalingMinMax.y < 0) currentData.NoiseScalingMinMax.y = 0;
                }
            }

            // Rotation Randomising
            if (currentData.RandomiseNoiseRotation)
                DrawProperty(() => currentData.NoiseRandomiseRotationMinMax = GUIUtilities.DrawVector2Field(currentData.NoiseRandomiseRotationMinMax, new GUIContent("Random Rotation Min Max", "(x: Min Rotation Degrees, y: Max Rotation Degrees)\n\nRange that each voronoi cell is randomly rotated by")));
        }


        /// <summary>
        /// Draws the variation properties in a material section
        /// </summary>
        /// <param name="materialPrefix">
        /// The material property prefix for the material section
        /// </param>
        /// <param name="sectionIndex">
        /// The material section index
        /// </param>
        protected virtual void DrawMaterialVariationProperties(string materialPrefix, int sectionIndex)
        {
            RepetitionlessMaterialData currentData = GetMaterialData(sectionIndex);

            // Variation Mode
            ETextureType prevVariationMode = currentData.VariationMode;
            EditorGUI.BeginChangeCheck();
            DrawProperty(() => currentData.VariationMode = (ETextureType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), new GUIContent("Variation Mode", "Using a custom texture can cause visible tiling"), currentData.VariationMode));
            if (EditorGUI.EndChangeCheck() && currentData.VariationMode != prevVariationMode)
                UpdateVariationTexture(sectionIndex, prevVariationMode);

            // Opacity
            DrawProperty(() => currentData.VariationOpacity = EditorGUI.Slider(GUIUtilities.GetLineRect(), new GUIContent("Opacity", "Transparency of the variation"), currentData.VariationOpacity, 0, 1));

            // Brightness
            DrawProperty(() => currentData.VariationBrightness = EditorGUI.Slider(GUIUtilities.GetLineRect(), new GUIContent("Brightness", "Intensity of the variation"), currentData.VariationBrightness, 0, 1));

            // Scaling
            DrawProperty(() => currentData.VariationSmallScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Small Scale", "Scale of the small variation sample"), currentData.VariationSmallScale));
            DrawProperty(() => currentData.VariationMediumScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Medium Scale", "Scale of the medium variation sample"), currentData.VariationMediumScale));
            DrawProperty(() => currentData.VariationLargeScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Large Scale", "Scale of the large variation sample"), currentData.VariationLargeScale));

            if (currentData.VariationMode != ETextureType.CustomTexture) { // Noise
                // Strength
                DrawProperty(() => currentData.VariationNoiseStrength = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Strength", currentData.VariationNoiseStrength));

                // Scale
                DrawProperty(() => currentData.VariationNoiseScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", currentData.VariationNoiseScale));

                // Offset
                DrawProperty(() => currentData.VariationNoiseOffset = GUIUtilities.DrawVector2Field(currentData.VariationNoiseOffset, new GUIContent("Noise Offset")));
            } else {
                // Texture
                DrawTexture(sectionIndex, 6, new GUIContent("Variation Texture", "Variation (R), other channels are ignored\n\nTexture that is drawn onto other materials, can cause visible tiling"), $"_{materialPrefix}VariationTexture");
                
                // Tiling & Offset
                DrawProperty(() => currentData.VariationTextureTO = GUIUtilities.DrawTilingOffset(currentData.VariationTextureTO, "Variation Scale", "Variation Offset"));
            }
        }
        #endregion

        #region Materials GUI
        /// <summary>
        /// Draws the base material GUI
        /// </summary>
        /// <param name="propertiesPrefix">
        /// The material property prefix for the material
        /// </param>
        protected virtual void DrawBaseMaterialGUI(string propertiesPrefix = "")
        {
            GUIUtilities.BeginBackgroundVertical();

            DrawMaterialGUI($"{propertiesPrefix}Base", 0, "Base Material");

            GUIUtilities.EndBackgroundVertical();
        }

        /// <summary>
        /// Draws the distance blend GUI
        /// </summary>
        /// <param name="propertiesPrefix">
        /// The material property prefix for the material
        /// </param>
        protected virtual void DrawDistanceBlendGUI(string propertiesPrefix = "")
        {
            RepetitionlessLayerData layerData = GetLayerData();

            // Start Background
            GUIUtilities.BeginBackgroundVertical();

            // Distance Blend Enabled Toggle
            DrawProperty(() => layerData.DistanceBlendEnabled = GUIUtilities.DrawMajorToggleButton(layerData.DistanceBlendEnabled, "Distance Blending"));

            // Draw distance blending settings
            if (layerData.DistanceBlendEnabled) {
                GUILayout.Space(5);

                // Distance Blend Mode
                DrawProperty(() => layerData.DistanceBlendMode = (EDistanceBlendMode)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), new GUIContent("Blend Mode", "Tiling & Offset: Resamples materials with defined Tiling & Offset\nMaterial: Samples far material"), layerData.DistanceBlendMode));

                // Distance Blend Min Max
                EditorGUI.BeginChangeCheck();
                DrawProperty(() => layerData.DistanceBlendMinMax = GUIUtilities.DrawVector2Field(layerData.DistanceBlendMinMax, new GUIContent("Distance Blend Min Max", "(x: Min Distance, y: Max Distance)\n\nBlend distance which the material will be sampled. Materials will be blended with regular material at min, and far material at max")));
                if (EditorGUI.EndChangeCheck()) {
                    if (layerData.DistanceBlendMinMax.x < 0) layerData.DistanceBlendMinMax.x = 0;
                    if (layerData.DistanceBlendMinMax.y < 0) layerData.DistanceBlendMinMax.y = 0;
                }

                int sectionIndex = 1;
                switch (layerData.DistanceBlendMode) {
                    case EDistanceBlendMode.TilingOffset:
                        // Tiling & Offset GUI
                        RepetitionlessMaterialData farMaterialData = GetMaterialData(sectionIndex);
                        DrawProperty(() => farMaterialData.TilingOffset = GUIUtilities.DrawTilingOffset(farMaterialData.TilingOffset));
                        break;
                    case EDistanceBlendMode.Material:
                        GUILayout.Space(10);

                        // Material GUI
                        DrawMaterialGUI($"{propertiesPrefix}Far", sectionIndex, "Far Material");
                        break;
                }
            }

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }

        /// <summary>
        /// Draws the blend material GUI
        /// </summary>
        /// <param name="propertiesPrefix">
        /// The material property prefix for the material
        /// </param>
        protected virtual void DrawMaterialBlendGUI(string propertiesPrefix = "")
        {
            RepetitionlessLayerData layerData = GetLayerData();

            // Start Background
            GUIUtilities.BeginBackgroundVertical();
            // Material Blend Enabled Toggle
            DrawProperty(() => layerData.MaterialBlendEnabled = GUIUtilities.DrawMajorToggleButton(layerData.MaterialBlendEnabled, "Material Blending"));

            if (layerData.MaterialBlendEnabled) {
                // Mask
                GUIUtilities.DrawHeaderLabelLarge("Mask");

                DrawProperty(() => layerData.BlendMaskType = (ETextureType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Mask Type", layerData.BlendMaskType));

                DrawProperty(() => layerData.BlendMaskOpacity = EditorGUI.Slider(GUIUtilities.GetLineRect(), new GUIContent("Mask Opacity", "Opacity of the mask and in response the blend material"), layerData.BlendMaskOpacity, 0, 1));
                DrawProperty(() => layerData.BlendMaskStrength = EditorGUI.FloatField(GUIUtilities.GetLineRect(), new GUIContent("Mask Strength", "The higher the value, the sharper the edges and vice versa"), layerData.BlendMaskStrength));

                if (layerData.BlendMaskType != ETextureType.CustomTexture) { // Noise
                    // Scale & Offset
                    DrawProperty(() => layerData.BlendMaskNoiseScale = EditorGUI.FloatField(GUIUtilities.GetLineRect(), "Noise Scale", layerData.BlendMaskNoiseScale));
                    DrawProperty(() => layerData.BlendMaskNoiseOffset = GUIUtilities.DrawVector2Field(layerData.BlendMaskNoiseOffset, new GUIContent("Noise Offset")));
                } else { // Custom Texture
                    // Texture
                    MaterialProperty blendMaskTextureProp = FindProperty($"_{propertiesPrefix}BlendMaskTexture"); // THIS SHOULD BE CHANGED TO AN ARRAY
                    _editor.TexturePropertySingleLine(new GUIContent("Blend Mask", "Blend Mask (R), other channels are ignored\n\nTexture that is sampled as the mask for the blend material. Color from black-white represents opacity (0-1)"), blendMaskTextureProp);

                    // Scale & Offset
                    DrawProperty(() => layerData.BlendMaskTextureTO = GUIUtilities.DrawTilingOffset(layerData.BlendMaskTextureTO));
                }

                GUILayout.Space(10);

                // Distance Blending
                if (layerData.DistanceBlendEnabled) {
                    // Calculate scaled text min width
                    int minScaledTextWidth = 0;
                    minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Override Distance Blending")).x;
                    if (layerData.OverrideDistanceBlend && layerData.DistanceBlendMode == EDistanceBlendMode.TilingOffset)
                        minScaledTextWidth += (int)GUI.skin.button.CalcSize(new GUIContent("Override Tiling & Offset")).x;

                    // Header
                    GUIUtilities.DrawHeaderLabelLarge("Distance Blending");

                    GUILayout.BeginHorizontal();

                    // Override Distance Blending Toggle
                    string distanceBlendEnabledStyle = layerData.OverrideDistanceBlend && layerData.DistanceBlendMode == 0 ? "ButtonLeft" : "Button";
                    DrawProperty(() => layerData.OverrideDistanceBlend = GUILayout.Toggle(layerData.OverrideDistanceBlend, new GUIContent(GetScaledText(minScaledTextWidth, "Override Distance Blending", "ODB"), "Draws the blend material on top of the far material"), distanceBlendEnabledStyle));

                    // Override Tiling & Offset Options
                    bool endedHorizontal = false;
                    if (layerData.OverrideDistanceBlend && layerData.DistanceBlendMode == EDistanceBlendMode.TilingOffset) {
                        // Override Tiling & Offset Toggle
                        DrawProperty(() => layerData.OverrideDistanceBlendTO = GUILayout.Toggle(layerData.OverrideDistanceBlendTO, new GUIContent(GetScaledText(minScaledTextWidth, "Override Tiling & Offset", "OTO"), "Uses defined Tiling & Offset rather than distance blend Tiling & Offset"), "ButtonRight"));

                        endedHorizontal = true;
                        GUILayout.EndHorizontal();

                        // Override Tiling & Offset
                        if (layerData.OverrideDistanceBlendTO)
                            DrawProperty(() => layerData.BlendMaskDistanceTO = GUIUtilities.DrawTilingOffset(layerData.BlendMaskDistanceTO));
                    }

                    if (!endedHorizontal)
                        GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                }

                // Material
                DrawMaterialGUI($"{propertiesPrefix}Blend", 2, "Blend Material");
            }

            // End Background
            GUIUtilities.EndBackgroundVertical();
        }
        #endregion

        #region Debug GUI
        /// <summary>
        /// Draws the debug selection GUI
        /// </summary>
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
        #endregion
    }
}
#endif