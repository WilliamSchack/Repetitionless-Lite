#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor.CustomWindows
{
    using Painter;
    using Materials;
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
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;

            _painter = new Painter();
            _painter.OnPropertyChanged += Repaint;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;

            _painter.StopPainting();
        }
        
        private void OnGUI()
        {
            GUIUtilities.BeginBackgroundVertical();

            EditorGUI.BeginChangeCheck();
            GUIUtilities.DrawMajorToggleButton(_painter.Painting, "Painting");
            if (EditorGUI.EndChangeCheck())
                _painter.TogglePainting();

            GUI.enabled = _painter.LastPaintedObject != null;
            if (GUILayout.Button("Edit Last Painted Material")) {
                // Get the material and select it
                MeshRenderer meshRenderer = _painter.LastPaintedObject.GetComponent<MeshRenderer>(); // MeshRendere is required to paint
                Material material = RepetitionlessLayeredMaterialUtilities.GetFirstLayeredMaterial(meshRenderer);
                Selection.activeObject = material;
            }
            GUI.enabled = true;

            GUIUtilities.EndBackgroundVertical();

            GUIUtilities.BeginBackgroundVertical();
            GUILayout.Space(5);

            EditorGUILayout.LabelField("Painting Settings", EditorStyles.boldLabel);            
            _painter.EditingLayer = EditorGUILayout.IntSlider("Painting Layer", _painter.EditingLayer + 1, 1, Constants.MAX_LAYERS_TERRAIN) - 1;
            _painter.TextureResolution = EditorGUILayout.IntField(new GUIContent("Control Resolution", "The resolution of the control textures"), _painter.TextureResolution);

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Brush Settings", EditorStyles.boldLabel);            
            _painter.BrushTexture = (Texture2D)EditorGUILayout.ObjectField("Brush Texture", _painter.BrushTexture, typeof(Texture2D), false, GUILayout.Height(GUIUtilities.LINE_HEIGHT));
            _painter.BrushRadiusReal = Mathf.Max(0.01f, EditorGUILayout.FloatField("Brush Radius", _painter.BrushRadiusReal));
            _painter.BrushOpacity = EditorGUILayout.Slider("Brush Opacity", _painter.BrushOpacity, 0, 1);
            _painter.BrushSmoothness = EditorGUILayout.Slider("Brush Smoothness", _painter.BrushSmoothness, 0, 1);
            _painter.BrushRotationDegrees = EditorGUILayout.Slider("Brush Rotation", _painter.BrushRotationDegrees, 0, 360);
            GUIUtilities.EndBackgroundVertical();
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.G) {
                _painter.TogglePainting();
                Repaint();
            }
        }
    }
}

#endif