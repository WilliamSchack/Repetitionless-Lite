# Editor.Inspectors.RepetitionlessMaterialEditorTerrain

## Description

`Unity Editor Only`

The editor for the terrain repetitionless material

## Variables

| Variable | Description |
|----------|-------------|
| _maxLayers | The max amount of layers for the material |

---

## OnEnable(MaterialEditor)

### Declaration

``` csharp
public override void OnEnable(MaterialEditor materialEditor)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialEditor | The material editor being used |

### Description

Called when the inspector is first opened

---

## OnPropertiesCreated(RepetitionlessMaterialDataSO)

### Declaration

``` csharp
protected override void OnPropertiesCreated(RepetitionlessMaterialDataSO materialProperties)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialProperties | The material properties that were just created |

### Description

Called when the material properties are first created

---

## OnGUI(MaterialEditor, MaterialProperty[])

### Declaration

``` csharp
public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialEditor | The material editor being used |
| properties | The material properties |

### Description

Base OnGUI function

---

## DrawTexture(int, int, int, GUIContent)

### Declaration

``` csharp
protected override Rect DrawTexture(int layerIndex, int sectionIndex, int textureIndex, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer which the texture will be drawn |
| sectionIndex | The section that this texture is in |
| textureIndex | The index of the texture in this section |
| content | The GUIContent to use in the field |

### Returns

The rect that the texture field is using

### Description

Used to draw all the texture fields

---

## DrawProperty(int, Action)

### Declaration

``` csharp
protected override void DrawProperty(int layerIndex, Action drawPropertyAction)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer index of this property |
| drawPropertyAction | The draw function |

### Description

Saves the material property if changed in the action

<strong>Each gui function modifying the material properties should be using this function</strong>

---

