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
    }
}
#endif