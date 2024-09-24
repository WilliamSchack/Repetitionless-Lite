using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    Texture2D tex1 = null;
    Texture2D tex2 = null;
    Texture2D tex3 = null;
    Texture2D tex4 = null;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        EditorGUI.BeginChangeCheck();
        tex1 = (Texture2D)EditorGUILayout.ObjectField("Image", tex1, typeof(Texture2D), false);
        tex2 = (Texture2D)EditorGUILayout.ObjectField("Image", tex2, typeof(Texture2D), false);
        tex3 = (Texture2D)EditorGUILayout.ObjectField("Image", tex3, typeof(Texture2D), false);
        tex4 = (Texture2D)EditorGUILayout.ObjectField("Image", tex4, typeof(Texture2D), false);

        if (EditorGUI.EndChangeCheck()) {
            Texture2D[] textures = {
                tex1,
                tex2,
                tex3,
                tex4
            };

            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] == null) return;

                // Check if readable, if not set Read/Write to true
                string texturePath = AssetDatabase.GetAssetPath(textures[i]);
                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                if (!ti.isReadable) {
                    Debug.LogWarning($"Texture {i} is not readable, setting Read/Write to true...");
                    ti.isReadable = true;
                    ti.SaveAndReimport();
                }
            }

            // Check if first texture used unsupported format, if it does use ARGB32
            TextureFormat format = textures[0].format;
            if (crunchCompressedFormats.Contains(format)) {
                Debug.LogWarning("Texture 1 uses unsupported format. Automatically assigning Texture Array format to ARGB32.\nPlease assign a format in the texture import settings.");
                format = TextureFormat.ARGB32;
            }

            bool mipMapEnabled = textures[0].mipmapCount > 1; // If texture 0 has mipmap enabled

            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, format, mipMapEnabled);
            for (int i = 0; i < textures.Length; i++)
                array.SetPixels(textures[i].GetPixels(), i);

            array.filterMode = textures[0].filterMode;
            array.wrapMode = textures[0].wrapMode;

            array.Apply();

            string folderPath = AssetDatabase.GetAssetPath(materialEditor.target);
            folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));

            AssetDatabase.CreateFolder(folderPath, "SeamlessMaterialData");
            AssetDatabase.CreateAsset(array, $"{folderPath}/SeamlessMaterialData/TextureArray.asset");

            // Set Property
            FindProperty("_Array", properties).textureValue = array;
        }
    }
}
#endif