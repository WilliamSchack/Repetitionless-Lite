#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Processors
{
    internal static class RepetitionlessMaterialUtilities
    {
        private const string NOISE_TEXTURE_PROP_NAME = "_NoiseTexture";

        public static void SetKeyword(Material mat, string keyword, bool enabled)
        {
            // Delay call to prevent recursive warnings, this will take a while if variant not cached
            EditorApplication.delayCall += () => {
                // Using a keyword variable with SetKeyword sometimes gives errors
                if (enabled) mat.EnableKeyword(keyword);
                else         mat.DisableKeyword(keyword);

                mat.SetInt(keyword, enabled ? 1 : 0); // Required to save for some reason
                EditorUtility.SetDirty(mat);
            };
        }

        public static void UpdateNoiseQualityTexture(Material mat, ENoiseQuality noiseQuality)
        {
            switch (noiseQuality) {
                case ENoiseQuality.High:
                    mat.SetTexture(NOISE_TEXTURE_PROP_NAME, null);
                    break;
                case ENoiseQuality.Medium: {
                    Texture2D texture = Resources.Load<Texture2D>(Constants.NOISE_TEXTURE_NAME_4K);
                    mat.SetTexture(NOISE_TEXTURE_PROP_NAME, texture);
                    break;
                } case ENoiseQuality.Low: {
                    Texture2D texture = Resources.Load<Texture2D>(Constants.NOISE_TEXTURE_NAME_1K);
                    mat.SetTexture(NOISE_TEXTURE_PROP_NAME, texture);
                    break;
                }
            }
        }

        public static void SetNoiseQuality(Material mat, ENoiseQuality noiseQuality)
        {
            SetKeyword(mat, Constants.NOISE_TEXTURE_KEYWORD, noiseQuality != ENoiseQuality.High);
            UpdateNoiseQualityTexture(mat, noiseQuality);
        }

        public static void SetTriplanarEnabled(Material mat, bool enabled)
        {
            SetKeyword(mat, Constants.TRIPLANAR_KEYWORD, enabled);
        }

        // Should move more utility functions here from the inspector but they arent globally needed so for now this is it
    }
}
#endif