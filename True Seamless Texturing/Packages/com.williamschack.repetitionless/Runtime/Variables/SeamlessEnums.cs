namespace Repetitionless.Variables
{
    /// <summary>
    /// Available surface types for Repetitionless Materials
    /// </summary>
    public enum ESurfaceType
    {
        /// <summary>
        /// Opaque surface with no transparency
        /// </summary>
        Opaque,

        /// <summary>
        /// TransparentCutout surface (Opaque with alpha clipping)
        /// </summary>
        Cutout,

        /// <summary>
        /// Transparent surface
        /// </summary>
        Transparent
    }
    
    /// <summary>
    /// Available mask texture types for Repetitionless Materials
    /// </summary>
    public enum ETextureType
    {
        /// <summary>
        /// Procedurally generated with perlin noise
        /// </summary>
        PerlinNoise,

        /// <summary>
        /// Procedurally generated with simplex noise
        /// </summary>
        SimplexNoise,

        /// <summary>
        /// Uses a custom input texture
        /// </summary>
        CustomTexture
    }

    /// <summary>
    /// Available distance blend modes for Repetitionless Materials
    /// </summary>
    public enum EDistanceBlendMode
    {
        /// <summary>
        /// Blends into the same material with a different tiling and offset
        /// </summary>
        TilingOffset,

        /// <summary>
        /// Blends into a different material
        /// </summary>
        Material
    }
}