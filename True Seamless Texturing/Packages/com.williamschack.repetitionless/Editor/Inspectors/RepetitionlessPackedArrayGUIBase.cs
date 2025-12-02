#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Inspectors
{
    using GUIUtilities;
    using Data;
    using Compression;
    using TextureUtilities;
    using CustomWindows;
    using Codice.Client.Common;

    public class RepetitionlessPackedArrayGUIBase : RepetitionlessGUIBase
    {
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
        protected const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";

        // Overridable
        protected virtual int _materialCount => 3;

        // Texture drawers
        protected TextureArrayCustomChannelsGUIDrawer _avTexturesDrawer;
        protected TextureArrayCustomChannelsGUIDrawer _nsoTexturesDrawer;
        protected TextureArrayCustomChannelsGUIDrawer _emTexturesDrawer;

        // Data
        protected MaterialDataManager _dataManager;
        protected RepetitionlessTextureData _textureData;

        // Array Settings Button
        private GUIContent _settingsIconContent;
        private int _settingsIconWidth;

        protected TextureDrawerDetails GetTextureDrawerDetails(int textureIndex, bool packedTexture)
        {
            if (packedTexture && textureIndex == 1) {
                return new TextureDrawerDetails(_nsoTexturesDrawer, 3); // Packed Texture
            }

            switch (textureIndex) {
                case 0: return new TextureDrawerDetails(_avTexturesDrawer, 0);  // Albedo
                case 1: return new TextureDrawerDetails(_emTexturesDrawer, 1);  // Metallic
                case 2:                                                         // Smoothness
                case 3: return new TextureDrawerDetails(_nsoTexturesDrawer, 1); // Roughness
                case 4: return new TextureDrawerDetails(_nsoTexturesDrawer, 0); // Normal
                case 5: return new TextureDrawerDetails(_nsoTexturesDrawer, 2); // Occlussion
                case 6: return new TextureDrawerDetails(_emTexturesDrawer, 0);  // Emission
                case 7: return new TextureDrawerDetails(_avTexturesDrawer, 1);  // Variation
            }

            return new TextureDrawerDetails(null, 0);
        }

        private TexturePacker.TextureData[] GetArrayLayerTextureData(int materialIndex, int layerIndex)
        {
            RepetitionlessTextureData.MaterialTextureData materialData = _textureData.MaterialsTextureData[layerIndex];
            
            switch(materialIndex) {
                case 0: return materialData.AVTextures;
                case 1: return materialData.NSOTextures;
                case 2: return materialData.EMTextures;
            }

            return null;
        }

        protected void SaveTextureData()
        {
            EditorUtility.SetDirty(_textureData);
            AssetDatabase.SaveAssetIfDirty(_textureData);
        }

        protected override int HandleAssignedTextures(string materialPrefix, int sectionIndex, MaterialProperty settingsProp)
        {
            RepetitionlessTextureData.MaterialTextureData materialTextureData = _textureData.MaterialsTextureData[sectionIndex];

            MaterialProperty settingTogglesProp = FindProperty($"_{materialPrefix}Settings");
            int settingToggles = (int)settingTogglesProp.vectorValue.x;
            bool packedTexture = BooleanCompression.GetValue(settingToggles, 5);

            bool packedTextureAssigned = packedTexture ? materialTextureData.NSOTextures[3].Texture != null : false;

            bool metallicAssigned   = packedTextureAssigned ? true : materialTextureData.EMTextures[1].Texture != null;
            bool smoothnessAssigned = packedTextureAssigned ? true : materialTextureData.NSOTextures[1].Texture != null;
            bool roughnessAssigned  = smoothnessAssigned; // Possibly remove?
            bool normalAssigned     = materialTextureData.NSOTextures[0].Texture != null;
            bool occlussionAssigned = packedTextureAssigned ? true : materialTextureData.NSOTextures[2].Texture != null;
            bool emissionAssigned   = materialTextureData.EMTextures[0].Texture != null;
            bool albedoAssigned     = materialTextureData.AVTextures[0].Texture != null;
            bool variationAssigned  = materialTextureData.AVTextures[1].Texture != null;

            //bool metallicAssigned = metallicTexProp.textureValue != null;
            //bool smoothnessAssigned = smoothnessTexProp.textureValue != null;
            //bool roughnessAssigned = roughnessTexProp.textureValue != null;
            //bool normalAssigned = normalTexProp.textureValue != null;
            //bool occlussionAssigned = occlussionTexProp.textureValue != null;
            //bool emissionAssigned = emissionTexProp.textureValue != null;

            int compressedAssignedTextures = BooleanCompression.CompressValues(metallicAssigned, smoothnessAssigned, roughnessAssigned, normalAssigned, occlussionAssigned, emissionAssigned, albedoAssigned, variationAssigned);
            return compressedAssignedTextures;
        }

        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);

            // Initialize styles
            _settingsIconContent = EditorGUIUtility.IconContent("Settings");
            _settingsIconContent.tooltip = "Texture Array Settings";
            _settingsIconWidth = (int)GUI.skin.button.CalcSize(_settingsIconContent).x;

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

            if (_dataManager.AssetExists(TEXTURE_DATA_FILE_NAME)) {
                _textureData = _dataManager.LoadAsset<RepetitionlessTextureData>(TEXTURE_DATA_FILE_NAME);
            } else {
                _textureData = ScriptableObject.CreateInstance<RepetitionlessTextureData>();
                _dataManager.CreateAsset(_textureData, TEXTURE_DATA_FILE_NAME);
                _textureData.Init(_materialCount);
                SaveTextureData();
            }

            // Texture Drawers
            _avTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(0, i); }, SaveTextureData, RepetitionlessTextureData.DEFAULT_AV_COLOUR, albedoVTexturesProp, assignedAlbedoVTexturesProp, _materialCount);
            _nsoTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(1, i); }, SaveTextureData, RepetitionlessTextureData.DEFAULT_NSO_COLOUR, normalSOTexturesProp, assignedNormalSOTexturesProp, _materialCount);
            _emTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(2, i); }, SaveTextureData, RepetitionlessTextureData.DEFAULT_EM_COLOUR, emissionMTexturesProp, assignedEmissionMTexturesProp, _materialCount);

            _avTexturesDrawer.TextureFormat = TextureFormat.BC7;
            _nsoTexturesDrawer.TextureFormat = TextureFormat.BC7;
            _nsoTexturesDrawer.ArrayLinear = true;
            _emTexturesDrawer.TextureFormat = TextureFormat.BC7;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Base Material
            DrawBaseMaterialGUI();

            GUILayout.Space(SETTING_PADDING);

            // Distance Blend Material
            DrawDistanceBlendGUI();

            GUILayout.Space(SETTING_PADDING);

            // Material Blend
            DrawMaterialBlendGUI();

            GUILayout.Space(SETTING_PADDING);

            // Footer Settings
            DrawDebugGUI();
        }

        protected override void DrawMaterialSettingsGUI(string materialPrefix, bool showNoise = true, bool showVariation = true, bool showPT = true, bool showEmission = true, bool showSR = true, int extraWidth = 0)
        {
            // Add settings button width as extra width
            base.DrawMaterialSettingsGUI(materialPrefix, showNoise, showVariation, showPT, showEmission, showSR, _settingsIconWidth);
        }

        protected override int DrawRightMaterialSettingsGUI(int compressedValues, string materialPrefix, int settingToggles, int minScaledTextWidth, bool showPT = true, bool showEmission = true, bool showSR = true)
        {
            int compressedSettingToggles = base.DrawRightMaterialSettingsGUI(compressedValues, materialPrefix, settingToggles, minScaledTextWidth, showPT, showEmission, showSR);
        
            // Array settings button
            if (GUILayout.Button(_settingsIconContent))
            {
                bool packedTexture = BooleanCompression.GetValue(settingToggles, 5);

                // Get the texture array for this material
                int sectionIndex = materialPrefix.Contains("Far") ? 1 : 2;
                TextureDrawerDetails textureDrawerDetails = GetTextureDrawerDetails(sectionIndex, packedTexture);

                if (textureDrawerDetails.TextureDrawer.Array != null) {
                    ConfigureArrayWindowLimited.ShowWindow(textureDrawerDetails.TextureDrawer.Array, $"{materialPrefix} Array", (Texture2DArray newArray) => {
                        textureDrawerDetails.TextureDrawer.UpdateArray(newArray);
                    });
                }
                else
                    Debug.LogWarning($"{materialPrefix} has no textures assigned to modify...");
            }

            return compressedSettingToggles;
        }

        protected override Rect DrawTexture(int sectionIndex, int textureIndex, GUIContent content, string texturePropertyName)
        {
            Rect lineRect = GUIUtilities.GetLineRect();

            string materialPrefix;
            switch (sectionIndex) {
                case 0: materialPrefix = "Base";  break;
                case 1: materialPrefix = "Far";   break;
                case 2: materialPrefix = "Blend"; break;
                default: return lineRect;
            }

            MaterialProperty settingsProp = FindProperty($"_{materialPrefix}Settings");
            int settingToggles = (int)settingsProp.vectorValue.x;
            bool packedTexture = BooleanCompression.GetValue(settingToggles, 5);

            TextureDrawerDetails textureDrawerDetails = GetTextureDrawerDetails(textureIndex, packedTexture);


            EditorGUI.BeginChangeCheck();
            textureDrawerDetails.TextureDrawer.DrawTexture(lineRect, 0, textureDrawerDetails.ChannelIndex, content);

            // If packed texture was changed, manually update texture in emission array aswell
            if (EditorGUI.EndChangeCheck() && packedTexture && textureIndex == 1) {
                _emTexturesDrawer.UpdateTexture(GetArrayLayerTextureData(0, 1)[3].Texture, 0, 2);
            }

            // Return rect after texture field
            lineRect = MaterialEditor.GetRectAfterLabelWidth(lineRect);
            return lineRect;
        }
    }
}
#endif