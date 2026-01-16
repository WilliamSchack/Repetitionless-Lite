#if UNITY_EDITOR
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

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.WelcomeWindowShown = true;
                p.LastProcessedVersion = RepetitionlessPackageInfo.Info.version;
            });

            AssetDatabase.importPackageCompleted -= PackageImported;
        }
    }
}
#endif