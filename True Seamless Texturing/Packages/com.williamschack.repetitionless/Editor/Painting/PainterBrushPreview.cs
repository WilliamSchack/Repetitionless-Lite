#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Repetitionless.Editor.Painter
{
    public class PainterBrushPreview
    {
        private static readonly Vector2 MOUSE_NOTIFICATION_OFFSET = new Vector2(20, 0); 

        private GUIStyle _notificationBoxStyle;
        private GUIStyle _notificationLabelStyle;
        private bool _notificationStylesSetup = false;

        private void SetupNotificationGUIStyles()
        {
            _notificationStylesSetup = true;

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

        public void DrawBrush(RaycastHit mouseHit, SceneView sceneView, float radius, float smoothness)
        {
            // Always draw brush if hovering something
            
            Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, radius, 3);

            // Draw smoothness disc
            float innerRadius = radius * (1 - smoothness);
            Handles.DrawWireDisc(mouseHit.point, mouseHit.normal, innerRadius, 1);

            sceneView.Repaint();
        }

        public void DrawMousePopup(string label, Color backgroundColor, bool alphaToColour = false)
        {
            DrawMousePopup(label, backgroundColor, alphaToColour, Vector2.zero);
        }

        public void DrawMousePopup(string label, Color backgroundColor, bool alphaToColour, Vector2 positionOffset)
        {
            if (!_notificationStylesSetup || _notificationBoxStyle?.normal.background == null)
                SetupNotificationGUIStyles();

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
    }
}
#endif