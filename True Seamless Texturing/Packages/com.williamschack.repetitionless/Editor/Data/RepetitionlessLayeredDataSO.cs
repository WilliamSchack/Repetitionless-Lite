using Repetitionless.Runtime.Variables;
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Editor.Data
{
    using TextureUtilities;

    public class RepetitionlessLayeredDataSO : ScriptableObject
    {
        [SerializeField] public EControlMode LayerMode = EControlMode.ControlTextures;

        [SerializeField] public Texture2D[] ControlTextures = new Texture2D[Constants.MAX_LAYERS_TERRAIN];
        [SerializeField] public TexturePacker.TextureChannel[] ControlTextureChannels = new TexturePacker.TextureChannel[Constants.MAX_LAYERS_TERRAIN];

        /// <summary>
        /// Saves this object
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}