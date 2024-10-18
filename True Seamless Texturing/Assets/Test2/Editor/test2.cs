using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using SeamlessMaterial.Editor;
using UnityEngine;
using System.Linq;
using SeamlessMaterial.Compression;

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
        // New and much better checking which index is what

        if (GUILayout.Button("TEST")) {
            Texture2D[] textures = main.textures;

            bool[] values = new bool[textures.Length];
            for (int i = 0; i < textures.Length; i++) {
                values[i] = textures[i] != null;
            }

            int compressed = BooleanCompression.CompressValues(values);

            Debug.Log(compressed);

            bool[] newValues = BooleanCompression.GetCompressedValues(compressed, textures.Length);

            string valuesExploded = string.Join("", newValues.Select(x => x ? "1" : "0"));

            Debug.Log(valuesExploded);

            string assignedIndexes = "";
            for (int i = 0; i < newValues.Length; i++) {
                bool assigned = (int)char.GetNumericValue(valuesExploded[i]) != 0;
                if (assigned)
                    assignedIndexes += i;
            }

            Debug.Log(assignedIndexes);
        }

        if (GUILayout.Button("CREATE ARRAY")) {
            Texture2D[] textures = main.textures;

            // Weird compression, relies on array length <= 9, uses non-assigned values as 9, assigned values as their index
            // Very strange but it works for this

            int[] textureIndexes = new int[8];
            int currentArrayIndex = 0;
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] != null) {
                    textureIndexes[i] = currentArrayIndex;
                    Debug.Log($"TEXTURE ({i}) AT INDEX ({currentArrayIndex})");
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