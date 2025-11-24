using UnityEditor;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    const int THREADS_X = 8;
    const int THREADS_Y = 8;

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
        int height = _texG.height;

        // Assumes all textures are the same size
        RenderTexture rt = new RenderTexture(width, height, 0);
        rt.enableRandomWrite = true;
        rt.Create();

        int kernel = _shader.FindKernel("CSMain");

        _shader.SetFloat("Width", width);
        _shader.SetFloat("Height", height);

        _shader.SetTexture(kernel, "RTex", _texR);
        _shader.SetTexture(kernel, "GTex", _texG);
        _shader.SetTexture(kernel, "BTex", _texB);
        _shader.SetTexture(kernel, "ATex", _texA);

        _shader.SetInts("Channels", 0, 0, 0, 0);

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
