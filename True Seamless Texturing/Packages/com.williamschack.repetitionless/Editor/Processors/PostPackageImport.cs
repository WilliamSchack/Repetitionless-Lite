#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Repetitionless.Editor.Processors
{
    using Data;
    using CustomWindows;
    using Config;

    [InitializeOnLoad]
    public static class PostPackageImport
    {
        static PostPackageImport()
        {
            if (RepetitionlessPrefs.Data.LiteMode) {
                ProUpgrade();
            }

            if (NewVersionImported()) {
                HandleVersionUpdate();
                
                RepetitionlessPrefs.UpdatePrefs((p) => {
                    p.LastProcessedVersion = RepetitionlessPackageInfo.Info.version;
                });
            }

            if (RepetitionlessPrefs.Data.WelcomeWindowShown)
                return;

            // Open the window after importing
            AssetDatabase.importPackageCompleted += PackageImported;
        }

        private static void ProUpgrade()
        {
            WelcomeWindow.Open();
            ShowReviewLog();

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.LiteMode = false;
            });
        }

        private static int[] SplitVersion(string version)
        {
            string[] partStrings = version.Split(".");

            int[] numbers = new int[3];
            for (int i = 0; i < numbers.Length; i++) {
                numbers[i] = int.Parse(partStrings[i]);
            }

            return numbers;
        }

        private static bool NewVersionImported()
        {
            return RepetitionlessPrefs.Data.LastProcessedVersion != RepetitionlessPackageInfo.Info.version;
        }

        private static int[] GetLastVersion()
        {
            return SplitVersion(RepetitionlessPrefs.Data.LastProcessedVersion);
        }


        private static void HandleVersionUpdate()
        {
            int[] splitLastVersion = GetLastVersion();
            if (splitLastVersion[0] == 0) return;

            if (splitLastVersion[0] == 1 && splitLastVersion[1] == 0 && splitLastVersion[2] <= 3) {
                ShowReviewLog();
            }

            if (splitLastVersion[0] == 1 && splitLastVersion[1] <= 2) {
                AssetDatabase.importPackageCompleted += ConvertTerrainMaterials;
                //ConvertTerrainMaterials();
            }
        }

        private static void PackageImported(string packageName)
        {
            WelcomeWindow.Open(true);
            ShowReviewLog();

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.WelcomeWindowShown = true;
                p.LastProcessedVersion = RepetitionlessPackageInfo.Info.version;
            });

            AssetDatabase.importPackageCompleted -= PackageImported;
        }

        private static void ShowReviewLog()
        {
            Debug.Log("<b>Thanks for purchasing Repetitionless! <color=#3FFFFF>Please consider leaving a review to support the asset and its development, any feedback is appreciated!</color></b>");
        }

        private static void ConvertTerrainMaterials(string packageName = "")
        {
            AssetDatabase.importPackageCompleted -= ConvertTerrainMaterials;

            EditorUtility.DisplayDialog("Repetitionless Update", "RepetitionlessTerrain materials have been changed to RepetitionlessLayered materials. Any existing terrain materials will be converted.", "Ok");

            // Convert terrain materials to layered materials
            List<Material> repetitionlessMaterials = RepetitionlessMaterialFinder.GetAll();
            
            bool convertedTerrains = false;
            foreach (Material mat in repetitionlessMaterials) {
                string shaderName = mat.shader.name;
                if (!shaderName.EndsWith("RepetitionlessTerrain"))
                    continue;

                string rp = shaderName.Split("/")[1];
                string newShaderName = $"{Constants.SHADER_FOLDER}{rp}/{Constants.SHADER_MATERIAL_NAME_LAYERED}";

                mat.shader = Shader.Find(newShaderName);

                MaterialDataManager dataManager = new MaterialDataManager(mat);
                RepetitionlessMaterialDataSO materialProperties = dataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
                materialProperties.CallOnExternalDataChanged();

                EditorUtility.SetDirty(mat);
                AssetDatabase.SaveAssetIfDirty(mat);

                convertedTerrains = true;
            }

            // Delete the old shaders
            string projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            string shaderGraphsFolder = $"{projectFolder}/Packages/com.williamschack.repetitionless/Shaders/ShaderGraphs";
            string shaderFileName = "RepetitionlessTerrain.shadergraph";
            string[] pipelineFolderNames = {
                "BIRP",
                "URP",
                "HDRP"
            };

            foreach (string pipelineFolderName in pipelineFolderNames) {
                string filePath = $"{shaderGraphsFolder}/{pipelineFolderName}/{shaderFileName}";
                string metaPath = $"{filePath}.meta";
                if (File.Exists(filePath)) File.Delete(filePath);
                if (File.Exists(metaPath)) File.Delete(metaPath);
            }

            AssetDatabase.Refresh();

            if (convertedTerrains)
                EditorUtility.DisplayDialog("Repetitionless Update", "Some terrains using Repetitionless may have pink materials.\nTo fix this, click the Save Textures button on the RepetitionlessTerrain component and make sure those materials are set to the RepetitionlessLayered shader", "Ok");
        }
    }
}
#endif