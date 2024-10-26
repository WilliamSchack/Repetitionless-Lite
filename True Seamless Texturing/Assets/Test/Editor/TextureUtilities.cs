using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    public static class TextureUtilities
    {
        /// <summary>
        /// Scales the input texture to the desired resolution
        /// </summary>
        public static Texture2D ResizeTexture(Texture2D texture, int newWidth, int newHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture.active = rt;

            Graphics.Blit(texture, rt);
            Texture2D result = new Texture2D(newWidth, newHeight);

            result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }

        /// <summary>
        /// Re-Imports the input texture as readable if necessary
        /// </summary>
        public static void SetReadable(Texture2D texture, bool logChanges = false)
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);
            if (texturePath == "") return;

            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            if (ti != null && !ti.isReadable) {
                if(logChanges) Debug.LogWarning($"{texture.name} is not readable, setting read to true...");
                
                ti.isReadable = true;
                ti.SaveAndReimport();
            }
        }

        /// <summary>
        /// Re-Imports the input textures as readable if necessary
        /// </summary>
        public static void SetReadable(Texture2D[] textures, bool logChanges = false)
        {
            for (int i = 0; i < textures.Length; i++) {
                SetReadable(textures[i], logChanges);
            }
        }

        /// <summary>
        /// Converts a unity compressed normal map back to an uncompressed one <br />
        /// From red to blue essentially
        /// </summary>
        public static Texture2D ConvertFromCompressedNormal(Texture2D texture)
        {
            if (texture == null) return null;

            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++) {
                Color pixel = pixels[i];
                pixel.b = pixel.r;
                pixel.r = pixel.a;
                pixel.a = 1.0f;
                pixels[i] = pixel;
            }

            Texture2D normalTexture = new Texture2D(texture.width, texture.height);
            normalTexture.SetPixels(pixels);
            normalTexture.Apply();

            return normalTexture;
        }
    }
}
#endif