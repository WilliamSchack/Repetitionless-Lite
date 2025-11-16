# CustomWindows.Texture2DArrayWindowBase

## Description

`Unity Editor Only`

Base class for creating the Create/Configure Array windows in Texture Array Essentials

## Variables

| Variable | Description |
|----------|-------------|
| _textures | The textures that are assigned in the window<br />Should be updated in tandem with _texturesResizing and _textureErrors |
| _texturesResizing | <b>AUTOMATICALLY UPDATES</b><br />Which textures have been marked as resizing<br />Should be updated in tandem with _textures and _textureErrors |
| _textureErrors | <b>AUTOMATICALLY UPDATES</b><br />Which textures have an error that needs to be addressed<br />Should be updated in tandem with _textures and _texturesResizing |
| _arrayTextureFormatIndex | Default: <b>Index Of TextureFormat.DXT5</b><br />The format index of the output array<br /><i>Modifiable in the window</i> |
| _arrayMipMaps | Default: <b>true</b><br />If the output array will transfer mipmaps from the inputted textures<br /><i>Modifiable in the window</i> |
| _arrayLinear | Default: <b>false</b><br />If the output array will be linear<br />Recommended in the Built-In Render Pipeline only when including normal maps<br /><b>Not Recommended in URP/HDRP as it will result in brighter textures</b><br /><i>Modifiable in the window</i> |
| _arrayWrapMode | Default: <b>TextureWrapMode.Repeat</b><br />The Wrap Mode of the output array<br /><i>Modifiable in the window</i> |
| _arrayFilterMode | Default: <b>FilterMode.Bilinear</b><br />The Filter Mode of the output array<br /><i>Modifiable in the window</i> |
| _arrayAnisoLevel | Default: <b>1</b><br />The Aniso Level of the output array<br /><i>Modifiable in the window</i> |
| _resizeTextures | Default: <b>false</b><br />If all the inputted textures will be resized to _arrayResolution<br /><i>Modifiable in the window</i> |
| _arrayResolution | Default: <b>First Assigned Texture Resolution</b><br />The resolution that all inputted textures will be resized to if _resizeTextures is enabled<br /><i>Displayed and modifiable in the window when _resizeTextures enabled</i> |
| _resizeFilterMode | Default: <b>FilterMode.Bilinear</b><br />The filter mode used when resizing textures<br /><i>Displayed and modifiable in the window when _resizeTextures enabled or _texturesResizing has any values set to true</i> |
| _textureFormatSettingEnabled | Default: <b>true</b><br />If the Texture Format setting appears in the window |
| _mipmapsSettingEnabled | Default: <b>true</b><br />If the Generate Mipmaps setting appears in the window |
| _linearSettingEnabled | Default: <b>true</b><br />If the Linear setting appears in the window |
| _wrapModeSettingEnabled | Default: <b>true</b><br />If the Wrap Mode setting appears in the window |
| _filterModeSettingEnabled | Default: <b>true</b><br />If the Filter Mode setting appears in the window |
| _anisoLevelSettingEnabled | Default: <b>true</b><br />If the Aniso Level setting appears in the window |
| _resizeSettingEnabled | Default: <b>true</b><br />If the Resize Textures setting appears in the window |
| _canAddRemoveTextures | Default: <b>true</b><br />If textures can be added or removed in the window |
| _headerStyle | Style used for headers in the GUI |

---

## CreateGUI()

### Declaration

``` csharp
protected virtual void CreateGUI()
```

### Description

Called when theGUI is first created

<b>base.CreateGUI(); Must be called before performing operations</b>

---

## OnGUIUpdate()

### Declaration

``` csharp
protected virtual void OnGUIUpdate()
```

### Description

Called every GUI update, used due to other tasks in OnGUI

<b>base.OnGUI(); Will create the textures, settings, and output sections</b>

---

## CalculateElementHeight(int)

### Declaration

``` csharp
protected virtual float CalculateElementHeight(int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture list which the height is being calculated for the ReorderableList GUI |

### Returns

Height that the element will use

### Description

Calculates the height of an element for the ReorderableList GUI where the textures are assigned

<b>base.CalculateElementHeight(); Must be called before performing operations</b>

---

## DrawTextureField(Rect, int, bool, bool)

### Declaration

``` csharp
protected virtual void DrawTextureField(Rect rect, int index, bool isActive, bool isFocused)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | Rect with all the available space in the element<br />Available space is calculated and can be changed in CalculateElementHeight |
| index | The index of the current element in the textures list |
| isActive | If this element is is being moved or interacted with in the GUI |
| isFocused | If this element is selected in the GUI |

### Description

Draws a texture element in the textures list

<b>When modifying the height in any way Repaint(); should be called to update it on the same GUI call, otherwise will be updated on the next</b>

---

## DrawArraySettings()

### Declaration

``` csharp
protected virtual void DrawArraySettings()
```

### Description

Draws the settings of the output array. Settings that are enabled here can be toggles with the respective settingEnabled variables

All GUI called here will be drawn within the Array Settings section

<b>base.DrawArraySettings() will draw the settings</b>

---

## DrawFinalOutputFields()

### Declaration

``` csharp
protected virtual void DrawFinalOutputFields()
```

### Description

Draws the details of the output array

All GUI called here will be drawn within the Final Output section

<b>base.DrawFinalOutputFields() will draw the final output details</b>

---

## DisplayWarnings()

### Declaration

``` csharp
protected virtual void DisplayWarnings()
```

### Description

Displays any warnings for array creation

All GUI called here will be drawn within and at the bottom of the Final Output section

<b>base.DrawFinalOutputFields() must be called before showing any extra warnings</b>

---

## OnTextureAdd(ReorderableList)

### Declaration

``` csharp
protected virtual void OnTextureAdd(ReorderableList list)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| list | The ReorderableList used for displaying the GUI |

### Description

Called when the add button is clicked in the ReorderableList GUI

<b>base.OnTextureAdd() will add a texture to the texture lists and should be called</b>

---

## OnTextureRemove(ReorderableList)

### Declaration

``` csharp
protected virtual void OnTextureRemove(ReorderableList list)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| list | The ReorderableList used for displaying the GUI |

### Description

Called when the remove button or delete key is pressed in the ReorderableList GUI

base.OnTextureRemove() will remove the selected texture from the texture lists and should be called

---

## OnTexturesReorder(ReorderableList, int, int)

### Declaration

``` csharp
protected virtual void OnTexturesReorder(ReorderableList list, int oldActiveElement, int newActiveElement)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| list | The ReorderableList used for displaying the GUI |
| oldActiveElement | The previous index of the reordered element |
| newActiveElement | The new index of the reordered element |

### Description

Called when the textures are reordered in the ReorderableList GUI

<b>base.OnTexturesReorder() will handle reordering the texture lists and should be called</b>

---

## OnDragUpdate(Object[])

### Declaration

``` csharp
protected virtual void OnDragUpdate(Object[] objectReferences)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| objectReferences | The items that are being dragged |

### Description

Called every update while anything is dragged over the window

<b>base.OnDragUpdate(); Must be called before performing operations</b>

---

## OnDragPerform(Object[])

### Declaration

``` csharp
protected virtual void OnDragPerform(Object[] objectReferences)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| objectReferences | The items that are being dragged |

### Description

Called when the drag is complete over the window

<b>base.OnDragPerform(); Must be called before performing operations</b>

---

## ArrayTexturesExist()

### Declaration

``` csharp
protected bool ArrayTexturesExist()
```

### Returns

If any textures exist in the list

### Description

Checks if any textures exist in the list

---

## CreateArray(string, bool)

### Declaration

``` csharp
protected bool CreateArray(string path, bool pingArray = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| path | The file location that the array will be created within the project folder<br />Should always start with "Assets/" |
| pingArray | If the texture array will be pinged after creation |

### Returns

If the operation was successful

### Description

Creates the array using the specified textures and settings at a given path

---

