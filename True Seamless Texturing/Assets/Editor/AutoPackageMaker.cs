#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

public static class AutoPackageMaker
{
    private const string REPETITIONLESS_PACKAGE_DISPLAY_NAME = "Repetitionless";
    private const string REPETITIONLESS_PACKAGE_NAME = "com.williamschack.repetitionless";
    private const string REPETITIONLESS_PACKAGE_DIR = "/Packages/" + REPETITIONLESS_PACKAGE_NAME + "/";

    private const string ENV_IN_UNITY_EMAIL = "UNITY_EMAIL";
    private const string ENV_IN_UNITY_PASSWORD = "UNITY_PASSWORD";
    private const string ENV_IN_PACKAGE_VERSION = "PACKAGE_VERSION";
    private const string ENV_OUT_PACKAGE_PATH = "PACKAGE_PATH";

    private static object _hybridWorkflow = null;
    private static string _packagePath = "";

    private static Type _uploaderWindowType;
    private static EditorWindow _uploaderWindow;

    private static EditorApplication.CallbackFunction _uploadProgressCallback;

#region Reflection Helpers
    private static T GetPrivateField<T>(object obj, string fieldName, bool inBaseClass = false)
    {
        var type = obj.GetType();
        if (inBaseClass) type = type.BaseType;

        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            Debug.LogError($"Field '{fieldName}' not found on type {obj.GetType().FullName}");
        return (T)field.GetValue(obj);
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            Debug.LogError($"Field '{fieldName}' not found on type {obj.GetType().FullName}");
        field.SetValue(obj, value);
    }

    private static object CallPrivateFunction(object obj, string methodName, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
            Debug.LogError($"Method '{methodName}' not found on type {obj.GetType().FullName}");
        return method.Invoke(obj, args);
    }
#endregion

    private static void Finish()
    {
        // Only quit in batchmode
        if (!Application.isBatchMode)
            return;

        EditorApplication.Exit(0);
    }

    [MenuItem("Repetitionless/Reflection Shenanigans", false, 1)]
    public static void CreateAndUpload()
    {
        Debug.Log("Starting export and upload...");

        if (_uploaderWindowType == null) {
            _uploaderWindowType =
                AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    t.FullName == "AssetStoreTools.Uploader.UploaderWindow");

        }
        
        _uploaderWindow = EditorWindow.GetWindow(_uploaderWindowType);
        _uploaderWindow.Show();

        Debug.Log($"Got window: {_uploaderWindow}");

        // Remove cached window to ensure variables are initialized
        object cachingService = GetPrivateField<object>(_uploaderWindow, "_cachingService");
        VisualElement cachedWindow = GetPrivateField<VisualElement>(cachingService, "_cachedUploaderWindow");

        if (cachedWindow != null) {
            cachedWindow.RemoveFromHierarchy();
            cachedWindow.Clear();

            SetPrivateField(cachingService, "_cachedUploaderWindow", null);
            CallPrivateFunction(_uploaderWindow, "SetupWindow", null);
        }

        // Subscribe to login authentication
        object loginView = GetPrivateField<object>(_uploaderWindow, "_loginView");
        var loginViewType = loginView.GetType();

        Action<object> handler = user => UploaderAuthenticated();
        var evt = loginViewType.GetEvent("OnAuthenticated", BindingFlags.Instance | BindingFlags.Public);

        Delegate typedHandler = Delegate.CreateDelegate(evt.EventHandlerType, handler.Target, handler.Method, false);
        evt.AddEventHandler(loginView, typedHandler);

        // Login
        object authenticationService = GetPrivateField<object>(loginView, "_authenticationService");
        var authenticationServiceType = authenticationService.GetType();

        var cloudAuthenticationAvailableMethod = authenticationServiceType.GetMethod("CloudAuthenticationAvailable");
        bool cloudAuthAvailable = (bool)cloudAuthenticationAvailableMethod.Invoke(authenticationService, new object[] { null, null });

        if (cloudAuthAvailable) {
            Debug.Log("Cloud authentication available, logging in...");

            CallPrivateFunction(loginView, "LoginWithCloudToken");
            return;

        }

        Debug.Log("Cloud authentication not available, getting login from Environment Variables...");

        string unityEmail = Environment.GetEnvironmentVariable(ENV_IN_UNITY_EMAIL);
        string unityPassword = Environment.GetEnvironmentVariable(ENV_IN_UNITY_PASSWORD);

        TextField emailField = GetPrivateField<TextField>(loginView, "_emailField");
        TextField passwordField = GetPrivateField<TextField>(loginView, "_passwordField");

        emailField.value = unityEmail;
        passwordField.value = unityPassword;

        CallPrivateFunction(loginView, "LoginWithCredentials");
    }

    private static void UploaderAuthenticated()
    {
        Debug.Log("Uploader Authenticated...");

        object accountToolbar = GetPrivateField<object>(_uploaderWindow, "_accountToolbar");
        var toolbarType = accountToolbar.GetType();

        var onRefreshField = toolbarType.GetField("OnRefresh", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var onRefreshDelegate = onRefreshField.GetValue(accountToolbar) as Func<Task>;

        if (onRefreshDelegate != null) {
            // Call the refresh logic and await it
            Task refreshTask = onRefreshDelegate.Invoke();
            refreshTask.ContinueWith(t =>
            {
                if (t.Exception != null)
                    Debug.LogError(t.Exception);
                else
                    PackagesRefreshed();
            });
        }
    }

    private static void PackagesRefreshed()
    {
        Debug.Log("Found Packages...");

        object packageListView = GetPrivateField<object>(_uploaderWindow, "_packageListView");
        ScrollView packagesScrollView = GetPrivateField<ScrollView>(packageListView, "_packageScrollView");

        // First element will be a draft if there is one
        object packageGroupElement = packagesScrollView.Children().ElementAt(0);

        if (!packageGroupElement.GetType().Name.Contains("PackageGroupElement")){
            Debug.LogError("No draft exists, please create a draft to upload");
            Finish();

            return;
        }
        
        var groupNameProp = packageGroupElement.GetType().GetProperty("Name");
        string groupName = groupNameProp.GetValue(packageGroupElement) as string;

        if (groupName != "Draft") {
            Debug.LogError("No draft exists, please create a draft to upload");
            Finish();

            return;
        }

        // Find the repetitionless package
        IList packageElements = GetPrivateField<IList>(packageGroupElement, "_packageElements");

        foreach (object packageElement in packageElements) {
            var package = GetPrivateField<object>(packageElement, "_package");
            var packageNameProp = package.GetType().GetProperty("Name");
            string packageName = packageNameProp.GetValue(package) as string;

            if (packageName != REPETITIONLESS_PACKAGE_DISPLAY_NAME)
                continue;

            UploadPackage(packageElement);
            return;
        }
    }

    // Must be called from the main thread
    private static void UploadPackage(object packageElement)
    {
        Debug.Log("Uploading Package...");

        // Toggle must be executed in the main thread, delayCall is called from the main thread
        EditorApplication.delayCall += () => {
            // Setup element
            CallPrivateFunction(packageElement, "Toggle", null);
            object contentElement = GetPrivateField<object>(packageElement, "_contentElement");

            // Get and Select UPM package workflow (HybridPackageWorkflow)
            object packageContent = GetPrivateField<object>(contentElement, "_content");
            IList workflowElements = GetPrivateField<IList>(contentElement, "_workflowElements");

            object hybridWorkflowElement = null;
            _hybridWorkflow = null;

            foreach (object workflowElement in workflowElements) {
                if (workflowElement.GetType().Name.Contains("HybridPackageWorkflow")) {
                    hybridWorkflowElement = workflowElement;
                    _hybridWorkflow = GetPrivateField<object>(workflowElement, "_workflow");

                    var setActiveWorkflowMethod = packageContent.GetType().GetMethod("SetActiveWorkflow");
                    setActiveWorkflowMethod.Invoke(packageContent, new object[] { _hybridWorkflow });
                    break;
                }
            }

            if (_hybridWorkflow == null) {
                Debug.LogError("Could not find HybridPackageWorkflow");
                Finish();

                return;
            }

            // Set the package path
            object pathSelectionElement = GetPrivateField<object>(hybridWorkflowElement, "PathSelectionElement");

            var setPathMethod = pathSelectionElement.GetType().GetMethod("SetPath");
            setPathMethod.Invoke(pathSelectionElement, new object[] { REPETITIONLESS_PACKAGE_DIR });

            // Export and Upload
            var uploadElement = GetPrivateField<object>(hybridWorkflowElement, "UploadElement");
            CallPrivateFunction(uploadElement, "Upload", null);

            // Get progress
            ProgressBar progressBar = GetPrivateField<ProgressBar>(uploadElement, "_uploadProgressBar");
            _uploadProgressCallback = () => { GetProgress(progressBar); };
            EditorApplication.update += _uploadProgressCallback;
        };
    }

    static void GetProgress(ProgressBar progressBar)
    {
        // Get package path if not got already
        if (progressBar.value > 0.0f && _packagePath == "") {
            object unityPackageUploader = GetPrivateField<object>(_hybridWorkflow, "_activeUploader", true);
            object unityPackageUploaderSettings = GetPrivateField<object>(unityPackageUploader, "_settings");
            var packagePathProp = unityPackageUploaderSettings.GetType().GetProperty("UnityPackagePath");

            // Path is project relative, make it absolute
            string rootProjectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            string packagePathRelative = (string)packagePathProp.GetValue(unityPackageUploaderSettings);
            string tempPackagePath = rootProjectPath + packagePathRelative;

            string packageSuffix = "output";
            if (Application.isBatchMode)
                packageSuffix = Environment.GetEnvironmentVariable(ENV_IN_PACKAGE_VERSION);

            string newPackageName = $"Repetitionless_{packageSuffix}.unitypackage";
            string newPackagePath = rootProjectPath + newPackageName;

            // Copy temp package to persistent dir
            File.Copy(tempPackagePath, newPackagePath);
            _packagePath = newPackagePath;

            Debug.Log($"Package copied to: {_packagePath}");

            // Save to env variable if in batch mode
            if (Application.isBatchMode) {
                Environment.SetEnvironmentVariable(
                    ENV_OUT_PACKAGE_PATH,
                    _packagePath,
                    EnvironmentVariableTarget.Machine
                );

                Debug.Log($"Path saved to {ENV_OUT_PACKAGE_PATH}");
            }

            Debug.Log("Starting upload...");
        }

        // Only start showing progress when package exported
        if (_packagePath != "")
            Debug.Log($"Upload Progress: {progressBar.value.ToString("F2")}%");

        if (progressBar.value < 100.0f || progressBar.title.Contains("%"))
            return;

        Debug.Log(progressBar.title);

        EditorApplication.update -= _uploadProgressCallback;
        _uploadProgressCallback = null;

        Finish();
    }
}
#endif