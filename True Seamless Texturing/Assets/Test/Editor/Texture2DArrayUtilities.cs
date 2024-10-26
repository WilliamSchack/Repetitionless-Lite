using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

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

                // Compress texture down to array format if array is compressed
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
        public static Texture2DArray Create(Texture2D[] textures, bool compressed = true, bool linear = true)
        {
            // Set format depending on if it is compressed or not, compress with dxt5 for best compression with all channels
            TextureFormat format = compressed ? TextureFormat.DXT5 : TextureFormat.RGBA32;

            // Create array and assign textures to it
            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, format, textures[0].mipmapCount > 1, linear);
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] == null) continue;

                array = UpdateTexture(array, textures[i], i);
            }

            array.filterMode = textures[0].filterMode;
            array.wrapMode = textures[0].wrapMode;

            return array;
        }

        /// <summary>
        /// Creates a new Texture2DArray with the given textures <br />
        /// Takes into account unsupported formats and automatically rescales textures at different resolutions <br />
        /// Check for user input to decide whether to resize the texture or array if a texture is at a different resolution to the array
        /// </summary>
        /// <param name="autoResizeIndexes">
        /// Automatically resizes all input indexes instead of showing a prompt to the user
        /// </param>
        /// <returns></returns>
        public static async Task<Texture2DArray> CreateAsync(Texture2D[] textures, int[] autoResizeIndexes = null, bool compressed = true, bool linear = true)
        {
            // Set format depending on if it is compressed or not, compress with dxt5 for best compression with all channels
            TextureFormat format = compressed ? TextureFormat.DXT5 : TextureFormat.RGBA32;

            // Initial resolution
            Vector2Int resolution = new Vector2Int(textures[0].width, textures[0].height);

            // Check if any texture is a different resolution
            Texture2D[] differentResTextures = textures.Where(x => x.width != resolution.x || x.height != resolution.y).ToArray();
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] == null) continue;

                Texture2D texture = textures[i];

                if (texture.width != resolution.x || texture.height != resolution.y) {
                    // Automatically resize and skip popup if specified
                    if (autoResizeIndexes.Contains(i)) {
                        textures[i] = TextureUtilities.ResizeTexture(texture, resolution.x, resolution.y);
                        continue;
                    }

                    // Get user input to determine what to do with the texture
                    int returned = await EditableDisplayDialog.Show(
                        texture,
                        "Texture Resolution Difference",
                        $"Texture: {texture.width}x{texture.height} Array: {resolution.x}x{resolution.y}",
                        "Texture size is not the same as they array. Would you like to resize this texture to the array resolution, or resize the array to this texture resolution?",
                        "Resize Texture", "Resize Array", "" // Don't allow cancel
                    );

                    // If resizing array, skip the next textures and move onto resizing all of them
                    if(returned == 1) {
                        resolution = new Vector2Int(texture.width, texture.height);
                        break;
                    }

                    // If resizing texture, resize it and assign back to the texture array
                    if (returned == 0) {
                        textures[i] = TextureUtilities.ResizeTexture(texture, resolution.x, resolution.y);
                    }
                }
            }

            // If resolution has changed, resize all textures to the new resolution
            if(resolution != new Vector2Int(textures[0].width, textures[0].height)) {
                for (int i = 0; i < textures.Length; i++) {
                    if (textures[i] == null) continue;

                    Texture2D texture = textures[i];
                    if (texture.width != resolution.x || texture.height != resolution.y) {
                        textures[i] = TextureUtilities.ResizeTexture(texture, resolution.x, resolution.y);
                    }
                }
            }

            // Create array and assign textures to it
            Texture2DArray array = new Texture2DArray(resolution.x, resolution.y, textures.Length, format, textures[0].mipmapCount > 1, linear);
            for (int i = 0; i < textures.Length; i++) {
                if (textures[i] == null) continue;

                SetTextureAtIndex(array, textures[i], i);
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
            // If array has a depth of one, the only texture is being replaced, create a new array as it will cause mipmap or format errors otherwise
            if(array.depth == 1) {
                array = Create(new Texture2D[] { texture }, array.format == TextureFormat.DXT5);
                return (array, false);
            }

            // Only give user option if texture differs from its resolution
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
                        array = ResizeArray(array, texture.width, texture.height);

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

        public static Texture2DArray ResizeArray(Texture2DArray array, int newWidth, int newHeight)
        {
            Texture2D[] textures = GetTextures(array);
            for (int i = 0; i < textures.Length; i++) {
                // Only resize texture if resolution is not the target
                if (textures[i].width != newWidth && textures[i].height != newHeight)
                    textures[i] = TextureUtilities.ResizeTexture(textures[i], newWidth, newHeight);
            }

            return Create(textures, array.format == TextureFormat.DXT5);
        }
    }
}
#endif