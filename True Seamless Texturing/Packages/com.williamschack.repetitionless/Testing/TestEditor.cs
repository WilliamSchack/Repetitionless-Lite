#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Repetitionless.GUIUtilities;
using Repetitionless.Data;
using Repetitionless.TextureUtilities;
using Codice.CM.Client.Gui;

public class TestEditor : ShaderGUI
{
    private const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";

    private Texture2D testTexture1;
    private Texture2D testTexture2;
    public TexturePacker.TextureData[] TestTextureData;

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
        _albedoVTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.AVTextures, RepetitionlessTextureData.DEFAULT_AV_COLOURS, albedoVTexturesProp, assignedAlbedoVTexturesProp, 3);
        _normalSOTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.NSOTextures, RepetitionlessTextureData.DEFAULT_NSO_COLOURS, normalSOTexturesProp, assignedNormalSOTexturesProp, 3);
        _emissionMTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, _textureData.EMTextures, RepetitionlessTextureData.DEFAULT_EM_COLOURS, emissionMTexturesProp, assignedEmissionMTexturesProp, 3);

        _albedoVTexturesDrawer.TextureFormat = TextureFormat.BC7;
        _normalSOTexturesDrawer.TextureFormat = TextureFormat.BC7;
        _normalSOTexturesDrawer.ArrayLinear = true;
        _emissionMTexturesDrawer.TextureFormat = TextureFormat.BC7;

        // Testing
        TestTextureData = new TexturePacker.TextureData[2];

        TestTextureData[0] = new TexturePacker.TextureData() {
            NormalMap = false,
            FromToChannels = new List<TexturePacker.FromToChannel>() {
                new TexturePacker.FromToChannel(
                    TexturePacker.TextureChannel.R,
                    TexturePacker.TextureChannel.R
                ),
                new TexturePacker.FromToChannel(
                    TexturePacker.TextureChannel.G,
                    TexturePacker.TextureChannel.G
                ),
                new TexturePacker.FromToChannel(
                    TexturePacker.TextureChannel.B,
                    TexturePacker.TextureChannel.B
                )
            }
        };
        
        TestTextureData[1] = new TexturePacker.TextureData() {
            NormalMap = false,
            FromToChannels = new List<TexturePacker.FromToChannel>() {
                new TexturePacker.FromToChannel(
                    TexturePacker.TextureChannel.R,
                    TexturePacker.TextureChannel.A
                )
            }
        };
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (_firstStart) {
            OnEnable(properties);
            _firstStart = false;
        }

        GUILayout.Label("PT Testing:");

        EditorGUI.BeginChangeCheck();
        testTexture1 = GUIUtilities.DrawTexture(testTexture1, new GUIContent("Testing1"));
        testTexture2 = GUIUtilities.DrawTexture(testTexture2, new GUIContent("Testing2"));
        if(EditorGUI.EndChangeCheck()) {
            TestTextureData[0].Texture = testTexture1;
            TestTextureData[1].Texture = testTexture2;
            Texture2D testTextureOut = TexturePacker.PackTextures(TestTextureData, RepetitionlessTextureData.DEFAULT_AV_COLOURS);

            _dataManager.CreateAsset(testTextureOut, "Testing.asset");
        }

        GUILayout.Space(20);
        GUILayout.Label("Array Testing:");

        _albedoVTexturesDrawer.DrawTexture(0, 0, new GUIContent("Testing1"));
        _albedoVTexturesDrawer.DrawTexture(0, 1, new GUIContent("Testing2"));
    }
}
#endif