#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Processors
{
    using System;
    using Data;

    internal static class RepetitionlessMaterialCreator
    {
        private const string PROGRESS_BAR_TITLE = "Updating Material";
        private const string NOISE_TEXTURE_PROP_NAME = "_NoiseTexture";

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
                
                SetNoiseQuality(mat, materialProperties.NoiseQuality);

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

        private static void SetKeyword(Material mat, string keyword, bool enabled)
        {
            // Delay call to prevent recursive warnings, this will take a while if variant not cached
            EditorApplication.delayCall += () => {
                // Using a keyword variable with SetKeyword sometimes gives errors
                if (enabled) mat.EnableKeyword(keyword);
                else         mat.DisableKeyword(keyword);

                mat.SetInt(keyword, enabled ? 1 : 0); // Required to save for some reason
                EditorUtility.SetDirty(mat);
            };
        }

        private static void UpdateNoiseQualityTexture(Material mat, ENoiseQuality noiseQuality)
        {
            switch (noiseQuality) {
                case ENoiseQuality.High:
                    mat.SetTexture(NOISE_TEXTURE_PROP_NAME, null);
                    break;
                case ENoiseQuality.Medium: {
                    Texture2D texture = Resources.Load<Texture2D>(Constants.NOISE_TEXTURE_NAME_4K);
                    mat.SetTexture(NOISE_TEXTURE_PROP_NAME, texture);
                    break;
                } case ENoiseQuality.Low: {
                    Texture2D texture = Resources.Load<Texture2D>(Constants.NOISE_TEXTURE_NAME_1K);
                    mat.SetTexture(NOISE_TEXTURE_PROP_NAME, texture);
                    break;
                }
            }
        }

        private static void SetNoiseQuality(Material mat, ENoiseQuality noiseQuality)
        {
            SetKeyword(mat, Constants.NOISE_TEXTURE_KEYWORD, noiseQuality != ENoiseQuality.High);
            UpdateNoiseQualityTexture(mat, noiseQuality);
        }
    }
}
#endif