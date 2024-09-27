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
    // Unsupported texture formats by Texture2DArray
    readonly TextureFormat[] crunchCompressedFormats = {
        TextureFormat.DXT1,
        TextureFormat.DXT1Crunched,
        TextureFormat.DXT5,
        TextureFormat.DXT5Crunched,
        TextureFormat.ETC2_RGB,
        TextureFormat.ETC2_RGBA1,
        TextureFormat.ETC2_RGBA8,
        TextureFormat.ETC2_RGBA8Crunched,
        TextureFormat.ETC_RGB4,
        TextureFormat.ETC_RGB4Crunched
    };

    Texture2D[] textures = new Texture2D[4];

    // ShaderGUI doesnt have an OnEnable function, using this instead
    private bool _firstSetup = true;

    public void OnEnable(MaterialEditor materialEditor)
    {
        string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
        folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));
        Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath($"{folderPath}/SeamlessMaterialData/TextureArray.asset", typeof(Texture2DArray));
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

            bool mipMapEnabled = newTextures[0].mipmapCount > 1;

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
                // Check if first texture used unsupported format, if it does use ARGB32
                TextureFormat format = newTextures[0].format;
                if (crunchCompressedFormats.Contains(format)) {
                    Debug.LogWarning("Texture 1 uses unsupported format, automatically assigning Texture Array format to ARGB32");
                    format = TextureFormat.ARGB32;
                }

                array = Texture2DArrayUtilities.CreateArray(newTextures, format, mipMapEnabled);

                if(!AssetDatabase.IsValidFolder(folderPath))
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