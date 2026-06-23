using UnityEngine;
using UnityEngine.Rendering;

namespace Repetitionless.Runtime.Utilities
{
    using Variables;

    public static class RenderPipelineUtilities
    {
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

        public static Material GetDefaultTerrainMaterial()
        {
            Shader terrainShader = GraphicsSettings.defaultRenderPipeline?.defaultTerrainMaterial?.shader;

            // Fallback
            if (terrainShader == null) {
                ERenderPipeline currentPipeline = GetActiveRenderPipeline();
                switch (currentPipeline) {
                    case ERenderPipeline.Builtin:
                        terrainShader = Shader.Find("Nature/Terrain/Standard");
                        break;
                    case ERenderPipeline.URP:
                        terrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                        break;
                    case ERenderPipeline.HDRP:
                        terrainShader = Shader.Find("HDRP/TerrainLit");
                        break;
                    default:
                        return null;
                }
            }

            return new Material(terrainShader);
        }
    }
}