#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Processors
{
    using CustomWindows;
    using Config;

    [InitializeOnLoad]
    public class PostPackageImport
    {
        static PostPackageImport()
        {
            if (RepetitionlessPrefs.Data.WelcomeWindowShown)
                return;

            // Open the window after importing
            AssetDatabase.importPackageCompleted += PackageImported;
        }

    /*
        private static bool WelcomeWindowShown()
        {
            // Use an empty file to detect if the window has been shown or not
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));

            string libraryPathFull = $"{projectPath}{LIBRARY_PATH}";
            string windowOpenedFilePathFul = $"{projectPath}{WINDOW_OPENED_FILE_PATH}";

            if (!Directory.Exists(libraryPathFull))
                Directory.CreateDirectory(libraryPathFull);

            if (File.Exists(windowOpenedFilePathFul))
                return true;

            File.Create(windowOpenedFilePathFul);
            return false;
        }
        */

        private static void PackageImported(string packageName)
        {
            WelcomeWindow.Open(true);

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.WelcomeWindowShown = true;
                p.LastCheckedVersion = RepetitionlessPackageInfo.Info.version;
            });

            AssetDatabase.importPackageCompleted -= PackageImported;
        }
    }
}
#endif