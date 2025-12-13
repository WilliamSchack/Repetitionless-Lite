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
    using Compression;

    public class RepetitionlessTextureDataSO : ScriptableObject
    {
        public const int MATERIAL_COUNT = 3;

        private const string TEXTURE_ASSET_NAME = "AssignedTextures.asset";
        public const string TEXTURE_PROP_NAME = "_AssignedTexturesTexture";

        private const string AV_TEXTURES_PROP_NAME  = "_AVTextures";
        private const string NSO_TEXTURES_PROP_NAME = "_NSOTextures";
        private const string EM_TEXTURES_PROP_NAME  = "_EMTextures";
        private const string BM_TEXTURES_PROP_NAME  = "_BMTextures";

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

        [HideInInspector][SerializeField] private int[] _assignedAVTextures = new int[3];
        [HideInInspector][SerializeField] private int[] _assignedNSOTextures = new int[3];
        [HideInInspector][SerializeField] private int[] _assignedEMTextures = new int[3];
        [HideInInspector][SerializeField] private int _assignedBMTextures = 0;

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

        private MaterialProperty GetTexturesProp(int sectionIndex)
        {
            string propName = "";
            switch (sectionIndex) {
                case 0: propName = AV_TEXTURES_PROP_NAME; break;
                case 1: propName = NSO_TEXTURES_PROP_NAME; break;
                case 2: propName = EM_TEXTURES_PROP_NAME; break;
                case 3: propName = BM_TEXTURES_PROP_NAME; break;
                default: return null;
            }

            MaterialProperty prop = MaterialEditor.GetMaterialProperty(new Object[] { _dataManager.Material }, propName);
            return prop;
        }

        private ref int GetAssignedTexturesValue(int sectionIndex, int chunkIndex)
        {
            switch (sectionIndex) {
                case 0: return ref _assignedAVTextures[chunkIndex];
                case 1: return ref _assignedNSOTextures[chunkIndex];
                case 2: return ref _assignedEMTextures[chunkIndex];
                case 3: return ref _assignedBMTextures;
            }

            return ref _assignedAVTextures[0];
        }

        private int AssignedTexturesGetter(int sectionIndex, int chunkIndex)
        {
            return GetAssignedTexturesValue(sectionIndex, chunkIndex);
        }

        private void AssignedTexturesSetter(int sectionIndex, int chunkIndex, int compressedValues)
        {
            ref int assignedTexturesVal = ref GetAssignedTexturesValue(sectionIndex, chunkIndex);
            assignedTexturesVal = compressedValues;

            UpdateAssignedTexturesTexture();
            return;
        }

        // Returns:
        // Item1: First half
        // Item2: Second half
        private ushort[] GetAssignedTexturesData()
        {
            // Split assigned textures value into two 16 bit integers
            // Cannot store single 32 bit integers in a texture

            ushort[] data = new ushort[8 * 3]; // 8 pixels, 3 channels
            for (int section = 0; section < 4; section++) {
                for (int chunk = 0; chunk < 3; chunk++) {
                    int value = GetAssignedTexturesValue(section, chunk);
                    (ushort, ushort) valueSplit = BooleanCompression.Split32BitInt(value);

                    // x = 0 first half, x = 1 second half
                    int offsetFirst    = (section * 2 + 0) * 3 + chunk;
                    int offsetSecond   = (section * 2 + 1) * 3 + chunk;
                    data[offsetFirst]  = valueSplit.Item1;
                    data[offsetSecond] = valueSplit.Item2;
                }
            }

            return data;
        }

        // I would use 4 properties instead of a texture but this way i can store ints in the shader graph
        // You cannot store int properties in a shader graph and floating point precision would be an issue
        public void UpdateAssignedTexturesTexture()
        {
            MaterialProperty textureProp = MaterialEditor.GetMaterialProperty(new Object[] { _dataManager.Material }, TEXTURE_PROP_NAME);
            UpdateAssignedTexturesTexture(textureProp);
        }

        public void UpdateAssignedTexturesTexture(MaterialProperty property)
        {
            Texture2D texture;
            if (_dataManager.AssetExists(TEXTURE_ASSET_NAME)) {
                // Load and modify the texture
                texture = _dataManager.LoadAsset<Texture2D>(TEXTURE_ASSET_NAME);
            } else {
                // Create a new texture
                texture = new Texture2D(2, 4, TextureFormat.RGB48, false);
                _dataManager.CreateAsset(texture, TEXTURE_ASSET_NAME);
            }

            ushort[] pixelData = GetAssignedTexturesData();
            texture.SetPixelData(pixelData, 0);
            texture.Apply();

            if ((Texture2D)property.textureValue != texture)
                property.textureValue = texture;
        }

        // Should be called every time before using the drawers
        public void SetupTextureDrawers(MaterialDataManager dataManager)
        {
            _dataManager = dataManager;

            MaterialProperty avTexturesProp  = GetTexturesProp(0);
            MaterialProperty nsoTexturesProp = GetTexturesProp(1);
            MaterialProperty emTexturesProp  = GetTexturesProp(2);
            MaterialProperty bmTexturesProp  = GetTexturesProp(3);

            AVTexturesDrawer  = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetTextureDrawerTextureData(i, 0); }, Save, (int i) => { return AssignedTexturesGetter(0, i); }, (int i, int at) => { AssignedTexturesSetter(0, i, at); }, DEFAULT_AV_COLOUR,  avTexturesProp,  LayersTextureData.Length * 3);
            NSOTexturesDrawer = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetTextureDrawerTextureData(i, 1); }, Save, (int i) => { return AssignedTexturesGetter(1, i); }, (int i, int at) => { AssignedTexturesSetter(1, i, at); }, DEFAULT_NSO_COLOUR, nsoTexturesProp, LayersTextureData.Length * 3);
            EMTexturesDrawer  = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetTextureDrawerTextureData(i, 2); }, Save, (int i) => { return AssignedTexturesGetter(2, i); }, (int i, int at) => { AssignedTexturesSetter(2, i, at); }, DEFAULT_EM_COLOUR,  emTexturesProp,  LayersTextureData.Length * 3);
            BMTexturesDrawer  = new TextureArrayCustomChannelsGUIDrawer(_dataManager, (int i) => { return ref GetBlendMaskTextureData(i);        }, Save, (int i) => { return AssignedTexturesGetter(3, i); }, (int i, int at) => { AssignedTexturesSetter(3, i, at); }, DEFAULT_BM_COLOUR,  bmTexturesProp,  LayersTextureData.Length);

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