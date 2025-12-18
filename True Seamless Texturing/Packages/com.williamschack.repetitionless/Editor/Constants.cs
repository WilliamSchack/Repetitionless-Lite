namespace Repetitionless.Editor
{
    public static class Constants
    {
        public const string PACKAGE_NAME = "com.williamschack.repetitionless";
        public const string PACKAGE_DIR = "Packages/" + PACKAGE_NAME;

        public const string SAMPLES_PATH = PACKAGE_DIR + "/Samples~";
        public const string SAMPLES_PATH_ASSETS = SAMPLES_PATH + "/Assets";
        public const string SAMPLES_PATH_BIRP = SAMPLES_PATH + "/BIRP";
        public const string SAMPLES_PATH_URP = SAMPLES_PATH + "/URP";
        public const string SAMPLES_PATH_HDRP = SAMPLES_PATH + "/HDRP";

        public const string DOCUMENTATION_URL = "https://docs.wilschack.dev/repetitionless/";
        public const string LOCAL_DOCUMENTATION_PATH = PACKAGE_DIR + "/OfflineDocumentation.pdf";

        public const string GITHUB_URL = "https://github.com/WilliamSchack/Repetitionless-Issues";
        public const string GITHUB_NEW_ISSUE_URL = "https://github.com/WilliamSchack/Repetitionless-Issues/issues/new/choose";

        public const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";
        public const string PROPERTIES_FILE_NAME = "Properties.asset";
        public const string DEFAULT_VARIATION_TEXTURE_NAME = "repetitionless_VariationTexture_2048";

        public const int MATERIALS_PER_LAYER_COUNT = 3;

        public const int COMPRESSED_MATERIAL_VARIABLES_COUNT = 9;
        public const int COMPRESSED_LAYER_VARIABLES_COUNT = COMPRESSED_MATERIAL_VARIABLES_COUNT * MATERIALS_PER_LAYER_COUNT + 4;

        public const string TRIPLANAR_KEYWORD = "_REPETITIONLESS_TRIPLANAR"; 
    }
}