#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.CustomWindows
{
    using Painter;
    using Utilities.GUI;

    public class PainterWindow : EditorWindow
    {
        private int _textureResolution = 512;

        private int _editingLayer = 1;

        private Texture2D _brushTexture = null;
        private float _brushRadiusReal = 15;
        private float _brushRadius => _brushRadiusReal * 0.01f;
        private float _brushOpacity = 1.0f;
        private float _brushSmoothness = 0.5f;

        private GUIStyle _notificationBoxStyle;
        private GUIStyle _notificationLabelStyle;
        private bool _guiStylesSetup = false;

        private Painter _painter;

        [MenuItem("Window/Repetitionless/Open Painter", priority = 1)]
        public static void Open()
        {
            PainterWindow window = GetWindow<PainterWindow>(false, "Repetitionless Painter");
            window.Show();
        }

        private void CreateGUI()
        {
            _painter = new Painter();
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

        private void OnGUI()
        {
            if (!_guiStylesSetup)
                SetupGUIStyles();

            if (GUILayout.Button("Paint")) {
                if (_painter.Painting) _painter.StopPainting();
                else                   _painter.StartPainting();
            }

            _editingLayer = EditorGUILayout.IntSlider("Layer", _editingLayer + 1, 1, Constants.MAX_LAYERS_TERRAIN) - 1;

            GUILayout.Space(10);

            _brushTexture = (Texture2D)EditorGUILayout.ObjectField("Brush Texture", _brushTexture, typeof(Texture2D), false, GUILayout.Height(GUIUtilities.LINE_HEIGHT));
            _brushRadiusReal = Mathf.Max(0.01f, EditorGUILayout.FloatField("Brush Radius", _brushRadiusReal));
            _brushOpacity = EditorGUILayout.Slider("Brush Opacity", _brushOpacity, 0, 1);
            _brushSmoothness = EditorGUILayout.Slider("Brush Smoothness", _brushSmoothness, 0, 1);
        }

        private void OnDisable()
        {
            _painter.StopPainting();
        }
    }
}

#endif