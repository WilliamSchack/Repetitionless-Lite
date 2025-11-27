#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Repetitionless.GUIUtilities;
using Repetitionless.Data;
using Repetitionless.TextureUtilities;

public class TestEditor : ShaderGUI
{
    private const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";

    private TextureArrayCustomChannelsGUIDrawer _albedoVTexturesDrawer;
    private TextureArrayCustomChannelsGUIDrawer _normalSOTexturesDrawer;
    private TextureArrayCustomChannelsGUIDrawer _emissionMTexturesDrawer;

    bool _firstStart = true;

    MaterialDataManager _dataManager;
    RepetitionlessTextureData _textureData;

    private void OnEnable(MaterialProperty[] properties)
    {
        MaterialProperty albedoVTexturesProp = FindProperty("_AlbedoVTextures", properties);
        MaterialProperty normalSOTexturesProp = FindProperty("_NormalSOTextures", properties);
        MaterialProperty emissionMTexturesProp = FindProperty("_EmissionMTextures", properties);
        MaterialProperty assignedAlbedoVTexturesProp = FindProperty("_AssignedAlbedoVTextures", properties);
        MaterialProperty assignedNormalSOTexturesProp = FindProperty("_AssignedNormalSOTextures", properties);
        MaterialProperty assignedEmissionMTexturesProp = FindProperty("_AssignedEmissionMTextures", properties);
        
        // Data Manager
        Material material = (Material)albedoVTexturesProp.targets[0];
        _dataManager = new MaterialDataManager(material);

        // Texture Data
        if (_dataManager.AssetExists(TEXTURE_DATA_FILE_NAME)) {
            _textureData = _dataManager.LoadAsset<RepetitionlessTextureData>(TEXTURE_DATA_FILE_NAME);
        } else {
            _textureData = ScriptableObject.CreateInstance<RepetitionlessTextureData>();
            _dataManager.CreateAsset(_textureData, TEXTURE_DATA_FILE_NAME);
            _textureData.Init();
        }

        // Array Drawers
        _albedoVTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.AVTextures, albedoVTexturesProp, assignedAlbedoVTexturesProp, 3);
        _normalSOTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.NSOTextures, normalSOTexturesProp, assignedNormalSOTexturesProp, 3);
        _emissionMTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.EMTextures, emissionMTexturesProp, assignedEmissionMTexturesProp, 3);

        _albedoVTexturesDrawer.TextureFormat = TextureFormat.BC7;
        _normalSOTexturesDrawer.TextureFormat = TextureFormat.BC7;
        _normalSOTexturesDrawer.ArrayLinear = true;
        _emissionMTexturesDrawer.TextureFormat = TextureFormat.BC7;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (_firstStart) {
            OnEnable(properties);
            _firstStart = false;
        }

        _albedoVTexturesDrawer.DrawTextures();
    }
}
#endif