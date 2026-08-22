#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using Repetitionless.Runtime.Variables;

namespace Repetitionless.Editor.Painter
{
    using Data;
    using Materials;
    using Utilities.Texture;
    using Utilities.GUI;

    public class Painter
    {
        private const string PAINT_TEXTURE_COMPUTE_RESOURCES_PATH = "repetitionless_PaintControlTexture";
        private const int COMPUTE_THREADS_X = 8;
        private const int COMPUTE_THREADS_Y = 8;

        private const double LAYER_CHANGE_NOTIFICATION_HOLD_DURATION = 1.0f;
        private const double LAYER_CHANGE_NOTIFICATION_FADE_DURATION = 0.4f;

        private static readonly Color SELECTION_OUTLINE_COLOUR = Color.blue;

        private const float BRUSH_RADIUS_SENSITIVITY = 0.2f;
        private const float BRUSH_OPACITY_SENSITIVITY = 0.008f;
        private const float BRUSH_SMOOTHNESS_SENSITIVITY = 0.008f;
        private const float BRUSH_SENSITIVITY_SHIFT_MULTIPLIER = 0.1f;

        private const string UNDO_STROKE_NAME = "Repetitionless Paint Brush Stroke";        

        private double _layerNotificationDisplayUntil = -1;

        ComputeShader _computeShader = null;

        private int _editingLayer = 1;
        private int _textureResolution = 512;
        private float _brushRadiusReal = 15;
        private float _brushRadius => _brushRadiusReal * 0.01f;

        private float _brushOpacity = 1.0f;
        private float _brushSmoothness = 0.5f;

        private float _brushResizeLastMousePosX;

        private int _strokeUndoGroup = -1;

        private Texture2D _brushTexture = null;

        List<GameObject> _selectedPaintableObjects = new List<GameObject>();
        Dictionary<GameObject, PaintableObjectData> _paintableObjectData = new Dictionary<GameObject, PaintableObjectData>();

        List<GameObject> _paintingObjects = new List<GameObject>();
        GameObject _currentlyPaintingObject = null;



        // New vars
        private PainterSceneInteraction _sceneInteraction = new PainterSceneInteraction();
        private PainterBrushPreview _brushPreview = new PainterBrushPreview();

        public bool Painting = false;

        public void StartPainting()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;

            ObjectChangeEvents.changesPublished -= ChangesPublished;
            ObjectChangeEvents.changesPublished += ChangesPublished;

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;

            _sceneInteraction.Listen();
            _sceneInteraction.ResizePressed  -= ResizePressed;
            _sceneInteraction.ResizeHeld     -= ResizeHeld;
            _sceneInteraction.ResizeReleased -= ResizeReleased;
            _sceneInteraction.ZoomPressed    -= ZoomPressed;
            _sceneInteraction.ResizePressed  += ResizePressed;
            _sceneInteraction.ResizeHeld     += ResizeHeld;
            _sceneInteraction.ResizeReleased += ResizeReleased;
            _sceneInteraction.ZoomPressed    += ZoomPressed;

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

            Painting = true;
        }

        public void StopPainting()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            _sceneInteraction.StopListening();
            _sceneInteraction.ResizePressed  -= ResizePressed;
            _sceneInteraction.ResizeHeld     -= ResizeHeld;
            _sceneInteraction.ResizeReleased -= ResizeReleased;
            _sceneInteraction.ZoomPressed    -= ZoomPressed;

            Painting = false;
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

            // Draw custom outline to fake selection
            Handles.DrawOutline(_selectedPaintableObjects, SELECTION_OUTLINE_COLOUR, 0);

            // Dont do anything when moving cam
            if (Event.current.alt) return;

            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
                FinishPaintStroke();

            if (!_sceneInteraction.ResizingBrush)
                HandleSelection(_sceneInteraction.LastMouseHit, sceneView);

            if (_sceneInteraction.LastMouseHit.collider != null) {
                _brushPreview.DrawBrush(_sceneInteraction.LastMouseHit, sceneView, _brushRadius, _brushSmoothness);

                if (!_sceneInteraction.ResizingBrush)
                    Paint(_sceneInteraction.LastMouseHit);
            }

            if (_sceneInteraction.ResizingBrush)
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

                //Repaint();
                currentEvent.Use();
            }
        }

        private void ResizePressed(EResizingProperty resizingProperty)
        {
            _brushResizeLastMousePosX = Event.current.mousePosition.x;
        }

        private void ResizeHeld(EResizingProperty resizingProperty)
        {
            Event currentEvent = Event.current;
            float delta = currentEvent.mousePosition.x - _brushResizeLastMousePosX;
            _brushResizeLastMousePosX = currentEvent.mousePosition.x;

            float sensitivityMultiplier = _sceneInteraction.ShiftHeld ? BRUSH_SENSITIVITY_SHIFT_MULTIPLIER : 1.0f;

            switch (resizingProperty) {
                case EResizingProperty.Radius:
                    _brushRadiusReal = Mathf.Max(0.01f, _brushRadiusReal + delta * (BRUSH_RADIUS_SENSITIVITY * sensitivityMultiplier));
                    break;
                case EResizingProperty.Opacity:
                    _brushOpacity = Mathf.Clamp01(_brushOpacity + delta * (BRUSH_OPACITY_SENSITIVITY * sensitivityMultiplier));
                    break;
                case EResizingProperty.Smoothness:
                    _brushSmoothness = Mathf.Clamp01(_brushSmoothness + delta * (BRUSH_SMOOTHNESS_SENSITIVITY * sensitivityMultiplier));
                    break;
            }
        }

        private void ResizeReleased()
        {
            //
        }

        private void ZoomPressed(SceneView sceneView, Vector3 pos)
        {
            sceneView.Frame(new Bounds(pos, Vector3.one), false);
        }

        private void DrawBrushResizeNotification()
        {
            if (!_sceneInteraction.ResizingBrush)
                return;

            string text = "";
            switch (_sceneInteraction.ResizingProperty) {
                case EResizingProperty.Radius:
                    text = $"Radus {_brushRadiusReal.ToString(_sceneInteraction.ShiftHeld ? "0.00" : "0.0")}";
                    break;
                case EResizingProperty.Opacity:
                    text = $"Opacity {_brushOpacity.ToString(_sceneInteraction.ShiftHeld ? "0.000" : "0.00")}";
                    break;
                case EResizingProperty.Smoothness:
                    text = $"Smoothness {_brushSmoothness.ToString(_sceneInteraction.ShiftHeld ? "0.000" : "0.00")}";
                    break;
            }
            
            _brushPreview.DrawMousePopup(text, new Color(0.1f, 0.1f, 0.1f, 1.0f), true, new Vector2(0, -25));
        }

        private void DrawLayerChangeNotification(SceneView sceneView)
        {
            double timeRemaining = _layerNotificationDisplayUntil - EditorApplication.timeSinceStartup;
            if (timeRemaining <= 0) {
                _layerNotificationDisplayUntil = -1;
                return;
            }

            float alpha = timeRemaining < LAYER_CHANGE_NOTIFICATION_FADE_DURATION ? Mathf.Clamp01((float)(timeRemaining / LAYER_CHANGE_NOTIFICATION_FADE_DURATION)) : 1.0f;

            _brushPreview.DrawMousePopup($"Layer {_editingLayer + 1}", new Color(0.1f, 0.1f, 0.1f, alpha), true, new Vector2(0, -25));
            sceneView.Repaint();
        }

        private void Paint(RaycastHit mouseHit)
        {
            Event currentEvent = Event.current;

            GameObject gameObject = mouseHit.collider.gameObject;
            if (!_selectedPaintableObjects.Contains(gameObject))
                return;

            // Cannot paint if selected layer exceeds available layers, show on hover
            if (gameObject != null && _editingLayer >= (int)_paintableObjectData[gameObject].MaxLayers) {
                _brushPreview.DrawMousePopup(
                    $"You are painting on an invalid Layer ({_editingLayer + 1})\nUpdate the Max Layers property on this material",
                    new Color(0.25f, 0, 0, 1)
                );

                return;
            }

            if (currentEvent.button != 0)
                return;

            if (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag)
                return;

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
        
            _computeShader.SetTexture(kernel, "BrushTexture", _brushTexture == null ? Texture2D.whiteTexture : _brushTexture);
            _computeShader.SetInt("TargetSlice", _editingLayer / 4);
            _computeShader.SetInt("TargetChannel", _editingLayer % 4);
            _computeShader.SetInt("BrushChannel", 0);
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
    }
}

#endif