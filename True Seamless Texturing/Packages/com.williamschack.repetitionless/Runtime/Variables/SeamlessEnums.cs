namespace Repetitionless.Runtime.Variables
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
    /// Available UV spaces
    /// </summary>
    public enum EUVSpace
    {
        /// <summary>
        /// Local uvs from the object
        /// </summary>
        Local,

        /// <summary>
        /// World uvs from the world position
        /// </summary>
        World
    }

    /// <summary>
    /// Available noise qualities
    /// </summary>
    public enum ENoiseQuality
    {
        /// <summary>
        /// Dynamically creates the noise on the fly<br />
        /// Required to adjust the Noise Angle Offset
        /// </summary>
        High,

        /// <summary>
        /// Uses the 4k pre-rendered noise texture
        /// </summary>
        Medium,

        /// <summary>
        /// Uses the 1k pre-rendered noise texture
        /// </summary>
        Low
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
        CustomTexture,
        
        /// <summary>
        /// Uses a defined colour from the vertex data
        /// </summary>
        VertexColour
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