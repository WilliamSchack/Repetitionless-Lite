using System.Linq;
using UnityEngine;

namespace SeamlessMaterial.Utilities
{
    public static class Texture2DArrayUtilities
    {
        // Unsupported texture formats by Texture2DArray
        static readonly TextureFormat[] crunchCompressedFormats = {
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

        public static Texture2DArray Create(Texture2D[] textures)
        {
            // Check if first texture used unsupported format, if it does use ARGB32
            TextureFormat format = textures[0].format;
            if (crunchCompressedFormats.Contains(format)) {
                Debug.LogWarning("Texture 1 uses unsupported format, automatically assigning Texture Array format to ARGB32");
                format = TextureFormat.ARGB32;
            }

            // Create array and assign textures to it
            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, format, textures[0].mipmapCount > 1);
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] == null) continue;

                array.SetPixels(textures[i].GetPixels(), i);
            }

            array.filterMode = textures[0].filterMode;
            array.wrapMode = textures[0].wrapMode;

            array.Apply();

            return array;
        }

        public static Texture2DArray UpdateTexture(Texture2DArray array, Texture2D texture, int index)
        {
            if (texture.width != array.width) {
                Debug.LogError("Texture is not the same size as the array, cannot be assigned. Please use a texture with the same resolution as the rest");
                return array;
            }

            array.SetPixels(texture.GetPixels(), index);
            array.Apply();

            return array;
        }

        public static Texture2D[] GetTextures(Texture2DArray array)
        {
            Texture2D[] textures = new Texture2D[array.depth];

            for (int i = 0; i < array.depth; i++) {
                Texture2D texture = new Texture2D(array.width, array.height);
                texture.SetPixels(array.GetPixels(i));
                texture.Apply();

                textures[i] = texture;
            }

            return textures;
        }
    }
}