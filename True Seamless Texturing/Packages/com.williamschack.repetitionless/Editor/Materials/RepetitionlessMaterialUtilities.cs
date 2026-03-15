#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Materials
{
    internal static class RepetitionlessMaterialUtilities
    {
        private const string NOISE_TEXTURE_PROP_NAME = "_NoiseTexture";
        private const string SURFACE_TYPE_PROP_NAME = "_SurfaceTypeSetting";

        public static ERenderPipeline GetActiveRenderPipeline()
        {
            RenderPipelineAsset currentPipeline = GraphicsSettings.currentRenderPipeline;
            if (currentPipeline == null)
                return ERenderPipeline.Builtin;
            
            if (currentPipeline.GetType().Name.Contains("UniversalRenderPipeline"))
                return ERenderPipeline.URP;
            
            if (currentPipeline.GetType().Name.Contains("HDRenderPipeline"))
                return ERenderPipeline.HDRP;

            return ERenderPipeline.Unknown;
        }

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

        public static void SetSurface(Material mat, ESurfaceType surfaceType, ERenderPipeline pipeline)
        {
            mat.SetFloat(SURFACE_TYPE_PROP_NAME, (int)surfaceType);

            switch (surfaceType) {
                case ESurfaceType.Opaque:
                    mat.renderQueue = (int)RenderQueue.Geometry;
                    mat.SetOverrideTag("RenderType", "Opaque");

                    if (pipeline == ERenderPipeline.HDRP) {
                        mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        mat.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                        mat.SetInt("_SurfaceType", 0);
                        mat.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                        mat.SetInt("_AlphaDstBlend", (int)BlendMode.Zero);
                        mat.SetInt("_AlphaCutoffEnable", 0);
                        mat.SetInt("_ZWrite", 1);

                        mat.renderQueue = 2475;
                    } else {
                        mat.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        mat.SetInt("_BUILTIN_Surface", 0);
                        mat.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        mat.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        mat.SetInt("_BUILTIN_ZWrite", 1);
                    }
                    break;
                case ESurfaceType.Cutout:
                    mat.renderQueue = (int)RenderQueue.AlphaTest;
                    mat.SetOverrideTag("RenderType", "TransparentCutout");

                    if (pipeline == ERenderPipeline.HDRP) {
                        mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

                        mat.SetInt("_SurfaceType", 0);
                        mat.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                        mat.SetInt("_AlphaDstBlend", (int)BlendMode.Zero);
                        mat.SetInt("_AlphaCutoffEnable", 1);
                        mat.SetInt("_ZWrite", 1);
                    } else {
                        mat.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");
                        mat.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                        mat.SetInt("_BUILTIN_Surface", 0);
                        mat.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        mat.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        mat.SetInt("_BUILTIN_ZWrite", 1); 
                    }
                    break;
                case ESurfaceType.Transparent:
                    mat.renderQueue = (int)RenderQueue.Transparent;
                    mat.SetOverrideTag("RenderType", "Transparent");

                    if (pipeline == ERenderPipeline.HDRP) {
                        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        mat.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");

                        mat.SetInt("_SurfaceType", 1);
                        mat.SetInt("_AlphaSrcBlend", (int)BlendMode.One);
                        mat.SetInt("_AlphaDstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_AlphaCutoffEnable", 0);
                        mat.SetInt("_ZWrite", 0);
                    } else {
                        mat.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        mat.SetInt("_BUILTIN_Surface", 1);
                        mat.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.SrcAlpha);
                        mat.SetInt("_BUILTIN_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_BUILTIN_ZWrite", 0);
                    }
                    break;
            }
        }

        public static void SetSurface(Material mat, ESurfaceType surfaceType)
        {
            ERenderPipeline currentPipeline = GetActiveRenderPipeline();
            SetSurface(mat, surfaceType, currentPipeline);
        }

        // Should move more utility functions here from the inspector but they arent globally needed so for now this is it
    }
}
#endif