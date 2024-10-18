using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SeamlessMaterial.Compression;
using SeamlessMaterial.Editor;

#if UNITY_EDITOR
using UnityEditor;

public class TestEditor : ShaderGUI
{
    Texture2D[] _textures = new Texture2D[8];
    bool[] _assignedTextures = new bool[8];

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;

    // Texture setting not working OnEnable
    public void OnEnable(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
        folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
        Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(ArrayPath(properties), typeof(Texture2DArray));
        if (array != null) {
            Texture2D[] textures = Texture2DArrayUtilities.GetTextures(array);

            int compressedAssignedTextures = (int)FindProperty("_TexturesAssigned", properties).floatValue;
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

        materialEditor.TexturePropertySingleLine(new GUIContent("LABEL"), FindProperty("_1", properties), FindProperty("_TestSlider", properties));
        materialEditor.TexturePropertySingleLine(new GUIContent("LABEL"), FindProperty("_1", properties));
        FindProperty("_TestSlider", properties).floatValue = GUIUtilities.DrawTexturePropertyWithSlider(materialEditor, FindProperty("_1", properties), true, FindProperty("_TestSlider", properties).floatValue, new GUIContent("LABEL 2"));


        GUIUtilities.DrawTexture(_textures[0], new GUIContent("LABEL", "TOOLTIP"));
        GUIUtilities.DrawTextureWithSlider(_textures[0], FindProperty("_TestSlider", properties).floatValue, new GUIContent("LABEL", "TOOLTIP"));

        GUILayout.Space(10);
        GUIUtilities.DrawHeaderLabelLarge("Texture Array Stuff");

        if (GUILayout.Button(new GUIContent("Clear Texture2DArray"))) {
            DeleteArray(properties);
        }

        // MAKE ASSIGNING TEXTURE TO NONE WORK

        //EditorGUI.BeginChangeCheck();
        Texture2D[] newTextures = new Texture2D[8];

        newTextures[0] = GUIUtilities.DrawTexture(_textures[0], new GUIContent("Texture 1"));
        newTextures[1] = GUIUtilities.DrawTexture(_textures[1], new GUIContent("Texture 2"));
        newTextures[2] = GUIUtilities.DrawTexture(_textures[2], new GUIContent("Texture 3"));
        newTextures[3] = GUIUtilities.DrawTexture(_textures[3], new GUIContent("Texture 4"));
        newTextures[4] = GUIUtilities.DrawTexture(_textures[4], new GUIContent("Texture 5"));
        newTextures[5] = GUIUtilities.DrawTexture(_textures[5], new GUIContent("Texture 6"));
        newTextures[6] = GUIUtilities.DrawTexture(_textures[6], new GUIContent("Texture 7"));
        newTextures[7] = GUIUtilities.DrawTexture(_textures[7], new GUIContent("Texture 8"));

        if (EditorGUI.EndChangeCheck()) {

            Texture2DArray array = null;

            List<Texture2D> arrayTextures = new List<Texture2D>();

            bool[] assignedTexturesArray = new bool[_textures.Length];

            // If all textures are null, clear array
            if(!newTextures.Any(x => x != null)) {
                DeleteArray(properties);
            }

            // If any texture exists, update array
            else {
                string assignedTextures = "";

                for (int i = 0; i < newTextures.Length; i++) {
                    bool assigned = newTextures[i] != null;

                    if (assigned) {
                        arrayTextures.Add(newTextures[i]);
                    }

                    assignedTexturesArray[i] = assigned;
                    assignedTextures += assigned ? "1" : "0";
                }

                if (arrayTextures.Count == 0)
                    return;

                for (int i = 0; i < arrayTextures.Count; i++) {
                    // Check if readable,
                    // if not set Read/Write to true
                    string texturePath = AssetDatabase.GetAssetPath(arrayTextures[i]);
                    if (texturePath == "") continue;

                    TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                    if (!ti.isReadable) {
                        Debug.LogWarning($"Texture {i} is not readable, setting Read/Write to true...");
                        ti.isReadable = true;
                        ti.SaveAndReimport();
                    }
                }

                array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(ArrayPath(properties), typeof(Texture2DArray));

                if (array != null) {
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
                            int newTextureIndex = assignedTextures.Substring(0, changedIndex).Count(x => x == '1');

                            (Texture2DArray, bool) updatedArray = await Texture2DArrayUtilities.UpdateTextureAsync(array, arrayTextures[newTextureIndex], newTextureIndex);

                            if (updatedArray.Item1 == null || updatedArray.Item2)
                                return;

                            // If array is resized to texture, update file
                            if(array != updatedArray.Item1) {
                                AssetDatabase.CreateAsset(updatedArray.Item1, ArrayPath(properties));
                            }

                            array = updatedArray.Item1;
                        } else {
                            array = Texture2DArrayUtilities.Create(arrayTextures.ToArray());
                            AssetDatabase.CreateAsset(array, ArrayPath(properties));
                        }
                    }

                } else {
                    array = Texture2DArrayUtilities.Create(arrayTextures.ToArray());

                    string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
                    folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));

                    if (!AssetDatabase.IsValidFolder($"{folderPath}/{FolderName(materialEditor)}"))
                        AssetDatabase.CreateFolder(folderPath, FolderName(materialEditor));

                    AssetDatabase.CreateAsset(array, $"{folderPath}/{FolderName(materialEditor)}/TextureArray.asset");
                }
            }



            // Set Property
            FindProperty("_Array", properties).textureValue = array;
            FindProperty("_TexturesAssigned", properties).floatValue = BooleanCompression.CompressValues(assignedTexturesArray);

            _assignedTextures = assignedTexturesArray;
        }

        _textures = newTextures;
    }

    private string ArrayPath(MaterialProperty[] properties)
    {
        return AssetDatabase.GetAssetPath(FindProperty("_Array", properties).textureValue);
    }

    private string FolderName(MaterialEditor editor)
    {
        string path = AssetDatabase.GetAssetPath(editor.target);
        int lastIndex = path.LastIndexOf("/");
        string fileName = path.Substring(lastIndex + 1, path.Length - lastIndex - 1).Split(".")[0];

        return fileName + "_TextureData";
    }

    private void DeleteArray(MaterialProperty[] properties)
    {
        string arrayPath = ArrayPath(properties);

        if (System.IO.File.Exists(arrayPath)) {
            AssetDatabase.DeleteAsset(arrayPath);
            for (int i = 0; i < _textures.Length; i++) {
                _textures[i] = null;
            }
        }
    }
}
#endif