using UnityEditor.PackageManager;

namespace Repetitionless.Editor.Config
{
    /// <summary>
    /// Used to retrieve the package info for repetitionless
    /// </summary>
    public static class RepetitionlessPackageInfo
    {
        private const string PACKAGE_INFO_PATH = Constants.PACKAGE_PATH + "/package.json";

        private static PackageInfo _packageInfoCache;

        /// <summary>
        /// The repetitionless package info
        /// </summary>
        public static PackageInfo Info => _packageInfoCache ??= LoadPackageInfo();

        private static PackageInfo LoadPackageInfo()
        {
            return PackageInfo.FindForAssetPath(PACKAGE_INFO_PATH);
        }
    }
}