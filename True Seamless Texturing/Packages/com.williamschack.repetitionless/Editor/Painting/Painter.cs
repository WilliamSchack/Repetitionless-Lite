#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Repetitionless.Editor.Painter
{
    using Materials;

    public class Painter
    {
        private const string PAINT_TEXTURE_COMPUTE_RESOURCES_PATH = "repetitionless_PaintControlTexture";
        private const int COMPUTE_THREADS_X = 8;
        private const int COMPUTE_THREADS_Y = 8;

        private const double LAYER_CHANGE_POPUP_HOLD_DURATION = 1.0f;
        private const double LAYER_CHANGE_POPUP_FADE_DURATION = 0.4f;

        private const float BRUSH_RADIUS_SENSITIVITY = 0.2f;
        private const float BRUSH_OPACITY_SENSITIVITY = 0.008f;
        private const float BRUSH_SMOOTHNESS_SENSITIVITY = 0.008f;
        private const float BRUSH_ROTATION_SENSITIVITY = 2.0f;
        private const float BRUSH_SENSITIVITY_SHIFT_MULTIPLIER = 0.1f;

        private const string UNDO_STROKE_NAME = "Repetitionless Paint Brush Stroke";

        public Action OnPropertyChanged;

        private PainterSceneInteraction _sceneInteraction = new PainterSceneInteraction();
        private PainterBrushPreview _brushPreview = new PainterBrushPreview();
        private PainterSelection _selection = new PainterSelection();

        private ComputeShader _computeShader = null;

        public int TextureResolution {
            get { return _selection.TextureResolution; }
            set { _selection.TextureResolution = value; }
        }
        
        public int EditingLayer = 1;
        public Texture2D BrushTexture = null;
        public float BrushRadiusReal = 15;
        public float BrushRadius => BrushRadiusReal * 0.01f;
        public float BrushOpacity = 1.0f;
        public float BrushSmoothness = 0.5f;
        public float BrushRotationDegrees = 0.0f;

        private float _brushResizeLastMousePosX;

        private int _strokeUndoGroup = -1;


        private List<GameObject> _paintingObjects = new List<GameObject>();
        private GameObject _currentlyPaintingObject = null;
        private GameObject _lastPaintedObject = null;

        public GameObject CurrentlyPaintingObject => _currentlyPaintingObject;
        public GameObject LastPaintedObject => _lastPaintedObject;

        private bool _painting = false;
        public bool Painting => _painting;

        public void StartPainting()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;

            _sceneInteraction.Listen();
            _sceneInteraction.OnResizePressed  -= ResizePressed;
            _sceneInteraction.OnResizeHeld     -= ResizeHeld;
            _sceneInteraction.OnResizeReleased -= ResizeReleased;
            _sceneInteraction.OnZoomPressed    -= ZoomPressed;
            _sceneInteraction.OnLayerDecreased -= LayerDecreased;
            _sceneInteraction.OnLayerIncreased -= LayerIncreased;
            _sceneInteraction.OnResizePressed  += ResizePressed;
            _sceneInteraction.OnResizeHeld     += ResizeHeld;
            _sceneInteraction.OnResizeReleased += ResizeReleased;
            _sceneInteraction.OnZoomPressed    += ZoomPressed;
            _sceneInteraction.OnLayerDecreased += LayerDecreased;
            _sceneInteraction.OnLayerIncreased += LayerIncreased;

            if (_computeShader == null) {
                _computeShader = Resources.Load<ComputeShader>(PAINT_TEXTURE_COMPUTE_RESOURCES_PATH);
                if (_computeShader == null)
                    Debug.LogError("No texture paint compute shader found...");
            }
            
            _selection.Setup();
            _selection.AddSelected();

            _painting = true;
            SceneView.RepaintAll();
        }

        public void StopPainting()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            _sceneInteraction.StopListening();

            _selection.Cleanup();

            _painting = false;
            SceneView.RepaintAll();
        }

        public void TogglePainting()
        {
            if (_painting) StopPainting();
            else           StartPainting();
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            if (_computeShader == null)
                return;

            // Dont do anything when moving cam
            if (_sceneInteraction.AltHeld) return;

            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
                FinishPaintStroke();

            if (!_sceneInteraction.ResizingBrush)
                _selection.DuringSceneGUI(_sceneInteraction.LastMouseHit, sceneView);

            if (_sceneInteraction.LastMouseHit.collider != null) {
                _brushPreview.DrawBrush(_sceneInteraction.LastMouseHit, sceneView, BrushRadius, BrushSmoothness);

                if (!_sceneInteraction.ResizingBrush)
                    Paint(_sceneInteraction.LastMouseHit);
            }

            if (_sceneInteraction.ResizingBrush)
                DrawBrushResizeNotification();

            _brushPreview.OnSceneGUI();
        }

        private void LayerDecreased()
        {
            if (_sceneInteraction.ResizingBrush)
                return;

            UpdateLayer(Mathf.Max(0, EditingLayer - 1));
        }

        private void LayerIncreased()
        {
            if (_sceneInteraction.ResizingBrush)
                return;

            UpdateLayer(Mathf.Min(EditingLayer + 1, Constants.MAX_LAYERS_TERRAIN - 1));
        }

        private void UpdateLayer(int newLayer)
        {
            // Update layer
            EditingLayer = newLayer;
            OnPropertyChanged?.Invoke();

            // Draw popup
            _brushPreview.ClearFadingPopups();
            _brushPreview.AddFadingPopup(
                EditorApplication.timeSinceStartup + LAYER_CHANGE_POPUP_HOLD_DURATION,
                LAYER_CHANGE_POPUP_FADE_DURATION,
                    $"Layer {EditingLayer + 1}", new Color(0.1f, 0.1f, 0.1f), true, new Vector2(0, -25)
            );
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
                    BrushRadiusReal = Mathf.Max(0.01f, BrushRadiusReal + delta * (BRUSH_RADIUS_SENSITIVITY * sensitivityMultiplier));
                    break;
                case EResizingProperty.Opacity:
                    BrushOpacity = Mathf.Clamp01(BrushOpacity + delta * (BRUSH_OPACITY_SENSITIVITY * sensitivityMultiplier));
                    break;
                case EResizingProperty.Smoothness:
                    BrushSmoothness = Mathf.Clamp01(BrushSmoothness + delta * (BRUSH_SMOOTHNESS_SENSITIVITY * sensitivityMultiplier));
                    break;
                case EResizingProperty.Rotation:
                    BrushRotationDegrees = Mathf.Clamp(BrushRotationDegrees + delta * (BRUSH_ROTATION_SENSITIVITY * sensitivityMultiplier), 0, 360);
                    break;
            }
            
            OnPropertyChanged?.Invoke();
        }

        private void ResizeReleased()
        {
            //
        }

        private void ZoomPressed(SceneView sceneView, Vector3 pos)
        {
            sceneView.Frame(new Bounds(pos, Vector3.one), false);
        }

        private void UndoRedoPerformed()
        {
            // Blit control textures back to painted objects as they may have changed
            foreach (PaintableObjectData objectData in _selection.PaintableObjectData.Values) {
                for (int i = 0; i < objectData.ControlTextures.Count; i++) {
                    Graphics.Blit(objectData.ControlTextures[i], objectData.RenderTextures[i]);
                }
            }
        }

        private void DrawBrushResizeNotification()
        {
            if (!_sceneInteraction.ResizingBrush)
                return;

            string text = "";
            switch (_sceneInteraction.ResizingProperty) {
                case EResizingProperty.Radius:
                    text = $"Radus: {BrushRadiusReal.ToString(_sceneInteraction.ShiftHeld ? "0.00" : "0.0")}";
                    break;
                case EResizingProperty.Opacity:
                    text = $"Opacity: {BrushOpacity.ToString(_sceneInteraction.ShiftHeld ? "0.000" : "0.00")}";
                    break;
                case EResizingProperty.Smoothness:
                    text = $"Smoothness: {BrushSmoothness.ToString(_sceneInteraction.ShiftHeld ? "0.000" : "0.00")}";
                    break;
                case EResizingProperty.Rotation:
                    text = $"Rotation: {BrushRotationDegrees.ToString(_sceneInteraction.ShiftHeld ? "0.0" : "0")}\u00B0";
                    break;
            }
            
            _brushPreview.DrawMousePopup(text, new Color(0.1f, 0.1f, 0.1f, 1.0f), true, new Vector2(0, -25));
        }

        private void Paint(RaycastHit mouseHit)
        {
            Event currentEvent = Event.current;

            GameObject gameObject = mouseHit.collider.gameObject;
            if (!_selection.SelectedPaintableObjects.Contains(gameObject))
                return;

            // Cannot paint if selected layer exceeds available layers, show on hover
            if (gameObject != null && EditingLayer >= (int)_selection.PaintableObjectData[gameObject].MaxLayers) {
                _brushPreview.DrawMousePopup(
                    $"You are painting on an invalid Layer ({EditingLayer + 1})\nUpdate the Max Layers property on this material",
                    new Color(0.25f, 0, 0, 1)
                );

                return;
            }

            if (currentEvent.button != 0)
                return;

            if (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag)
                return;

            PaintableObjectData objectData = _selection.PaintableObjectData[gameObject];

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
            _lastPaintedObject = gameObject;

            // Dispatch paint compute shader
            int kernel = _computeShader.FindKernel("CSMain");
            for (int i = 0; i < objectData.RenderTextures.Count; i++)
                _computeShader.SetTexture(kernel, $"Control{i}", objectData.RenderTextures[i]);
        
            _computeShader.SetTexture(kernel, "BrushTexture", BrushTexture == null ? Texture2D.whiteTexture : BrushTexture);
            _computeShader.SetInt("TargetSlice", EditingLayer / 4);
            _computeShader.SetInt("TargetChannel", EditingLayer % 4);
            _computeShader.SetInt("BrushChannel", 0);
            _computeShader.SetVector("HitUV", new Vector4(mouseHit.textureCoord.x, mouseHit.textureCoord.y, 0, 0));
            _computeShader.SetFloat("Radius", BrushRadius);
            _computeShader.SetFloat("Opacity", BrushOpacity);
            _computeShader.SetFloat("Smoothness", BrushSmoothness);
            _computeShader.SetFloat("RotationRadians", BrushRotationDegrees * Mathf.Deg2Rad);

            int groupsX = Mathf.CeilToInt(objectData.ControlTextures[0].width  / (float)COMPUTE_THREADS_X);
            int groupsY = Mathf.CeilToInt(objectData.ControlTextures[0].height / (float)COMPUTE_THREADS_Y);

            _computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }

        private void InitialisePainting(GameObject gameObject)
        {
            _paintingObjects.Add(gameObject);

            PaintableObjectData objectData = _selection.PaintableObjectData[gameObject];

            // Register control textures for undo
            foreach (Texture2D controlTexture in objectData.ControlTextures)
                Undo.RegisterCompleteObjectUndo(controlTexture, UNDO_STROKE_NAME);

            Material repetitionlessMaterial = RepetitionlessLayeredMaterialUtilities.GetFirstLayeredMaterial(objectData.MeshRenderer);

            for (int i = 0; i < objectData.ControlTextures.Count; i++) {
                // Apply to the object material
                repetitionlessMaterial.SetTexture($"_Control{i}", objectData.RenderTextures[i]);
            }
        }

        private void FinishPaintStroke()
        {
            foreach (GameObject gameObject in _paintingObjects) {
                PaintableObjectData objectData = _selection.PaintableObjectData[gameObject];
                Material repetitionlessMaterial = RepetitionlessLayeredMaterialUtilities.GetFirstLayeredMaterial(objectData.MeshRenderer);

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
    }
}

#endif