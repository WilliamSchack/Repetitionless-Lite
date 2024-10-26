using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SeamlessMaterial.Compression;
using SeamlessMaterial.Editor;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;

public class TestEditor : ShaderGUI
{
    MaterialEditor _editor;
    MaterialProperty[] _properties;

    Texture2D[] _textures = new Texture2D[8];
    bool[] _assignedTextures = new bool[8];

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;

    Texture2D _normalPreview;

    public void OnEnable(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        _editor = materialEditor;
        _properties = properties;

        string folderPath = AssetDatabase.GetAssetPath(_editor.target);
        folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
        Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(ArrayPath(), typeof(Texture2DArray));
        if (array != null) {
            Texture2D[] textures = Texture2DArrayUtilities.GetTextures(array);

            int compressedAssignedTextures = (int)FindProperty("_TexturesAssigned", _properties).floatValue;
            bool[] assignedTextures = BooleanCompression.GetCompressedValues(compressedAssignedTextures, _textures.Length);

            int currentIndex = 0;
            for (int i = 0; i < assignedTextures.Length; i++) {
                if (assignedTextures[i]) {
                    _textures[i] = textures[currentIndex];
                    currentIndex++;
                }
            }

            _assignedTextures = assignedTextures;
        }
    }

    public override async void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        GUILayout.Space(30);

        // OnEnable if first call
        if (_firstSetup) {
            OnEnable(materialEditor, properties);
            _firstSetup = false;
        }

        GUIUtilities.DrawHeaderLabelLarge("Texture Array Stuff");

        if (GUILayout.Button(new GUIContent("Clear Texture2DArray"))) {
            _normalPreview = null;
            DeleteArray();
        }

        // MAKE ASSIGNING TEXTURE TO NONE WORK

        EditorGUI.BeginChangeCheck();
        Texture2D[] newTextures = new Texture2D[_textures.Length];

        newTextures[0] = GUIUtilities.DrawTexture(_textures[0], new GUIContent("Texture 1"));
        
        // Convert red normal map to blue for the inspector
        if (_normalPreview == null && _textures[1] != null) {
            _normalPreview = TextureUtilities.ConvertFromCompressedNormal(_textures[1]);
        }

        newTextures[1] = GUIUtilities.DrawTexture(_normalPreview, new GUIContent("Texture 2 | NORMAL"));

        if (newTextures[1] == _normalPreview) // If unchanged dont use normal preview but use regular texture
            newTextures[1] = _textures[1];
        else // If changed set the preview the new texture
            _normalPreview = TextureUtilities.ConvertFromCompressedNormal(newTextures[1]);

        //newTextures[1] = GUIUtilities.DrawTexture(_textures[1], new GUIContent("Texture 2"));


        newTextures[2] = GUIUtilities.DrawTexture(_textures[2], new GUIContent("Texture 3"));
        newTextures[3] = GUIUtilities.DrawTexture(_textures[3], new GUIContent("Texture 4"));
        newTextures[4] = GUIUtilities.DrawTexture(_textures[4], new GUIContent("Texture 5"));
        newTextures[5] = GUIUtilities.DrawTexture(_textures[5], new GUIContent("Texture 6"));
        newTextures[6] = GUIUtilities.DrawTexture(_textures[6], new GUIContent("Texture 7"));
        newTextures[7] = GUIUtilities.DrawTexture(_textures[7], new GUIContent("Texture 8"));

        if (EditorGUI.EndChangeCheck()) {

            // Create and initialise new array variables
            Texture2DArray array = null;
            List<Texture2D> arrayTextures = new List<Texture2D>();
            bool[] assignedTexturesArray = new bool[_textures.Length];

            // If all textures are null, clear array
            if(!newTextures.Any(x => x != null)) {
                _normalPreview = null;
                DeleteArray();
            }

            // If any texture exists, update array
            else {

                // Get which textures are assigned in inspector
                for (int i = 0; i < newTextures.Length; i++) {
                    bool assigned = newTextures[i] != null;

                    if (assigned) {
                        arrayTextures.Add(newTextures[i]);
                    }

                    assignedTexturesArray[i] = assigned;
                }

                if (arrayTextures.Count == 0)
                    return;

                // Make textures readable if they are not already
                TextureUtilities.SetReadable(arrayTextures.ToArray(), true);

                array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(ArrayPath(), typeof(Texture2DArray));

                if (array != null) {
                    // Get index of which texture changed
                    int changedIndex = -1;
                    for (int i = 0; i < newTextures.Length; i++) {
                        if (newTextures[i] != _textures[i]) {
                            changedIndex = i;
                            break;
                        }
                    }

                    if (changedIndex != -1) {
                        bool textureAssigned = _assignedTextures[changedIndex];

                        // Only update array if texture is already assigned and is not none, otherwise re-create it
                        if (textureAssigned && newTextures[changedIndex] != null) {
                            
                            // Get array index by counting how many textures assigned before changed index
                            int arrayIndex = 0;
                            for (int i = 0; i < changedIndex; i++) {
                                if (assignedTexturesArray[i] == true)
                                    arrayIndex++;
                            }
                            
                            // Assign texture to array
                            (Texture2DArray, bool) updatedArray = await Texture2DArrayUtilities.UpdateTextureAsync(array, arrayTextures[arrayIndex], arrayIndex);

                            // If update failed or user cancelled, return
                            if (updatedArray.Item1 == null || updatedArray.Item2) {
                                _normalPreview = null;
                                return;
                            }

                            // If array is resized to texture, update file
                            if(array != updatedArray.Item1)
                                OverwriteArray(updatedArray.Item1);

                            array = updatedArray.Item1;
                        } else {
                            // Automatically resize textures other than the changed one, prevents popups for textures that have already been decided
                            int[] autoResizeIndexes = new int[_textures.Length - 1];

                            int currentIndex = 0;
                            for (int i = 0; i < _textures.Length; i++) {
                                if (i == changedIndex) continue;
                                
                                autoResizeIndexes[currentIndex] = i;
                                currentIndex++;
                            }

                            array = await Texture2DArrayUtilities.CreateAsync(arrayTextures.ToArray(), autoResizeIndexes);
                            OverwriteArray(array);
                        }
                    }

                } else {
                    // Create array
                    array = Texture2DArrayUtilities.Create(arrayTextures.ToArray());

                    // Create new folder for array
                    string folderPath = AssetDatabase.GetAssetPath(_editor.target);
                    folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));

                    if (!AssetDatabase.IsValidFolder($"{folderPath}/{FolderName()}"))
                        AssetDatabase.CreateFolder(folderPath, FolderName());

                    // Create asset in folder
                    AssetDatabase.CreateAsset(array, $"{folderPath}/{FolderName()}/TextureArray.asset");
                }
            }

            // Set Property
            FindProperty("_Array", _properties).textureValue = array;
            FindProperty("_TexturesAssigned", _properties).floatValue = BooleanCompression.CompressValues(assignedTexturesArray);

            _assignedTextures = assignedTexturesArray;
        }

        _textures = newTextures;
    }

    private string ArrayPath()
    {
        return AssetDatabase.GetAssetPath(FindProperty("_Array", _properties).textureValue);
    }

    private void OverwriteArray(Texture2DArray array)
    {
        string path = ArrayPath();
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(array, path);
    }

    private string FolderName()
    {
        string path = AssetDatabase.GetAssetPath(_editor.target);
        int lastIndex = path.LastIndexOf("/");
        string fileName = path.Substring(lastIndex + 1, path.Length - lastIndex - 1).Split(".")[0];

        return fileName + "_TextureData";
    }

    private void DeleteArray()
    {
        string arrayPath = ArrayPath();

        // Delete array if it exists
        if (System.IO.File.Exists(arrayPath)) {
            AssetDatabase.DeleteAsset(arrayPath);
            for (int i = 0; i < _textures.Length; i++) {
                _textures[i] = null;
            }
        }

        // If data folder is empty, delete it
        string folderPath = AssetDatabase.GetAssetPath(_editor.target);
        folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
        folderPath = $"{folderPath}/{FolderName()}";

        if (AssetDatabase.IsValidFolder(folderPath)) {
            bool empty = !Directory.EnumerateFiles(folderPath).Any();
            if (empty) AssetDatabase.DeleteAsset(folderPath);
        }
    }
}
#endif