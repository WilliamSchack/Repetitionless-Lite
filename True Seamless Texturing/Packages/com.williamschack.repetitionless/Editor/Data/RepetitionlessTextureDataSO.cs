using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Data
{
    using TextureUtilities;

    public class RepetitionlessTextureDataSO : ScriptableObject
    {
        public static readonly Vector4 DEFAULT_AV_COLOUR  = new Vector4 ( 1.0f, 1.0f, 1.0f, 0.0f );
        public static readonly Vector4 DEFAULT_NSO_COLOUR = new Vector4 ( 0.0f, 0.0f, 0.0f, 1.0f );
        public static readonly Vector4 DEFAULT_EM_COLOUR  = new Vector4 ( 1.0f, 1.0f, 1.0f, 0.0f );
        public static readonly Vector4 DEFAULT_BM_COLOUR  = new Vector4 ( 1.0f, 0.0f, 0.0f, 0.0f);

        [System.Serializable]
        public struct MaterialTextureData
        {
            public TexturePacker.TextureData[] AVTextures;
            public TexturePacker.TextureData[] NSOTextures;
            public TexturePacker.TextureData[] EMTextures;
        }

        [System.Serializable]
        public class LayerTextureData
        {
            public MaterialTextureData BaseMaterialTextures;
            public MaterialTextureData FarMaterialTextures;
            public MaterialTextureData BlendMaterialTextures;

            public TexturePacker.TextureData BlendMaskTexture;
        }

        public LayerTextureData[] LayersTextureData;

        public void Init(int layersCount)
        {
            LayersTextureData = new LayerTextureData[layersCount];

            for (int i = 0; i < layersCount; i++) {
                SetupLayer(i);
            }
        }

        public void SetupLayer(int index)
        {
            LayersTextureData[index] = new LayerTextureData();

            SetupMaterial(ref LayersTextureData[index].BaseMaterialTextures);
            SetupMaterial(ref LayersTextureData[index].FarMaterialTextures);
            SetupMaterial(ref LayersTextureData[index].BlendMaterialTextures);

            LayersTextureData[index].BlendMaskTexture = new TexturePacker.TextureData() {
                Disabled = false,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.R
                    )
                }
            };
        }

        public void SetupMaterial(ref MaterialTextureData data)
        {
            // Setup Textures
            // AVTextures: albedo (rgb), variation (a)
            // NSOTextures: normal (rg), smooth/rough (b), occlussion (a)
            // EMTextures: emission (rgb), metallic (a)

            // Includes packed texture at indexes (Disabled by default):
            // NSOTextures[3], EMTextures[2]

            data = new MaterialTextureData();

            data.AVTextures = new TexturePacker.TextureData[2];
            data.NSOTextures = new TexturePacker.TextureData[4];
            data.EMTextures = new TexturePacker.TextureData[3];

            // Albedo
            data.AVTextures[0] = new TexturePacker.TextureData() {
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
            data.AVTextures[1] = new TexturePacker.TextureData() {
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
            data.NSOTextures[0] = new TexturePacker.TextureData() {
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
            data.NSOTextures[1] = new TexturePacker.TextureData() {
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
            data.NSOTextures[2] = new TexturePacker.TextureData() {
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
            data.NSOTextures[3] = new TexturePacker.TextureData() {
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
            data.EMTextures[0] = new TexturePacker.TextureData() {
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
            data.EMTextures[1] = new TexturePacker.TextureData() {
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
            data.EMTextures[2] = new TexturePacker.TextureData() {
                Disabled = true,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.A
                    )
                }
            };
        }

        public ref MaterialTextureData GetTextureData(int layerIndex, int materialIndex)
        {
            LayerTextureData layerData = LayersTextureData[layerIndex];

            switch (materialIndex) {
                case 0: return ref layerData.BaseMaterialTextures;
                case 1: return ref layerData.FarMaterialTextures;
                case 2: return ref layerData.BlendMaterialTextures; 
            }

            return ref layerData.BaseMaterialTextures;
        }

        // Assumes Init was called and texture data is in same format
        public void SetPackedTextureEnabled(int layerIndex, int materialIndex, bool enabled)
        {
            ref MaterialTextureData textureData = ref GetTextureData(layerIndex, materialIndex);

            textureData.NSOTextures[1].Disabled = enabled;
            textureData.NSOTextures[2].Disabled = enabled;
            textureData.NSOTextures[3].Disabled = !enabled;

            textureData.EMTextures[1].Disabled = enabled;
            textureData.EMTextures[2].Disabled = !enabled;
        }
    }
}