using UnityEngine;

namespace SeamlessMaterial.Utilities
{
    public static class Texture2DArrayUtilities
    {
        public static Texture2DArray CreateArray(Texture2D[] textures, TextureFormat format = TextureFormat.ARGB32, bool mipChain = false)
        {
            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, format, mipChain);
            for (int i = 0; i < textures.Length; i++)
                array.SetPixels(textures[i].GetPixels(), i);

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