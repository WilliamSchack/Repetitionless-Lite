#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Utilities.Texture;

    public class Brush : EditorWindow
    {
        private const string PAINT_TEXTURE_COMPUTE_RESOURCES_PATH = "repetitionless_PaintControlTexture";
        private const int COMPUTE_THREADS_X = 8;
        private const int COMPUTE_THREADS_Y = 8;

        private static readonly Color SELECTION_OUTLINE_COLOUR = Color.blue;

        ComputeShader _computeShader = null;

        private int _editingLayer = 1;
        private int _textureResolution = 2048;
        private float _brushRadius = 0.1f;

        List<GameObject> _selectedPaintableObjects = new List<GameObject>();
        Dictionary<GameObject, MeshRenderer> _paintableObjectRenderers = new Dictionary<GameObject, MeshRenderer>();

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
                if (ObjectCanBeSelected(selectedObject)) {
                    _selectedPaintableObjects.Add(selectedObject);
                    _paintableObjectRenderers.Add(selectedObject, selectedObject.GetComponent<MeshRenderer>());
                }
            }

            // INSTEAD OF CLEARING, CACHE SELECTION AND RESELECT ON DISABLE
            Selection.objects = new Object[] {};
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            if (_computeShader == null)
                return;

            // Disable default left click events
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            // Draw custom outline to fake selection
            Handles.DrawOutline(_selectedPaintableObjects, SELECTION_OUTLINE_COLOUR, 0);

            Event currentEvent = Event.current;

            // Dont paint when moving cam
            if (currentEvent.alt) return;

            RaycastHit mouseHit = GetMouseHit();
            if (mouseHit.collider == null) return;

            HandleSelection(mouseHit);

            // Always draw brush if hovering something
            Handles.DrawSolidDisc(mouseHit.point, mouseHit.normal, 0.1f);
            sceneView.Repaint();

            if (currentEvent.button != 0 || (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag))
                return;

            if (!_selectedPaintableObjects.Contains(mouseHit.collider.gameObject))
                return;

            // == Test with the first control for now

            // If no texture exists, create a blank one

            // Need to test if:
            // Repetitionless material is removed
            Material repetitionlessMaterial = GetFirstRepetitionlessMaterial(_paintableObjectRenderers[mouseHit.collider.gameObject]);
            MaterialDataManager dataManager = new MaterialDataManager(repetitionlessMaterial);

            RepetitionlessLayeredDataSO layeredDataSO = dataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            Texture2D texture = dataManager.LoadAsset<Texture2D>(Constants.CONTROL_TEXTURE_FILE_NAME_PREFIX + "0.asset"); 
            if (texture.width != _textureResolution) {
                TextureUtilities.ResizeTexture(texture, _textureResolution, _textureResolution, modifyOriginal: true);
                EditorUtility.SetDirty(texture);
                AssetDatabase.SaveAssetIfDirty(texture);
            }

            // Should check for each channel
            layeredDataSO.ControlTextures[0].ChannelTextures[0].Texture = texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[1].Texture = texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[2].Texture = texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[3].Texture = texture;
            layeredDataSO.ControlTextures[0].ChannelTextures[0].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.R, TexturePacker.TextureChannel.R);
            layeredDataSO.ControlTextures[0].ChannelTextures[1].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.G, TexturePacker.TextureChannel.G);
            layeredDataSO.ControlTextures[0].ChannelTextures[2].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.B, TexturePacker.TextureChannel.B);
            layeredDataSO.ControlTextures[0].ChannelTextures[3].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.A, TexturePacker.TextureChannel.A);
            layeredDataSO.Save();

            // Apply mask

            // Create Render Texture (CACHE THIS PLEASE)
            RenderTexture rt = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32) {
                enableRandomWrite = true,
                filterMode = texture.filterMode
            };
            rt.Create();
            Graphics.Blit(texture, rt); // Copy texture to rt

            int kernel = _computeShader.FindKernel("CSMain");
            _computeShader.SetTexture(kernel, "ControlTexture", rt);
            _computeShader.SetVector("HitUV", new Vector4(mouseHit.textureCoord.x, mouseHit.textureCoord.y, 0, 0));
            _computeShader.SetFloat("Radius", _brushRadius);
            _computeShader.SetInt("TargetChannel", _editingLayer % 4);

            int groupsX = Mathf.CeilToInt(texture.width  / (float)COMPUTE_THREADS_X);
            int groupsY = Mathf.CeilToInt(texture.height / (float)COMPUTE_THREADS_Y);

            _computeShader.Dispatch(kernel, groupsX, groupsY, 1);

            // Read result and apply back to texture
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture.active = rt;

            texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previousRT;
            rt.Release();
        }

        private void HandleSelection(RaycastHit mouseHit)
        {
            Event currentEvent = Event.current;

            // On click decide if it will be selected
            if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown) {
                GameObject hitObject = mouseHit.collider.gameObject;

                // Check if object is valid and add to selected
                if (ObjectCanBeSelected(mouseHit.collider)) {
                    if (!_selectedPaintableObjects.Contains(hitObject)) {
                        _selectedPaintableObjects.Add(hitObject);
                        _paintableObjectRenderers.Add(hitObject, hitObject.GetComponent<MeshRenderer>());
                    }
                }

                // If holding shift and the object is selected, remove it
                if (currentEvent.shift && _selectedPaintableObjects.Contains(hitObject)) {
                    if (_selectedPaintableObjects.Contains(hitObject)) {
                        _selectedPaintableObjects.Remove(hitObject);
                        _paintableObjectRenderers.Remove(hitObject);
                    }
                }

                // If select into the void, deselect all

                // If ctrl clicking, deselect all
            }
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