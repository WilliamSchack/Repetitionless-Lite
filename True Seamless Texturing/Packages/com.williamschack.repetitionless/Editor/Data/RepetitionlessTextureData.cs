using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Data
{
    using TextureUtilities;

    public class RepetitionlessTextureData : ScriptableObject
    {
        private const int TOTAL_SECTIONS = 3;

        // 1: albedo (rgb), variation (a)
        // 2: normal (rg), smooth/rough (b), occlussion (a)
        // 3: emission (rgb), metallic (a)

        public List<TexturePacker.TextureData> AVTextures;
        public List<TexturePacker.TextureData> NSOTextures;
        public List<TexturePacker.TextureData> EMTextures;

        //public Texture2D GetAVTexture(int layerIndex, int sectionIndex)
        //{
        //    int index = layerIndex * TOTAL_SECTIONS + sectionIndex;
        //    return AVTextures[index];
        //}
    }
}