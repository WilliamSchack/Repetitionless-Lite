#if UNITY_EDITOR
using System.Reflection;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Repetitionless.Editor.Materials
{
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

        private static string GetCurrentProjectWindowPath()
        {
            MethodInfo tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
    
            object[] args = new object[] { null };
            bool found = (bool)tryGetActiveFolderPath.Invoke(null, args);
            if (found) return (string)args[0];

            // If cannot get folder resort to project root
            return "Assets";
        }

        [MenuItem("Window/Repetitionless/Create Material", secondaryPriority = 1)]
        private static void CreateMaterialToolbar()
        {
            CreateMaterial(GetCurrentProjectWindowPath());
        }

        [MenuItem("Window/Repetitionless/Create Terrain Material", secondaryPriority = 2)]
        private static void CreateTerrainMaterialToolbar()
        {
            CreateTerrainMaterial(GetCurrentProjectWindowPath());
        }

        private static string GetShaderFolder()
        {
            string shaderFolder = Constants.SHADER_FOLDER;

            RenderPipelineAsset currentPipeline = GraphicsSettings.currentRenderPipeline;
            if (currentPipeline == null) { // Built-In RP
                shaderFolder += Constants.SHADER_FOLDER_BIRP;
            } else if (currentPipeline.GetType().Name.Contains("UniversalRenderPipeline")) {
                shaderFolder += Constants.SHADER_FOLDER_URP;
            } else if (currentPipeline.GetType().Name.Contains("HDRenderPipeline")) {
                shaderFolder += Constants.SHADER_FOLDER_HDRP;
            } else return "";

            return shaderFolder;
        }

        private static void PingAsset(Object asset)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static void CreateMaterial(string folderPath)
        {
            string shaderName = GetShaderFolder();
            if (shaderName == "") return;

            shaderName += Constants.SHADER_MATERIAL_NAME_REGULAR;
            Shader shader = Shader.Find(shaderName);
            if (shader == null) {
                Debug.LogError("Could not find shader: " + shaderName);
                return;
            }

            Material material = new Material(shader);

            string assetPath = folderPath + "/" + Constants.DEFAULT_MATERIAL_NAME_REGULAR;
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(material, assetPath);
            SetupMaterial(material, Constants.MAX_LAYERS_REGULAR);

            PingAsset(material);
        }

        public static void CreateTerrainMaterial(string folderPath)
        {
            string shaderName = GetShaderFolder();
            if (shaderName == "") return;

            shaderName += Constants.SHADER_MATERIAL_NAME_TERRAIN;
            Shader shader = Shader.Find(shaderName);
            if (shader == null) {
                Debug.LogError("Could not find shader: " + shaderName);
                return;
            }

            Material material = new Material(shader);

            string assetPath = folderPath + "/" + Constants.DEFAULT_MATERIAL_NAME_TERRAIN;
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(material, assetPath);
            SetupMaterial(material, Constants.MAX_LAYERS_TERRAIN, (RepetitionlessMaterialDataSO data) => { RepetitionlessTerrainMaterialUtilities.SetupProperties(material, data); });

            PingAsset(material);
        }

        public static MaterialDataObjects SetupMaterial(Material mat, int maxLayers, System.Action<RepetitionlessMaterialDataSO> onPropertiesCreatedCallback = null)
        {
            MaterialDataManager dataManager = new MaterialDataManager(mat);
            RepetitionlessTextureDataSO textureData;
            RepetitionlessMaterialDataSO materialProperties;

            bool progressBarUsed = false;
            
            try {
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
            } finally {
                // Clear in finally in case of an error
                if (progressBarUsed)
                    EditorUtility.ClearProgressBar();
            }


            return new MaterialDataObjects {
                DataManager = dataManager,
                TextureDataSO = textureData,
                MaterialDataSO = materialProperties
            };
        }
    }
}
#endif