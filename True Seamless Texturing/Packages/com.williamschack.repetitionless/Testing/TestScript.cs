using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public enum TextureChannel
    {
        R, G, B, A
    }

    public struct FromToChannel
    {
        public TextureChannel FromChannel;
        public TextureChannel ToChannel;
    }

    public struct TextureData
    {
        public Texture2D Texture;
        public bool NormalMap;
        public List<FromToChannel> FromToChannels;
    }

    struct TextureDataGPU
    {
        public int NormalMap;
        public int SRGB;

        public int ChannelsUsedAmount;
        public Vector4 FromChannels;
        public Vector4 ToChannels;
    }

    private const int THREADS_X = 8;
    private const int THREADS_Y = 8;

    private static readonly Color[] DEFAULT_COLOURS = { Color.black, Color.white, Color.white, Color.black };
 
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private string _outPath;

    [Space(20)]

    [SerializeField] private Texture2D _texR;
    [SerializeField] private Texture2D _texG;
    [SerializeField] private Texture2D _texB;
    [SerializeField] private Texture2D _texA;

    private static TextureImporter GetTextureImporter(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

        return importer;
    }

    private static bool TextureIsNormal(Texture2D texture)
    {
        TextureImporter importer = GetTextureImporter(texture);
        
        if (importer == null) return false;
        return importer.textureType == TextureImporterType.NormalMap;
    }

    private static void SetTextureToNormal(Texture2D texture)
    {
        TextureImporter importer = GetTextureImporter(texture);
        if (importer == null) return;

        importer.textureType = TextureImporterType.NormalMap;
        importer.SaveAndReimport();
    }

    [ContextMenu("Create Texture")]
    public void CreatePTInspector()
    {
        List<TextureData> textureData = new List<TextureData>() {
            new TextureData() {
                Texture = _texR,
                NormalMap = true,
                FromToChannels = new List<FromToChannel>() {
                    new FromToChannel() {
                        FromChannel = TextureChannel.R,
                        ToChannel = TextureChannel.R
                    },
                    new FromToChannel() {
                        FromChannel = TextureChannel.G,
                        ToChannel = TextureChannel.G
                    },
                    new FromToChannel() {
                        FromChannel = TextureChannel.B,
                        ToChannel = TextureChannel.B
                    }
                }
            }//,
            //new TextureData() {
            //    Texture = _texG,
            //    NormalMap = false,
            //    FromToChannels = new List<FromToChannel>() {
            //        new FromToChannel() {
            //            FromChannel = TextureChannel.R,
            //            ToChannel = TextureChannel.B
            //        },
            //    }
            //},
            //new TextureData() {
            //    Texture = _texB,
            //    NormalMap = false,
            //    FromToChannels = new List<FromToChannel>() {
            //        new FromToChannel() {
            //            FromChannel = TextureChannel.R,
            //            ToChannel = TextureChannel.A
            //        },
            //    }
            //}
        };

        Texture2D packedTexture = PackTextures(_shader, textureData);

        Debug.Log(packedTexture);

        // Save tex
        AssetDatabase.DeleteAsset(_outPath);
        
        string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        fullPath += _outPath;

        byte[] png = packedTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, png);
        AssetDatabase.Refresh();
    }

    public static Texture2D PackTextures(ComputeShader shader, List<TextureData> textureData)
    {
        if (textureData.Count == 0 || textureData.Count > 4) {
            Debug.LogError("Texture packing can only take 1-4 input textures");
            return null;
        }

        // Get resolution
        Vector2Int resolution = new Vector2Int(0, 0);
        for (int i = 0; i < textureData.Count; i++) {
            Texture2D currentTexture = textureData[i].Texture;
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
        for (int i = 0; i < textureData.Count; i++) {
            TextureData currentTextureData = textureData[i];

            inputTextures.Add(currentTextureData.Texture);

            TextureDataGPU gpuData;
            gpuData.NormalMap = currentTextureData.NormalMap ? 1 : 0;
            gpuData.SRGB = currentTextureData.Texture.isDataSRGB ? 1 : 0;

            gpuData.ChannelsUsedAmount = currentTextureData.FromToChannels.Count;

            int fromToChannelsCount = currentTextureData.FromToChannels.Count;
            if (fromToChannelsCount > 4) {
                Debug.LogWarning("Only using the first 4 channels for " + currentTextureData.Texture.name);
                fromToChannelsCount = 4;
            }

            gpuData.FromChannels = new Vector4();
            gpuData.ToChannels = new Vector4();

            for (int j = 0; j < fromToChannelsCount; j++) {
                FromToChannel currentFromToChannel = currentTextureData.FromToChannels[j];
                gpuData.FromChannels[j] = (int)currentFromToChannel.FromChannel;
                gpuData.ToChannels[j] = (int)currentFromToChannel.ToChannel;
            }

            textureDataGPU.Add(gpuData);
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

        ComputeBuffer textureDataBuffer = new ComputeBuffer(textureDataGPU.Count, Marshal.SizeOf<TextureDataGPU>(), ComputeBufferType.Structured);
        textureDataBuffer.SetData(textureDataGPU);

        shader.SetBuffer(kernel, "TexturesData", textureDataBuffer);
        shader.SetFloat("TextureCount", inputTextures.Count);

        shader.SetVector("DefaultRColour", DEFAULT_COLOURS[0]);
        shader.SetVector("DefaultGColour", DEFAULT_COLOURS[1]);
        shader.SetVector("DefaultBColour", DEFAULT_COLOURS[2]);
        shader.SetVector("DefaultAColour", DEFAULT_COLOURS[3]);

        shader.SetTexture(kernel, "Result", rt);

        // Pack textures
        int groupsX = Mathf.CeilToInt(resolution.x  / (float)THREADS_X);
        int groupsY = Mathf.CeilToInt(resolution.y / (float)THREADS_Y);

        shader.Dispatch(kernel, groupsX, groupsY, 1);

        // Blit packed texture to texture2D
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