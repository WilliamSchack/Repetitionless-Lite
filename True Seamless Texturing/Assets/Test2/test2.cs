using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SeamlessMaterial.Utilities;
using System.Linq;

public class test2 : MonoBehaviour
{
    public Texture2D[] textures = new Texture2D[8];
}

[CustomEditor(typeof(test2))] [CanEditMultipleObjects]
public class test2Editor : Editor
{
    test2 main;

    private void OnEnable()
    {
        main = (test2)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("CREATE ARRAY")) {
            Texture2D[] textures = main.textures;

            // Weird compression, relies on single digit length array, uses non-assigned values as 0, assigned values as index + 1
            // Very strange but it works for this

            int[] textureIndexes = new int[8];
            int currentArrayIndex = 1;
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] != null) {
                    textureIndexes[i] = currentArrayIndex;
                    Debug.Log($"TEXTURE ({i}) AT INDEX ({currentArrayIndex - 1})");
                    currentArrayIndex++;
                    continue;
                }

                textureIndexes[i] = 0;
            }

            textures = textures.Where(x => x != null).ToArray();

            int compressedTextureIndexes = int.Parse(string.Join("", textureIndexes));
            Debug.Log(compressedTextureIndexes);

            Texture2DArray array = Texture2DArrayUtilities.Create(textures);

            string folderPath = "Assets/Test2";

            if (!AssetDatabase.IsValidFolder($"{folderPath}/SeamlessMaterialData"))
                AssetDatabase.CreateFolder(folderPath, "SeamlessMaterialData");

            AssetDatabase.CreateAsset(array, $"{folderPath}/SeamlessMaterialData/TextureArray.asset");
        }

        base.OnInspectorGUI();
    }
}