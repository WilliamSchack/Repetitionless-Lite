#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Processors
{
    using System;
    using Data;

    internal static class RepetitionlessMaterialCreator
    {
        private const string PROGRESS_BAR_TITLE = "Updating Material";

        public struct MaterialDataObjects
        {
            public MaterialDataManager DataManager;
            public RepetitionlessTextureDataSO TextureDataSO;
            public RepetitionlessMaterialDataSO MaterialDataSO;
        }

        public static MaterialDataObjects SetupMaterial(Material mat, int maxLayers, Action<RepetitionlessMaterialDataSO> onPropertiesCreatedCallback = null)
        {
            MaterialDataManager dataManager = new MaterialDataManager(mat);
            RepetitionlessTextureDataSO textureData;
            RepetitionlessMaterialDataSO materialProperties;

            bool progressBarUsed = false;
            
            if (dataManager.AssetExists(Constants.TEXTURE_DATA_FILE_NAME)) {
                textureData = dataManager.LoadAsset<RepetitionlessTextureDataSO>(Constants.TEXTURE_DATA_FILE_NAME);
            } else {
                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Creating Texture Data", 0.0f);
                progressBarUsed = true;

                textureData = ScriptableObject.CreateInstance<RepetitionlessTextureDataSO>();
                dataManager.CreateAsset(textureData, Constants.TEXTURE_DATA_FILE_NAME);
                textureData.Init(maxLayers);

                textureData.Save();
                AssetDatabase.SaveAssetIfDirty(textureData);
            }

            if (dataManager.AssetExists(Constants.PROPERTIES_FILE_NAME)) {
                materialProperties = dataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
            } else {
                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Creating Properties", 0.3f);
                progressBarUsed = true;

                materialProperties = ScriptableObject.CreateInstance<RepetitionlessMaterialDataSO>();
                dataManager.CreateAsset(materialProperties, Constants.PROPERTIES_FILE_NAME);
                materialProperties.Init(maxLayers);
                
                RepetitionlessMaterialUtilities.SetNoiseQuality(mat, materialProperties.NoiseQuality);

                materialProperties.Save();
                AssetDatabase.SaveAssetIfDirty(materialProperties);

                EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Writing Properties", 0.8f);
                materialProperties.UpdateMaterialTexture(mat, 0);

                if (onPropertiesCreatedCallback != null)
                    onPropertiesCreatedCallback(materialProperties);
            }

            if (progressBarUsed)
                EditorUtility.ClearProgressBar();

            return new MaterialDataObjects {
                DataManager = dataManager,
                TextureDataSO = textureData,
                MaterialDataSO = materialProperties
            };
        }
    }
}
#endif