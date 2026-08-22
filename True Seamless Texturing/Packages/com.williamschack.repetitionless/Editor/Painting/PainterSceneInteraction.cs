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

        public Action LayerDecreased;
        public Action LayerIncreased;

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
            TrackLayerChange(currentEvent);
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
        }

        private void TrackZoom(Event currentEvent, SceneView sceneView)
        {
            if (_lastMouseHit.collider == null || currentEvent.keyCode != KeyCode.F || currentEvent.type != EventType.KeyDown)
                return;

            ZoomPressed?.Invoke(sceneView, _lastMouseHit.point);
            currentEvent.Use();
        }

        private void TrackLayerChange(Event currentEvent)
        {
            // If shift + mouse wheel, change layer
            if (currentEvent.shift && currentEvent.type == EventType.ScrollWheel) {
                if (currentEvent.delta.y > 0) LayerDecreased?.Invoke();
                else                          LayerIncreased?.Invoke();

                currentEvent.Use();
            }
        }
    }
}

#endif