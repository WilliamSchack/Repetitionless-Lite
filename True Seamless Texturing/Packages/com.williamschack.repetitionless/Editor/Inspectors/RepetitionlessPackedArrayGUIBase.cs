#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Inspectors
{
    using GUIUtilities;
    using Data;

    public class RepetitionlessPackedArrayGUIBase : RepetitionlessGUIBase
    {
        // Constants
        protected const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";

        // Texture drawers
        protected TextureArrayCustomChannelsGUIDrawer _avTexturesDrawer;
        protected TextureArrayCustomChannelsGUIDrawer _nsoTexturesDrawer;
        protected TextureArrayCustomChannelsGUIDrawer _emTexturesTrawer;

        // Data
        protected MaterialDataManager _dataManager;
        protected RepetitionlessTextureData _textureData;

        // Array Settings Button
        private GUIContent _settingsIconContent;
        private int _settingsIconWidth;

        protected void SaveTextureData()
        {
            EditorUtility.SetDirty(_textureData);
            AssetDatabase.SaveAssetIfDirty(_textureData);
        }

        protected override int HandleAssignedTextures(string materialPrefix, int sectionIndex, MaterialProperty settingsProp)
        {
            

            return 0;
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
                //_textureData.Init();
                //SaveTextureData();
            }

            // Texture Drawers
            //_avTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.AVTextures, SaveTextureData, RepetitionlessTextureData.DEFAULT_AV_COLOUR, albedoVTexturesProp, assignedAlbedoVTexturesProp, 3);
            //_nsoTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.NSOTextures, SaveTextureData, RepetitionlessTextureData.DEFAULT_NSO_COLOUR, normalSOTexturesProp, assignedNormalSOTexturesProp, 3);
            //_emTexturesTrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.EMTextures, SaveTextureData, RepetitionlessTextureData.DEFAULT_EM_COLOUR, emissionMTexturesProp, assignedEmissionMTexturesProp, 3);
//
            //_avTexturesDrawer.TextureFormat = TextureFormat.BC7;
            //_nsoTexturesDrawer.TextureFormat = TextureFormat.BC7;
            //_nsoTexturesDrawer.ArrayLinear = true;
            //_emTexturesTrawer.TextureFormat = TextureFormat.BC7;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);


        }
    }
}
#endif