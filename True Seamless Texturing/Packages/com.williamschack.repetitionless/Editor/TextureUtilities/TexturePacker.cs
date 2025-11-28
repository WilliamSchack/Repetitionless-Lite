#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.TextureUtilities
{
    public static class TexturePacker
    {
        public enum TextureChannel
        {
            R, G, B, A
        }

        [System.Serializable]
        public struct FromToChannel
        {
            public TextureChannel From;
            public TextureChannel To;

            public FromToChannel(TextureChannel from, TextureChannel to)
            {
                From = from;
                To = to;
            }
        }

        [System.Serializable]
        public struct TextureData
        {
            public Texture2D Texture;
            public bool Disabled;
            public bool NormalMap;
            public List<FromToChannel> FromToChannels;
        }

        private struct TextureDataGPU
        {
            public int NormalMap;
            public int SRGB;

            public int ChannelsUsedAmount;
            public Vector4 FromChannels;
            public Vector4 ToChannels;
        }

        private const string SHADER_RESOURCES_PATH = "repetitionless_CreatePackedTexture";

        private const int THREADS_X = 8;
        private const int THREADS_Y = 8;

        public static Texture2D PackTextures(TextureData[] textureData, Vector4 defaultColours)
        {
            if (textureData.Length == 0 || textureData.Length > 4) {
                Debug.LogError("Texture packing can only take 1-4 input textures");
                return null;
            }

            ComputeShader shader = Resources.Load<ComputeShader>(SHADER_RESOURCES_PATH);
            if (shader == null) {
                Debug.LogError("Could not find texture packing compute shader...");
                return null;
            }

            // Get resolution
            Vector2Int resolution = new Vector2Int(0, 0);
            for (int i = 0; i < textureData.Length; i++) {
                Texture2D currentTexture = textureData[i].Texture;
                if (currentTexture == null || textureData[i].Disabled)
                    continue;

                int currentWidth = currentTexture.width;
                int currentHeight = currentTexture.height;

                if (resolution.x == 0 && resolution.y == 0) {
                    resolution.x = currentWidth;
                    resolution.y = currentHeight;
                    continue;
                }

                if (currentWidth != resolution.x || currentHeight != resolution.y) {
                    Debug.LogError("All textures must have the same resolution to pack...");
                    return null;
                }
            }

            // Convert texture data to gpu friendly
            List<Texture2D> inputTextures = new List<Texture2D>();
            List<TextureDataGPU> textureDataGPU = new List<TextureDataGPU>();
            for (int i = 0; i < textureData.Length; i++) {
                TextureData currentTextureData = textureData[i];
                if (currentTextureData.Texture == null || currentTextureData.Disabled)
                    continue;

                inputTextures.Add(currentTextureData.Texture);

                TextureDataGPU gpuData;
                gpuData.NormalMap = currentTextureData.NormalMap ? 1 : 0;
                gpuData.SRGB = currentTextureData.Texture.isDataSRGB ? 1 : 0;

                int fromToChannelsCount = currentTextureData.FromToChannels.Count;
                if (fromToChannelsCount > 4) {
                    Debug.LogWarning("Only using the first 4 channels for " + currentTextureData.Texture.name);
                    fromToChannelsCount = 4;
                }

                gpuData.ChannelsUsedAmount = fromToChannelsCount;

                gpuData.FromChannels = new Vector4();
                gpuData.ToChannels = new Vector4();

                for (int j = 0; j < fromToChannelsCount; j++) {
                    FromToChannel currentFromToChannel = currentTextureData.FromToChannels[j];
                    gpuData.FromChannels[j] = (int)currentFromToChannel.From;
                    gpuData.ToChannels[j] = (int)currentFromToChannel.To;
                }

                textureDataGPU.Add(gpuData);
            }

            // No textures are assigned in the data
            if (inputTextures.Count == 0) {
                Debug.LogError("Input textures must be assigned...");
                return null;
            }

            // Create render texture
            RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 0);
            rt.enableRandomWrite = true;
            rt.Create();

            // Assign data
            int kernel = shader.FindKernel("CSMain");

            shader.SetFloat("Width", resolution.x);
            shader.SetFloat("Height", resolution.y);

            Texture2D dummyTexture = new Texture2D(1, 1);
            shader.SetTexture(kernel, "Tex0", inputTextures[0]);
            shader.SetTexture(kernel, "Tex1", inputTextures.Count > 1 ? inputTextures[1] : dummyTexture);
            shader.SetTexture(kernel, "Tex2", inputTextures.Count > 2 ? inputTextures[2] : dummyTexture);
            shader.SetTexture(kernel, "Tex3", inputTextures.Count > 3 ? inputTextures[3] : dummyTexture);

            ComputeBuffer textureDataBuffer = new ComputeBuffer(textureDataGPU.Count, System.Runtime.InteropServices.Marshal.SizeOf<TextureDataGPU>(), ComputeBufferType.Structured);
            textureDataBuffer.SetData(textureDataGPU);

            shader.SetBuffer(kernel, "Data", textureDataBuffer);
            shader.SetFloat("TextureCount", inputTextures.Count);

            shader.SetVector("DefaultColours", defaultColours);

            shader.SetTexture(kernel, "Result", rt);

            // Pack textures
            int groupsX = Mathf.CeilToInt(resolution.x  / (float)THREADS_X);
            int groupsY = Mathf.CeilToInt(resolution.y / (float)THREADS_Y);

            shader.Dispatch(kernel, groupsX, groupsY, 1);

            textureDataBuffer.Release();

            // Copy packed texture to texture2D
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D outTex = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
            outTex.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
            outTex.Apply();

            RenderTexture.active = previousRT;
            rt.Release();

            return outTex;
        }
    }
}
#endif