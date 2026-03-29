using UnityEngine;

namespace Repetitionless.Editor
{
    internal static class Constants
    {
        public const string PACKAGE_ID = "345604";
        public const string ASSET_STORE_URL = "https://assetstore.unity.com/packages/slug/" + PACKAGE_ID;
        public const string ASSET_STORE_REVIEW_URL = ASSET_STORE_URL + "#reviews";
        public const string ASSET_ITCH_URL = "https://wilschack.itch.io/repetitionless";

        public const string PACKAGE_NAME = "com.williamschack.repetitionless";
        public const string PACKAGE_PATH = "Packages/" + PACKAGE_NAME;
        public const string LIBRARY_PATH = "Library/" + PACKAGE_NAME;

        public const string SHADER_FOLDER = "Repetitionless/";
        public const string SHADER_MATERIAL_NAME_REGULAR = "Repetitionless";
        public const string SHADER_MATERIAL_NAME_TERRAIN = "RepetitionlessTerrain";
        public const string SHADER_FOLDER_BIRP = "BIRP/";
        public const string SHADER_FOLDER_URP = "URP/";
        public const string SHADER_FOLDER_HDRP = "HDRP/";

        public const string SAMPLES_PATH = PACKAGE_PATH + "/Samples~";
        public const string SAMPLES_PATH_ASSETS = SAMPLES_PATH + "/Assets";
        public const string SAMPLES_PATH_BIRP = SAMPLES_PATH + "/BIRP";
        public const string SAMPLES_PATH_URP = SAMPLES_PATH + "/URP";
        public const string SAMPLES_PATH_HDRP = SAMPLES_PATH + "/HDRP";

        public const string DOCUMENTATION_URL = "https://docs.wilschack.dev/repetitionless/";
        public const string LOCAL_DOCUMENTATION_PATH = PACKAGE_PATH + "/OfflineDocumentation.pdf";

        public const string UNITY_FORUM_URL = "https://discussions.unity.com/t/repetitionless-texture-tiling-remover/1705964";

        public const string GITHUB_URL = "https://github.com/WilliamSchack/Repetitionless-Issues";
        public const string GITHUB_NEW_ISSUE_URL = GITHUB_URL + "/issues/new/choose";
        public const string GITHUB_TAGS_URL = "https://api.github.com/repos/WilliamSchack/Repetitionless-Issues/tags";

        public const string SUPPORT_EMAIL_URL = "mailto:support@wilschack.dev";

        public const string DISCORD_INVITE_LINK_ANNOUNCEMENTS = "https://discord.gg/2NTvBDB3xq";
        public const string DISCORD_INVITE_LINK_SUPPORT = "https://discord.gg/ebtsz3CstU";

        public const string TEXTURE_DATA_FILE_NAME = "TextureData.asset";
        public const string PROPERTIES_FILE_NAME = "Properties.asset";
        public const string TERRAIN_DATA_FILE_NAME = "TerrainData.asset";

        public const string PROPERTIES_TEXTURE_ASSET_NAME = "PropertiesTexture.asset";
        public const string ARRAY_ASSIGNED_TEXTURES_ASSET_NAME = "AssignedTextures.asset";

        public const string DEFAULT_VARIATION_TEXTURE_NAME_4K = "repetitionless_VariationTexture_4096";
        public const string DEFAULT_VARIATION_TEXTURE_NAME_2K = "repetitionless_VariationTexture_2048";

        public const string NOISE_TEXTURE_NAME_4K = "repetitionless_NoiseTexture_4096";
        public const string NOISE_TEXTURE_NAME_1K = "repetitionless_NoiseTexture_1024";

        public const int MAX_LAYERS_REGULAR = 1;
        public const int MAX_LAYERS_TERRAIN = 32;

        public static readonly Vector4 DEFAULT_TILING_OFFSET_TERRAIN = new Vector4(100, 100, 0, 0);

        public const int MATERIALS_PER_LAYER_COUNT = 3;
        public const int COMPRESSED_MATERIAL_VARIABLES_COUNT = 9;
        public const int COMPRESSED_LAYER_VARIABLES_COUNT = COMPRESSED_MATERIAL_VARIABLES_COUNT * MATERIALS_PER_LAYER_COUNT + 5;

        public const string NOISE_TEXTURE_KEYWORD = "_REPETITIONLESS_NOISE_TEXTURE";
        public const string TRIPLANAR_KEYWORD = "_REPETITIONLESS_TRIPLANAR";
        public const string SPECULAR_HIGHLIGHTS_OFF_KEYWORD = "_SPECULARHIGHLIGHTS_OFF";
        public const string ENVIRONMENT_REFLECTIONS_OFF_KEYWORD = "_ENVIRONMENTREFLECTIONS_OFF";
    }
}