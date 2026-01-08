using System;
using System.IO;
using UnityEngine;

namespace Repetitionless.Editor.Data
{
    public static class RepetitionlessPrefs
    {
        private const string PREFS_FILE_PATH = Constants.LIBRARY_PATH + "/prefs.json";

        public class Prefs
        {
            public bool WelcomeWindowShown = false;
            public string LastCheckedVersion = "0.0.0";
        }

        private static Prefs _prefsCache;
        public static Prefs Data => _prefsCache ??= LoadPrefs();

        private static FileInfo GetPrefsFileInfo()
        {
            FileInfo prefsFileInfo = new FileInfo(PREFS_FILE_PATH);
            if (!prefsFileInfo.Exists)
                CreatePrefs();

            return prefsFileInfo;
        }

        private static void CreatePrefs()
        {
            FileInfo prefsFileInfo = new FileInfo(PREFS_FILE_PATH);
            if (prefsFileInfo.Exists) return;

            string parentDir = prefsFileInfo.DirectoryName;
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);

            Prefs prefs = new Prefs();
            string prefsJson = JsonUtility.ToJson(prefs);

            File.WriteAllText(prefsFileInfo.FullName, prefsJson);
        }

        private static Prefs LoadPrefs()
        {
            FileInfo prefsFileInfo = GetPrefsFileInfo();

            string prefsJson = File.ReadAllText(prefsFileInfo.FullName);
            return JsonUtility.FromJson<Prefs>(prefsJson);
        }

        private static void WritePrefs(Prefs prefs)
        {
            FileInfo prefsFileInfo = GetPrefsFileInfo();
            
            string prefsJson = JsonUtility.ToJson(prefs);
            File.WriteAllText(prefsFileInfo.FullName, prefsJson);
        }

        public static void UpdatePrefs(Action<Prefs> updater)
        {
            updater(Data);
            WritePrefs(Data);
        }
    }
}