using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Data
{
    using TextureUtilities;

    public class RepetitionlessTextureData : ScriptableObject
    {
        private const int TOTAL_SECTIONS = 3;

        public static readonly Vector4 DEFAULT_AV_COLOUR  = new Vector4 ( 1.0f, 1.0f, 1.0f, 0.0f );
        public static readonly Vector4 DEFAULT_NSO_COLOUR = new Vector4 ( 0.0f, 0.0f, 0.0f, 1.0f );
        public static readonly Vector4 DEFAULT_EM_COLOUR  = new Vector4 ( 1.0f, 1.0f, 1.0f, 0.0f );

        [System.Serializable]
        public struct MaterialTextureData
        {
            public TexturePacker.TextureData[] AVTextures;
            public TexturePacker.TextureData[] NSOTextures;
            public TexturePacker.TextureData[] EMTextures;
        }

        public MaterialTextureData[] MaterialsTextureData;

        public void Init(int materialsCount)
        {
            MaterialsTextureData = new MaterialTextureData[materialsCount];

            for (int i = 0; i < materialsCount; i++)
                SetupMaterial(i);
        }

        public void SetupMaterial(int index)
        {
            if (index >= MaterialsTextureData.Length)
                return;

            // Setup Textures
            // AVTextures: albedo (rgb), variation (a)
            // NSOTextures: normal (rg), smooth/rough (b), occlussion (a)
            // EMTextures: emission (rgb), metallic (a)

            // Includes packed texture at indexes (Disabled by default):
            // NSOTextures[3], EMTextures[2]

            MaterialTextureData newMaterial = new MaterialTextureData();

            newMaterial.AVTextures = new TexturePacker.TextureData[2];
            newMaterial.NSOTextures = new TexturePacker.TextureData[4];
            newMaterial.EMTextures = new TexturePacker.TextureData[3];

            // Albedo
            newMaterial.AVTextures[0] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.R
                    ),
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.G,
                        TexturePacker.TextureChannel.G
                    ),
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.B,
                        TexturePacker.TextureChannel.B
                    )
                }
            };
            
            // Variation
            newMaterial.AVTextures[1] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };

            // Normal
            newMaterial.NSOTextures[0] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = true,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.R
                    ),
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.G,
                        TexturePacker.TextureChannel.G
                    )
                } 
            };

            // Smoothness / Roughness
            newMaterial.NSOTextures[1] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.B
                    )
                }
            };

            // Occlussion
            newMaterial.NSOTextures[2] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };

            // Packed Texture (Occlussion, Smoothness)
            newMaterial.NSOTextures[3] = new TexturePacker.TextureData() {
                Disabled = true,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    // Occlussion
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.G,
                        TexturePacker.TextureChannel.A
                    ),
                    // Smoothness
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.A,
                        TexturePacker.TextureChannel.B
                    )
                }
            };

            // Emission
            newMaterial.EMTextures[0] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.R
                    ),
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.G,
                        TexturePacker.TextureChannel.G
                    ),
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.B,
                        TexturePacker.TextureChannel.B
                    )
                }
            };

            // Metallic
            newMaterial.EMTextures[1] = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };

            // Packed Texture (Metallic)
            newMaterial.EMTextures[2] = new TexturePacker.TextureData() {
                Disabled = true,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };

            MaterialsTextureData[index] = newMaterial;
        }



        // Assumes Init was called and texture data is in same format
        public void SetPackedTextureEnabled(int materialIndex, bool enabled)
        {
            MaterialsTextureData[materialIndex].NSOTextures[1].Disabled = enabled;
            MaterialsTextureData[materialIndex].NSOTextures[2].Disabled = enabled;
            MaterialsTextureData[materialIndex].NSOTextures[3].Disabled = !enabled;

            MaterialsTextureData[materialIndex].EMTextures[1].Disabled = enabled;
            MaterialsTextureData[materialIndex].EMTextures[2].Disabled = !enabled;
        }
    }
}