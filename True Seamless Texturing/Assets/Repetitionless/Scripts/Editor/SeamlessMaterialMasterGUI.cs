using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    using Compression;
    using Variables;

    public class SeamlessMaterialMasterGUI : SeamlessMaterialGUI
    {
        //// Textures
        //private Dictionary<string, KeyValuePair<Texture2D[], bool[]>> _textures = new Dictionary<string, KeyValuePair<Texture2D[], bool[]>> {
        //    { "Base", new KeyValuePair<Texture2D[], bool[]> (new Texture2D[8], new bool[8]) },
        //    { "Far", new KeyValuePair<Texture2D[], bool[]> (new Texture2D[8], new bool[8]) },
        //    { "Blend", new KeyValuePair<Texture2D[], bool[]> (new Texture2D[9], new bool[9]) }
        //};
        //
        //#region Utilities
        //private void LoadTextureGroup(string textureGroup)
        //{
        //    KeyValuePair<Texture2D[], bool[]> texturesKeyValuePair = _textures[textureGroup];
        //
        //    // Get the array storing the textures
        //    string arrayPath = AssetDatabase.GetAssetPath(FindProperty($"_{textureGroup}Textures").textureValue);
        //    Texture2DArray array = (Texture2DArray)AssetDatabase.LoadAssetAtPath(arrayPath, typeof(Texture2DArray));
        //    if (array != null) {
        //        // Read the textures from the array
        //        Texture2D[] arrayTextures = Texture2DArrayUtilities.GetTextures(array);
        //
        //        // Get which textures are assigned
        //        int compressedAssignedTextures = (int)FindProperty($"_{textureGroup}AssignedTextures").floatValue;
        //        bool[] currentAssignedTextures = BooleanCompression.GetCompressedValues(compressedAssignedTextures, texturesKeyValuePair.Key.Length);
        //
        //        // Figure out which texture in the array goes to which texture here
        //        Texture2D[] textures = texturesKeyValuePair.Key;
        //
        //        int currentIndex = 0;
        //        for (int i = 0; i < currentAssignedTextures.Length; i++) {
        //            if (currentAssignedTextures[i]) {
        //                textures[i] = arrayTextures[currentIndex];
        //                currentIndex++;
        //            }
        //        }
        //
        //        texturesKeyValuePair = new KeyValuePair<Texture2D[], bool[]>(textures, currentAssignedTextures);
        //    }
        //
        //    _textures[textureGroup] = texturesKeyValuePair;
        //}
        //#endregion

        //public override void OnEnable(MaterialEditor materialEditor)
        //{
        //    base.OnEnable(materialEditor);
        //}

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Base Material
            DrawBaseMaterialGUI();

            GUILayout.Space(SETTING_SPACING);

            // Distance Blend Material
            DrawDistanceBlendGUI();

            GUILayout.Space(SETTING_SPACING);

            // Material Blend
            DrawMaterialBlendGUI();

            GUILayout.Space(SETTING_SPACING);

            // Footer Settings
            DrawDebugGUI();
        }

    }
}
#endif