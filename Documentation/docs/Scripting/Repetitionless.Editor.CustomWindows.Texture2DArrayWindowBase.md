# Editor.CustomWindows.Texture2DArrayWindowBase

## Description

`Unity Editor Only`

Base class for creating the Create/Configure Array windows in Texture Array Essentials

## Variables

| Variable | Description |
|----------|-------------|
| _textures | The textures that are assigned in the window<br />Should be updated in tandem with _texturesResizing and _textureErrors |
| _texturesResizing | <strong>AUTOMATICALLY UPDATES</strong><br />Which textures have been marked as resizing<br />Should be updated in tandem with _textures and _textureErrors |
| _textureErrors | <strong>AUTOMATICALLY UPDATES</strong><br />Which textures have an error that needs to be addressed<br />Should be updated in tandem with _textures and _texturesResizing |
| _arrayTextureFormatIndex | Default: <strong>Index Of TextureFormat.DXT5</strong><br />The format index of the output array<br /><em>Modifiable in the window</em> |
| _arrayMipMaps | Default: <strong>true</strong><br />If the output array will transfer mipmaps from the inputted textures<br /><em>Modifiable in the window</em> |
| _arrayLinear | Default: <strong>false</strong><br />If the output array will be linear<br />Recommended in the Built-In Render Pipeline only when including normal maps<br /><strong>Not Recommended in URP/HDRP as it will result in brighter textures</strong><br /><em>Modifiable in the window</em> |
| _arrayWrapMode | Default: <strong>TextureWrapMode.Repeat</strong><br />The Wrap Mode of the output array<br /><em>Modifiable in the window</em> |
| _arrayFilterMode | Default: <strong>FilterMode.Bilinear</strong><br />The Filter Mode of the output array<br /><em>Modifiable in the window</em> |
| _arrayAnisoLevel | Default: <strong>1</strong><br />The Aniso Level of the output array<br /><em>Modifiable in the window</em> |
| _resizeTextures | Default: <strong>false</strong><br />If all the inputted textures will be resized to _arrayResolution<br /><em>Modifiable in the window</em> |
| _arrayResolution | Default: <strong>First Assigned Texture Resolution</strong><br />The resolution that all inputted textures will be resized to if _resizeTextures is enabled<br /><em>Displayed and modifiable in the window when _resizeTextures enabled</em> |
| _resizeFilterMode | Default: <strong>FilterMode.Bilinear</strong><br />The filter mode used when resizing textures<br /><em>Displayed and modifiable in the window when _resizeTextures enabled or _texturesResizing has any values set to true</em> |
| _textureFormatSettingEnabled | Default: <strong>true</strong><br />If the Texture Format setting appears in the window |
| _mipmapsSettingEnabled | Default: <strong>true</strong><br />If the Generate Mipmaps setting appears in the window |
| _linearSettingEnabled | Default: <strong>true</strong><br />If the Linear setting appears in the window |
| _wrapModeSettingEnabled | Default: <strong>true</strong><br />If the Wrap Mode setting appears in the window |
| _filterModeSettingEnabled | Default: <strong>true</strong><br />If the Filter Mode setting appears in the window |
| _anisoLevelSettingEnabled | Default: <strong>true</strong><br />If the Aniso Level setting appears in the window |
| _resizeSettingEnabled | Default: <strong>true</strong><br />If the Resize Textures setting appears in the window |
| _canAddRemoveTextures | Default: <strong>true</strong><br />If textures can be added or removed in the window |
| _headerStyle | Style used for headers in the GUI |

---

## CreateGUI()

### Declaration

``` csharp
protected virtual void CreateGUI()
```

### Description

Called when the GUI is first created

<strong>base.CreateGUI(); Must be called before performing operations</strong>

---

## OnGUIUpdate()

### Declaration

``` csharp
protected virtual void OnGUIUpdate()
```

### Description

Called every GUI update, used due to other tasks in OnGUI

<strong>base.OnGUI(); Will create the textures, settings, and output sections</strong>

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

<strong>base.CalculateElementHeight(); Must be called before performing operations</strong>

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

<strong>When modifying the height in any way Repaint(); should be called to update it on the same GUI call, otherwise will be updated on the next</strong>

---

## DrawArraySettings()

### Declaration

``` csharp
protected virtual void DrawArraySettings()
```

### Description

Draws the settings of the output array. Settings that are enabled here can be toggles with the respective settingEnabled variables

All GUI called here will be drawn within the Array Settings section

<strong>base.DrawArraySettings() will draw the settings</strong>

---

## DrawFinalOutputFields()

### Declaration

``` csharp
protected virtual void DrawFinalOutputFields()
```

### Description

Draws the details of the output array

All GUI called here will be drawn within the Final Output section

<strong>base.DrawFinalOutputFields() will draw the final output details</strong>

---

## DisplayWarnings()

### Declaration

``` csharp
protected virtual void DisplayWarnings()
```

### Description

Displays any warnings for array creation

All GUI called here will be drawn within and at the bottom of the Final Output section

<strong>base.DrawFinalOutputFields() must be called before showing any extra warnings</strong>

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

<strong>base.OnTextureAdd() will add a texture to the texture lists and should be called</strong>

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

<strong>base.OnTexturesReorder() will handle reordering the texture lists and should be called</strong>

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

<strong>base.OnDragUpdate(); Must be called before performing operations</strong>

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

<strong>base.OnDragPerform(); Must be called before performing operations</strong>

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
| path | The file location that the array will be created within the project folder<br />Should always start with &quot;Assets/&quot; |
| pingArray | If the texture array will be pinged after creation |

### Returns

If the operation was successful

### Description

Creates the array using the specified textures and settings at a given path

---

