namespace Repetitionless.Data
{
    public static class MaterialDataConstants
    {
        public const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";
        public const string PROPERTIES_FILE_NAME = "Properties.asset";
        public const string DEFAULT_VARIATION_TEXTURE_NAME = "repetitionless_VariationTexture_2048";

        public const int MATERIALS_PER_LAYER_COUNT = 3;

        public const int COMPRESSED_MATERIAL_VARIABLES_COUNT = 9;
        public const int COMPRESSED_LAYER_VARIABLES_COUNT = COMPRESSED_MATERIAL_VARIABLES_COUNT * MATERIALS_PER_LAYER_COUNT + 4;
    }
}