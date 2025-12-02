#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Repetitionless.Data
{
    public class MaterialDataManager
    {
        private string DATA_FOLDER_SUFFIX = "_Data";

        private Material _material;

        public MaterialDataManager(Material material)
        {
            _material = material;
        }

#region Path
        public string DataFolderParentPath()
        {
            string materialPath = AssetDatabase.GetAssetPath(_material);   
            if (materialPath == "") return "";

            int lastDirIndex = materialPath.LastIndexOf("/");
            return materialPath.Substring(0, lastDirIndex);
        }

        public string DataFolderName()
        {
            return _material.name + DATA_FOLDER_SUFFIX;
        }

        public string DataFolderPath()
        {
            string parentPath = DataFolderParentPath();
            if (parentPath == "") return "";

            return $"{parentPath}/{DataFolderName()}" ;
        }

        private bool DataFolderCreated()
        {
            return AssetDatabase.IsValidFolder(DataFolderPath());
        }

        private bool DataFolderEmpty()
        {
            return !Directory.EnumerateFiles(DataFolderPath()).Any();
        }

        private bool CreateDataFolder()
        {
            if (!DataFolderCreated()) {
                string folderGUID = AssetDatabase.CreateFolder(DataFolderParentPath(), DataFolderName());
                return folderGUID != "";
            }

            return true;
        }

        private bool DeleteDataFolder()
        {
            if (DataFolderCreated()) {
                string folderPath = DataFolderPath();
                bool empty = DataFolderEmpty();
                if (empty) AssetDatabase.DeleteAsset(folderPath);

                return empty;
            }

            return false;
        }
#endregion

#region Asset Management
        public void CreateAsset(Object asset, string fileName, bool overwrite = false)
        {
            bool folderExists = CreateDataFolder();
            if (!folderExists) return;

            string assetPath = $"{DataFolderPath()}/{fileName}";

            if (overwrite) AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        public T LoadAsset<T>(string fileName) where T : Object
        {
            string dataFolderPath = DataFolderPath();
            if (dataFolderPath == "") return null;

            string assetPath = $"{dataFolderPath}/{fileName}";
            return (T)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
        }

        public bool AssetExists(string fileName)
        {
            string assetPath = $"{DataFolderPath()}/{fileName}";
            string projectPath = Application.dataPath;

            // Remove "/Assets"
            int lastDirIndex = projectPath.LastIndexOf("/");
            projectPath = projectPath.Substring(0, lastDirIndex);

            string fullPath = $"{projectPath}/{assetPath}";
            return File.Exists(fullPath);
        }

        public void DeleteAsset(string fileName)
        {
            string assetPath = $"{DataFolderPath()}/{fileName}";

            if (AssetExists(fileName))
                AssetDatabase.DeleteAsset(assetPath);

            if (DataFolderEmpty())
                DeleteDataFolder();
        }
#endregion
    }
}
#endif