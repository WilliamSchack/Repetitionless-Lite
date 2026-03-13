#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Processors
{
    using CustomWindows;
    using Config;

    [InitializeOnLoad]
    public static class PostPackageImport
    {
        static PostPackageImport()
        {
            if (RepetitionlessPrefs.Data.WelcomeWindowShown)
                return;

            // Open the window after importing
            AssetDatabase.importPackageCompleted += PackageImported;
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
            Debug.Log("<b>Thanks for purchasing Repetitionless!\n<color=#3FFFFF>Please consider leaving a review to support the asset and its development, any feedback is appreciated!</color></b>");
        }
    }
}
#endif