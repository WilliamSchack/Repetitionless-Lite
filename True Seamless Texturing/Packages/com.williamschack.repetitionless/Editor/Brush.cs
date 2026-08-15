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

    public class Brush : EditorWindow
    {
        private struct PaintableObjectData
        {
            public MaterialDataManager DataManager;
            public MeshRenderer MeshRenderer;
            public RenderTexture RenderTexture;
            public Texture2D Texture;
        }

        private const string PAINT_TEXTURE_COMPUTE_RESOURCES_PATH = "repetitionless_PaintControlTexture";
        private const int COMPUTE_THREADS_X = 8;
        private const int COMPUTE_THREADS_Y = 8;

        private static readonly Color SELECTION_OUTLINE_COLOUR = Color.blue;

        ComputeShader _computeShader = null;

        private int _editingLayer = 1;
        private int _textureResolution = 2048;
        private float _brushRadiusReal = 15;
        private float _brushRadius => _brushRadiusReal * 0.01f;

        private Texture2D _brushTexture = null;

        List<GameObject> _selectedPaintableObjects = new List<GameObject>();
        Dictionary<GameObject, PaintableObjectData> _paintableObjectData = new Dictionary<GameObject, PaintableObjectData>();

        List<GameObject> _paintingObjects = new List<GameObject>();

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

            _computeShader = Resources.Load<ComputeShader>(PAINT_TEXTURE_COMPUTE_RESOURCES_PATH);
            if (_computeShader == null)
                Debug.LogError("No texture paint compute shader found...");

            // Check all selected objects and add paintable ones
            foreach (GameObject selectedObject in Selection.objects) {
                if (ObjectCanBeSelected(selectedObject))
                    SelectionAdd(selectedObject);
            }

            // INSTEAD OF CLEARING, CACHE SELECTION AND RESELECT ON DISABLE
            Selection.objects = new Object[] {};
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void OnGUI()
        {
            _editingLayer = EditorGUILayout.IntSlider("Layer", _editingLayer, 0, 3);
            _brushRadiusReal = Mathf.Max(0, EditorGUILayout.FloatField("Brush Radius", _brushRadiusReal));
            _brushTexture = (Texture2D)EditorGUILayout.ObjectField("Brush Texture", _brushTexture, typeof(Texture2D), false, GUILayout.Height(GUIUtilities.LINE_HEIGHT));
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

            RaycastHit mouseHit = GetMouseHit();
            HandleSelection(mouseHit, sceneView);
            if (mouseHit.collider == null) return;

            DrawBrush(mouseHit, sceneView);
            Paint(mouseHit);
        }

        private void HandleSelection(RaycastHit mouseHit, SceneView sceneView)
        {
            Event currentEvent = Event.current;

            // On click decide if it will be selected
            if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown) {
                // Clear selection if clicked nothing
                if (mouseHit.collider == null) {
                    // Clear render textures
                    foreach (PaintableObjectData objectData in _paintableObjectData.Values)
                        objectData.RenderTexture.Release();

                    _selectedPaintableObjects.Clear();
                    _paintableObjectData.Clear();
                    sceneView.Repaint();

                    return;
                }

                GameObject hitObject = mouseHit.collider.gameObject;

                // Check if object is valid and add to selected
                if (ObjectCanBeSelected(mouseHit.collider))
                    SelectionAdd(hitObject);

                // If holding shift and the object is selected, remove it
                if (currentEvent.shift && _selectedPaintableObjects.Contains(hitObject))
                    SelectionRemove(hitObject);
            }
        }

        private void DrawBrush(RaycastHit mouseHit, SceneView sceneView)
        {
            // Always draw brush if hovering something
            Handles.DrawSolidDisc(mouseHit.point, mouseHit.normal, _brushRadius);
            
            sceneView.Repaint();
        }

        private void Paint(RaycastHit mouseHit)
        {
            Event currentEvent = Event.current;

            if (currentEvent.button != 0)
                return;

            GameObject gameObject = mouseHit.collider.gameObject;
            if (!_selectedPaintableObjects.Contains(gameObject))
                return;

            if (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag)
                return;

            // == Test with the first control for now

            PaintableObjectData objectData = _paintableObjectData[gameObject];

            // If stroke just passed over object, initialise painting
            if (!_paintingObjects.Contains(gameObject))
                InitialisePainting(gameObject);

            // Dispatch paint compute shader
            int kernel = _computeShader.FindKernel("CSMain");
            _computeShader.SetTexture(kernel, "ControlTexture", objectData.RenderTexture);
            _computeShader.SetVector("HitUV", new Vector4(mouseHit.textureCoord.x, mouseHit.textureCoord.y, 0, 0));
            _computeShader.SetFloat("Radius", _brushRadius);
            _computeShader.SetInt("TargetChannel", _editingLayer % 4);

            int groupsX = Mathf.CeilToInt(objectData.Texture.width  / (float)COMPUTE_THREADS_X);
            int groupsY = Mathf.CeilToInt(objectData.Texture.height / (float)COMPUTE_THREADS_Y);

            _computeShader.Dispatch(kernel, groupsX, groupsY, 1);
        }

        private void InitialisePainting(GameObject gameObject)
        {
            PaintableObjectData objectData = _paintableObjectData[gameObject];

            _paintingObjects.Add(gameObject);

            // Apply render texture to material
            Graphics.Blit(objectData.Texture, objectData.RenderTexture); // Copy texture to rt

            Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(objectData.MeshRenderer);
            repetitionlessMaterial.SetTexture("_Control0", objectData.RenderTexture);
        }

        private void FinishPaintStroke()
        {
            foreach (GameObject gameObject in _paintingObjects) {
                PaintableObjectData objectData = _paintableObjectData[gameObject];

                // Save rt to texture
                RenderTexture previousRT = RenderTexture.active;
                RenderTexture.active = objectData.RenderTexture;

                objectData.Texture.ReadPixels(new Rect(0, 0, objectData.Texture.width, objectData.Texture.height), 0, 0);
                objectData.Texture.Apply();

                RenderTexture.active = previousRT;

                // Apply texture material
                Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(objectData.MeshRenderer);
                repetitionlessMaterial.SetTexture("_Control0", objectData.Texture);
            }

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

            // Get/Create control texture

            // Need to test if:
            // Repetitionless material is removed
            Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(objectData.MeshRenderer);
            objectData.DataManager = new MaterialDataManager(repetitionlessMaterial);

            objectData.Texture = objectData.DataManager.LoadAsset<Texture2D>(Constants.CONTROL_TEXTURE_FILE_NAME_PREFIX + "0.asset"); 
            if (objectData.Texture.width != _textureResolution) {
                TextureUtilities.ResizeTexture(objectData.Texture, _textureResolution, _textureResolution, modifyOriginal: true);
                EditorUtility.SetDirty(objectData.Texture);
                AssetDatabase.SaveAssetIfDirty(objectData.Texture);
            }

            // Assign texture to layered data
            // SHOULD BE CHECKED FREQUENTLY
            RepetitionlessLayeredDataSO layeredDataSO = objectData.DataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            // Make sure its mode is set to control textures
            RepetitionlessLayeredMaterialUtilities.UpdateLayerModeShader(objectData.DataManager, ELayerMode.ControlTextures);
            layeredDataSO.LayerMode = ELayerMode.ControlTextures;

            layeredDataSO.ControlTextures[0].ChannelTextures[0].Texture = objectData.Texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[1].Texture = objectData.Texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[2].Texture = objectData.Texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[3].Texture = objectData.Texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[0].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.R, TexturePacker.TextureChannel.R);
            layeredDataSO.ControlTextures[0].ChannelTextures[1].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.G, TexturePacker.TextureChannel.G);
            layeredDataSO.ControlTextures[0].ChannelTextures[2].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.B, TexturePacker.TextureChannel.B);
            layeredDataSO.ControlTextures[0].ChannelTextures[3].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.A, TexturePacker.TextureChannel.A);
            
            layeredDataSO.Save();

            // Create render texture
            objectData.RenderTexture = new RenderTexture(objectData.Texture.width, objectData.Texture.height, 0, RenderTextureFormat.ARGB32) {
                enableRandomWrite = true,
                filterMode = objectData.Texture.filterMode
            };

            objectData.RenderTexture.Create();

            _paintableObjectData.Add(obj, objectData);
        }

        private void SelectionRemove(GameObject obj)
        {
            if (!_selectedPaintableObjects.Contains(obj))
                return;
            
            _selectedPaintableObjects.Remove(obj);

            // Clear Render Texture
            PaintableObjectData objectData = _paintableObjectData[obj];
            objectData.RenderTexture.Release();

            _paintableObjectData.Remove(obj);
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