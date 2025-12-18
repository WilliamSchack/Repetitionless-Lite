namespace Repetitionless.Editor
{
    /// <summary>
    /// Contains the main constants used for the Repetitionless asset
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The package name
        /// </summary>
        public const string PACKAGE_NAME = "com.williamschack.repetitionless";
        /// <summary>
        /// The package path relative to the project path
        /// </summary>
        public const string PACKAGE_PATH = "Packages/" + PACKAGE_NAME;

        /// <summary>
        /// The samples path relative to the project path
        /// </summary>
        public const string SAMPLES_PATH = PACKAGE_PATH + "/Samples~";
        /// <summary>
        /// The core sample assets path relative to the project path
        /// </summary>
        public const string SAMPLES_PATH_ASSETS = SAMPLES_PATH + "/Assets";
        /// <summary>
        /// The Built-In samples path relative to the project path
        /// </summary>
        public const string SAMPLES_PATH_BIRP = SAMPLES_PATH + "/BIRP";
        /// <summary>
        /// The URP samples path relative to the project path
        /// </summary>
        public const string SAMPLES_PATH_URP = SAMPLES_PATH + "/URP";
        /// <summary>
        /// The HDRP samples path relative to the project path
        /// </summary>
        public const string SAMPLES_PATH_HDRP = SAMPLES_PATH + "/HDRP";

        /// <summary>
        /// The online documentation url
        /// </summary>
        public const string DOCUMENTATION_URL = "https://docs.wilschack.dev/repetitionless/";
        /// <summary>
        /// The local documentation path relative to the project path
        /// </summary>
        public const string LOCAL_DOCUMENTATION_PATH = PACKAGE_PATH + "/OfflineDocumentation.pdf";

        /// <summary>
        /// The github issues repo url
        /// </summary>
        public const string GITHUB_URL = "https://github.com/WilliamSchack/Repetitionless-Issues";
        /// <summary>
        /// Link to submit a new issue in the github repo
        /// </summary>
        public const string GITHUB_NEW_ISSUE_URL = "https://github.com/WilliamSchack/Repetitionless-Issues/issues/new/choose";

        /// <summary>
        /// The texture data asset file name that will be stored along side a material
        /// </summary>
        public const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";
        /// <summary>
        /// The material properties asset file name that will be stored along side a material
        /// </summary>
        public const string PROPERTIES_FILE_NAME = "Properties.asset";
        
        /// <summary>
        /// The properties texture file name that will be stored along side a material
        /// </summary>
        public const string PROPERTIES_TEXTURE_ASSET_NAME = "PropertiesTexture.asset";
        /// <summary>
        /// The array assigned textures texture file name that will be stored along side a material
        /// </summary>
        public const string ARRAY_ASSIGNED_TEXTURES_ASSET_NAME = "AssignedTextures.asset";

        /// <summary>
        /// The default variation texture file name with a resolution of 2048x2048
        /// </summary>
        public const string DEFAULT_VARIATION_TEXTURE_NAME_2K = "repetitionless_VariationTexture_2048";
        /// <summary>
        /// The default variation texture file name with a resolution of 4096x4096
        /// </summary>
        public const string DEFAULT_VARIATION_TEXTURE_NAME_4K = "repetitionless_VariationTexture_4096";

        /// <summary>
        /// How many materials are used per layer
        /// </summary>
        public const int MATERIALS_PER_LAYER_COUNT = 3;

        /// <summary>
        /// Amount of compressed variables per material
        /// </summary>
        public const int COMPRESSED_MATERIAL_VARIABLES_COUNT = 9;
        /// <summary>
        /// Amount of compressed variables per layer
        /// </summary>
        public const int COMPRESSED_LAYER_VARIABLES_COUNT = COMPRESSED_MATERIAL_VARIABLES_COUNT * MATERIALS_PER_LAYER_COUNT + 4;

        /// <summary>
        /// The triplanar keyword for repetitionless materials
        /// </summary>
        public const string TRIPLANAR_KEYWORD = "_REPETITIONLESS_TRIPLANAR"; 
    }
}