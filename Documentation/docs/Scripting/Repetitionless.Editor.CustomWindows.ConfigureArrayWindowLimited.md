# Editor.CustomWindows.ConfigureArrayWindowLimited

## Description

`Unity Editor Only`

A window used to configure a texture array


---

## Open(Texture2DArray, string, Action<Texture2DArray>)

### Declaration

``` csharp
public static void Open(Texture2DArray array, string header = "Array", Action<Texture2DArray> onTextureRecreatedCallback = null)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array to modify |
| header | The header text |
| onTextureRecreatedCallback | Callback for when the texture is recreated |

### Description

Opens the window

---

## CreateGUI()

### Declaration

``` csharp
protected override void CreateGUI()
```

### Description

Called when the GUI is first created

---

## OnGUIUpdate()

### Declaration

``` csharp
protected override void OnGUIUpdate()
```

### Description

Called every GUI update, used due to other tasks in OnGUI

---

## OnTextureAdd(ReorderableList)

### Declaration

``` csharp
protected override void OnTextureAdd(ReorderableList list)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| list | The ReorderableList used for displaying the GUI |

### Description

Called when the add button is clicked in the ReorderableList GUI

---

## OnTextureRemove(ReorderableList)

### Declaration

``` csharp
protected override void OnTextureRemove(ReorderableList list)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| list | The ReorderableList used for displaying the GUI |

### Description

Called when the remove button or delete key is pressed in the ReorderableList GUI

---

## OnTexturesReorder(ReorderableList, int, int)

### Declaration

``` csharp
protected override void OnTexturesReorder(ReorderableList list, int oldActiveElement, int newActiveElement)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| list | The ReorderableList used for displaying the GUI |
| oldActiveElement | The previous index of the reordered element |
| newActiveElement | The new index of the reordered element |

### Description

Called when the textures are reordered in the ReorderableList GUI

---

## OnDragUpdate(Object[])

### Declaration

``` csharp
protected override void OnDragUpdate(Object[] objectReferences)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| objectReferences | The items that are being dragged |

### Description

Called every update while anything is dragged over the window

---

## OnDragPerform(Object[])

### Declaration

``` csharp
protected override void OnDragPerform(Object[] objectReferences)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| objectReferences | The items that are being dragged |

### Description

Called when the drag is complete over the window

---

## CalculateElementHeight(int)

### Declaration

``` csharp
protected override float CalculateElementHeight(int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | Index of the texture list which the height is being calculated for the ReorderableList GUI |

### Returns

Height that the element will use

### Description

Calculates the height of an element for the ReorderableList GUI where the textures are assigned

---

## DrawTextureField(Rect, int, bool, bool)

### Declaration

``` csharp
protected override void DrawTextureField(Rect rect, int index, bool isActive, bool isFocused)
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

---

