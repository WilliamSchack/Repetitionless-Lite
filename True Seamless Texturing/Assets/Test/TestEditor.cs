using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

using SeamlessMaterial.Utilities;


#if UNITY_EDITOR
using UnityEditor;

public class TestEditor : ShaderGUI
{


    Texture2D[] textures = new Texture2D[4];

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;

    public void OnEnable(MaterialEditor materialEditor)
    {
        string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
        folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
        Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath($"{folderPath}/SeamlessMaterialData/TextureArray.asset", typeof(Texture2DArray));
        if(array != null)
            textures = Texture2DArrayUtilities.GetTextures(array);
    }


    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        // OnEnable if first call
        if (_firstSetup) {
            OnEnable(materialEditor);
            _firstSetup = false;
        }

        if (GUILayout.Button(new GUIContent("Clear Texture2DArray"))) {
            string assetsPath = Application.dataPath;
            assetsPath = assetsPath.Substring(0, assetsPath.LastIndexOf("/")); // Remove "/Assets", included in filePath
            
            string filePath = AssetDatabase.GetAssetPath(materialEditor.target);
            filePath = filePath.Substring(0, filePath.LastIndexOf("/"));
            filePath = $"{filePath}/SeamlessMaterialData/TextureArray.asset";

            if (System.IO.File.Exists($"{assetsPath}/{filePath}")) {
                AssetDatabase.DeleteAsset(filePath);
                for(int i = 0;i < textures.Length; i++) {
                    textures[i] = null;
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        Texture2D[] newTextures = new Texture2D[4];
        newTextures[0] = (Texture2D)EditorGUILayout.ObjectField("Texture 1", textures[0], typeof(Texture2D), false);
        newTextures[1] = (Texture2D)EditorGUILayout.ObjectField("Texture 2", textures[1], typeof(Texture2D), false);
        newTextures[2] = (Texture2D)EditorGUILayout.ObjectField("Texture 3", textures[2], typeof(Texture2D), false);
        newTextures[3] = (Texture2D)EditorGUILayout.ObjectField("Texture 4", textures[3], typeof(Texture2D), false);

        if (EditorGUI.EndChangeCheck()) {
            for (int i = 0; i < newTextures.Length; i++) {
                if (newTextures[i] == null) {
                    textures = newTextures;
                    return;
                }

                // Check if readable, if not set Read/Write to true
                string texturePath = AssetDatabase.GetAssetPath(newTextures[i]);
                if (texturePath == "") continue;

                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                if (!ti.isReadable) {
                    Debug.LogWarning($"Texture {i} is not readable, setting Read/Write to true...");
                    ti.isReadable = true;
                    ti.SaveAndReimport();
                }
            }

            string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
            folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
            
            Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath($"{folderPath}/SeamlessMaterialData/TextureArray.asset", typeof(Texture2DArray));
            if(array != null) {
                int changedIndex = -1;
                for (int i = 0; i < newTextures.Length; i++) {
                    if (newTextures[i] != textures[i])
                        changedIndex = i;
                }

                if(changedIndex != -1)
                    array = Texture2DArrayUtilities.UpdateTexture(array, newTextures[changedIndex], changedIndex);

            } else {
                array = Texture2DArrayUtilities.Create(newTextures);

                if(!AssetDatabase.IsValidFolder($"{folderPath}/SeamlessMaterialData"))
                    AssetDatabase.CreateFolder(folderPath, "SeamlessMaterialData");

                AssetDatabase.CreateAsset(array, $"{folderPath}/SeamlessMaterialData/TextureArray.asset");
            }

            // Set Property
            FindProperty("_Array", properties).textureValue = array;
        }

        textures = newTextures;
    }
}
#endif