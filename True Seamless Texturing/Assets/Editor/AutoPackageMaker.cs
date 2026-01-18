#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public static class AutoPackageMaker
{
    private const string REPETITIONLESS_PACKAGE_DISPLAY_NAME = "Repetitionless";
    private const string REPETITIONLESS_PACKAGE_NAME = "com.williamschack.repetitionless";
    private const string REPETITIONLESS_PACKAGE_DIR = "/Packages/" + REPETITIONLESS_PACKAGE_NAME + "/";

    private static Type _uploaderWindowType;
    private static EditorWindow _uploaderWindow;

    private static EditorApplication.CallbackFunction _uploadProgressCallback;

#region Reflection Helpers
    private static T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
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
        if (SystemInfo.graphicsDeviceName != null)
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

        string unityEmail = Environment.GetEnvironmentVariable("UNITY_EMAIL");
        string unityPassword = Environment.GetEnvironmentVariable("UNITY_PASSWORD");

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
            object hybridWorkflow = null;
            foreach (object workflowElement in workflowElements) {
                if (workflowElement.GetType().Name.Contains("HybridPackageWorkflow")) {
                    hybridWorkflowElement = workflowElement;
                    hybridWorkflow = GetPrivateField<object>(workflowElement, "_workflow");

                    var setActiveWorkflowMethod = packageContent.GetType().GetMethod("SetActiveWorkflow");
                    setActiveWorkflowMethod.Invoke(packageContent, new object[] { hybridWorkflow });
                    break;
                }
            }

            if (hybridWorkflow == null) {
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