using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Repetitionless.Data
{
    using TextureUtilities;
    using GUIUtilities;
    using Variables;

    public class RepetitionlessTextureDataSO : ScriptableObject
    {
        public const int MATERIAL_COUNT = 3;

        private const string AV_TEXTURES_PROP_NAME  = "_AVTextures";
        private const string NSO_TEXTURES_PROP_NAME = "_NSOTextures";
        private const string EM_TEXTURES_PROP_NAME  = "_EMTextures";
        private const string BM_TEXTURES_PROP_NAME  = "_BMTextures";
        private const string AV_ASSIGNED_TEXTURES_PROP_NAME = "_AssignedAVTextures";
        private const string NSO_ASSIGNED_TEXTURES_PROP_NAME = "_AssignedNSOTextures";
        private const string EM_ASSIGNED_TEXTURES_PROP_NAME = "_AssignedEMTextures";
        private const string BM_ASSIGNED_TEXTURES_PROP_NAME = "_AssignedBMTextures";

        private static readonly Vector4 DEFAULT_AV_COLOUR  = new Vector4 ( 1.0f, 1.0f, 1.0f, 0.0f );
        private static readonly Vector4 DEFAULT_NSO_COLOUR = new Vector4 ( 0.0f, 0.0f, 0.0f, 1.0f );
        private static readonly Vector4 DEFAULT_EM_COLOUR  = new Vector4 ( 1.0f, 1.0f, 1.0f, 0.0f );
        private static readonly Vector4 DEFAULT_BM_COLOUR  = new Vector4 ( 1.0f, 0.0f, 0.0f, 0.0f);

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

            // Storing in an array to make it easier to pass into texture drawer
            public TexturePacker.TextureData[] BlendMaskTexture;
        }

        public LayerTextureData[] LayersTextureData;

        // Non-Serializable
        private MaterialDataManager _dataManager;

        public TextureArrayCustomChannelsGUIDrawer AVTexturesDrawer;
        public TextureArrayCustomChannelsGUIDrawer NSOTexturesDrawer;
        public TextureArrayCustomChannelsGUIDrawer EMTexturesDrawer;
        public TextureArrayCustomChannelsGUIDrawer BMTexturesDrawer;

        public void Init(int layersCount)
        {
            LayersTextureData = new LayerTextureData[layersCount];

            for (int i = 0; i < layersCount; i++) {
                SetupLayer(i);
            }
        }

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        // Should be called every time before using the drawers
        public void SetupTextureDrawers(MaterialDataManager dataManager)
        {
            _dataManager = dataManager;

            AVTexturesDrawer  = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetTextureDrawerTextureData(i, 0); }, Save, DEFAULT_AV_COLOUR, dataManager.Material, AV_TEXTURES_PROP_NAME, AV_ASSIGNED_TEXTURES_PROP_NAME, LayersTextureData.Length * 3);
            NSOTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetTextureDrawerTextureData(i, 1); }, Save, DEFAULT_NSO_COLOUR, dataManager.Material, NSO_TEXTURES_PROP_NAME, NSO_ASSIGNED_TEXTURES_PROP_NAME, LayersTextureData.Length * 3);
            EMTexturesDrawer  = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetTextureDrawerTextureData(i, 2); }, Save, DEFAULT_EM_COLOUR, dataManager.Material, EM_TEXTURES_PROP_NAME, EM_ASSIGNED_TEXTURES_PROP_NAME, LayersTextureData.Length * 3);
            BMTexturesDrawer  = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetBlendMaskTextureData(i); }, Save, DEFAULT_BM_COLOUR, dataManager.Material, BM_TEXTURES_PROP_NAME, BM_ASSIGNED_TEXTURES_PROP_NAME, LayersTextureData.Length);

            AVTexturesDrawer.TextureFormat  = TextureFormat.BC7;
            NSOTexturesDrawer.TextureFormat = TextureFormat.BC7;
            NSOTexturesDrawer.ArrayLinear   = true;
            EMTexturesDrawer.TextureFormat  = TextureFormat.BC7;
            BMTexturesDrawer.TextureFormat  = TextureFormat.BC7;
        }

        public void SetupLayer(int index)
        {
            LayersTextureData[index] = new LayerTextureData();

            SetupMaterial(ref LayersTextureData[index].BaseMaterialTextures);
            SetupMaterial(ref LayersTextureData[index].FarMaterialTextures);
            SetupMaterial(ref LayersTextureData[index].BlendMaterialTextures);

            LayersTextureData[index].BlendMaskTexture = new TexturePacker.TextureData[1];
            LayersTextureData[index].BlendMaskTexture[0] = new TexturePacker.TextureData() {
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

        public ref MaterialTextureData GetMaterialTextureData(int layerIndex, int materialIndex)
        {
            LayerTextureData layerData = LayersTextureData[layerIndex];

            switch (materialIndex) {
                case 0: return ref layerData.BaseMaterialTextures;
                case 1: return ref layerData.FarMaterialTextures;
                case 2: return ref layerData.BlendMaterialTextures; 
            }

            return ref layerData.BaseMaterialTextures;
        }

        public ref TexturePacker.TextureData[] GetTextureData(ref MaterialTextureData materialData, int texturesIndex)
        {
            switch(texturesIndex) {
                case 0: return ref materialData.AVTextures;
                case 1: return ref materialData.NSOTextures;
                case 2: return ref materialData.EMTextures;
            }

            return ref materialData.AVTextures;
        }

        public ref TexturePacker.TextureData[] GetTextureData(int layerIndex, int materialIndex, int texturesIndex)
        {
            ref MaterialTextureData materialTextureData = ref GetMaterialTextureData(layerIndex, materialIndex);
            return ref GetTextureData(ref materialTextureData, texturesIndex);
        }

        private ref TexturePacker.TextureData[] GetTextureDrawerTextureData(int arrayLayerIndex, int texturesIndex)
        {
            int layerIndex    = (int)Mathf.Floor(arrayLayerIndex / 3.0f);
            int materialIndex = arrayLayerIndex % MATERIAL_COUNT;

            return ref GetTextureData(layerIndex, materialIndex, texturesIndex);
        }

        private ref TexturePacker.TextureData[] GetBlendMaskTextureData(int layerIndex)
        {
            return ref LayersTextureData[layerIndex].BlendMaskTexture;
        }

        // Assumes Init was called and texture data is in same format
        public void UpdatePackedTexture(int layerIndex, int materialIndex, bool enabled)
        {
            ref MaterialTextureData textureData = ref GetMaterialTextureData(layerIndex, materialIndex);

            // Update Variables
            textureData.NSOTextures[1].Disabled = enabled;
            textureData.NSOTextures[2].Disabled = enabled;
            textureData.NSOTextures[3].Disabled = !enabled;

            textureData.EMTextures[1].Disabled = enabled;
            textureData.EMTextures[2].Disabled = !enabled;

            // Update textures
            if (enabled) {
                // Use packed texture
                NSOTexturesDrawer.UpdateTexture(textureData.NSOTextures[3].Texture, materialIndex, 3, true);
                EMTexturesDrawer.UpdateTexture(textureData.EMTextures[2].Texture, materialIndex, 2, true);
            } else {
                // Use regular textures
                NSOTexturesDrawer.UpdateTexture(textureData.NSOTextures[1].Texture, materialIndex, 1, true);
                NSOTexturesDrawer.UpdateTexture(textureData.NSOTextures[2].Texture, materialIndex, 2, true);
                EMTexturesDrawer.UpdateTexture(textureData.EMTextures[1].Texture, materialIndex, 1, true);
            }

            Save();
        }
    }
}