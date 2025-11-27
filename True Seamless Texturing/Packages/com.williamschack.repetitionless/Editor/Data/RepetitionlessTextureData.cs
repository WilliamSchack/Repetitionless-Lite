using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Data
{
    using TextureUtilities;

    public class RepetitionlessTextureData : ScriptableObject
    {
        private const int TOTAL_SECTIONS = 3;

        public static readonly Color[] DEFAULT_AV_COLOURS = { Color.white, Color.white, Color.white, Color.black };
        public static readonly Color[] DEFAULT_NSO_COLOURS = { Color.black, Color.black, Color.black, Color.white };
        public static readonly Color[] DEFAULT_EM_COLOURS = { Color.white, Color.white, Color.white, Color.black };

        public TexturePacker.TextureData[] AVTextures;
        public TexturePacker.TextureData[] NSOTextures;
        public TexturePacker.TextureData[] EMTextures;

        public void Init()
        {
            // Setup Textures
            // AVTextures: albedo (rgb), variation (a)
            // NSOTextures: normal (rg), smooth/rough (b), occlussion (a)
            // EMTextures: emission (rgb), metallic (a)

            AVTextures = new TexturePacker.TextureData[2];
            NSOTextures = new TexturePacker.TextureData[3];
            EMTextures = new TexturePacker.TextureData[2];

            // Albedo
            AVTextures[0] = new TexturePacker.TextureData() {
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
            AVTextures[1] = new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };

            // Normal
            NSOTextures[0] = new TexturePacker.TextureData() {
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
            NSOTextures[1] = new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.B
                    )
                }
            };

            // Occlussion
            NSOTextures[2] = new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };

            // Emission
            EMTextures[0] = new TexturePacker.TextureData() {
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
            EMTextures[1] = new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };
        }
    }
}