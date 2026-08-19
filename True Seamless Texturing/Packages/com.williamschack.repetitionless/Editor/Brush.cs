#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor
{
    using Data;
    using Materials;
    using Utilities.Texture;
    using Utilities.GUI;
    using System.Linq;

    public class Brush : EditorWindow
    {
        // Class for reference
        private class PaintableObjectData
        {
            public System.Action DataChangedAction;

            public MaterialDataManager DataManager;
            public EMaxLayers MaxLayers;

            public MeshRenderer MeshRenderer;

            public List<RenderTexture> RenderTextures;
            public List<Texture2D> ControlTextures;
        }

        private enum ResizingProperty
        {
            None,
            Radius,
            Opacity,
            Smoothness
        }

        private const string PAINT_TEXTURE_COMPUTE_RESOURCES_PATH = "repetitionless_PaintControlTexture";
        private const int COMPUTE_THREADS_X = 8;
        private const int COMPUTE_THREADS_Y = 8;

        private static readonly Vector2 MOUSE_NOTIFICATION_OFFSET = new Vector2(20, 0); 
        private const double LAYER_CHANGE_NOTIFICATION_HOLD_DURATION = 1.0f;
        private const double LAYER_CHANGE_NOTIFICATION_FADE_DURATION = 0.4f;

        private static readonly Color SELECTION_OUTLINE_COLOUR = Color.blue;

        private const float BRUSH_RADIUS_SENSITIVITY = 0.2f;
        private const float BRUSH_OPACITY_SENSITIVITY = 0.008f;
        private const float BRUSH_SMOOTHNESS_SENSITIVITY = 0.008f;
        private const float BRUSH_SENSITIVITY_SHIFT_MULTIPLIER = 0.1f;

        private const string UNDO_STROKE_NAME = "Repetitionless Paint Brush Stroke";

        private GUIStyle _notificationBoxStyle;
        private GUIStyle _notificationLabelStyle;
        private bool _guiStylesSetup = false;

        private double _layerNotificationDisplayUntil = -1;

        ComputeShader _computeShader = null;

        private int _editingLayer = 1;
        private int _textureResolution = 512;
        private float _brushRadiusReal = 15;
        private float _brushRadius => _brushRadiusReal * 0.01f;

        private float _brushOpacity = 1.0f;
        private float _brushSmoothness = 0.5f;

        private ResizingProperty _resizingProperty = ResizingProperty.None;
        private bool _resizingBrush = false;
        private float _brushResizeStartValue;
        private float _brushResizeLastMousePosX;
        private RaycastHit _lastMouseHit;

        private bool _rightClickHeld = false;

        private int _strokeUndoGroup = -1;

        private Texture2D _brushTexture = null;

        List<GameObject> _selectedPaintableObjects = new List<GameObject>();
        Dictionary<GameObject, PaintableObjectData> _paintableObjectData = new Dictionary<GameObject, PaintableObjectData>();

        List<GameObject> _paintingObjects = new List<GameObject>();
        GameObject _currentlyPaintingObject = null;

        [MenuItem("Window/Repetitionless/Open Painter", priority = 0)]
        public static void Open()
        {
            Brush window = GetWindow<Brush>(false, "Repetitionless Painter");
            window.Show();
        }

        private void CreateGUI()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;

            ObjectChangeEvents.changesPublished -= ChangesPublished;
            ObjectChangeEvents.changesPublished += ChangesPublished;

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;

            _computeShader = Resources.Load<ComputeShader>(PAINT_TEXTURE_COMPUTE_RESOURCES_PATH);
            if (_computeShader == null)
                Debug.LogError("No texture paint compute shader found...");

            // Check all selected objects and add paintable ones
            foreach (Object selectedObject in Selection.objects) {
                if (selectedObject is not GameObject) continue;

                GameObject selectedGameObject = (GameObject)selectedObject;
                if (ObjectCanBeSelected(selectedGameObject))
                    SelectionAdd(selectedGameObject);
            }

            // INSTEAD OF CLEARING, CACHE SELECTION AND RESELECT ON DISABLE
            Selection.objects = new Object[] {};
        }

        private void SetupGUIStyles()
        {
            _guiStylesSetup = true;

            // Notification box
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, Color.white);
            backgroundTexture.Apply();

            _notificationBoxStyle = new GUIStyle(GUI.skin.box) {
                normal = { background = backgroundTexture },
                padding = { right = 5 }
            };

            // Notification label
            _notificationLabelStyle = new GUIStyle(GUI.skin.label);
            _notificationLabelStyle.fontSize = 14;
            _notificationLabelStyle.fontStyle = FontStyle.Bold;
        }

        // Resets on domain reload
        private void KeepNotificationBackgroundAlive()
        {
            if (_notificationBoxStyle?.normal.background != null)
                return;

            SetupGUIStyles();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        private void OnGUI()
        {
            if (!_guiStylesSetup)
                SetupGUIStyles();
            KeepNotificationBackgroundAlive();

            _editingLayer = EditorGUILayout.IntSlider("Layer", _editingLayer + 1, 1, Constants.MAX_LAYERS_TERRAIN) - 1;
            
            GUILayout.Space(10);

            _brushTexture = (Texture2D)EditorGUILayout.ObjectField("Brush Texture", _brushTexture, typeof(Texture2D), false, GUILayout.Height(GUIUtilities.LINE_HEIGHT));
            _brushRadiusReal = Mathf.Max(0.01f, EditorGUILayout.FloatField("Brush Radius", _brushRadiusReal));
            _brushOpacity = EditorGUILayout.Slider("Brush Opacity", _brushOpacity, 0, 1);
            _brushSmoothness = EditorGUILayout.Slider("Brush Smoothness", _brushSmoothness, 0, 1);
        }

        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            // Listen for when an object is deleted in the scene
            for (int i = 0; i < stream.length; i++) {
                switch (stream.GetEventType(i)) {
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                        stream.GetDestroyGameObjectHierarchyEvent(i, out DestroyGameObjectHierarchyEventArgs destroyGameObjectHierarchyEvent);
#if UNITY_6000_3_OR_NEWER
                        Object destroyedObject = EditorUtility.EntityIdToObject(destroyGameObjectHierarchyEvent.instanceId);
#else
                        Object destroyedObject = EditorUtility.InstanceIDToObject(destroyGameObjectHierarchyEvent.instanceId);
#endif

                        // There is a scene object that was deleted
                        // We dont know exactly which one but make sure none of the painting objects were deleted
                        SelectionRemoveNull();

                        break;
                }
            }
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            if (_computeShader == null)
                return;

            // Disable default left click events
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Draw custom outline to fake selection
            Handles.DrawOutline(_selectedPaintableObjects, SELECTION_OUTLINE_COLOUR, 0);

            // Dont do anything when moving cam
            if (Event.current.alt) return;

            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
                FinishPaintStroke();

            if (!_resizingBrush) {
                _lastMouseHit = GetMouseHit();
                HandleSelection(_lastMouseHit, sceneView);
            }

            HandleBrushResize();
            HandleZoom(_lastMouseHit, sceneView);

            if (_lastMouseHit.collider != null) {
                DrawBrush(_lastMouseHit, sceneView);

                if (!_resizingBrush)
                    Paint(_lastMouseHit);
            }

            if (_resizingBrush)
                DrawBrushResizeNotification();

            if (_layerNotificationDisplayUntil >= 0)
                DrawLayerChangeNotification(sceneView);
        }

        private void UndoRedoPerformed()
        {
            // Blit control textures back to painted objects as they may have changed
            foreach (PaintableObjectData objectData in _paintableObjectData.Values) {
                for (int i = 0; i < objectData.ControlTextures.Count; i++) {
                    Graphics.Blit(objectData.ControlTextures[i], objectData.RenderTextures[i]);
                }
            }
        }

        private void HandleSelection(RaycastHit mouseHit, SceneView sceneView)
        {
            Event currentEvent = Event.current;

            // On click decide if it will be selected
            if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown) {
                // Clear selection if clicked nothing
                if (mouseHit.collider == null) {
                    SelectionRemoveAll();
                    sceneView.Repaint();
                    return;
                }

                GameObject hitObject = mouseHit.collider.gameObject;

                // If holding ctrl/shift and the object is selected, remove it
                if ((currentEvent.shift || currentEvent.control) && _selectedPaintableObjects.Contains(hitObject)) {
                    SelectionRemove(hitObject);
                    currentEvent.Use();
                }
                // Check if object is valid and add to selected
                else if (ObjectCanBeSelected(mouseHit.collider))
                    SelectionAdd(hitObject);
            }

            // If shift + mouse wheel, change layer
            if (currentEvent.shift && currentEvent.type == EventType.ScrollWheel) {
                if (currentEvent.delta.y > 0) _editingLayer = Mathf.Max(0, _editingLayer - 1);
                else                          _editingLayer = Mathf.Min(_editingLayer + 1, Constants.MAX_LAYERS_TERRAIN - 1);

                _layerNotificationDisplayUntil = EditorApplication.timeSinceStartup + LAYER_CHANGE_NOTIFICATION_HOLD_DURATION;

                Repaint();
                currentEvent.Use();
            }
        }

        private void HandleBrushResize()
        {
            Event currentEvent = Event.current;

            // Track right click, cant check on any resize keys event as its a different event
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1) _rightClickHeld = true;
            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 1) _rightClickHeld = false;

            // If starting to move during resize, cancel resize
            if (_resizingBrush && _rightClickHeld) {
                _resizingBrush = false;
                _resizingProperty = ResizingProperty.None;
            }

            // Cancel on control to allow ctrl+s saving
            // Cancel on right click to avoid camera movement
            if (currentEvent.control || _rightClickHeld)
                return;

            ResizingProperty prevResizingProperty = _resizingProperty;
            bool isResizeKey = true;
            switch (currentEvent.keyCode) {
                case KeyCode.S: _resizingProperty = ResizingProperty.Radius; break;
                case KeyCode.A: _resizingProperty = ResizingProperty.Opacity; break;
                case KeyCode.D: _resizingProperty = ResizingProperty.Smoothness; break;
                default: isResizeKey = false; break;
            }

            // Dont allow other key presses to cancel the resize
            if (_resizingBrush && !isResizeKey && (currentEvent.type == EventType.KeyDown || currentEvent.type == EventType.KeyUp))
                return;

            // Dont allow resizing multiple properties at once
            if (_resizingBrush && prevResizingProperty != _resizingProperty) {
                _resizingProperty = prevResizingProperty;
                return;
            }

            if (_resizingProperty == ResizingProperty.None && !_resizingBrush)
                return;

            // Start resize
            if (currentEvent.type == EventType.KeyDown && !_resizingBrush) {
                _resizingBrush = true;
                _brushResizeLastMousePosX = currentEvent.mousePosition.x;

                switch (_resizingProperty) {
                    case ResizingProperty.Radius: _brushResizeStartValue = _brushRadiusReal; break;
                    case ResizingProperty.Opacity: _brushResizeStartValue = _brushOpacity; break;
                    case ResizingProperty.Smoothness: _brushResizeStartValue = _brushSmoothness; break;
                }
                
                currentEvent.Use();
                return;
            }

            // Finish resize
            if (currentEvent.type == EventType.KeyUp && _resizingBrush) {
                _resizingBrush = false;
                _resizingProperty = ResizingProperty.None;

                currentEvent.Use();
                return;
            }

            // Resizing
            if (currentEvent.type != EventType.MouseMove)
                return;

            float delta = currentEvent.mousePosition.x - _brushResizeLastMousePosX;
            _brushResizeLastMousePosX = currentEvent.mousePosition.x;

            float sensitivityMultiplier = currentEvent.shift ? BRUSH_SENSITIVITY_SHIFT_MULTIPLIER : 1.0f;

            switch (_resizingProperty) {
                case ResizingProperty.Radius:
                    _brushRadiusReal = Mathf.Max(0.01f, _brushRadiusReal + delta * (BRUSH_RADIUS_SENSITIVITY * sensitivityMultiplier));
                    break;
                case ResizingProperty.Opacity:
                    _brushOpacity = Mathf.Clamp01(_brushOpacity + delta * (BRUSH_OPACITY_SENSITIVITY * sensitivityMultiplier));
                    break;
                case ResizingProperty.Smoothness:
                    _brushSmoothness = Mathf.Clamp01(_brushSmoothness + delta * (BRUSH_SMOOTHNESS_SENSITIVITY * sensitivityMultiplier));
                    break;
            }

            Repaint();
        }

        private void DrawBrushResizeNotification()
        {
            if (!_resizingBrush)
                return;

            string text = "";
            switch (_resizingProperty) {
                case ResizingProperty.Radius:
                    text = $"Radus {_brushRadiusReal:0.0}";
                    break;
                case ResizingProperty.Opacity:
                    text = $"Opacity {_brushOpacity:0.00}";
                    break;
                case ResizingProperty.Smoothness:
                    text = $"Smoothness {_brushSmoothness:0.00}";
                    break;
            }
            
            DrawMousePopupLabel(text, new Color(0.1f, 0.1f, 0.1f, 1.0f), true, new Vector2(0, -25));
        }

        private void HandleZoom(RaycastHit mouseHit, SceneView sceneView)
        {
            Event currentEvent = Event.current;

            if (_lastMouseHit.collider == null || currentEvent.keyCode != KeyCode.F || currentEvent.type != EventType.KeyDown)
                return;

            sceneView.Frame(new Bounds(mouseHit.point, Vector3.one), false);
            currentEvent.Use();
        }

        private void DrawBrush(RaycastHit mouseHit, SceneView sceneView)
        {
            // Always draw brush if hovering something
            
            Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, _brushRadius, 3);

            // Draw smoothness disc
            float innerRadius = _brushRadius * (1 - _brushSmoothness);
            Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, innerRadius, 1);

            sceneView.Repaint();
        }

        private void DrawLayerChangeNotification(SceneView sceneView)
        {
            double timeRemaining = _layerNotificationDisplayUntil - EditorApplication.timeSinceStartup;
            if (timeRemaining <= 0) {
                _layerNotificationDisplayUntil = -1;
                return;
            }

            float alpha = timeRemaining < LAYER_CHANGE_NOTIFICATION_FADE_DURATION ? Mathf.Clamp01((float)(timeRemaining / LAYER_CHANGE_NOTIFICATION_FADE_DURATION)) : 1.0f;

            DrawMousePopupLabel($"Layer {_editingLayer + 1}", new Color(0.1f, 0.1f, 0.1f, alpha), true, new Vector2(0, -25));
            sceneView.Repaint();
        }

        private void DrawMousePopupLabel(string label, Color backgroundColor, bool alphaToColour = false)
        {
            DrawMousePopupLabel(label, backgroundColor, alphaToColour, Vector2.zero);
        }

        private void DrawMousePopupLabel(string label, Color backgroundColor, bool alphaToColour, Vector2 positionOffset)
        {
            Handles.BeginGUI();

            // Calculate size based on contents
            Vector2 size = _notificationLabelStyle.CalcSize(new GUIContent(label));
            size.x += _notificationBoxStyle.padding.left + _notificationBoxStyle.padding.right;
            size.y += _notificationBoxStyle.padding.top + _notificationBoxStyle.padding.bottom;

            Vector2 mousePos = Event.current.mousePosition;
            Rect maxAreaRect = new Rect(mousePos.x + MOUSE_NOTIFICATION_OFFSET.x + positionOffset.x, mousePos.y + MOUSE_NOTIFICATION_OFFSET.y + positionOffset.y, size.x, size.y);

            Color prevColour = GUI.color;
            if (alphaToColour) {
                Color newColour = prevColour;
                newColour.a = backgroundColor.a;
                GUI.color = newColour;
            }

            GUILayout.BeginArea(maxAreaRect);

            Color prevBackgroundColour = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            GUILayout.BeginVertical(_notificationBoxStyle);
            GUI.backgroundColor = prevBackgroundColour;

            GUILayout.Label(label, _notificationLabelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            if (alphaToColour)
                GUI.color = prevColour;

            Handles.EndGUI();
        }

        private void Paint(RaycastHit mouseHit)
        {
            Event currentEvent = Event.current;

            GameObject gameObject = mouseHit.collider.gameObject;
            if (!_selectedPaintableObjects.Contains(gameObject))
                return;

            // Cannot paint if selected layer exceeds available layers, show on hover
            if (gameObject != null && _editingLayer >= (int)_paintableObjectData[gameObject].MaxLayers) {
                DrawMousePopupLabel(
                    $"You are painting on an invalid Layer ({_editingLayer + 1})\nUpdate the Max Layers property on this material",
                    new Color(0.25f, 0, 0, 1)
                );

                return;
            }

            if (currentEvent.button != 0)
                return;

            if (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag)
                return;

            // == Test with the first control for now

            PaintableObjectData objectData = _paintableObjectData[gameObject];

            // If stroke just passed over object, initialise painting
            if (!_paintingObjects.Contains(gameObject)) {
                // If starting stroke, register undo
                if (_paintingObjects.Count == 0) {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName(UNDO_STROKE_NAME);
                    _strokeUndoGroup = Undo.GetCurrentGroup();
                }

                InitialisePainting(gameObject);
            }

            _currentlyPaintingObject = gameObject;

            // Dispatch paint compute shader
            int kernel = _computeShader.FindKernel("CSMain");
            for (int i = 0; i < objectData.RenderTextures.Count; i++)
                _computeShader.SetTexture(kernel, $"Control{i}", objectData.RenderTextures[i]);

            _computeShader.SetInt("TargetSlice", _editingLayer / 4);
            _computeShader.SetInt("TargetChannel", _editingLayer % 4);
            _computeShader.SetVector("HitUV", new Vector4(mouseHit.textureCoord.x, mouseHit.textureCoord.y, 0, 0));
            _computeShader.SetFloat("Radius", _brushRadius);
            _computeShader.SetFloat("Opacity", _brushOpacity);
            _computeShader.SetFloat("Smoothness", _brushSmoothness);

            int groupsX = Mathf.CeilToInt(objectData.ControlTextures[0].width  / (float)COMPUTE_THREADS_X);
            int groupsY = Mathf.CeilToInt(objectData.ControlTextures[0].height / (float)COMPUTE_THREADS_Y);

            _computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }

        private void InitialisePainting(GameObject gameObject)
        {
            _paintingObjects.Add(gameObject);

            PaintableObjectData objectData = _paintableObjectData[gameObject];

            // Register control textures for undo
            foreach (Texture2D controlTexture in objectData.ControlTextures)
                Undo.RegisterCompleteObjectUndo(controlTexture, UNDO_STROKE_NAME);

            Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(objectData.MeshRenderer);

            for (int i = 0; i < objectData.ControlTextures.Count; i++) {
                // Apply to the object material
                repetitionlessMaterial.SetTexture($"_Control{i}", objectData.RenderTextures[i]);
            }
        }

        private void FinishPaintStroke()
        {
            foreach (GameObject gameObject in _paintingObjects) {
                PaintableObjectData objectData = _paintableObjectData[gameObject];
                Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(objectData.MeshRenderer);

                RenderTexture previousRT = RenderTexture.active;

                for (int i = 0; i < objectData.ControlTextures.Count; i++) {
                    // Save rt to texture
                    RenderTexture.active = objectData.RenderTextures[i];

                    Texture2D controlTexture = objectData.ControlTextures[i];
                    controlTexture.ReadPixels(new Rect(0, 0, controlTexture.width, controlTexture.height), 0, 0);
                    controlTexture.Apply();

                    EditorUtility.SetDirty(controlTexture);

                    // Apply texture material
                    repetitionlessMaterial.SetTexture($"_Control{i}", controlTexture);
                }

                RenderTexture.active = previousRT;
            }

            // Merge texture changes into one undo group
            if (_strokeUndoGroup >= 0) {
                Undo.CollapseUndoOperations(_strokeUndoGroup);
                _strokeUndoGroup = -1;
            }

            _currentlyPaintingObject = null;
            _paintingObjects.Clear();
        }

        private void SelectionAdd(GameObject obj)
        {
            if (_selectedPaintableObjects.Contains(obj))
                return;
            
            _selectedPaintableObjects.Add(obj);

            PaintableObjectData objectData = new PaintableObjectData {
                MeshRenderer = obj.GetComponent<MeshRenderer>()
            };

            // Need to test if:
            // Repetitionless material is removed
            Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(objectData.MeshRenderer);
            objectData.DataManager = new MaterialDataManager(repetitionlessMaterial);

            RepetitionlessMaterialDataSO materialPropertiesSO = objectData.DataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
            objectData.DataChangedAction = () => { MaterialExternalDataChanged(obj); };
            materialPropertiesSO.OnExternalDataChanged += objectData.DataChangedAction;

            // Assign texture to layered data
            // SHOULD BE CHECKED FREQUENTLY
            RepetitionlessLayeredDataSO layeredDataSO = objectData.DataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            objectData.MaxLayers = layeredDataSO.MaxLayers;

            // Make sure its mode is set to control textures
            RepetitionlessLayeredMaterialUtilities.UpdateLayerModeShader(objectData.DataManager, ELayerMode.ControlTextures);
            layeredDataSO.LayerMode = ELayerMode.ControlTextures;

            objectData.ControlTextures = new List<Texture2D>();
            objectData.RenderTextures = new List<RenderTexture>();

            int controlTextureCount = Constants.MAX_LAYERS_TERRAIN / 4;
            for (int i = 0; i < controlTextureCount; i++) {
                // Get/Create control texture
                Texture2D texture = objectData.DataManager.LoadAsset<Texture2D>($"{Constants.CONTROL_TEXTURE_FILE_NAME_PREFIX}{i}.asset");
                
                // Resize texture to target
                if (texture.width != _textureResolution || texture.height != _textureResolution) {
                    TextureUtilities.ResizeTexture(texture, _textureResolution, _textureResolution, modifyOriginal: true);
                    EditorUtility.SetDirty(texture);
                    AssetDatabase.SaveAssetIfDirty(texture);
                }

                objectData.ControlTextures.Add(texture);

                // Setup layered data
                layeredDataSO.ControlTextures[i].ChannelTextures[0].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[1].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[2].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[3].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[0].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.R, TexturePacker.TextureChannel.R);
                layeredDataSO.ControlTextures[i].ChannelTextures[1].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.G, TexturePacker.TextureChannel.G);
                layeredDataSO.ControlTextures[i].ChannelTextures[2].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.B, TexturePacker.TextureChannel.B);
                layeredDataSO.ControlTextures[i].ChannelTextures[3].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.A, TexturePacker.TextureChannel.A);

                // Create render texture
                RenderTexture renderTexture = new RenderTexture(_textureResolution, _textureResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) {
                    enableRandomWrite = true,
                    filterMode = FilterMode.Point
                };
                renderTexture.Create();

                // Copy control texture to the rt
                Graphics.Blit(texture, renderTexture);

                objectData.RenderTextures.Add(renderTexture);
            }
            
            layeredDataSO.Save();

            _paintableObjectData.Add(obj, objectData);
        }

        private void SelectionRemove(GameObject obj)
        {
            if (!_selectedPaintableObjects.Contains(obj))
                return;
            
            _selectedPaintableObjects.Remove(obj);

            // Clear Render Textures
            PaintableObjectData objectData = _paintableObjectData[obj];
            foreach (RenderTexture rt in objectData.RenderTextures)
                rt.Release();

            RepetitionlessMaterialDataSO materialPropertiesSO = objectData.DataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
            materialPropertiesSO.OnExternalDataChanged -= objectData.DataChangedAction;

            _paintableObjectData.Remove(obj);
        }

        // Removes all objects that have been deleted from the painted list
        private void SelectionRemoveNull()
        {
            List<GameObject> destroyedObjects = _selectedPaintableObjects.Where(obj => obj == null).ToList();
            foreach (GameObject gameObject in destroyedObjects)
                SelectionRemove(gameObject);
        }

        private void SelectionRemoveAll()
        {
            // Loop backwards to allow removing elements during loop
            for (int i = _selectedPaintableObjects.Count - 1; i >= 0; i--)
                SelectionRemove(_selectedPaintableObjects[i]);
        }

        private void MaterialExternalDataChanged(GameObject obj)
        {
            PaintableObjectData objectData = _paintableObjectData[obj];
            RepetitionlessLayeredDataSO layeredDataSO = objectData.DataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            // If max layers is changed
            if (layeredDataSO.MaxLayers != objectData.MaxLayers)
                objectData.MaxLayers = layeredDataSO.MaxLayers;
                
        }

        private bool ObjectCanBeSelected(Collider hitCollider)
        {
            // Must be mesh collider to have proper uvs
            // Need to add some sort of warning
            if (hitCollider is not MeshCollider meshCollider || meshCollider.sharedMesh == null)
                return false;

            return ObjectCanBeSelectedInner(hitCollider.gameObject);
        }

        private bool ObjectCanBeSelected(GameObject obj)
        {
            // Must be mesh collider to have proper uvs
            // Need to add some sort of warning
            MeshCollider meshCollider = null;
            obj.TryGetComponent(out meshCollider);
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return false;

            return ObjectCanBeSelectedInner(obj);
        }

        private bool ObjectCanBeSelectedInner(GameObject obj)
        {
            // Mesh must have a repetitionless material
            MeshRenderer meshRenderer;
            obj.TryGetComponent(out meshRenderer);
            if (meshRenderer == null) return false;

            Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(meshRenderer);

            // If the repetitionless material is using the terrain shader, dont allow either
            // Need to add a message to change
            if (repetitionlessMaterial == null) return false;

            return true;
        }

        private Material GetFirstRepetitionlessMaterial(MeshRenderer renderer)
        {
            foreach (Material mat in renderer.sharedMaterials) {
                if (!mat.shader.name.Contains(Constants.SHADER_MATERIAL_NAME_LAYERED))
                    continue;

                return mat; // Assume only one material is on the object
            }

            return null;
        }

        private static RaycastHit GetMouseHit()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                return hit;
            }

            return new RaycastHit();
        }
    }
}
#endif