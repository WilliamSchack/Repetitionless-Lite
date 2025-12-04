using UnityEngine;
using Repetitionless.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class RepetitionlessMaterialDataSO : ScriptableObject
{
    private const string TEXTURE_ASSET_NAME = "PropertiesTexture.asset";
    private const TextureFormat DATA_TEXTURE_FORMAT = TextureFormat.RGBAHalf;

    public RepetitionlessLayerData Data;
    private RepetitionlessLayerDataCompressed _dataCompressed;

    private int _layerCount = 1;

    MaterialDataManager _dataManager;

    public void Init(int layerCount)
    {
        _layerCount = layerCount;

        Data = new RepetitionlessLayerData();
        _dataCompressed = new RepetitionlessLayerDataCompressed();
    }

    // Must be called for each session using this SO
    public void SetDataManager(MaterialDataManager dataManager)
    {
        _dataManager = dataManager;
    }

#if UNITY_EDITOR
    private Color? GetDataColour(int compressedFieldIndex)
    {
        return RepetitionlessDataPacker.GetLayerFieldColour(_dataCompressed, compressedFieldIndex);
    }

    private Color[] GetLayerDataColour()
    {
        Color[] dataColours = new Color[RepetitionlessDataPacker.COMPRESSED_LAYER_VARIABLES_COUNT];
        for (int i = 0; i < dataColours.Length; i++) {
           dataColours[i] = GetDataColour(i).Value;
        }

        return dataColours;
    }

    public void UpdateMaterialTexture(MaterialProperty property, int layerIndex)
    {
        if (property.propertyType != UnityEngine.Rendering.ShaderPropertyType.Texture) {
            Debug.LogError("Property type must be a texture");
            return;
        }

        Texture2D texture;
        if (_dataManager.AssetExists(TEXTURE_ASSET_NAME)) {
            // Load and modify the texture
            texture = _dataManager.LoadAsset<Texture2D>(TEXTURE_ASSET_NAME);
            
            int compressedFieldChangedIndex = RepetitionlessDataPacker.UpdateCompressedLayerDataSingle(ref _dataCompressed, Data);
            Color? dataColour = GetDataColour(compressedFieldChangedIndex);
            
            // The value has not changed
            if (!dataColour.HasValue)
                return;

            texture.SetPixel(compressedFieldChangedIndex, layerIndex, dataColour.Value);
            texture.Apply();

            EditorUtility.SetDirty(texture);
            AssetDatabase.SaveAssetIfDirty(texture);
        } else {
            // Create a new texture
            texture = new Texture2D(RepetitionlessDataPacker.COMPRESSED_LAYER_VARIABLES_COUNT, _layerCount, DATA_TEXTURE_FORMAT, false);

            Color[] dataColours = GetLayerDataColour();
            
            texture.SetPixels(dataColours);
            texture.Apply();

            _dataManager.CreateAsset(texture, TEXTURE_ASSET_NAME);
        }

        if ((Texture2D)property.textureValue != texture)
            property.textureValue = texture;
    }
#endif
}