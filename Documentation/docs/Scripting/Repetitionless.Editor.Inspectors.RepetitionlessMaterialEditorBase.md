# Editor.Inspectors.RepetitionlessMaterialEditorBase

## Description

`Unity Editor Only`

Base class for creating the Master/Terrain repetitionless inspector windows

This assumes individual textures which is now unused in the current materials

To use the current packed texture arrays, use RepetitionlessPackedArrayGUIBase

## Variables

| Variable | Description |
|----------|-------------|
| HEADER_PADDING | Amount of padding at the top of the inspector |
| SETTING_PADDING | Amount of padding between sections |
| SCALED_TEXT_PADDING | Buffer ontop of minWidth for GetScaledText |
| _maxLayers | The max amount of layers for the material |
| _dataManager | The data manager used for this material |
| _textureData | The texture data for this material |
| _materialProperties | The material properties for this material |
| _material | The material being edited |
| _editor | The material editor being used |
| _cachedProperties | <strong>Use FindProperty for getting properties</strong><br />Contains all the material properties<br /> |
| _foldoutStates | Contains the current states for all foldouts<br />Keys are the material property prefix for that section |

---

## FindProperty(string)

### Declaration

``` csharp
protected MaterialProperty FindProperty(string name)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| name | The name of the material property |

### Returns

The material property requested

### Description

Gets a property from the cached properties

---

## GetScaledText(int, string, string)

### Declaration

``` csharp
protected string GetScaledText(int minWidth, string largeText, string smallText)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| minWidth | The minimum width for the window width to be to show the large text |
| largeText | The text to return if the window width is greater than minWidth |
| smallText | The text to return if the window width is less than minWidth |

### Returns

The scaled text

### Description

Dynamically returns a text if the window width is within the given minWidth 

---

## GetLayerData(int)

### Declaration

``` csharp
protected RepetitionlessLayerData GetLayerData(int layerIndex = 0)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to get the data from |

### Returns

The layer data at a layer

### Description

Gets the layer data for a layer

---

## GetMaterialData(int, int)

### Declaration

``` csharp
protected RepetitionlessMaterialData GetMaterialData(int layerIndex, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to get the data from |
| sectionIndex | The section to get the data from:0: Base, 1: Far, 2: Blend |

### Returns



### Description

Gets the material data for a specific material

---

## DrawTexture(int, int, int, GUIContent)

### Declaration

``` csharp
protected virtual Rect DrawTexture(int layerIndex, int sectionIndex, int textureIndex, GUIContent content)
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
protected virtual void DrawProperty(int layerIndex, Action drawPropertyAction)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer index  |
| drawPropertyAction |  |

### Description

Saves the material property if changed in the action

<strong>Each gui function modifying the material properties should be using this function</strong>

---

## UpdateAssignedTextures(int, int)

### Declaration

``` csharp
protected virtual void UpdateAssignedTextures(int layerIndex, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer that will be updated |
| sectionIndex | The section that this texture is in |

### Description

Handles assigned textures that the shader uses to determine whether to use textures or values

Can be overrided to change how the assigned textures are set

---

## UpdateMaterialPropertiesTexture(int)

### Declaration

``` csharp
protected virtual void UpdateMaterialPropertiesTexture(int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to update |

### Description

Updates the material properties texture

---

## GetTextureDrawerDetails(int, bool)

### Declaration

``` csharp
protected RepetitionlessMaterialEditorBase.TextureDrawerDetails GetTextureDrawerDetails(int textureIndex, bool packedTexture)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| textureIndex | The texture index:0: Albedo, 1: Metallic, 2: Smoothness, 3: Normal, 4: Occlussion, 5: Emission, 6: Variation |
| packedTexture | If the drawer details for the packed texture will return |

### Returns

The texture drawer detailsUpdateMaterialTexture

### Description

Gets the texture drawer details for a texture

---

## OnEnable(MaterialEditor)

### Declaration

``` csharp
public virtual void OnEnable(MaterialEditor materialEditor)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialEditor | The material editor being used |

### Description

Called when the inspector is first opened

---

## OnPropertiesCreated()

### Declaration

``` csharp
protected virtual void OnPropertiesCreated()
```

### Description

Called when the material properties are first created

No need to call base, nothing happens

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

## DrawMaterialPropertiesGUI()

### Declaration

``` csharp
protected virtual void DrawMaterialPropertiesGUI()
```

### Description

Draws the general material settings at the top of the inspector

---

## DrawMaterialGUI(int, int, string)

### Declaration

``` csharp
protected virtual void DrawMaterialGUI(int layerIndex, int sectionIndex, string headerText = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| sectionIndex | The material section index |
| headerText | The header label text for the section |

### Description

Draws a material section GUI

---

## DrawMaterialSettingsGUI(int, int, bool, bool, bool, bool, bool, int)

### Declaration

``` csharp
protected virtual void DrawMaterialSettingsGUI(int layerIndex, int sectionIndex, bool showNoise = true, bool showVariation = true, bool showPT = true, bool showEmission = true, bool showSR = true, int extraWidth = 0)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| sectionIndex | The section index to draw |
| showNoise | Toggles if the noise settings are enabled |
| showVariation | Toggles if the variation setting is enabled |
| showPT | Toggles if the packed texture setting is enabled |
| showEmission | Toggles if the emission setting is enabled |
| showSR | Toggles if the smoothness/roughness toggle setting is enabled |
| extraWidth | Any extra width required for the whole section<br />Used to increase the required width for the labels to expand |

### Description

Draws the settings at the top of each material section

---

## DrawLeftMaterialSettingsGUI(RepetitionlessMaterialData, int, int, int, bool, bool)

### Declaration

``` csharp
protected virtual void DrawLeftMaterialSettingsGUI(RepetitionlessMaterialData currentData, int layerIndex, int sectionIndex, int minScaledTextWidth, bool showNoise = true, bool showVariation = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| currentData | The current material data |
| layerIndex | The layer to draw |
| sectionIndex | The section index to draw |
| minScaledTextWidth | The minimum width required for labels to expand |
| showNoise | Toggles if the noise settings are enabled |
| showVariation | Toggles if the variation setting is enabled |

### Description

Draws the left section of the material settings

---

## DrawRightMaterialSettingsGUI(RepetitionlessMaterialData, int, int, int, bool, bool, bool)

### Declaration

``` csharp
protected virtual void DrawRightMaterialSettingsGUI(RepetitionlessMaterialData currentData, int layerIndex, int sectionIndex, int minScaledTextWidth, bool showPT = true, bool showEmission = true, bool showSR = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| currentData | The current material data |
| layerIndex | The layer to draw |
| sectionIndex | The section index to draw |
| minScaledTextWidth | The minimum width required for labels to expand |
| showPT | Toggles if the packed texture setting is enabled |
| showEmission | Toggles if the emission setting is enabled |
| showSR | Toggles if the smoothness/roughness toggle setting is enabled |

### Description

Draws the right section of the material settings

---

## DrawMaterialMainProperties(int, int)

### Declaration

``` csharp
protected virtual void DrawMaterialMainProperties(int layerIndex, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| sectionIndex | The material section index |

### Description

Draws the main properties in a material section

---

## DrawMaterialNoiseGUI(int, int)

### Declaration

``` csharp
protected virtual void DrawMaterialNoiseGUI(int layerIndex, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| sectionIndex | The section index to draw |

### Description

Draws the noise properties in a material section

---

## DrawMaterialVariationProperties(int, int)

### Declaration

``` csharp
protected virtual void DrawMaterialVariationProperties(int layerIndex, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| sectionIndex | The material section index |

### Description

Draws the variation properties in a material section

---

## DrawBaseMaterialGUI(int, string)

### Declaration

``` csharp
protected virtual void DrawBaseMaterialGUI(int layerIndex, string propertiesPrefix = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| propertiesPrefix | The material property prefix for the material |

### Description

Draws the base material GUI

---

## DrawDistanceBlendGUI(int, string)

### Declaration

``` csharp
protected virtual void DrawDistanceBlendGUI(int layerIndex, string propertiesPrefix = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| propertiesPrefix | The material property prefix for the material |

### Description

Draws the distance blend GUI

---

## DrawMaterialBlendGUI(int, string)

### Declaration

``` csharp
protected virtual void DrawMaterialBlendGUI(int layerIndex, string propertiesPrefix = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to draw |
| propertiesPrefix | The material property prefix for the material |

### Description

Draws the blend material GUI

---

## DrawDebugGUI()

### Declaration

``` csharp
protected virtual void DrawDebugGUI()
```

### Description

Draws the debug selection GUI

---

