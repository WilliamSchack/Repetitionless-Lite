#if UNITY_EDITOR
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
#if UNITY_6000_3_OR_NEWER
            if (!newTerrainActive) return;
#else
            if (newTerrainActive) return;
#endif

            
        }
    }
}
#endif