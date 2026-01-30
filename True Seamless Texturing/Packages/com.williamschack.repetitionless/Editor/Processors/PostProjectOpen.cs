#if UNITY_EDITOR
using System;
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
            if (!IsEditorStartup())
                return;

            // Setup colour space checker
            RepetitionlessColourSpaceUpdater.Initialize();

            // Open window if update available
            if (UpdateChecker.UpdateAvailable($"v{RepetitionlessPackageInfo.Info.version}") && RepetitionlessPrefs.Data.OpenWindowOnUpdate)
                WelcomeWindow.Open(showUpdateMessage: true);

            // Update prefs days used
            RepetitionlessPrefs.UpdatePrefs((p) => {
                UpdateDaysUsed(p);
            });

            // Check for review popup
            if (RepetitionlessPrefs.Data.NumDaysActive >= Constants.DAYS_UNTIL_REVIEW_POPUP && !RepetitionlessPrefs.Data.ReviewPopupShown) {
                RepetitionlessPrefs.UpdatePrefs((p) => {
                    p.ReviewPopupShown = true;
                });
                
                // Double delay call to make sure editor is initialized
                EditorApplication.delayCall += () => {
                    EditorApplication.delayCall += ShowReviewPopup;
                };
            }
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

        private static void UpdateDaysUsed(RepetitionlessPrefs.Prefs prefs)
        {
            int currentDate = GetCurrentDate();
            if (currentDate == prefs.LastDateUsed)
                return;

            prefs.LastDateUsed = currentDate;
            prefs.NumDaysActive++;
        }

        // Returns int yyyymmdd
        private static int GetCurrentDate()
        {
            return
                DateTime.Now.Year  * 10000 + // YYYY0000
                DateTime.Now.Month * 100 +   // MM00
                DateTime.Now.Day;            // DD
        }

        private static void ShowReviewPopup()
        {
            string title = "Review Repetitionless";
            string message = "Thank for using Repetitionless! Pease consider leaving a review to support the asset and its development. Any feedback is appreciated!";

            RepetitionlessPackageInfo.EPackageSource packageSource = RepetitionlessPackageInfo.PackageSource;

            string url = "";
            if (packageSource == RepetitionlessPackageInfo.EPackageSource.Unknown) {
                int selected = EditorUtility.DisplayDialogComplex(
                    title,
                    message,
                    "Itch.io",
                    "Asset Store",
                    "Cancel"
                );

                url = "";
                switch (selected) {
                    case 0: // Itch.io
                        url = Constants.ASSET_ITCH_URL;
                        break;
                    case 1: // Asset Store
                        url = Constants.ASSET_STORE_REVIEW_URL;
                        break;
                    case 2: // Cancel
                        return;
                }
            } else {
                bool openReviewPage = EditorUtility.DisplayDialog(
                    title,
                    message,
                    "Ok",
                    "Cancel"
                );

                if (!openReviewPage)
                    return;

                url = packageSource == RepetitionlessPackageInfo.EPackageSource.AssetStore ? Constants.ASSET_STORE_REVIEW_URL : Constants.ASSET_ITCH_URL;
            }

            Application.OpenURL(url);
        }
    }
}
#endif