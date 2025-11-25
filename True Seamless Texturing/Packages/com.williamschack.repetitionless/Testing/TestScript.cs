using UnityEditor;
using UnityEngine;

public class TestScript : MonoBehaviour
{
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
        Texture2D[] inputTextures = { _texR, _texG, _texB, _texA };
        CreatePT(_shader, _outPath, inputTextures);
    }

    public static void CreatePT(ComputeShader shader, string outPath, Texture2D[] inputTextures)
    {
        Debug.Log(TextureIsNormal(inputTextures[0]));
        SetTextureToNormal(inputTextures[0]);
        Debug.Log(TextureIsNormal(inputTextures[0]));

        int width = inputTextures[0].width;
        int height = inputTextures[0].height;

        // Assumes all textures are the same size
        RenderTexture rt = new RenderTexture(width, height, 0);
        rt.enableRandomWrite = true;
        rt.Create();

        int[] assignedTextures = new int[4];

        for (int i = 0; i < inputTextures.Length; i++) {
            bool textureAssigned = inputTextures[i] != null;
            assignedTextures[i] = textureAssigned ? 1 : 0;

            if (!textureAssigned) {
                inputTextures[i] = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                inputTextures[i].SetPixel(0, 0, DEFAULT_COLOURS[i]);
                inputTextures[i].Apply();
            }
        }

        int kernel = shader.FindKernel("CSMain");

        shader.SetFloat("Width", width);
        shader.SetFloat("Height", height);

        shader.SetInts("NormalIndex", 0);
        shader.SetInts("FromChannels", 0, 1, 2, 3);

        shader.SetInts("AssignedTextures", assignedTextures);

        shader.SetTexture(kernel, "RTex", inputTextures[0]);
        shader.SetTexture(kernel, "GTex", inputTextures[1]);
        shader.SetTexture(kernel, "BTex", inputTextures[2]);
        shader.SetTexture(kernel, "ATex", inputTextures[3]);

        shader.SetTexture(kernel, "Result", rt);

        int groupsX = Mathf.CeilToInt(width  / (float)THREADS_X);
        int groupsY = Mathf.CeilToInt(height / (float)THREADS_Y);

        shader.Dispatch(kernel, groupsX, groupsY, 1);

        // Blit to texture
        RenderTexture previousRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D outTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        outTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        outTex.Apply();

        RenderTexture.active = previousRT;
        rt.Release();

        // Save tex
        AssetDatabase.DeleteAsset(outPath);
        
        string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        fullPath += outPath;

        byte[] png = outTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, png);
        AssetDatabase.Refresh();

    }
}
