#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Processors
{
    using CustomWindows;

    [InitializeOnLoad]
    public class PostPackageImport
    {
        private const string LIBRARY_PATH = "Library/com.williamschack.repetitionless";
        private const string WINDOW_OPENED_FILE_PATH = LIBRARY_PATH + "/.welcomeWindowShown";

        static PostPackageImport()
        {
            if (WelcomeWindowShown())
                return;

            EditorApplication.delayCall += WelcomeWindow.Open;
        }

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
    }
}
#endif