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

        [SerializeField] public ELayerMode LayerMode = ELayerMode.ControlTextures;

        // 8 Control textures, 4 channels/textures per
        [SerializeField] public ControlTexture[] ControlTextures = new ControlTexture[Constants.MAX_LAYERS_TERRAIN / 4];

        [SerializeField] public TexturePacker.TextureData HolesTexture = new TexturePacker.TextureData();

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
        /// Resets the texture data fields
        /// </summary>
        public void SetupTextures()
        {
            SetupControlTextures();
            SetupHolesTexture();
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
            int controlTextureIndex = (int)Mathf.Floor(layerIndex / 4.0f);
            int channelIndex = layerIndex % 4;

            SetupControlChannelTexture(controlTextureIndex, channelIndex);
        }

        public ref TexturePacker.TextureData GetControlTextureData(int layerIndex)
        {
            int controlTextureIndex = (int)Mathf.Floor(layerIndex / 4.0f);
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
    }
}