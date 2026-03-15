#if UNITY_EDITOR
using System.Reflection;

using UnityEditor;
using UnityEngine;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Materials
{
    using Data;

    internal static class RepetitionlessMaterialCreator
    {
        public const string DEFAULT_MATERIAL_NAME_REGULAR = "RepetitionlessMaterial.mat";
        public const string DEFAULT_MATERIAL_NAME_TERRAIN = "RepetitionlessTerrainMaterial.mat";
        private const string PROGRESS_BAR_TITLE = "Updating Material";

        public struct MaterialDataObjects
        {
            public Material Material;
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
            string path = GetCurrentProjectWindowPath();
            EditorApplication.delayCall += () => { CreateMaterial(path); };
        }

        [MenuItem("Window/Repetitionless/Create Terrain Material", secondaryPriority = 2)]
        private static void CreateTerrainMaterialToolbar()
        {
            string path = GetCurrentProjectWindowPath();
            EditorApplication.delayCall += () => { CreateTerrainMaterial(path); };
        }

        private static string GetShaderFolder(ERenderPipeline pipeline)
        {
            if (pipeline == ERenderPipeline.Unknown)
                return "";

            string shaderFolder = Constants.SHADER_FOLDER;
            switch (pipeline) {
                case ERenderPipeline.Builtin:
                    shaderFolder += Constants.SHADER_FOLDER_BIRP;
                    break;
                case ERenderPipeline.URP:
                    shaderFolder += Constants.SHADER_FOLDER_URP;
                    break;
                case ERenderPipeline.HDRP:
                    shaderFolder += Constants.SHADER_FOLDER_HDRP;
                    break;
            }

            return shaderFolder;
        }

        private static Shader GetShader(string name)
        {
            Shader shader = Shader.Find(name);
            if (shader == null) {
                Debug.LogError("Could not find shader: " + name);
                return null;
            }

            return shader;
        }

        private static void PingAsset(Object asset)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static MaterialDataObjects CreateMaterial(ERenderPipeline pipeline, string folderPath, string fileName = DEFAULT_MATERIAL_NAME_REGULAR, bool ping = true)
        {
            MaterialDataObjects materialDataObjects = new MaterialDataObjects();

            string shaderName = GetShaderFolder(pipeline);
            if (shaderName == "") return materialDataObjects;

            shaderName += Constants.SHADER_MATERIAL_NAME_REGULAR;
            Shader shader = GetShader(shaderName);
            if (shader == null) return materialDataObjects;

            Material material = new Material(shader);

            string assetPath = folderPath + "/" + fileName;
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(material, assetPath);
            materialDataObjects = SetupMaterial(material, Constants.MAX_LAYERS_REGULAR);

            if (ping)
                PingAsset(material);

            return materialDataObjects;
        }

        public static MaterialDataObjects CreateMaterial(string folderPath, string fileName = DEFAULT_MATERIAL_NAME_REGULAR, bool ping = true)
        {
            ERenderPipeline currentPipeline = RepetitionlessMaterialUtilities.GetActiveRenderPipeline();
            return CreateMaterial(currentPipeline, folderPath, fileName, ping);
        }

        public static MaterialDataObjects CreateTerrainMaterial(ERenderPipeline pipeline, string folderPath, string fileName = DEFAULT_MATERIAL_NAME_TERRAIN, bool ping = true)
        {
            MaterialDataObjects materialDataObjects = new MaterialDataObjects();

            string shaderName = GetShaderFolder(pipeline);
            if (shaderName == "") return materialDataObjects;

            shaderName += Constants.SHADER_MATERIAL_NAME_TERRAIN;
            Shader shader = GetShader(shaderName);
            if (shader == null) return materialDataObjects;

            Material material = new Material(shader);

            string assetPath = folderPath + "/" + fileName;
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(material, assetPath);
            materialDataObjects = SetupMaterial(material, Constants.MAX_LAYERS_TERRAIN, (RepetitionlessMaterialDataSO data) => { RepetitionlessTerrainMaterialUtilities.SetupProperties(material, data); });

            if (ping)
                PingAsset(material);

            return materialDataObjects;
        }

        public static MaterialDataObjects CreateTerrainMaterial(string folderPath, string fileName = DEFAULT_MATERIAL_NAME_TERRAIN, bool ping = true)
        {
            ERenderPipeline currentPipeline = RepetitionlessMaterialUtilities.GetActiveRenderPipeline();
            return CreateTerrainMaterial(currentPipeline, folderPath, fileName, ping);
        }

        public static MaterialDataObjects SetupMaterial(Material mat, int maxLayers, System.Action<RepetitionlessMaterialDataSO> onPropertiesCreatedCallback = null)
        {
            MaterialDataManager dataManager = new MaterialDataManager(mat);
            RepetitionlessTextureDataSO textureData = null;
            RepetitionlessMaterialDataSO materialProperties = null;

            try {
                if (dataManager.AssetExists(Constants.TEXTURE_DATA_FILE_NAME)) {
                    textureData = dataManager.LoadAsset<RepetitionlessTextureDataSO>(Constants.TEXTURE_DATA_FILE_NAME);
                } else {
                    EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Creating Texture Data", 0.2f);

                    AssetDatabase.StartAssetEditing();
                    try {
                        textureData = ScriptableObject.CreateInstance<RepetitionlessTextureDataSO>();
                        dataManager.CreateAsset(textureData, Constants.TEXTURE_DATA_FILE_NAME);
                        textureData.Init(maxLayers);

                        textureData.Save();
                        AssetDatabase.SaveAssetIfDirty(textureData);
                    } finally {
                        AssetDatabase.StopAssetEditing();
                    }
                }

                if (dataManager.AssetExists(Constants.PROPERTIES_FILE_NAME)) {
                    materialProperties = dataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
                } else {
                    EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Creating Properties", 0.5f);

                    AssetDatabase.StartAssetEditing();
                    try {
                        materialProperties = ScriptableObject.CreateInstance<RepetitionlessMaterialDataSO>();
                        dataManager.CreateAsset(materialProperties, Constants.PROPERTIES_FILE_NAME);
                        materialProperties.Init(maxLayers);
                        
                        RepetitionlessMaterialUtilities.SetNoiseQuality(mat, materialProperties.NoiseQuality);

                        materialProperties.Save();
                        AssetDatabase.SaveAssetIfDirty(materialProperties);
                    } finally {
                        AssetDatabase.StopAssetEditing();
                    }

                    EditorUtility.DisplayProgressBar(PROGRESS_BAR_TITLE, "Writing Properties", 0.8f);
                    materialProperties.UpdateMaterialTexture(mat, 0);

                    if (onPropertiesCreatedCallback != null)
                        onPropertiesCreatedCallback(materialProperties);
                }
            } finally {
                // Clear progress in case of error to prevent infinite progress bar
                EditorUtility.ClearProgressBar();
            }


            return new MaterialDataObjects {
                Material = mat,
                DataManager = dataManager,
                TextureDataSO = textureData,
                MaterialDataSO = materialProperties
            };
        }
    }
}
#endif