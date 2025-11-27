using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Data
{
    using TextureUtilities;

    public class RepetitionlessTextureData : ScriptableObject
    {
        private const int TOTAL_SECTIONS = 3;

        public List<TexturePacker.TextureData> AVTextures = new List<TexturePacker.TextureData>();
        public List<TexturePacker.TextureData> NSOTextures = new List<TexturePacker.TextureData>();
        public List<TexturePacker.TextureData> EMTextures = new List<TexturePacker.TextureData>();

        public void Init()
        {
            // Setup Textures
            // AVTextures: albedo (rgb), variation (a)
            // NSOTextures: normal (rg), smooth/rough (b), occlussion (a)
            // EMTextures: emission (rgb), metallic (a)

            // Albedo
            AVTextures.Add(new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.R
                    },
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.G,
                        ToChannel = TexturePacker.TextureChannel.G
                    },
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.B,
                        ToChannel = TexturePacker.TextureChannel.B
                    }
                }
            });
            
            // Variation
            AVTextures.Add(new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.A
                    }
                }
            });

            // Normal
            NSOTextures.Add(new TexturePacker.TextureData() {
                NormalMap = true,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.R
                    },
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.G,
                        ToChannel = TexturePacker.TextureChannel.G
                    }
                } 
            });

            // Smoothness / Roughness
            NSOTextures.Add(new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.B
                    }
                }
            });

            // Occlussion
            NSOTextures.Add(new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.A
                    }
                }
            });

            // Emission
            EMTextures.Add(new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.R
                    },
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.G,
                        ToChannel = TexturePacker.TextureChannel.G
                    },
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.B,
                        ToChannel = TexturePacker.TextureChannel.B
                    }
                }
            });

            // Metallic
            EMTextures.Add(new TexturePacker.TextureData() {
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel() {
                        FromChannel = TexturePacker.TextureChannel.R,
                        ToChannel = TexturePacker.TextureChannel.A
                    }
                }
            });
        }
    }
}