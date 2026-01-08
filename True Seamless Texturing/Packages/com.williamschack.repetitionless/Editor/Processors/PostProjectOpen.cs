#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.Processors
{
    using Config;

    [InitializeOnLoad]
    public static class PostProjectOpen
    {
        static PostProjectOpen()
        {
            RepetitionlessPrefs.UpdatePrefs((p) => {
                UpdateDaysUsed(p);
            });

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
            int selected = EditorUtility.DisplayDialogComplex(
                "Review Repetitionless",
                "Thank for using Repetitionless! Pease consider leaving a review to support the asset and its development. Any feedback is appreciated!",
                "Itch.io",
                "Asset Store",
                "Cancel"
            );

            string url = "";
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

            Application.OpenURL(url);
        }
    }
}
#endif