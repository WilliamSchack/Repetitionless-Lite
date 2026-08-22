#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Repetitionless.Editor.Painter
{
    public class PainterSceneInteraction
    {
        // Callbacks
        public Action<EResizingProperty> ResizePressed;
        public Action<EResizingProperty> ResizeHeld;
        public Action ResizeReleased;

        public Action<SceneView, Vector3> ZoomPressed;

        // Variables
        private bool _shiftHeld = false;
        private bool _rightClickHeld = false;
        public bool ShiftHeld => _shiftHeld;
        public bool RightClickHeld => _rightClickHeld;

        private EResizingProperty _resizingProperty = EResizingProperty.None;
        public EResizingProperty ResizingProperty => _resizingProperty;

        private bool _resizingBrush = false;
        public bool ResizingBrush => _resizingBrush;

        private RaycastHit _lastMouseHit;
        public RaycastHit LastMouseHit => _lastMouseHit;

        public void Listen()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        public void StopListening()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            Event currentEvent = Event.current;

            // Disable default left click events
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            UpdateMouseHit();

            TrackHeldKeys(currentEvent);
            TrackResize(currentEvent);
            TrackZoom(currentEvent, sceneView);
        }

        // Mouse Hit
        private void UpdateMouseHit()
        {
            if (_resizingBrush)
                return;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                _lastMouseHit = hit;
                return;
            }

            _lastMouseHit = new RaycastHit();
        }

        // Keybinds
        private void TrackHeldKeys(Event currentEvent)
        {
            // Right click
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1) _rightClickHeld = true;
            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 1) _rightClickHeld = false;

            // Shift
            if (currentEvent.type == EventType.KeyDown && (currentEvent.keyCode == KeyCode.LeftShift || currentEvent.keyCode == KeyCode.RightShift)) _shiftHeld = true;
            if (currentEvent.type == EventType.KeyUp && (currentEvent.keyCode == KeyCode.LeftShift || currentEvent.keyCode == KeyCode.RightShift)) _shiftHeld = false;
        }

        private void TrackResize(Event currentEvent)
        {
            // If starting to move during resize, cancel resize
            if (_resizingBrush && _rightClickHeld) {
                _resizingBrush = false;
                _resizingProperty = EResizingProperty.None;
            }

            // Cancel on control to allow ctrl+s saving
            // Cancel on right click to avoid camera movement
            if (currentEvent.control || _rightClickHeld)
                return;

            EResizingProperty prevEResizingProperty = _resizingProperty;
            bool isResizeKey = true;
            switch (currentEvent.keyCode) {
                case KeyCode.S: _resizingProperty = EResizingProperty.Radius; break;
                case KeyCode.A: _resizingProperty = EResizingProperty.Opacity; break;
                case KeyCode.D: _resizingProperty = EResizingProperty.Smoothness; break;
                default: isResizeKey = false; break;
            }

            // Dont allow other key presses to cancel the resize
            if (_resizingBrush && !isResizeKey && (currentEvent.type == EventType.KeyDown || currentEvent.type == EventType.KeyUp))
                return;

            // Dont allow resizing multiple properties at once
            if (_resizingBrush && prevEResizingProperty != _resizingProperty) {
                _resizingProperty = prevEResizingProperty;
                return;
            }

            if (_resizingProperty == EResizingProperty.None && !_resizingBrush)
                return;

            // Start resize
            if (currentEvent.type == EventType.KeyDown && !_resizingBrush) {
                _resizingBrush = true;
                ResizePressed?.Invoke(_resizingProperty);
                currentEvent.Use();
                return;

                //_brushResizeLastMousePosX = currentEvent.mousePosition.x;
//
                //switch (_resizingProperty) {
                //    case EResizingProperty.Radius: _brushResizeStartValue = _brushRadiusReal; break;
                //    case EResizingProperty.Opacity: _brushResizeStartValue = _brushOpacity; break;
                //    case EResizingProperty.Smoothness: _brushResizeStartValue = _brushSmoothness; break;
                //}
                //
                //currentEvent.Use();
                //return;
            }

            // Finish resize
            if (currentEvent.type == EventType.KeyUp && _resizingBrush) {
                _resizingBrush = false;
                _resizingProperty = EResizingProperty.None;
                ResizeReleased?.Invoke();
                currentEvent.Use();
                return;
            }

            // Resizing
            if (currentEvent.type != EventType.MouseMove || !_resizingBrush)
                return;

            ResizeHeld(_resizingProperty);
            currentEvent.Use();

            //float delta = currentEvent.mousePosition.x - _brushResizeLastMousePosX;
            //_brushResizeLastMousePosX = currentEvent.mousePosition.x;
//
            //float sensitivityMultiplier = _shiftHeld ? BRUSH_SENSITIVITY_SHIFT_MULTIPLIER : 1.0f;
//
            //switch (_resizingProperty) {
            //    case EResizingProperty.Radius:
            //        _brushRadiusReal = Mathf.Max(0.01f, _brushRadiusReal + delta * (BRUSH_RADIUS_SENSITIVITY * sensitivityMultiplier));
            //        break;
            //    case EResizingProperty.Opacity:
            //        _brushOpacity = Mathf.Clamp01(_brushOpacity + delta * (BRUSH_OPACITY_SENSITIVITY * sensitivityMultiplier));
            //        break;
            //    case EResizingProperty.Smoothness:
            //        _brushSmoothness = Mathf.Clamp01(_brushSmoothness + delta * (BRUSH_SMOOTHNESS_SENSITIVITY * sensitivityMultiplier));
            //        break;
            //}
        }

        private void TrackZoom(Event currentEvent, SceneView sceneView)
        {
            if (_lastMouseHit.collider == null || currentEvent.keyCode != KeyCode.F || currentEvent.type != EventType.KeyDown)
                return;

            ZoomPressed?.Invoke(sceneView, _lastMouseHit.point);
            currentEvent.Use();
        }
    }
}

#endif