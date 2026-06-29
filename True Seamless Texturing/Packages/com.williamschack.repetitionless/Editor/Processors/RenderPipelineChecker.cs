#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.PackageManager;
using System.IO;

namespace Repetitionless.Editor.Processors
{
    using Config;

    [UnityEditor.InitializeOnLoad]
    public static class RenderPipelineChecker
    {
        private const string URP_PACKAGE_NAME = "com.unity.render-pipelines.universal";
        private const string HDRP_PACKAGE_NAME = "com.unity.render-pipelines.high-definition";

        static RenderPipelineChecker()
        {
            Events.registeredPackages += OnPackagesRegistered;
        }

        private static void OnPackagesRegistered(PackageRegistrationEventArgs args)
        {
            foreach (PackageInfo added in args.added) {
                if (added.name == URP_PACKAGE_NAME)
                    UnhideURP();

                if (added.name == HDRP_PACKAGE_NAME)
                    UnhideHDRP();
            }

            foreach (PackageInfo removed in args.removed) {
                if (removed.name == URP_PACKAGE_NAME)
                    HideURP();

                if (removed.name == HDRP_PACKAGE_NAME)
                    HideHDRP();
            }
        }

        internal static void CheckInstalledPackages()
        {
            bool urpFound = false;
            bool hdrpFound = false;
            foreach (PackageInfo package in PackageInfo.GetAllRegisteredPackages()) {
                if (package.name == URP_PACKAGE_NAME)  urpFound = true;
                if (package.name == HDRP_PACKAGE_NAME) hdrpFound = true;
            }

            if (urpFound)  UnhideURP();
            else           HideURP();
            if (hdrpFound) UnhideHDRP();
            else           HideHDRP();
        }

        private static void UnhideFolder(string folderPath)
        {
            if (Directory.Exists(folderPath + "~"))
                Directory.Move(folderPath + "~", folderPath);

            UnityEditor.AssetDatabase.Refresh();
        }

        private static void HideFolder(string folderPath)
        {
            if (Directory.Exists(folderPath)) {
                Directory.Move(folderPath, folderPath + "~");
                File.Delete(folderPath + ".meta");
            }

            UnityEditor.AssetDatabase.Refresh();
        }

        private static string GetPathURP()
        {
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            return projectPath + "/" + Constants.PACKAGE_PATH + "/Shaders/URP";
        }

        private static string GetPathHDRP()
        {
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            return projectPath + "/" + Constants.PACKAGE_PATH + "/Shaders/HDRP";
        }

        private static void UnhideURP()
        {
            if (RepetitionlessPrefs.Data.URPActive)
                return;

            UnhideFolder(GetPathURP());

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.URPActive = true; 
            });
        }

        private static void HideURP()
        {
            if (!RepetitionlessPrefs.Data.URPActive)
                return;

            HideFolder(GetPathURP());

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.URPActive = false; 
            });
        }

        private static void UnhideHDRP()
        {
            if (RepetitionlessPrefs.Data.HDRPActive)
                return;

            UnhideFolder(GetPathHDRP());

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.HDRPActive = true; 
            });

            PostProjectOpen.CheckAndUpdateHDRPTerrainShader(true);
        }

        private static void HideHDRP()
        {
            if (!RepetitionlessPrefs.Data.HDRPActive)
                return;

            HideFolder(GetPathHDRP());

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.HDRPActive = false; 
            });
        }
    }
}
#endif
