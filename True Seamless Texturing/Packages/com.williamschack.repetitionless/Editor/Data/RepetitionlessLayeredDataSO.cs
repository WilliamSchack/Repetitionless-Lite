using Repetitionless.Runtime.Variables;
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Editor.Data
{
    using System.Collections.Generic;
    using TextureUtilities;

    public class RepetitionlessLayeredDataSO : ScriptableObject
    {
        [SerializeField] public ELayerMode LayerMode = ELayerMode.ControlTextures;

        [SerializeField] public TexturePacker.TextureData[] ControlTextures = new TexturePacker.TextureData[Constants.MAX_LAYERS_TERRAIN];
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
            ControlTextures = new TexturePacker.TextureData[Constants.MAX_LAYERS_TERRAIN];

            for (int i = 0; i < ControlTextures.Length; i++) {
                SetupControlTexture(i);
            }
        }

        /// <summary>
        /// Resets a control textures data
        /// </summary>
        /// <param name="layerIndex">
        /// The layer index to reset
        /// </param>
        public void SetupControlTexture(int layerIndex)
        {
            TexturePacker.TextureChannel[] textureChannels = {
                TexturePacker.TextureChannel.R,
                TexturePacker.TextureChannel.G,
                TexturePacker.TextureChannel.B,
                TexturePacker.TextureChannel.A
            };

            int channelIndex = i % textureChannels.Length;
            TexturePacker.TextureChannel textureChannel = textureChannels[channelIndex];

            ControlTextures[i] = new TexturePacker.TextureData() {
                Texture = ControlTextures[i].Texture,
                Disabled = false,
                DataTexture = true,
                NormalMap = false,
                FromToChannels = new List<TexturePacker.FromToChannel>() {
                    new TexturePacker.FromToChannel(
                        TexturePacker.TextureChannel.R,
                        textureChannel
                    )
                }
            };
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