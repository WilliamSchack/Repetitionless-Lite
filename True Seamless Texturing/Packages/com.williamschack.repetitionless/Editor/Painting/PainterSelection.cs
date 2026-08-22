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

    public class PainterSelection
    {
        private static readonly Color SELECTION_OUTLINE_COLOUR = Color.blue;

        private int _textureResolution = 512;

        private List<GameObject> _selectedPaintableObjects = new List<GameObject>();
        private Dictionary<GameObject, PaintableObjectData> _paintableObjectData = new Dictionary<GameObject, PaintableObjectData>();
        
        public List<GameObject> SelectedPaintableObjects => _selectedPaintableObjects;
        public Dictionary<GameObject, PaintableObjectData> PaintableObjectData => _paintableObjectData;

        public void Setup()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            ObjectChangeEvents.changesPublished += ChangesPublished;
        }

        public void Cleanup()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
        }

        // Must be called in OnSceneGUI
        public void DuringSceneGUI(RaycastHit mouseHit, SceneView sceneView)
        {
            Event currentEvent = Event.current;

            // Draw outline to fake selection
            Handles.DrawOutline(_selectedPaintableObjects, SELECTION_OUTLINE_COLOUR, 0);

            // On click decide if it will be selected
            if (currentEvent.button == 0 && currentEvent.type == EventType.MouseDown) {
                // Clear selection if clicked nothing
                if (mouseHit.collider == null) {
                    RemoveAll();
                    sceneView.Repaint();
                    return;
                }

                GameObject hitObject = mouseHit.collider.gameObject;

                // If holding ctrl/shift and the object is selected, remove it
                if ((currentEvent.shift || currentEvent.control) && _selectedPaintableObjects.Contains(hitObject)) {
                    Remove(hitObject);
                    currentEvent.Use();
                }
                // Check if object is valid and add to selected
                else if (ObjectCanBeSelected(mouseHit.collider))
                    Add(hitObject);
            }
        }

        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            // Listen for when an object is deleted in the scene
            for (int i = 0; i < stream.length; i++) {
                switch (stream.GetEventType(i)) {
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                        stream.GetDestroyGameObjectHierarchyEvent(i, out DestroyGameObjectHierarchyEventArgs destroyGameObjectHierarchyEvent);
#if UNITY_6000_3_OR_NEWER
                        Object destroyedObject = EditorUtility.EntityIdToObject(destroyGameObjectHierarchyEvent.instanceId);
#else
                        Object destroyedObject = EditorUtility.InstanceIDToObject(destroyGameObjectHierarchyEvent.instanceId);
#endif

                        // There is a scene object that was deleted
                        // We dont know exactly which one but make sure none of the painting objects were deleted
                        RemoveNull();

                        break;
                }
            }
        }

        public void Add(GameObject obj)
        {
            if (_selectedPaintableObjects.Contains(obj))
                return;
            
            _selectedPaintableObjects.Add(obj);

            PaintableObjectData objectData = new PaintableObjectData {
                MeshRenderer = obj.GetComponent<MeshRenderer>()
            };

            // Need to test if:
            // Repetitionless material is removed
            Material repetitionlessMaterial = RepetitionlessLayeredMaterialUtilities.GetFirstLayeredMaterial(objectData.MeshRenderer);
            objectData.DataManager = new MaterialDataManager(repetitionlessMaterial);

            RepetitionlessMaterialDataSO materialPropertiesSO = objectData.DataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
            objectData.DataChangedAction = () => { MaterialExternalDataChanged(obj); };
            materialPropertiesSO.OnExternalDataChanged += objectData.DataChangedAction;

            // Assign texture to layered data
            // SHOULD BE CHECKED FREQUENTLY
            RepetitionlessLayeredDataSO layeredDataSO = objectData.DataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            objectData.MaxLayers = layeredDataSO.MaxLayers;

            // Make sure its mode is set to control textures
            RepetitionlessLayeredMaterialUtilities.UpdateLayerModeShader(objectData.DataManager, ELayerMode.ControlTextures);
            layeredDataSO.LayerMode = ELayerMode.ControlTextures;

            objectData.ControlTextures = new List<Texture2D>();
            objectData.RenderTextures = new List<RenderTexture>();

            int controlTextureCount = Constants.MAX_LAYERS_TERRAIN / 4;
            for (int i = 0; i < controlTextureCount; i++) {
                // Get/Create control texture
                Texture2D texture = objectData.DataManager.LoadAsset<Texture2D>($"{Constants.CONTROL_TEXTURE_FILE_NAME_PREFIX}{i}.asset");
                
                // Resize texture to target
                if (texture.width != _textureResolution || texture.height != _textureResolution) {
                    TextureUtilities.ResizeTexture(texture, _textureResolution, _textureResolution, modifyOriginal: true);
                    EditorUtility.SetDirty(texture);
                    AssetDatabase.SaveAssetIfDirty(texture);
                }

                objectData.ControlTextures.Add(texture);

                // Setup layered data
                layeredDataSO.ControlTextures[i].ChannelTextures[0].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[1].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[2].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[3].Texture = texture;
                layeredDataSO.ControlTextures[i].ChannelTextures[0].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.R, TexturePacker.TextureChannel.R);
                layeredDataSO.ControlTextures[i].ChannelTextures[1].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.G, TexturePacker.TextureChannel.G);
                layeredDataSO.ControlTextures[i].ChannelTextures[2].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.B, TexturePacker.TextureChannel.B);
                layeredDataSO.ControlTextures[i].ChannelTextures[3].FromToChannels[0] = new TexturePacker.FromToChannel(TexturePacker.TextureChannel.A, TexturePacker.TextureChannel.A);

                // Create render texture
                RenderTexture renderTexture = new RenderTexture(_textureResolution, _textureResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear) {
                    enableRandomWrite = true,
                    filterMode = FilterMode.Point
                };
                renderTexture.Create();

                // Copy control texture to the rt
                Graphics.Blit(texture, renderTexture);

                objectData.RenderTextures.Add(renderTexture);
            }
            
            layeredDataSO.Save();

            _paintableObjectData.Add(obj, objectData);
        }

        // Check all selected objects and add paintable ones
        public void AddSelected()
        {
            foreach (Object selectedObject in Selection.objects) {
                if (selectedObject is not GameObject) continue;

                GameObject selectedGameObject = (GameObject)selectedObject;
                if (ObjectCanBeSelected(selectedGameObject))
                    Add(selectedGameObject);
            }

            // INSTEAD OF CLEARING, CACHE SELECTION AND RESELECT ON DISABLE
            Selection.objects = new Object[] {};
        }

        public void Remove(GameObject obj)
        {
            if (!_selectedPaintableObjects.Contains(obj))
                return;
            
            _selectedPaintableObjects.Remove(obj);

            // Clear Render Textures
            PaintableObjectData objectData = _paintableObjectData[obj];
            foreach (RenderTexture rt in objectData.RenderTextures)
                rt.Release();

            RepetitionlessMaterialDataSO materialPropertiesSO = objectData.DataManager.LoadAsset<RepetitionlessMaterialDataSO>(Constants.PROPERTIES_FILE_NAME);
            materialPropertiesSO.OnExternalDataChanged -= objectData.DataChangedAction;

            _paintableObjectData.Remove(obj);
        }

        // Removes all objects that have been deleted from the painted list
        public void RemoveNull()
        {
            List<GameObject> destroyedObjects = _selectedPaintableObjects.Where(obj => obj == null).ToList();
            foreach (GameObject gameObject in destroyedObjects)
                Remove(gameObject);
        }

        public void RemoveAll()
        {
            // Loop backwards to allow removing elements during loop
            for (int i = _selectedPaintableObjects.Count - 1; i >= 0; i--)
                Remove(_selectedPaintableObjects[i]);
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

            Material repetitionlessMaterial = RepetitionlessLayeredMaterialUtilities.GetFirstLayeredMaterial(meshRenderer);

            // If the repetitionless material is using the terrain shader, dont allow either
            // Need to add a message to change
            if (repetitionlessMaterial == null) return false;

            return true;
        }

        private void MaterialExternalDataChanged(GameObject obj)
        {
            PaintableObjectData objectData = _paintableObjectData[obj];
            RepetitionlessLayeredDataSO layeredDataSO = objectData.DataManager.LoadAsset<RepetitionlessLayeredDataSO>(Constants.LAYERED_DATA_FILE_NAME);

            // If max layers is changed
            if (layeredDataSO.MaxLayers != objectData.MaxLayers)
                objectData.MaxLayers = layeredDataSO.MaxLayers;
                
        }
    }
}
#endif