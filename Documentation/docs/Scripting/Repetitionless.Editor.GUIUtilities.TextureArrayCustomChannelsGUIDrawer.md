# Editor.GUIUtilities.TextureArrayCustomChannelsGUIDrawer

## Description

`Unity Editor Only`

Allows drawing textures stored in a Texture2DArray to the GUI as well as functions for reading and deleting the array

Automatically creates and manages its own array when textures are modified

## Variables

| Variable | Description |
|----------|-------------|
| Array | The array used for this field |
| TextureFormat | The texture format of the array |
| TransferMipmaps | If mipmaps will be transferred from the texture to the array if possible |
| ArrayLinear | If the output array will be linear<br />Recommended in the Built-In Render Pipeline only when including normal maps<br />Not Recommended in URP/HDRP as it will result in brighter textures |
| OnTextureUpdated | Callback for when the texture is updated |
| TextureArrayCustomChannelsGUIDrawer(MaterialDataManager, RefFunc<int, TextureData[]>, Action, Func<int, int>, Action<int, int>, Vector4, MaterialProperty, int, string) | Constructor |

---

## UpdateArray(Texture2DArray)

### Declaration

``` csharp
public void UpdateArray(Texture2DArray newArray)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| newArray | The new array |

### Description

Changes the array to a new one

!! Assumes no textures were added or removed !!

---

## RemoveArrayLayer(int)

### Declaration

``` csharp
public void RemoveArrayLayer(int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | The layer index |

### Description

Deletes a layer from the texture array

---

## UpdateTexture(Texture2D, int, int, bool)

### Declaration

``` csharp
public (Texture2D, bool) UpdateTexture(Texture2D newTexture, int index, int channelIndex, bool force = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| newTexture | The newly assigned texture |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| channelIndex | Index of the channel texture being changed at this index<br />Corresponds to the index in the initial given channelTexturesData |
| force | If the initial check is skipped |

### Returns

Item1: The changed texture<br />
Item2: If the array was updated

### Description

Updates the texture in the array while handling its order, asset file, and material variables

Automatically packs textures, specifically updating the texture at the input channelIndex

In some situations this will request user input when changing textures with a popup

---

## DrawTexture(int, int, GUIContent)

### Declaration

``` csharp
public Texture2D DrawTexture(int index, int channelTextureIndex, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | The layer index of the texture |
| channelTextureIndex | The channel index of the texture in the texture data |
| content | The gui content for the field |

### Returns

The assigned texture

### Description

Draws a texture from the array at a specific channel

Gets the texture from the assigned texture data

---

## DrawTexture(Rect, int, int, GUIContent)

### Declaration

``` csharp
public Texture2D DrawTexture(Rect rect, int index, int channelTextureIndex, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The rect that this field will use |
| index | The layer index of the texture |
| channelTextureIndex | The channel index of the texture in the texture data |
| content | The gui content for the field |

### Returns

The assigned texture

### Description

Draws a texture from the array at a specific channel

Gets the texture from the assigned texture data

---

## TextureAssignedAt(int)

### Declaration

``` csharp
public bool TextureAssignedAt(int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | The index of the texture being checked |

### Returns

If the texture at the given index is assigned in the inspector

### Description

Returns if a texture is assigned at the given index

---

## DeleteArray()

### Declaration

``` csharp
public void DeleteArray()
```

### Description

Clears the array and deletes its file and folder if empty

---

