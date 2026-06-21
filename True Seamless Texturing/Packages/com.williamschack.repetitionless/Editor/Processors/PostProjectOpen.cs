#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Processors
{
    using CustomWindows;
    using Config;
    using Updating;

    [InitializeOnLoad]
    public static class PostProjectOpen
    {
        static PostProjectOpen()
        {
            // Wait a frame for the editor id to initialize
            EditorApplication.delayCall += OnInitializeOnLoad;
        }

        private static void OnInitializeOnLoad()
        {
            if (!IsEditorStartup())
                return;

            // Setup colour space checker
            RepetitionlessColourSpaceUpdater.Initialize();

            // Update hdrp terrain shader if required
            CheckAndUpdateHDRPTerrainShader();

            // Open window if update available
            if (UpdateChecker.UpdateAvailable($"v{RepetitionlessPackageInfo.Info.version}") && RepetitionlessPrefs.Data.OpenWindowOnUpdate)
                WelcomeWindow.Open(showUpdateMessage: true);
        }

        // InitializeOnLoad is called every domain reload, this makes sure its only on startup
        private static bool IsEditorStartup()
        {
            long sessionId = EditorAnalyticsSessionInfo.id;
            long lastSessionId = RepetitionlessPrefs.Data.LastSessionId;

            Debug.Log(sessionId);
            Debug.Log(lastSessionId);

            if (sessionId == lastSessionId)
                return false;

            RepetitionlessPrefs.UpdatePrefs((p) => {
                p.LastSessionId = sessionId;
            });

            return true;
        }

        private static void CheckAndUpdateHDRPTerrainShader()
        {
            bool newTerrainActive = RepetitionlessPrefs.Data.HadNewHDRPSupport;
            Debug.Log(newTerrainActive);

#if UNITY_6000_3_OR_NEWER
            if (newTerrainActive) return;
            
            string oldFolderName = Constants.HDRP_TERRAN_OLD_FOLDER_PATH;
            string newFolderName = Constants.HDRP_TERRAN_NEW_FOLDER_PATH;
#else
            if (!newTerrainActive) return;

            string oldFolderName = Constants.HDRP_TERRAN_NEW_FOLDER_PATH;
            string newFolderName = Constants.HDRP_TERRAN_OLD_FOLDER_PATH;
#endif

            Debug.Log(oldFolderName);
            Debug.Log(newFolderName);

            AssetDatabase.RenameAsset(oldFolderName, oldFolderName + "~");
            AssetDatabase.RenameAsset(newFolderName + "~", newFolderName);
            AssetDatabase.Refresh();
        }
    }
}
#endif