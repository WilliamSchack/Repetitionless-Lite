namespace Repetitionless.Variables
{
    /// <summary>
    /// Available surface types for Repetitionless Materials
    /// </summary>
    public enum ESurfaceType
    {
        Opaque,
        Cutout,
        Transparent
    }
    
    /// <summary>
    /// Available mask texture types for Repetitionless Materials
    /// </summary>
    public enum ETextureType
    {
        PerlinNoise,
        SimplexNoise,
        CustomTexture
    }

    /// <summary>
    /// Available distance blend modes for Repetitionless Materials
    /// </summary>
    public enum EDistanceBlendMode
    {
        TilingOffset,
        Material
    }
}