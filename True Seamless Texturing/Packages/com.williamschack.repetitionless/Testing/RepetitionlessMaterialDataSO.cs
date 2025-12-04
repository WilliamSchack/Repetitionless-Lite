using UnityEngine;

[CreateAssetMenu]
public class RepetitionlessMaterialDataSO : ScriptableObject
{
    // Each material = 9, Extra Layer Variables = 4
    private const int COMPRESSED_VARIABLES_COUNT = 9 * 3 + 4;
    private const TextureFormat DATA_TEXTURE_FORMAT = TextureFormat.RGBAHalf;

    private int _layerCount = 1;

    private Texture2D _dataTexture;
    private Color[] _data;

    public RepetitionlessMaterialDataSO(int layerCount)
    {
        _layerCount = layerCount;
    }

    public Texture2D CreateTexture()
    {
        Texture2D texture = new Texture2D(COMPRESSED_VARIABLES_COUNT, _layerCount, DATA_TEXTURE_FORMAT, false);
        texture.SetPixels(_data);
        return texture;
    }

    public Texture2D UpdateTexture(Texture2D texture, Color value, int x, int y)
    {
        texture.SetPixel(x, y, value);
        return texture;
    }

    public Texture2D UpdateVariable(Color value, int propertyIndex, int layerIndex)
    {
        // x + y * width
        int dataIndex = propertyIndex + layerIndex * COMPRESSED_VARIABLES_COUNT;
        _data[dataIndex] = value;

        return CreateTexture();
    }
}