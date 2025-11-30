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
    private const int MATERIAL_COUNT = 3;

    private Texture2D testTexture1;
    private Texture2D testTexture2;
    public TexturePacker.TextureData[] TestTextureData;

    private TextureArrayCustomChannelsGUIDrawer _albedoVTexturesDrawer;
    private TextureArrayCustomChannelsGUIDrawer _normalSOTexturesDrawer;
    private TextureArrayCustomChannelsGUIDrawer _emissionMTexturesDrawer;

    bool _firstStart = true;

    bool[] _usingPT;

    MaterialDataManager _dataManager;
    RepetitionlessTextureData _textureData;

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

    private void SaveTextureData()
    {
        EditorUtility.SetDirty(_textureData);
        AssetDatabase.SaveAssetIfDirty(_textureData);
    }

    private void OnEnable(MaterialProperty[] properties)
    {
        MaterialProperty albedoVTexturesProp = FindProperty("_AVTextures", properties);
        MaterialProperty normalSOTexturesProp = FindProperty("_NSOTextures", properties);
        MaterialProperty emissionMTexturesProp = FindProperty("_EMTextures", properties);
        MaterialProperty assignedAlbedoVTexturesProp = FindProperty("_AssignedAVTextures", properties);
        MaterialProperty assignedNormalSOTexturesProp = FindProperty("_AssignedNSOTextures", properties);
        MaterialProperty assignedEmissionMTexturesProp = FindProperty("_AssignedEMTextures", properties);
        
        // Data Manager
        Material material = (Material)albedoVTexturesProp.targets[0];
        _dataManager = new MaterialDataManager(material);

        if (_dataManager.AssetExists(TEXTURE_DATA_FILE_NAME)) {
            _textureData = _dataManager.LoadAsset<RepetitionlessTextureData>(TEXTURE_DATA_FILE_NAME);
        } else {
            _textureData = ScriptableObject.CreateInstance<RepetitionlessTextureData>();
            _dataManager.CreateAsset(_textureData, TEXTURE_DATA_FILE_NAME);
            _textureData.Init(MATERIAL_COUNT);
            SaveTextureData();
        }

        _usingPT = new bool[MATERIAL_COUNT];
        for (int i = 0; i < MATERIAL_COUNT; i++) {
            _usingPT[i] = !_textureData.MaterialsTextureData[i].NSOTextures[3].Disabled;
        }

        // Array Drawers
        _albedoVTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(0, i); }, SaveTextureData, RepetitionlessTextureData.DEFAULT_AV_COLOUR, albedoVTexturesProp, assignedAlbedoVTexturesProp, MATERIAL_COUNT);
        _normalSOTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(1, i); }, SaveTextureData, RepetitionlessTextureData.DEFAULT_NSO_COLOUR, normalSOTexturesProp, assignedNormalSOTexturesProp, MATERIAL_COUNT);
        _emissionMTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return GetArrayLayerTextureData(2, i); }, SaveTextureData, RepetitionlessTextureData.DEFAULT_EM_COLOUR, emissionMTexturesProp, assignedEmissionMTexturesProp, MATERIAL_COUNT);

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
            Texture2D testTextureOut = TexturePacker.PackTextures(TestTextureData, RepetitionlessTextureData.DEFAULT_AV_COLOUR);

            _dataManager.CreateAsset(testTextureOut, "Testing.asset");
        }

        
        for (int i = 0; i < MATERIAL_COUNT; i++) {
            GUILayout.Space(20);
            GUILayout.Label($"MATERIAL {i}");

            bool wasUsingPT = _usingPT[i];
            _usingPT[i] = GUILayout.Toggle(_usingPT[i], "Packed Texture");
            if (wasUsingPT != _usingPT[i]) {
                _textureData.SetPackedTextureEnabled(i, _usingPT[i]);
                SaveTextureData();
            }

            GUILayout.Space(10);

            GUILayout.Label("AV");
            _albedoVTexturesDrawer.DrawTexture(i, 0, new GUIContent("Albedo"));
            _albedoVTexturesDrawer.DrawTexture(i, 1, new GUIContent("Variation"));

            GUILayout.Label("NSO");
            _normalSOTexturesDrawer.DrawTexture(i, 0, new GUIContent("Normal"));

            if (_usingPT[i]) {
                EditorGUI.BeginChangeCheck();
                _normalSOTexturesDrawer.DrawTexture(i, 3, new GUIContent("Packed Texture"));
                if (EditorGUI.EndChangeCheck()) {
                    // Manually update texture in emission array
                    _emissionMTexturesDrawer.UpdateTexture(_textureData.MaterialsTextureData[0].NSOTextures[3].Texture, 0, 2);
                }

                GUILayout.Label("EM");
                _emissionMTexturesDrawer.DrawTexture(i, 0, new GUIContent("Emission"));
            } else {
                _normalSOTexturesDrawer.DrawTexture(i, 1, new GUIContent("Smoothness"));
                _normalSOTexturesDrawer.DrawTexture(i, 2, new GUIContent("Occlussion"));

                GUILayout.Label("EM");
                _emissionMTexturesDrawer.DrawTexture(i, 0, new GUIContent("Emission"));
                _emissionMTexturesDrawer.DrawTexture(i, 1, new GUIContent("Metallic"));
            }
        }
    }
}
#endif