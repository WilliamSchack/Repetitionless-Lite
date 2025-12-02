// ---------------------------------------------------
// | Script created by me taken from my other asset: |
// | Texture Array Essentials (https://u3d.as/3s4d)  |
// ---------------------------------------------------

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Repetitionless.GUIUtilities
{
    using Compression;
    using TextureUtilities;
    using Data;
    using CustomDialog;

    /// <summary>
    /// Allows drawing textures stored in a Texture2DArray to the GUI as well as functions for reading and deleting the array<br />
    /// Automatically creates and manages its own array when textures are modified
    /// </summary>
    public class TextureArrayCustomChannelsGUIDrawer
    {
        // Array
        private Texture2DArray _array; 
        private string _fileName;

        /// <summary>
        /// The array used for this field
        /// </summary>
        public Texture2DArray Array { get { return _array; } }

        // Array Settings

        /// <summary>
        /// The texture format of the array
        /// </summary>
        public TextureFormat TextureFormat = TextureFormat.DXT5;

        /// <summary>
        /// If mipmaps will be transferred from the texture to the array if possible
        /// </summary>
        public bool TransferMipmaps = true;

        /// <summary>
        /// If the output array will be linear<br />
        /// Recommended in the Built-In Render Pipeline only when including normal maps<br />
        /// Not Recommended in URP/HDRP as it will result in brighter textures
        /// </summary>
        public bool ArrayLinear = false;

        // Material
        private Object _material;
        private MaterialProperty _arrayProperty;
        private MaterialProperty _assignedTexturesProperty;

        // Texture variables
        private Texture2D[] _textures;
        private int _textureCount;
        private bool[] _assignedTextures;

        private MaterialDataManager _dataManager;
        private System.Func<int, TexturePacker.TextureData[]> _getLayerChannelDataFunc;
        private System.Action _saveTextureDataAction;
        private List<Texture2D[]> _resizedTextures = new List<Texture2D[]>();
        private Vector4 _defaultChannelColours;

        /// <summary>
        /// Create a TextureArrayGUIDrawer using the Array and Assigned Textures properties<br />
        /// The Texture2DArray asset will be stored in a folder accompanying the material. Can be moved after creation
        /// </summary>
        /// <param name="arrayProperty">
        /// The material property for the Array (Texture2DArray)
        /// </param>
        /// <param name="assignedTexturesProperty">
        /// The material property for the Assigned Textures (Float)
        /// </param>
        /// <param name="textureCount">
        /// The max amount of textures this array will hold
        /// </param>
        /// <param name="fileName">
        /// The filename of the Texture2DArray asset stored in a folder accompanying the material<br />
        /// Used only on creation of the asset. Array is retrieved from the material afterwards
        /// </param>
        public TextureArrayCustomChannelsGUIDrawer(MaterialProperty arrayProperty, MaterialProperty assignedTexturesProperty, int textureCount, string fileName = null)
        {
            // Assign material
            _material = arrayProperty.targets[0];

            // Initialise
            Init(arrayProperty, assignedTexturesProperty, textureCount, fileName);
        }

        /// <summary>
        /// Create a TextureArrayGUIDrawer using a material and property names<br />
        /// The Texture2DArray asset will be stored in a folder accompanying the material. Can be moved after creation
        /// </summary>
        /// <param name="material">
        /// The targeted material
        /// </param>
        /// <param name="arrayPropertyName">
        /// The reference name for the Array (Texture2DArray) shader property
        /// </param>
        /// <param name="assignedTexturesPropertyName">
        /// The reference name for the Assigned Textures (Float) shader property
        /// </param>
        /// <param name="textureCount">
        /// The max amount of textures this array will hold
        /// </param>
        /// <param name="fileName">
        /// The filename of the Texture2DArray asset stored in a folder accompanying the material<br />
        /// Used only on creation of the asset. Can be changed as long as it is assigned in the material.
        /// </param>
        public TextureArrayCustomChannelsGUIDrawer(Material material, string arrayPropertyName, string assignedTexturesPropertyName, int textureCount, string fileName = null)
        {
            // Assign material and variables
            _material = material;
            
            // Check if properties are valid
            if (!PropertiesValid(arrayPropertyName, assignedTexturesPropertyName))
                return;

            MaterialProperty arrayProperty = MaterialEditor.GetMaterialProperty(new Object[] { material }, arrayPropertyName);
            MaterialProperty assignedTexturesProperty = MaterialEditor.GetMaterialProperty(new Object[] { material }, assignedTexturesPropertyName);

            // Initialise
            Init(arrayProperty, assignedTexturesProperty, textureCount, fileName);
        }

        public TextureArrayCustomChannelsGUIDrawer(MaterialDataManager dataManager, System.Func<int, TexturePacker.TextureData[]> getLayerChannelData, System.Action saveTextureDataAction, Vector4 defaultChannelColours, MaterialProperty arrayProperty, MaterialProperty assignedTexturesProperty, int textureCount, string fileName = null)
        {
            // Assign material
            _material = arrayProperty.targets[0];
            
            _dataManager = dataManager;
            _getLayerChannelDataFunc = getLayerChannelData;
            _saveTextureDataAction = saveTextureDataAction;
            _defaultChannelColours = defaultChannelColours;

            // Setup resized textures arrays
            for (int i = 0; i < textureCount; i++) {
                _resizedTextures.Add(new Texture2D[_getLayerChannelDataFunc(i).Length]);
            }

            // Initialise
            Init(arrayProperty, assignedTexturesProperty, textureCount, fileName);
        }

        /// <summary>
        /// Checks if the Array and Assigned Textures properties are valid
        /// </summary>
        private bool PropertiesValid(string arrayPropertyName, string assignedTexturesPropertyName)
        {
            Material material = (Material)_material;

            if (!material.HasProperty(arrayPropertyName)) {
                Debug.LogError($"Could not find Array property in {_material.name}. Please check shader property name: \"{arrayPropertyName}\"");
                return false;
            }

#if UNITY_6000_2_OR_NEWER
            if (_arrayProperty.propertyType != UnityEngine.Rendering.ShaderPropertyType.Texture
#else
            if (_arrayProperty.type != MaterialProperty.PropType.Texture
#endif
                || (_arrayProperty.textureValue != null && _arrayProperty.textureValue is not Texture2DArray))
            {
                Debug.LogError($"Array property in ({_material.name}) is not a Texture2DArray. Please change the shader property type or check the property name: \"{arrayPropertyName}\"");
                return false;
            }

            if (!material.HasProperty(assignedTexturesPropertyName)) {
                Debug.LogError($"Could not find Assigned Textures property in {_material.name}. Please check shader property name: \"{assignedTexturesPropertyName}\"");
                return false;
            }

#if UNITY_6000_2_OR_NEWER
            if (_assignedTexturesProperty.propertyType != UnityEngine.Rendering.ShaderPropertyType.Float)
#else
            if (_assignedTexturesProperty.type != MaterialProperty.PropType.Float)
#endif
            {
                Debug.LogError($"Assigned Textures property in ({_material.name}) is not a float. Please change the shader property type or check the property name: \"{assignedTexturesPropertyName}\"");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initialises this TextureArrayGUIDrawer<br />
        /// Preferably create a new object rather than calling this but it can be called if _material is assigned before.
        /// </summary>
        /// <param name="arrayProperty">
        /// The material property for the Array (Texture2DArray)
        /// </param>
        /// <param name="assignedTexturesProperty">
        /// The material property for the Assigned Textures (Float)
        /// </param>
        /// <param name="textureCount">
        /// The max amount of textures this array will hold
        /// </param>
        /// <param name="fileName">
        /// The filename of the Texture2DArray asset stored in a folder accompanying the material<br />
        /// Used only on creation of the asset. Can be changed as long as it is assigned in the material.
        /// </param>;
        private void Init(MaterialProperty arrayProperty, MaterialProperty assignedTexturesProperty, int textureCount, string fileName = null)
        {
            // Assign variables
            _arrayProperty = arrayProperty;
            _assignedTexturesProperty = assignedTexturesProperty;
            _assignedTextures = new bool[textureCount];

            _textureCount = textureCount;
            _textures = new Texture2D[textureCount];

            // Auto assign file name if not set: "TextureArray_ArrayPropertyDisplayName.asset"
            if (fileName == null) {
                string legalDisplayName = string.Join("_", arrayProperty.displayName.Split(System.IO.Path.GetInvalidFileNameChars())); // Remove invalid characters
                legalDisplayName = legalDisplayName.Replace(" ", "_"); // Remove spaces

                fileName = $"TextureArray_{legalDisplayName}.asset";
            }
            else if (!fileName.EndsWith(".asset")) {
                fileName += ".asset";
            }

            _fileName = fileName;

            // Load array
            Texture2DArray array = _dataManager.LoadAsset<Texture2DArray>(_fileName);

            // If array exists load textures and uncompress assigned textures
            if (array != null)
            {
                // Get textures from array
                Texture2D[] textures = Texture2DArrayUtilities.GetTextures(array);

                // Get assigned textures
                int compressedAssignedTextures = (int)assignedTexturesProperty.floatValue;
                bool[] assignedTextures = BooleanCompression.GetValues(compressedAssignedTextures, _textures.Length);

                // Add textures to array in correct positions
                int currentIndex = 0;
                for (int i = 0; i < assignedTextures.Length; i++)
                {
                    if (assignedTextures[i])
                    {
                        _textures[i] = textures[currentIndex];
                        currentIndex++;
                    }
                }

                _assignedTextures = assignedTextures;
            }

            _array = array;
        }

        /// <summary>
        /// Changes the array to a new one<br />
        /// !! Assumes no textures were added or removed !!
        /// </summary>
        /// <param name="newArray">
        /// The new array
        /// </param>
        public void UpdateArray(Texture2DArray newArray)
        {
            _arrayProperty.textureValue = newArray;
            _array = newArray;
        }

        /// <summary>
        /// Updates the texture in the array while handling its order, asset file, and material variables<br />
        /// Automatically packs textures, specifically updating the texture at the input channelIndex<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="newTexture">
        /// The newly assigned texture
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="channelIndex">
        /// Index of the channel texture being changed at this index<br />
        /// Corresponds to the index in the initial given channelTexturesData
        /// </param>
        public Texture2D UpdateTexture(Texture2D newTexture, int index, int channelIndex, bool force = false)
        {
            if (UnityEditor.EditorGUI.EndChangeCheck()) {
                TexturePacker.TextureData channelTextureData = _getLayerChannelDataFunc(index)[channelIndex];

                // Return if texture is not being changed, usually due to updates for accompanying variable
                if (newTexture == channelTextureData.Texture && !force)
                    return newTexture;

                // Register Undo, cannot if material or array are null
                if(_material != null && _array != null)
                    Undo.RegisterCompleteObjectUndo(new Object[] { _material, _array }, $"Modified Array Texture of {_material.name} at Index {index}");

                // Check if texture is assigned or removed
                int channelTexturesAssigned = _getLayerChannelDataFunc(index).Count(x => x.Texture != null);
                if (newTexture == null) channelTexturesAssigned--;
                else                    channelTexturesAssigned++;

                bool textureAssigned = channelTexturesAssigned > 0;

                // If all textures are null, clear array and return
                if (!textureAssigned && _textures.Count(x => x != null) <= 1) {
                    _dataManager.DeleteAsset(_fileName);

                    _assignedTextures[index] = false;
                    _textures[index] = null;
                    _getLayerChannelDataFunc(index)[channelIndex].Texture = null;
                    _saveTextureDataAction();

                    _arrayProperty.textureValue = _array;
                    _assignedTexturesProperty.floatValue = BooleanCompression.CompressValues(_assignedTextures);

                    return newTexture;
                }

                // If texture resolution is not multiple of 4 return
                if (textureAssigned && newTexture != null && (newTexture.width % 4 != 0 || newTexture.height % 4 != 0)) {
                    Debug.LogWarning("Cannot update array, Texture resolution must be a multiple of 4");
                    return newTexture;
                }

                // Make sure this texture is a normal map if set
                if (channelTextureData.NormalMap && !TextureUtilities.TextureIsNormal(newTexture)) {
                    bool convertingNormalMap = ShaderGUIDialog.DisplayDialog(
                        "Invalid Normal Map",
                        "The inputted normal map is not assigned as a normal map. Would you like to convert it to one?",
                        "Yes", "No"
                    );

                    if (convertingNormalMap)
                        TextureUtilities.SetTextureToNormal(newTexture);
                }

                // Copy texture data to modify
                TexturePacker.TextureData[] clonedTextureData = new TexturePacker.TextureData[_getLayerChannelDataFunc(index).Length];
                System.Array.Copy(_getLayerChannelDataFunc(index), clonedTextureData, _getLayerChannelDataFunc(index).Length);
                clonedTextureData[channelIndex].Texture = newTexture;

                // Check for resolution differences
                //if (_array != null) {
                    Vector2Int newArrayResolution;
                    if (_array != null) newArrayResolution = new Vector2Int(_array.width, _array.height);
                    else newArrayResolution = new Vector2Int(clonedTextureData[0].Texture.width, clonedTextureData[0].Texture.height);

                    for (int i = 0; i < clonedTextureData.Length; i++) {
                        Texture2D texture = clonedTextureData[i].Texture;

                        if (texture == null)
                            continue;

                        if (texture.width != newArrayResolution.x || texture.height != newArrayResolution.y) {
                            // Dont prompt if already resized a texture
                            Texture2D preResizedTexture = _resizedTextures[index][i];
                            if (preResizedTexture != null && preResizedTexture.width == newArrayResolution.x && preResizedTexture.height == newArrayResolution.y) {
                                clonedTextureData[i].Texture = preResizedTexture;
                                continue;
                            }

                            int returned = ShaderGUIDialog.DisplayDialogComplex(
                                "Texture Resolution Difference",
                                $"{texture.name}: {texture.width}x{texture.height} Array: {newArrayResolution.x}x{newArrayResolution.y}\n"
                                + "Texture size is not the same as the array. Would you like to resize this texture to the array resolution, or resize the array to this texture resolution?",
                                "Resize Texture", "Cancel", "Resize Array"
                            );

                            // If resizing, dont do anything it will be resized later

                            // If cancelling, abort updating the texture
                            if (returned == 1) {
                                return newTexture;
                            }

                            // If resizing the array, queue a new array size
                            else if (returned == 2) {
                                newArrayResolution = new Vector2Int(texture.width, texture.height);
                            }
                        }
                    }

                    // Resize textures that need to be resized
                    for (int i = 0; i < clonedTextureData.Length; i++) {
                        // Check if texture needs to be resized
                        Texture2D checkingTexture = clonedTextureData[i].Texture;
                        if (checkingTexture == null || (checkingTexture.width == newArrayResolution.x && checkingTexture.height == newArrayResolution.y))
                            continue;

                        // Resize texture and save for later use
                        clonedTextureData[i].Texture = TextureUtilities.ResizeTexture(clonedTextureData[i].Texture, newArrayResolution.x, newArrayResolution.y);
                        _resizedTextures[index][i] = clonedTextureData[i].Texture;
                    }
                //}

                // Pack texture
                _getLayerChannelDataFunc(index)[channelIndex].Texture = newTexture;
                _saveTextureDataAction();
                newTexture = TexturePacker.PackTextures(clonedTextureData, _defaultChannelColours);

                // Get the textures in order of the array
                List<Texture2D> arrayTextures = new List<Texture2D>();
                bool[] assignedTexturesArray = new bool[_textures.Length];

                // Get which textures are assigned in inspector and sort it into new list representing array layout
                for (int i = 0; i < _textures.Length; i++) {
                    bool assigned = _textures[i] != null;
                    Texture2D texture = _textures[i];

                    if (i == index) { // Retrieve changed texture at its index
                        assigned = textureAssigned;
                        texture = newTexture;
                    }

                    if (assigned) {
                        arrayTextures.Add(texture);
                    }

                    assignedTexturesArray[i] = assigned;
                }

                if (arrayTextures.Count == 0)
                    return _textures[index];

                // If array is null try loading from material
                if (_array == null) {
                    _array = _dataManager.LoadAsset<Texture2DArray>(_fileName);

                    // If array is still null create a new array and return changed texture
                    if (_array == null) {
                        _array = Texture2DArrayUtilities.CreateArrayUserInput(arrayTextures.ToArray(), TextureFormat, null, TransferMipmaps, ArrayLinear);
                        _dataManager.CreateAsset(_array, _fileName);

                        // Update variables
                        _assignedTextures[index] = textureAssigned;
                        _textures[index] = newTexture;

                        _arrayProperty.textureValue = _array;
                        _assignedTexturesProperty.floatValue = BooleanCompression.CompressValues(_assignedTextures);

                        return newTexture;
                    }
                }

                // If texture was changed to a different texture update array
                if (_assignedTextures[index] && newTexture != null) {
                    // Get array index by counting how many textures assigned before changed index
                    int arrayIndex = 0;
                    for (int i = 0; i < index; i++) {
                        if (_assignedTextures[i] == true)
                            arrayIndex++;
                    }

                    // Assign texture to array
                    (Texture2DArray, bool) updatedArray = Texture2DArrayUtilities.UpdateTextureUserInput(_array, arrayTextures[arrayIndex], arrayIndex, TransferMipmaps);

                    // If update failed or user cancelled, return unchanged texture
                    if (updatedArray.Item1 == null || updatedArray.Item2) {
                        return Texture2DArrayUtilities.GetTexture(_array, index);
                    }

                    // If array is resized to texture, update file
                    if (_array != updatedArray.Item1)
                        _dataManager.CreateAsset(updatedArray.Item1, _fileName, true);

                    _array = updatedArray.Item1;
                }

                // Otherwise recreate array 
                else {
                    // Automatically resize textures other than the changed one, prevents popups for textures that have already been decided
                    int[] autoResizeIndexes = new int[_textures.Length - 1];

                    int currentIndex = 0;
                    for (int i = 0; i < _textures.Length; i++) {
                        if (i == index) continue;

                        autoResizeIndexes[currentIndex] = i;
                        currentIndex++;
                    }

                    _array = Texture2DArrayUtilities.CreateArrayUserInput(arrayTextures.ToArray(), TextureFormat, autoResizeIndexes, TransferMipmaps, ArrayLinear);
                    _dataManager.CreateAsset(_array, _fileName, true);
                }

                // Save Asset
                EditorUtility.SetDirty(_array);

                // Assign variables
                _arrayProperty.textureValue = _array;

                _assignedTextures[index] = textureAssigned;
                _assignedTexturesProperty.floatValue = BooleanCompression.CompressValues(_assignedTextures);
                
                // Set texture
                _textures[index] = newTexture;
            }

            return newTexture;
        }

        public Texture2D DrawTexture(int index, int channelTextureIndex, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTexture(lineRect, index, channelTextureIndex, content);
        }

        public Texture2D DrawTexture(Rect rect, int index, int channelTextureIndex, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            Texture2D newTexture = GUIUtilities.DrawTexture(rect, _getLayerChannelDataFunc(index)[channelTextureIndex].Texture, content);
            return UpdateTexture(newTexture, index, channelTextureIndex);
        }

/*

        /// <summary>
        /// Draws a texture using the assigned array<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// The texture input into the inspector
        /// </returns>
        public Texture2D DrawTexture(int index, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTexture(lineRect, index, content);
        }

        /// <summary>
        /// Draws a texture using the assigned array<br />
        /// In some situations will request user input when changing textures with a popup
        /// </summary>
        /// <param name="rect">
        /// The space that the field will use
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// The texture input into the inspector
        /// </returns>
        public Texture2D DrawTexture(Rect rect, int index, GUIContent content)
        {
            // Check if valid
            if (_textures == null)
                return null;

            // Draw texture
            UnityEditor.EditorGUI.BeginChangeCheck();
            Texture2D newTexture = GUIUtilities.DrawTexture(rect, _textures[index], content);

            return UpdateTexture(newTexture, index);
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a integer value<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="intValue">
        /// The input integer value
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Int Value
        /// </returns>
        public (Texture2D, int) DrawTextureWithInt(int index, int intValue, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTextureWithInt(lineRect, index, intValue, content);
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a integer value<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="rect">
        /// The space that the field will use
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="intValue">
        /// The input integer value
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Int Value
        /// </returns>
        public (Texture2D, int) DrawTextureWithInt(Rect rect, int index, int intValue, GUIContent content)
        {
            // Check if valid
            if (_textures == null)
                return (null, 0);

            // Draw texture
            UnityEditor.EditorGUI.BeginChangeCheck();
            (Texture2D, int) output = GUIUtilities.DrawTextureWithInt(rect, _textures[index], intValue, content);

            output.Item1 = UpdateTexture(output.Item1, index);

            return output;
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a integer slider<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="sliderValue">
        /// The input slider value
        /// </param>
        /// <param name="sliderMin">
        /// The minimum value that the slider will allow
        /// </param>
        /// <param name="sliderMax">
        /// The maximum value that the slider will allow
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Slider Value
        /// </returns>
        public (Texture2D, int) DrawTextureWithIntSlider(int index, int sliderValue, int sliderMin, int sliderMax, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTextureWithIntSlider(lineRect, index, sliderValue, sliderMin, sliderMax, content);
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a integer slider<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="rect">
        /// The space that the field will use
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="sliderValue">
        /// The input slider value
        /// </param>
        /// <param name="sliderMin">
        /// The minimum value that the slider will allow
        /// </param>
        /// <param name="sliderMax">
        /// The maximum value that the slider will allow
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Slider Value
        /// </returns>
        public (Texture2D, int) DrawTextureWithIntSlider(Rect rect, int index, int sliderValue, int sliderMin, int sliderMax, GUIContent content)
        {
            // Check if valid
            if (_textures == null)
                return (null, 0);

            // Draw texture
            UnityEditor.EditorGUI.BeginChangeCheck();
            (Texture2D, int) output = GUIUtilities.DrawTextureWithIntSlider(rect, _textures[index], sliderValue, sliderMin, sliderMax, content);

            output.Item1 = UpdateTexture(output.Item1, index);

            return output;
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a float value<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="floatValue">
        /// The input float value
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Float Value
        /// </returns>
        public (Texture2D, float) DrawTextureWithFloat(int index, float floatValue, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTextureWithFloat(lineRect, index, floatValue, content);
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a float value<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="rect">
        /// The space that the field will use
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="floatValue">
        /// The input float value
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Float Value
        /// </returns>
        public (Texture2D, float) DrawTextureWithFloat(Rect rect, int index, float floatValue, GUIContent content)
        {
            // Check if valid
            if (_textures == null)
                return (null, 0.0f);

            // Draw texture
            UnityEditor.EditorGUI.BeginChangeCheck();
            (Texture2D, float) output = GUIUtilities.DrawTextureWithFloat(rect, _textures[index], floatValue, content);

            output.Item1 = UpdateTexture(output.Item1, index);

            return output;
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a slider<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="sliderValue">
        /// The input slider value
        /// </param>
        /// <param name="sliderMin">
        /// The minimum value that the slider will allow
        /// </param>
        /// <param name="sliderMax">
        /// The maximum value that the slider will allow
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Slider Value
        /// </returns>
        public (Texture2D, float) DrawTextureWithSlider(int index, float sliderValue, float sliderMin, float sliderMax, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTextureWithSlider(lineRect, index, sliderValue, sliderMin, sliderMax, content);
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a slider<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="rect">
        /// The space that the field will use
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="sliderValue">
        /// The input slider value
        /// </param>
        /// <param name="sliderMin">
        /// The minimum value that the slider will allow
        /// </param>
        /// <param name="sliderMax">
        /// The maximum value that the slider will allow
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Slider Value
        /// </returns>
        public (Texture2D, float) DrawTextureWithSlider(Rect rect, int index, float sliderValue, float sliderMin, float sliderMax, GUIContent content)
        {
            // Check if valid
            if (_textures == null)
                return (null, 0.0f);

            // Draw texture
            UnityEditor.EditorGUI.BeginChangeCheck();
            (Texture2D, float) output = GUIUtilities.DrawTextureWithSlider(rect, _textures[index], sliderValue, sliderMin, sliderMax, content);

            output.Item1 = UpdateTexture(output.Item1, index);

            return output;
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a color value<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="colorValue">
        /// The input color value
        /// </param>
        /// <param name="hdr">
        /// If the color is HDR
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Color Value
        /// </returns>
        public (Texture2D, Color) DrawTextureWithColor(int index, Color colorValue, bool hdr, GUIContent content)
        {
            Rect lineRect = GUIUtilities.GetLineRect();
            return DrawTextureWithColor(lineRect, index, colorValue, hdr, content);
        }

        /// <summary>
        /// Draws a texture using the assigned array along with a color value<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="rect">
        /// The space that the field will use
        /// </param>
        /// <param name="index">
        /// Index of the texture being changed in the desired array layout<br />
        /// Not the index of the current array layout or textures, think of it as a constant within the set texture count
        /// </param>
        /// <param name="colorValue">
        /// The input color value
        /// </param>
        /// <param name="hdr">
        /// If the color is HDR
        /// </param>
        /// <param name="content">
        /// The GUIContent for the field
        /// </param>
        /// <returns>
        /// Item1: Texture, Item2: Color Value
        /// </returns>
        public (Texture2D, Color) DrawTextureWithColor(Rect rect, int index, Color colorValue, bool hdr, GUIContent content)
        {
            // Check if valid
            if (_textures == null)
                return (null, Color.white);

            // Draw texture
            UnityEditor.EditorGUI.BeginChangeCheck();
            (Texture2D, Color) output = GUIUtilities.DrawTextureWithColor(rect, _textures[index], colorValue, hdr, content);

            output.Item1 = UpdateTexture(output.Item1, index);

            return output;
        }

        /// <summary>
        /// Draws all the textures in the array using DrawTexture<br />
        /// In some situations this will request user input when changing textures with a popup
        /// </summary>
        /// <param name="content">
        /// The GUIContent for each texture field in order<br />
        /// If unassigned each texture will be named "Texture 1", "Texture 2", ...
        /// </param>
        /// <returns>
        /// Array of textures input into the inspector
        /// </returns>
        public Texture2D[] DrawTextures(GUIContent[] content = null)
        {
            // Check if valid
            if (_textures == null)
                return null;

            // Draw textures
            Texture2D[] textures = new Texture2D[_textureCount];

            for (int i = 0; i < _textureCount; i++) {
                // Create content if not assigned
                GUIContent currentContent = new GUIContent($"Texture {i + 1}");
                if (content != null && content[i] != null)
                    currentContent = content[i];

                // Draw textures
                textures[i] = DrawTexture(i, currentContent);
            }

            return textures;
        }
*/

        /// <summary>
        /// Returns if a texture is assigned at the given index
        /// </summary>
        /// <param name="index">
        /// The index of the texture being checked
        /// </param>
        /// <returns>
        /// If the texture at the given index is assigned in the inspector
        /// </returns>
        public bool TextureAssignedAt(int index)
        {
            return _assignedTextures[index];
        }

        /// <summary>
        /// Clears the array and deletes its file and folder if empty
        /// </summary>
        public void DeleteArray()
        {
            _dataManager.DeleteAsset(_fileName);

            _textures = new Texture2D[_textureCount];
            _assignedTextures = new bool[_textureCount];

            _arrayProperty.textureValue = _array;
            _assignedTexturesProperty.floatValue = BooleanCompression.CompressValues(_assignedTextures);
        }
    }
}
#endif