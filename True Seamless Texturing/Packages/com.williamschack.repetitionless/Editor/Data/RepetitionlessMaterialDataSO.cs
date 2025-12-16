using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Repetitionless.Data
{
    using Variables;

    [CreateAssetMenu]
    public class RepetitionlessMaterialDataSO : ScriptableObject
    {
        public const string PROPERTIES_TEXTURE_PROP_NAME = "_PropertiesTexture";

        private const string TEXTURE_ASSET_NAME = "PropertiesTexture.asset";
        private const TextureFormat DATA_TEXTURE_FORMAT = TextureFormat.RGBAHalf;

        // Dont modify data in the SO inspector, do it in the material inspector
        public RepetitionlessLayerData[] Data;
        [HideInInspector][SerializeField] private RepetitionlessLayerDataCompressed[] _dataCompressed;

        MaterialDataManager _dataManager;

        public void Init(int layerCount)
        {
            Data = new RepetitionlessLayerData[layerCount];
            for (int i = 0; i < layerCount; i++)
                Data[i] = new RepetitionlessLayerData();
            
            _dataCompressed = new RepetitionlessLayerDataCompressed[layerCount];
            for (int i = 0; i < layerCount; i++)
                _dataCompressed[i] = new RepetitionlessLayerDataCompressed();
        }

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        // Must be called for each session using this SO
        public void SetDataManager(MaterialDataManager dataManager)
        {
            _dataManager = dataManager;
        }

    #if UNITY_EDITOR
        private Color? GetDataColour(int layerIndex, int compressedFieldIndex)
        {
            return RepetitionlessDataPacker.GetLayerFieldColour(_dataCompressed[layerIndex], compressedFieldIndex);
        }

        private Color[] GetLayerDataColour(int layerIndex)
        {
            Color[] dataColours = new Color[MaterialDataConstants.COMPRESSED_LAYER_VARIABLES_COUNT];
            for (int i = 0; i < dataColours.Length; i++) {
                dataColours[i] = GetDataColour(layerIndex, i).Value;
            }

            return dataColours;
        }

        public void UpdateMaterialTexture(Material material, int layerIndex)
        {
            UpdateMaterialTexture(material, PROPERTIES_TEXTURE_PROP_NAME, layerIndex);
        }

        public void UpdateMaterialTexture(Material material, string texturePropertyName, int layerIndex)
        {
            MaterialProperty textureProp = MaterialEditor.GetMaterialProperty(new Object[] { material }, texturePropertyName);
            UpdateMaterialTexture(textureProp, layerIndex);
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
                
                for (int i = 0; i < MaterialDataConstants.COMPRESSED_LAYER_VARIABLES_COUNT; i++) {
                    int fieldChangedIndex = RepetitionlessDataPacker.UpdateCompressedLayerData(ref _dataCompressed[layerIndex], Data[layerIndex]);

                    if (fieldChangedIndex == -1)
                        break;

                    Color? dataColour = GetDataColour(layerIndex, fieldChangedIndex);
                
                    // The value has not changed
                    if (!dataColour.HasValue)
                        continue;

                    texture.SetPixel(fieldChangedIndex, layerIndex, dataColour.Value);
                    texture.Apply();
                }
            } else {
                // Create a new texture
                texture = new Texture2D(MaterialDataConstants.COMPRESSED_LAYER_VARIABLES_COUNT, Data.Length, DATA_TEXTURE_FORMAT, false);

                Color[] dataColours = new Color[MaterialDataConstants.COMPRESSED_LAYER_VARIABLES_COUNT * Data.Length];
                for (int i = 0; i < Data.Length; i++) {
                    // Compress all values
                    RepetitionlessDataPacker.UpdateCompressedLayerData(ref _dataCompressed[i], Data[i], true);
                    Color[] layerDataColours = GetLayerDataColour(layerIndex);

                    // Add colours to the main array
                    int coloursOffset = MaterialDataConstants.COMPRESSED_LAYER_VARIABLES_COUNT * i;
                    for (int x = 0; x < MaterialDataConstants.COMPRESSED_LAYER_VARIABLES_COUNT; x++) {
                        dataColours[coloursOffset + x] = layerDataColours[x];
                    }
                }

                texture.SetPixels(dataColours);
                texture.Apply();

                _dataManager.CreateAsset(texture, TEXTURE_ASSET_NAME);
            }

            if ((Texture2D)property.textureValue != texture)
                property.textureValue = texture;
        }

        public RepetitionlessMaterialData GetMaterialData(int layerIndex, int materialIndex)
        {
            RepetitionlessMaterialData currentData = Data[layerIndex].BaseMaterialData;
            switch (materialIndex) {
              //case 0: currentData = Data.BaseMaterialData;  break;
                case 1: currentData = Data[layerIndex].FarMaterialData;   break;
                case 2: currentData = Data[layerIndex].BlendMaterialData; break; 
            }

            return currentData;
        }

        public void UpdateAssignedTextures(Material material, RepetitionlessTextureDataSO textureData, int materialIndex, int layerIndex)
        {
            RepetitionlessTextureDataSO.MaterialTextureData materialTextureData = textureData.GetMaterialTextureData(layerIndex, materialIndex);

            RepetitionlessMaterialData currentData = GetMaterialData(layerIndex, materialIndex);
            bool packedTextureAssigned = currentData.PackedTexture ? materialTextureData.NSOTextures[3].Texture != null : false;

            currentData.AlbedoAssigned        = materialTextureData.AVTextures[0].Texture != null;
            currentData.MetallicAssigned      = packedTextureAssigned ? true : materialTextureData.EMTextures[1].Texture != null;
            currentData.SmoothnessAssigned    = packedTextureAssigned ? true : materialTextureData.NSOTextures[1].Texture != null;
            currentData.NormalAssigned        = materialTextureData.NSOTextures[0].Texture != null;
            currentData.OcclussionAssigned    = packedTextureAssigned ? true : materialTextureData.NSOTextures[2].Texture != null;
            currentData.EmissionAssigned      = materialTextureData.EMTextures[0].Texture != null;
            currentData.VariationAssigned     = materialTextureData.AVTextures[1].Texture != null;
            currentData.PackedTextureAssigned = packedTextureAssigned;

            Data[layerIndex].BlendMaskAssigned = textureData.LayersTextureData[layerIndex].BlendMaskTexture[0].Texture != null;

            UpdateMaterialTexture(material, PROPERTIES_TEXTURE_PROP_NAME, layerIndex);
        }
    #endif
    }
}