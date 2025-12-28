#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace Repetitionless.Editor.CustomWindows
{
    using GUIUtilities;

    /// <summary>
    /// The welcome window that is shown when first installing the package
    /// </summary>
    public class WelcomeWindow : EditorWindow
    {
        private const string PACKAGE_PATH = Constants.PACKAGE_PATH + "/package.json";

        private const string LOGO_FILE_NAME = "repetitionless_WelcomeLogo";
        private const int LOGO_HEIGHT = 60;
        private const int LOGO_PADDING = 3;

        private UnityEditor.PackageManager.PackageInfo _packageInfo;

        private Texture _logoTextureDark;
        private Texture _logoTextureLight;
        private Color _logoBackgroundDarkColour;
        private Color _logoBackgroundLightColour;

        private GUIStyle _headerStyle;
        private GUIStyle _boldLabelStyle;
        private GUIStyle _buttonStyle;

        private bool _stylesSetup = false;

        /// <summary>
        /// Opens the window
        /// </summary>
        [MenuItem("Window/Repetitionless/Open Window")]
        public static void Open()
        {
            WelcomeWindow window = GetWindow<WelcomeWindow>(false, "Repetitionless ");
            window.Show();
        }

        private void CreateGUI()
        {
            _packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(PACKAGE_PATH);

            _logoTextureDark = Resources.Load<Texture>($"{LOGO_FILE_NAME}_Dark");
            _logoTextureLight = Resources.Load<Texture>($"{LOGO_FILE_NAME}_Light");
            _logoBackgroundDarkColour = new Color(20 / 256f, 20 / 256f, 20 / 256f);
            _logoBackgroundLightColour = new Color(240 / 256f, 240 / 256f, 240 / 256f);
        }

        private void SetupStyles()
        {
            _headerStyle = new GUIStyle("label");
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.fontSize = 30;
            _headerStyle.wordWrap = true;

            _boldLabelStyle = new GUIStyle("label");
            _boldLabelStyle.fontStyle = FontStyle.Bold;
            _boldLabelStyle.wordWrap = true;

            _buttonStyle = new GUIStyle("button");
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.fontSize = 14;
        }

        private void OnGUI()
        {
            // Named styles must be created in OnGUI
            if (!_stylesSetup) {
                SetupStyles();
                _stylesSetup = true;
            }

            GUIUtilities.BeginBackgroundVertical();

            DrawLogo();
            GUILayout.Space(10);

            float buttonMinWidth = position.width / 2 - 15;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Online Documentation",  GUILayout.MinWidth(buttonMinWidth))) OpenDocumentation(false);
            if (GUILayout.Button("Offline Documentation", GUILayout.MinWidth(buttonMinWidth))) OpenDocumentation(true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Github Issues", GUILayout.MinWidth(buttonMinWidth))) Application.OpenURL(Constants.GITHUB_URL);
            if (GUILayout.Button("Report Issue",  GUILayout.MinWidth(buttonMinWidth))) Application.OpenURL(Constants.GITHUB_NEW_ISSUE_URL);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Import Samples")) ImportSamples();

            GUILayout.Space(20);

            GUILayout.Label("Welcome to repetitionless! To get started view the getting started page in the documentation for instructions on how to use the asset, or import the samples for examples.", _boldLabelStyle);

            GUILayout.FlexibleSpace();

            GUIUtilities.EndBackgroundVertical();

            EditorGUILayout.HelpBox("Thanks for using Repetitionless, please consider leaving a review on the Asset Store to support development, any feedback is appreciated!", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Asset Store", GUILayout.MinWidth(buttonMinWidth))) Application.OpenURL(Constants.ASSET_STORE_REVIEW_URL);
            if (GUILayout.Button("Itch.io",     GUILayout.MinWidth(buttonMinWidth))) Application.OpenURL(Constants.ASSET_ITCH_URL);
            EditorGUILayout.EndHorizontal();

            GUIUtilities.BeginBackgroundHorizontal();
    
            GUILayout.Label($"v{_packageInfo.version}", _boldLabelStyle);

            GUILayout.FlexibleSpace();
            GUIUtilities.EndBackgroundHorizontal();
        }

        private void DrawLogo()
        {
            bool darkMode = EditorGUIUtility.isProSkin;
            Texture texture = darkMode ? _logoTextureDark : _logoTextureLight;
            Color backgroundColour = darkMode ? _logoBackgroundDarkColour : _logoBackgroundLightColour;

            Rect logoBackgroundRect = GUILayoutUtility.GetRect(1, LOGO_HEIGHT);
            EditorGUI.DrawRect(logoBackgroundRect, backgroundColour);

            Rect logoRect = logoBackgroundRect;
            logoRect.yMin += LOGO_PADDING;
            logoRect.yMax -= LOGO_PADDING;
            
            GUI.DrawTexture(logoRect, texture, ScaleMode.ScaleToFit);
        }

        private string ProjectPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        }

        private void OpenDocumentation(bool offline)
        {
            string url = "";
            if (offline) {
                url = $"{ProjectPath()}{Constants.LOCAL_DOCUMENTATION_PATH}";
            } else {
                url = Constants.DOCUMENTATION_URL;
            }

            Application.OpenURL(url);
        }

        private void ImportSamples()
        {
            // Get paths
            string projectPath = ProjectPath();

            string samplesCorePathFull = $"{projectPath}{Constants.SAMPLES_PATH_ASSETS}";
            string samplesPipelinePathFull = projectPath;

            string targetBasePath = $"Assets/Samples/Repetitionless/{_packageInfo.version}";
            string targetBasePathFull = $"{projectPath}{targetBasePath}";

            string targetCorePathFull = $"{targetBasePath}/Core Sample Assets";
            string targetPipelinePath = $"{targetBasePath}/";

            RenderPipelineAsset currentPipeline = GraphicsSettings.currentRenderPipeline;
            if (currentPipeline == null) { // Built-In RP
                samplesPipelinePathFull += Constants.SAMPLES_PATH_BIRP;
                targetPipelinePath += "BIRP Examples";
            } else if (currentPipeline.GetType().Name.Contains("UniversalRenderPipeline")) {
                samplesPipelinePathFull += Constants.SAMPLES_PATH_URP;
                targetPipelinePath += "URP Examples";
            } else if (currentPipeline.GetType().Name.Contains("HDRenderPipeline")) {
                samplesPipelinePathFull += Constants.SAMPLES_PATH_HDRP;
                targetPipelinePath += "HDRP Examples";
            } else return;

            string targetPipelinePathFull = $"{projectPath}{targetPipelinePath}";

            // Create samples folder
            bool anyFilesCreated = false;

            if (!Directory.Exists(targetBasePathFull)) {
                Directory.CreateDirectory(targetBasePathFull);
                anyFilesCreated = true;
            }

            // Copy samples
            if (!Directory.Exists(targetCorePathFull)) {
                FileUtil.CopyFileOrDirectory(samplesCorePathFull, targetCorePathFull);
                anyFilesCreated = true;
            }

            if (!Directory.Exists(targetPipelinePathFull)) {
                FileUtil.CopyFileOrDirectory(samplesPipelinePathFull, targetPipelinePathFull);
                anyFilesCreated = true;
            }

            if (anyFilesCreated)
                AssetDatabase.Refresh();
            else {
                // Ping object in project window if already created
                Object samplesFolderObject = AssetDatabase.LoadAssetAtPath<Object>(targetPipelinePath);
                EditorGUIUtility.PingObject(samplesFolderObject);
            }
        }
    }
}
#endif