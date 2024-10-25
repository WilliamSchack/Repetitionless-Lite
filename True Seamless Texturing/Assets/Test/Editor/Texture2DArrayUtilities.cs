using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    public static class Texture2DArrayUtilities
    {
        private static Texture2DArray SetTextureAtIndex(Texture2DArray array, Texture2D texture, int index)
        {
            // Change texture format if not the same as array
            if (texture.format != array.format) {
                // Convert to RGBA32 first, supports more formats
                Texture2D temp = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, texture.mipmapCount > 1);
                Color[] pixels = texture.GetPixels();
                temp.SetPixels(pixels);
                temp.Apply();

                // Compress texture down to array if not 
                if(array.format != TextureFormat.RGBA32)
                    EditorUtility.CompressTexture(temp, array.format, TextureCompressionQuality.Normal);

                texture = temp;
            }

            // Copy texture to array
            for (int mip = 0; mip < texture.mipmapCount; mip++) {
                Graphics.CopyTexture(texture, 0, mip, array, index, mip);
            }

            return array;
        }

        /// <summary>
        /// Creates a new Texture2DArray with the given textures <br />
        /// Takes into account unsupported formats and automatically rescales textures at different resolutions <br />
        /// Automatically resizes input textures to the array size if they are a different resolution
        /// </summary>
        public static Texture2DArray Create(Texture2D[] textures, bool compressed = true)
        {
            // Set format depending on if it is compressed or not, compress with dxt5 for best compression with all channels
            TextureFormat format = compressed ? TextureFormat.DXT5 : TextureFormat.RGBA32;

            // Create array and assign textures to it 
            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, format, textures[0].mipmapCount > 1);
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] == null) continue;

                array = UpdateTexture(array, textures[i], i);
            }

            array.filterMode = textures[0].filterMode;
            array.wrapMode = textures[0].wrapMode;

            return array;
        }

        /// <summary>
        /// Overwrites the texture at a given index to the input texture <br />
        /// Automatically resizes the input texture to the array size if it is a different resolution
        /// </summary>
        public static Texture2DArray UpdateTexture(Texture2DArray array, Texture2D texture, int index)
        {
            if (texture.width != array.width || texture.height != array.height) {
                Debug.LogWarning($"Texture at index {index} is not the same resolution as the array, resizing to array size. Please use a texture with the same resolution as the array");
                // Scale texture to array resolution
                texture = TextureUtilities.ResizeTexture(texture, array.width, array.height);
            }

            SetTextureAtIndex(array, texture, index);

            return array;
        }

        /// <summary>
        /// Returns: Array, User Cancelled <br />
        /// Overwrites the texture at a given index to the input texture <br />
        /// Waits for the user to input the outcome when a texture is a different resolution to the array
        /// </summary>
        public static async Task<(Texture2DArray, bool)> UpdateTextureAsync(Texture2DArray array, Texture2D texture, int index)
        {
            if(texture.width != array.width || texture.height != array.height) {

                // Get user input to determine what to do with the texture
                int returned = await EditableDisplayDialog.Show(
                    texture,
                    "Texture Resolution Difference",
                    $"Texture: {texture.width}x{texture.height} Array: {array.width}x{array.height}",
                    "Texture size is not the same as they array. Would you like to resize this texture to the array resolution, or resize the array to this texture resolution?",
                    "Resize Texture", "Resize Array", "Cancel"
                );

                switch (returned) {
                    case 0:
                        // Scale texture to array resolution
                        texture = TextureUtilities.ResizeTexture(texture, array.width, array.height);

                        break;
                    case 1:
                        // Scale array to texture resolution
                        // Create new array, resizing all textures except for this one
                        // This is a decently slow operation though so expect it to take some depending on the array depth and resolution

                        Texture2D[] textures = GetTextures(array);
                        textures[index] = texture;

                        for (int i = 0; i < textures.Length; i++) {
                            if (i == index) continue;

                            textures[i] = TextureUtilities.ResizeTexture(textures[i], texture.width, texture.height);
                        }

                        array = Create(textures);

                        // Return resized array
                        return (array, false);
                    
                    case 2:
                        // Cancel
                        return (array, true);
                }
            }

            SetTextureAtIndex(array, texture, index);

            return (array, false);
        }

        /// <summary>
        /// Returns all the textures from the input array
        /// </summary>
        public static Texture2D[] GetTextures(Texture2DArray array)
        {
            Texture2D[] textures = new Texture2D[array.depth];

            for (int i = 0; i < array.depth; i++) {
                textures[i] = new Texture2D(array.width, array.height, array.format, array.mipmapCount > 1);
                for (int mip = 0; mip < array.mipmapCount; mip++) {
                    Graphics.CopyTexture(array, i, mip, textures[i], 0, mip);
                }
            }

            return textures;
        }
    }
}
#endif