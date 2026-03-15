#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Materials
{
    using Data;

    public static class RepetitionlessMaterialConverter
    {
        private const string LIT_SHADER_NAME_BIRP = "Standard";
        private const string LIT_SHADER_NAME_URP  = "Universal Render Pipeline/Lit";
        private const string LIT_SHADER_NAME_HDRP = "HDRP/Lit";

        [MenuItem("Window/Repetitionless/Convert Selected Materials", secondaryPriority = 3)]
        private static void ConvertSelectedMaterialsToolbar()
        {
            Material[] selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.Assets);
            if (selectedMaterials.Length == 0) {
                Debug.LogWarning("No materials selected to convert");
                return;
            }

            ConvertMaterials(selectedMaterials);
        }

        private static void ConvertMaterials(Material[] materials)
        {
            foreach (Material material in materials) {
                // Check if the asset exists
                string path = AssetDatabase.GetAssetPath(material);
                if (path == "" ||! File.Exists(path)) {
                    Debug.LogWarning($"Could not convert {material.name}: Asset not found");
                    continue;
                }

                string directory = Path.GetDirectoryName(path);
                string fileName = Path.GetFileNameWithoutExtension(path);
                fileName += "_repetitionless.mat";

                string shaderName = material.shader.name;

                ERenderPipeline renderPipeline = ERenderPipeline.Unknown;
                if (shaderName == LIT_SHADER_NAME_BIRP) renderPipeline = ERenderPipeline.Builtin;
                if (shaderName == LIT_SHADER_NAME_URP)  renderPipeline = ERenderPipeline.URP;
                if (shaderName == LIT_SHADER_NAME_HDRP) renderPipeline = ERenderPipeline.HDRP;
                if (renderPipeline == ERenderPipeline.Unknown) {
                    Debug.LogWarning($"Could not convert {material.name}: Material must have a lit shader");
                    return;
                }

                // Create repetitionless material
                RepetitionlessMaterialCreator.MaterialDataObjects materialDataObjects = RepetitionlessMaterialCreator.CreateMaterial(renderPipeline, directory, fileName, false);
                if (materialDataObjects.Material == null) {
                    Debug.LogWarning($"Could not convert {material.name}: Failed to create material");
                    return;
                }

                // Get material properties
                Texture2D colourTex       = (Texture2D)material.GetTexture("_BaseMap");
                Texture2D metalTex        = (Texture2D)material.GetTexture("_MetallicGlossMap");
                Texture2D normalTex       = (Texture2D)material.GetTexture("_BumpMap");
                Texture2D occlussionTex   = (Texture2D)material.GetTexture("_OcclusionMap");
                Texture2D emissionTex     = (Texture2D)material.GetTexture("_EmissionMap");
                Color baseColour          = material.GetColor("_BaseColor");
                Color emissionColour      = material.GetColor("_EmissionColor");
                float metal               = material.GetFloat("_Metallic");
                float smoothness          = material.GetFloat("_Smoothness");
                float normalStrength      = material.GetFloat("_BumpScale");
                float occlussion          = material.GetFloat("_OcclusionStrength");
                float alphaClipping       = material.GetFloat("_Cutoff");
                Vector2 tiling            = material.GetTextureScale("_BaseMap");
                Vector2 offset            = material.GetTextureOffset("_BaseMap");
                bool emissionEnabled      = material.IsKeywordEnabled("_EMISSION");
                bool alphaClippingEnabled = material.GetFloat("_AlphaClip") == 1;

                // Assign properties to new material
                Material newMat                             = materialDataObjects.Material;
                RepetitionlessTextureDataSO textureData     = materialDataObjects.TextureDataSO;
                RepetitionlessMaterialDataSO materialData   = materialDataObjects.MaterialDataSO;
                RepetitionlessMaterialData baseMaterialData = materialData.Data[0].BaseMaterialData;

                textureData.SetupTextureDrawers();
                textureData.AVTexturesDrawer.UpdateTexture(colourTex, 0, 0, true);
                textureData.EMTexturesDrawer.UpdateTexture(metalTex, 0, 1, true);
                textureData.NSOTexturesDrawer.UpdateTexture(normalTex, 0, 0, true);
                textureData.NSOTexturesDrawer.UpdateTexture(occlussionTex, 0, 2, true);
                textureData.EMTexturesDrawer.UpdateTexture(emissionTex, 0, 0, true);
                textureData.Save();

                materialData.UpdateAssignedTextures(newMat, textureData, 0, 0);
                baseMaterialData.AlbedoTint = baseColour;
                baseMaterialData.EmissionColour = emissionColour;
                baseMaterialData.Metallic = metal;
                baseMaterialData.SmoothnessRoughness = smoothness;
                baseMaterialData.NormalScale = normalStrength;
                baseMaterialData.OcclussionStrength = occlussion;
                baseMaterialData.AlphaClipping = alphaClipping;
                baseMaterialData.TilingOffset = new Vector4(tiling.x, tiling.y, offset.x, offset.y);
                baseMaterialData.EmissionEnabled = emissionEnabled;
                materialData.Save();

                ESurfaceType surfaceType = alphaClippingEnabled ? ESurfaceType.Cutout : ESurfaceType.Opaque;
                RepetitionlessMaterialUtilities.SetSurface(newMat, surfaceType, renderPipeline);
                
                newMat.globalIlluminationFlags = material.globalIlluminationFlags;
            }
        }
    }
}
#endif