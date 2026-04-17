using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Data
{
    using TextureUtilities;

    public class RepetitionlessLayeredDataSO : ScriptableObject
    {
        // Using class for serialization
        [System.Serializable]
        public class ControlTexture
        {
            public TexturePacker.TextureData[] ChannelTextures;
        }

        private static readonly Vector4 DEFAULT_LAYER_COLOURS_FIRST = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
        private static readonly Vector4 DEFAULT_LAYER_COLOURS = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        [SerializeField] public ELayerMode LayerMode = ELayerMode.TerrainLayers;

        // 8 Control textures, 4 channels/textures per
        [SerializeField] public ControlTexture[] ControlTextures = new ControlTexture[Constants.MAX_LAYERS_TERRAIN / 4];
        [SerializeField] private Texture2D[] _packedControlTextures = new Texture2D[Constants.MAX_LAYERS_TERRAIN / 4];

        public Texture2D[] PackedControlTextures => _packedControlTextures;

        [SerializeField] public TexturePacker.TextureData HolesTexture = new TexturePacker.TextureData();

        // Non-Serializable
        private MaterialDataManager _dataManagerCache;
        private MaterialDataManager _dataManager {
            get {
                if (_dataManagerCache?.Material != null)
                    return _dataManagerCache;

                _dataManagerCache = new MaterialDataManager(this);
                return _dataManagerCache;
            }
        }

        /// <summary>
        /// Saves this object
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Resets the texture data fields and packs the initial textures
        /// </summary>
        public void Init()
        {
            SetupControlTextures();
            SetupHolesTexture();

            PackControlTextures();
        }

        /// <summary>
        /// Resets the control textures data
        /// </summary>
        public void SetupControlTextures()
        {
            ControlTextures = new ControlTexture[Constants.MAX_LAYERS_TERRAIN / 4];

            for (int i = 0; i < ControlTextures.Length; i++) {
                SetupControlTextures(i);
            }
        }

        public void SetupControlTextures(int controlIndex)
        {
            ControlTextures[controlIndex] = new ControlTexture { ChannelTextures = new TexturePacker.TextureData[4] };

            for (int i = 0; i < ControlTextures[controlIndex].ChannelTextures.Length; i++) {
                SetupControlChannelTexture(controlIndex, i);
            }
        }

        public void SetupControlChannelTexture(int controlIndex, int channelIndex)
        {
            TexturePacker.TextureChannel[] textureChannels = {
                TexturePacker.TextureChannel.R,
                TexturePacker.TextureChannel.G,
                TexturePacker.TextureChannel.B,
                TexturePacker.TextureChannel.A
            };

            ControlTextures[controlIndex].ChannelTextures[channelIndex] = new TexturePacker.TextureData() {
                Texture = ControlTextures[controlIndex].ChannelTextures[channelIndex].Texture,
                Disabled = false,
                DataTexture = true,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        textureChannels[channelIndex]
                    )
                }
            };
        }

        public void SetupControlTexture(int layerIndex)
        {
            int controlTextureIndex = GetControlIndexFromLayerIndex(layerIndex);
            int channelIndex = layerIndex % 4;

            SetupControlChannelTexture(controlTextureIndex, channelIndex);
        }

        public ref TexturePacker.TextureData GetControlTextureData(int layerIndex)
        {
            int controlTextureIndex = GetControlIndexFromLayerIndex(layerIndex);
            int channelIndex = layerIndex % 4;

            return ref ControlTextures[controlTextureIndex].ChannelTextures[channelIndex];
        }

        /// <summary>
        /// Resets the holes texture data
        /// </summary>
        public void SetupHolesTexture()
        {
            HolesTexture = new TexturePacker.TextureData() {
                Texture = HolesTexture.Texture,
                Disabled = false,
                DataTexture = true,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        TexturePacker.TextureChannel.R
                    )
                }
            };
        }

        public void PackControlTextures()
        {
            for (int i = 0; i < ControlTextures.Length; i++) {
                PackControlTexture(i);
            }
        }

        public int GetControlIndexFromLayerIndex(int layerIndex)
        {
            return (int)Mathf.Floor(layerIndex / 4.0f);
        }

        public void PackControlTexture(int controlIndex)
        {
            ref TexturePacker.TextureData[] textures = ref ControlTextures[controlIndex].ChannelTextures;

            // Get the highest resolution
            Vector2Int highestResolution = new Vector2Int(1, 1);
            for (int i = 0; i < textures.Length; i++) {
                Texture2D texture = textures[i].Texture;
                if (texture == null) continue;

                if (texture.width > highestResolution.x && texture.height > highestResolution.y)
                    highestResolution = new Vector2Int(texture.width, texture.height);
            }

            // Make sure all textures are the highest resolution
            TexturePacker.TextureData[] resizedTextures = (TexturePacker.TextureData[])textures.Clone();
            for (int i = 0; i < resizedTextures.Length; i++) {
                ref TexturePacker.TextureData textureData = ref resizedTextures[i];
                Texture2D texture = textureData.Texture;
                if (texture == null) continue;

                // Resize the texture if its a different resolution
                if (texture.width != highestResolution.x || texture.height != highestResolution.y)
                    textureData.Texture = TextureUtilities.ResizeTexture(texture, highestResolution.x, highestResolution.y);
            }

            // Pack textures
            Vector4 defaultColours = controlIndex == 0 ? DEFAULT_LAYER_COLOURS_FIRST : DEFAULT_LAYER_COLOURS;
            Texture2D packedTexture = TexturePacker.PackTextures(resizedTextures, defaultColours);
            if (packedTexture == null) return;

            string fileName = Constants.CONTROL_TEXTURE_FILE_NAME_PREFIX + controlIndex + ".asset";
            _dataManager.CreateAsset(packedTexture, fileName, true);
            _packedControlTextures[controlIndex] = packedTexture;
        }

        public void AssignControlTextures()
        {
            for (int i = 0; i < ControlTextures.Length; i++) {
                AssignControlTexture(i);
            }
        }

        public void AssignControlTexture(int index)
        {
            _dataManager.Material.SetTexture($"_Control{index}", PackedControlTextures[index]);
        }

        public void UpdateLayersCount()
        {
            // Get amount of control textures assigned
            int texturesAssigned = 0;
            for (int i = 0; i < ControlTextures.Length; i++) {
                for (int j = 0; j < ControlTextures[i].ChannelTextures.Length; j++) {
                    if (ControlTextures[i].ChannelTextures[j].Texture != null)
                        texturesAssigned++;
                }
            }

            texturesAssigned = Mathf.Max(1, texturesAssigned);

            // Set layers count
            _dataManager.Material.SetFloat("_LayersCount", texturesAssigned);
        }
    }
}