#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.CustomWindows
{
    using Painter;
    using Utilities.GUI;

    public class PainterWindow : EditorWindow
    {
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
            _painter.OnPropertyChanged += Repaint;
        }

        private void OnDisable()
        {
            _painter.StopPainting();
        }
        
        private void OnGUI()
        {
            if (GUILayout.Button("Paint")) {
                if (_painter.Painting) _painter.StopPainting();
                else                   _painter.StartPainting();
            }

            _painter.EditingLayer = EditorGUILayout.IntSlider("Layer", _painter.EditingLayer + 1, 1, Constants.MAX_LAYERS_TERRAIN) - 1;

            GUILayout.Space(10);

            _painter.BrushTexture = (Texture2D)EditorGUILayout.ObjectField("Brush Texture", _painter.BrushTexture, typeof(Texture2D), false, GUILayout.Height(GUIUtilities.LINE_HEIGHT));
            _painter.BrushRadiusReal = Mathf.Max(0.01f, EditorGUILayout.FloatField("Brush Radius", _painter.BrushRadiusReal));
            _painter.BrushOpacity = EditorGUILayout.Slider("Brush Opacity", _painter.BrushOpacity, 0, 1);
            _painter.BrushSmoothness = EditorGUILayout.Slider("Brush Smoothness", _painter.BrushSmoothness, 0, 1);
        }
    }
}

#endif