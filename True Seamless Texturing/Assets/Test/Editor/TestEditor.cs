using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SeamlessMaterial.Utilities;
using SeamlessMaterial.Compression;
using SeamlessMaterial.Editor;

#if UNITY_EDITOR
using UnityEditor;

public class TestEditor : ShaderGUI
{
    Texture2D[] _textures = new Texture2D[8];
    bool[] _assignedTextures = new bool[8];

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = false; // <-------------------------------------------------------------- set to true

    public void OnEnable(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
        folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
        Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(ArrayPath(properties), typeof(Texture2DArray));
        if (array != null) {
            Texture2D[] textures = Texture2DArrayUtilities.GetTextures(array);

            int compressedAssignedTextures = (int)FindProperty("_TexturesAssigned", properties).floatValue;
            Debug.Log(compressedAssignedTextures);

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

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        //Debug.LogError("FIGURE OUT NORMALS");

        base.OnGUI(materialEditor, properties);

        GUILayout.Space(30);

        // OnEnable if first call
        if (_firstSetup) {
            OnEnable(materialEditor, properties);
            _firstSetup = false;
        }

        materialEditor.TexturePropertySingleLine(new GUIContent("LABEL"), FindProperty("_1", properties), FindProperty("_TestSlider", properties));
        FindProperty("_TestSlider", properties).floatValue = GUIUtilities.DrawTextureWithSlider(materialEditor, FindProperty("_1", properties), true, FindProperty("_TestSlider", properties).floatValue, new GUIContent("LABEL 2"));
        FindProperty("_TestSlider", properties).floatValue = EditorGUILayout.Slider(FindProperty("_TestSlider", properties).floatValue, 0, 1);

        if (GUILayout.Button(new GUIContent("Clear Texture2DArray"))) {
            string arrayPath = ArrayPath(properties);

            if (System.IO.File.Exists(arrayPath)) {
                AssetDatabase.DeleteAsset(arrayPath);
                for(int i = 0;i < _textures.Length; i++) {
                    _textures[i] = null;
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        Texture2D[] newTextures = new Texture2D[8];
        newTextures[0] = (Texture2D)EditorGUILayout.ObjectField("Texture 1", _textures[0], typeof(Texture2D), false);
        newTextures[1] = (Texture2D)EditorGUILayout.ObjectField("Texture 2", _textures[1], typeof(Texture2D), false);
        newTextures[2] = (Texture2D)EditorGUILayout.ObjectField("Texture 3", _textures[2], typeof(Texture2D), false);
        newTextures[3] = (Texture2D)EditorGUILayout.ObjectField("Texture 4", _textures[3], typeof(Texture2D), false);
        newTextures[4] = (Texture2D)EditorGUILayout.ObjectField("Texture 5", _textures[4], typeof(Texture2D), false);
        newTextures[5] = (Texture2D)EditorGUILayout.ObjectField("Texture 6", _textures[5], typeof(Texture2D), false);
        newTextures[6] = (Texture2D)EditorGUILayout.ObjectField("Texture 7", _textures[6], typeof(Texture2D), false);
        newTextures[7] = (Texture2D)EditorGUILayout.ObjectField("Texture 8", _textures[7], typeof(Texture2D), false);

        if (EditorGUI.EndChangeCheck()) {

            List<Texture2D> arrayTextures = new List<Texture2D>();

            bool[] assignedTexturesArray = new bool[_textures.Length];
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
                // Check if readable, if not set Read/Write to true
                string texturePath = AssetDatabase.GetAssetPath(arrayTextures[i]);
                if (texturePath == "") continue;

                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                if (!ti.isReadable) {
                    Debug.LogWarning($"Texture {i} is not readable, setting Read/Write to true...");
                    ti.isReadable = true;
                    ti.SaveAndReimport();
                }
            }

            Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(ArrayPath(properties), typeof(Texture2DArray));
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

                    if (textureAssigned) {
                        int newTextureIndex = assignedTextures.Substring(0, changedIndex).Count(x => x == '1');

                        Texture2DArray updatedArray = Texture2DArrayUtilities.UpdateTexture(array, arrayTextures[newTextureIndex], newTextureIndex);
                        if (updatedArray == null)
                            return;

                        array = updatedArray;
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
}
#endif