#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Compression;
using Repetitionless.Editor.GUIUtilities;

public class DebugTextureWindow : EditorWindow
{
    private Texture2D _texture;
    private ushort[] _textureData;

    private Vector2 _scrollPos;

    [MenuItem("Window/Repetitionless/Debug Texture")]
    public static void Open()
    {
        DebugTextureWindow window = GetWindow<DebugTextureWindow>(false, "Debug Texture");
        window.Show();
    }

    private void OnGUI()
    {
        _texture = (Texture2D)EditorGUILayout.ObjectField(_texture, typeof(Texture2D), false);

        if (GUILayout.Button("Read assigned textures texture"))
            ReadTextureDetails();

        if (_textureData == null)
            return;

        GUILayout.Space(10);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        int channels = 4;
        for (int i = 0; i < _textureData.Length / channels; i++) {
            GUIUtilities.DrawHeaderLabelLarge($"Pixel {i}");

            int dataIndex = i * channels;
            ushort[] data = new ushort[channels];
            for (int j = 0; j < channels; j++) {
                data[j] = _textureData[dataIndex + j];
            }

            for (int x = 0; x < data.Length; x++) {
                int value = data[x];

                GUILayout.Label($"Channel {x}: {value}");

                bool[] bools = BooleanCompression.GetValues(value, 16);
                for (int b = 0; b < bools.Length; b++) {
                    GUILayout.Label($"\t{b}: {(bools[b] ? "True" : "False")}");
                }

                GUILayout.Space(5);
            }

            GUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();
    }

    private void ReadTextureDetails()
    {
        if (_texture == null)
            return;

        _textureData = _texture.GetPixelData<ushort>(0).ToArray();
    }
}
#endif