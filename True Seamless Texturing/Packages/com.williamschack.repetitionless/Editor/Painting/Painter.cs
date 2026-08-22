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

        private const double LAYER_CHANGE_POPUP_HOLD_DURATION = 1.0f;
        private const double LAYER_CHANGE_POPUP_FADE_DURATION = 0.4f;

        

        private const float BRUSH_RADIUS_SENSITIVITY = 0.2f;
        private const float BRUSH_OPACITY_SENSITIVITY = 0.008f;
        private const float BRUSH_SMOOTHNESS_SENSITIVITY = 0.008f;
        private const float BRUSH_SENSITIVITY_SHIFT_MULTIPLIER = 0.1f;

        private const string UNDO_STROKE_NAME = "Repetitionless Paint Brush Stroke";        

        private double _layerNotificationDisplayUntil = -1;

        ComputeShader _computeShader = null;

        private int _editingLayer = 1;
        
        private float _brushRadiusReal = 15;
        private float _brushRadius => _brushRadiusReal * 0.01f;

        private float _brushOpacity = 1.0f;
        private float _brushSmoothness = 0.5f;

        private float _brushResizeLastMousePosX;

        private int _strokeUndoGroup = -1;

        private Texture2D _brushTexture = null;

        List<GameObject> _paintingObjects = new List<GameObject>();
        GameObject _currentlyPaintingObject = null;



        // New vars
        private PainterSceneInteraction _sceneInteraction = new PainterSceneInteraction();
        private PainterBrushPreview _brushPreview = new PainterBrushPreview();
        private PainterSelection _selection = new PainterSelection();

        public bool Painting = false;

        public void StartPainting()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;

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
            
            _selection.Setup();
            _selection.AddSelected();

            Painting = true;
        }

        public void StopPainting()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;

            _sceneInteraction.StopListening();
            _sceneInteraction.ResizePressed  -= ResizePressed;
            _sceneInteraction.ResizeHeld     -= ResizeHeld;
            _sceneInteraction.ResizeReleased -= ResizeReleased;
            _sceneInteraction.ZoomPressed    -= ZoomPressed;

            _selection.Cleanup();

            Painting = false;
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            if (_computeShader == null)
                return;

            // Dont do anything when moving cam
            if (Event.current.alt) return;

            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
                FinishPaintStroke();

            if (!_sceneInteraction.ResizingBrush) {
                _selection.OnSceneGUI(_sceneInteraction.LastMouseHit, sceneView);
                HandleLayerChange();
            }

            if (_sceneInteraction.LastMouseHit.collider != null) {
                _brushPreview.DrawBrush(_sceneInteraction.LastMouseHit, sceneView, _brushRadius, _brushSmoothness);

                if (!_sceneInteraction.ResizingBrush)
                    Paint(_sceneInteraction.LastMouseHit);
            }

            if (_sceneInteraction.ResizingBrush)
                DrawBrushResizeNotification();

            _brushPreview.OnSceneGUI();
        }

        private void HandleLayerChange()
        {
            Event currentEvent = Event.current;

            // If shift + mouse wheel, change layer
            if (currentEvent.shift && currentEvent.type == EventType.ScrollWheel) {
                if (currentEvent.delta.y > 0) _editingLayer = Mathf.Max(0, _editingLayer - 1);
                else                          _editingLayer = Mathf.Min(_editingLayer + 1, Constants.MAX_LAYERS_TERRAIN - 1);

                _brushPreview.ClearFadingPopups();
                _brushPreview.AddFadingPopup(
                    EditorApplication.timeSinceStartup + LAYER_CHANGE_POPUP_HOLD_DURATION,
                    LAYER_CHANGE_POPUP_FADE_DURATION,
                     $"Layer {_editingLayer + 1}", new Color(0.1f, 0.1f, 0.1f), true, new Vector2(0, -25)
                );

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

        private void Paint(RaycastHit mouseHit)
        {
            Event currentEvent = Event.current;

            GameObject gameObject = mouseHit.collider.gameObject;
            if (!_selection.SelectedPaintableObjects.Contains(gameObject))
                return;

            // Cannot paint if selected layer exceeds available layers, show on hover
            if (gameObject != null && _editingLayer >= (int)_selection.PaintableObjectData[gameObject].MaxLayers) {
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

            PaintableObjectData objectData = _selection.PaintableObjectData[gameObject];

            // Register control textures for undo
            foreach (Texture2D controlTexture in objectData.ControlTextures)
                Undo.RegisterCompleteObjectUndo(controlTexture, UNDO_STROKE_NAME);

            Material repetitionlessMaterial = _selection.GetFirstRepetitionlessMaterial(objectData.MeshRenderer);

            for (int i = 0; i < objectData.ControlTextures.Count; i++) {
                // Apply to the object material
                repetitionlessMaterial.SetTexture($"_Control{i}", objectData.RenderTextures[i]);
            }
        }

        private void FinishPaintStroke()
        {
            foreach (GameObject gameObject in _paintingObjects) {
                PaintableObjectData objectData = _selection.PaintableObjectData[gameObject];
                Material repetitionlessMaterial = _selection.GetFirstRepetitionlessMaterial(objectData.MeshRenderer);

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