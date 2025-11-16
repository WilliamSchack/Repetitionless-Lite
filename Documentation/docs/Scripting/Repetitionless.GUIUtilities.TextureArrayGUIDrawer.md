# GUIUtilities.TextureArrayGUIDrawer

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
| TextureArrayGUIDrawer(MaterialProperty, MaterialProperty, int, string) | Create a TextureArrayGUIDrawer using the Array and Assigned Textures properties<br />The Texture2DArray asset will be stored in a folder accompanying the material. Can be moved after creation |
| TextureArrayGUIDrawer(Material, string, string, int, string) | Create a TextureArrayGUIDrawer using a material and property names<br />The Texture2DArray asset will be stored in a folder accompanying the material. Can be moved after creation |

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

## DrawTexture(int, GUIContent)

### Declaration

``` csharp
public Texture2D DrawTexture(int index, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| content | The GUIContent for the field |

### Returns

The texture input into the inspector

### Description

Draws a texture using the assigned array

In some situations this will request user input when changing textures with a popup

---

## DrawTexture(Rect, int, GUIContent)

### Declaration

``` csharp
public Texture2D DrawTexture(Rect rect, int index, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| content | The GUIContent for the field |

### Returns

The texture input into the inspector

### Description

Draws a texture using the assigned array

In some situations will request user input when changing textures with a popup

---

## DrawTextureWithInt(int, int, GUIContent)

### Declaration

``` csharp
public (Texture2D, int) DrawTextureWithInt(int index, int intValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| intValue | The input integer value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Int Value

### Description

Draws a texture using the assigned array along with a integer value

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithInt(Rect, int, int, GUIContent)

### Declaration

``` csharp
public (Texture2D, int) DrawTextureWithInt(Rect rect, int index, int intValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| intValue | The input integer value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Int Value

### Description

Draws a texture using the assigned array along with a integer value

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithIntSlider(int, int, int, int, GUIContent)

### Declaration

``` csharp
public (Texture2D, int) DrawTextureWithIntSlider(int index, int sliderValue, int sliderMin, int sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a texture using the assigned array along with a integer slider

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithIntSlider(Rect, int, int, int, int, GUIContent)

### Declaration

``` csharp
public (Texture2D, int) DrawTextureWithIntSlider(Rect rect, int index, int sliderValue, int sliderMin, int sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a texture using the assigned array along with a integer slider

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithFloat(int, float, GUIContent)

### Declaration

``` csharp
public (Texture2D, float) DrawTextureWithFloat(int index, float floatValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| floatValue | The input float value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Float Value

### Description

Draws a texture using the assigned array along with a float value

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithFloat(Rect, int, float, GUIContent)

### Declaration

``` csharp
public (Texture2D, float) DrawTextureWithFloat(Rect rect, int index, float floatValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| floatValue | The input float value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Float Value

### Description

Draws a texture using the assigned array along with a float value

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithSlider(int, float, float, float, GUIContent)

### Declaration

``` csharp
public (Texture2D, float) DrawTextureWithSlider(int index, float sliderValue, float sliderMin, float sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a texture using the assigned array along with a slider

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithSlider(Rect, int, float, float, float, GUIContent)

### Declaration

``` csharp
public (Texture2D, float) DrawTextureWithSlider(Rect rect, int index, float sliderValue, float sliderMin, float sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a texture using the assigned array along with a slider

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithColor(int, Color, bool, GUIContent)

### Declaration

``` csharp
public (Texture2D, Color) DrawTextureWithColor(int index, Color colorValue, bool hdr, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| colorValue | The input color value |
| hdr | If the color is HDR |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Color Value

### Description

Draws a texture using the assigned array along with a color value

In some situations this will request user input when changing textures with a popup

---

## DrawTextureWithColor(Rect, int, Color, bool, GUIContent)

### Declaration

``` csharp
public (Texture2D, Color) DrawTextureWithColor(Rect rect, int index, Color colorValue, bool hdr, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| index | Index of the texture being changed in the desired array layout<br />Not the index of the current array layout or textures, think of it as a constant within the set texture count |
| colorValue | The input color value |
| hdr | If the color is HDR |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Color Value

### Description

Draws a texture using the assigned array along with a color value

In some situations this will request user input when changing textures with a popup

---

## DrawTextures(GUIContent[])

### Declaration

``` csharp
public Texture2D[] DrawTextures(GUIContent[] content = null)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| content | The GUIContent for each texture field in order<br />If unassigned each texture will be named "Texture 1", "Texture 2", ... |

### Returns

Array of textures input into the inspector

### Description

Draws all the textures in the array using DrawTexture

In some situations this will request user input when changing textures with a popup

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

