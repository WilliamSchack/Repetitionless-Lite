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

    [ContextMenu("Create Texture")]
    public void CreatePT()
    {
        int width = _texR.width;
        int height = _texR.height;

        // Assumes all textures are the same size
        RenderTexture rt = new RenderTexture(width, height, 0);
        rt.enableRandomWrite = true;
        rt.Create();

        Texture2D[] inputTextures = { _texR, _texG, _texB, _texA };
        int[] assignedTextures = new int[4];

        for (int i = 0; i < inputTextures.Length; i++) {
            bool textureAssigned = inputTextures[i];
            assignedTextures[i] = textureAssigned ? 1 : 0;

            if (!textureAssigned) {
                inputTextures[i] = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                inputTextures[i].SetPixel(0, 0, DEFAULT_COLOURS[i]);
                inputTextures[i].Apply();
            }
        }

        int kernel = _shader.FindKernel("CSMain");

        _shader.SetFloat("Width", width);
        _shader.SetFloat("Height", height);

        _shader.SetInts("Channels", 0, 0, 0, 0);

        _shader.SetInts("AssignedTextures", assignedTextures);

        _shader.SetTexture(kernel, "RTex", inputTextures[0]);
        _shader.SetTexture(kernel, "GTex", inputTextures[1]);
        _shader.SetTexture(kernel, "BTex", inputTextures[2]);
        _shader.SetTexture(kernel, "ATex", inputTextures[3]);

        _shader.SetTexture(kernel, "Result", rt);

        int groupsX = (width + (THREADS_X - 1)) / THREADS_X;
        int groupsY = (height + (THREADS_Y - 1)) / THREADS_Y;

        _shader.Dispatch(kernel, groupsX, groupsY, 1);

        // Blit to texture
        RenderTexture previousRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D outTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        outTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        outTex.Apply();

        RenderTexture.active = previousRT;
        rt.Release();

        // Save tex
        AssetDatabase.DeleteAsset(_outPath);
        
        string fullPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
        fullPath += _outPath;

        byte[] png = outTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, png);

        AssetDatabase.Refresh();

    }
}
