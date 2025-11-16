# Inspectors.RepetitionlessGUIBase

## Description

`Unity Editor Only`

Base class for creating the Master/Terrain repetitionless inspector windows

## Variables

| Variable | Description |
|----------|-------------|
| HEADER_PADDING | Amount of padding at the top of the inspector |
| SETTING_PADDING | Amount of padding between sections |
| SCALED_TEXT_PADDING | Buffer ontop of minWidth for GetScaledText |
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

## DrawTexture(int, int, GUIContent, string)

### Declaration

``` csharp
protected virtual Rect DrawTexture(int sectionIndex, int textureIndex, GUIContent content, string texturePropertyName)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| sectionIndex | The section that this texture is in |
| textureIndex | The index of the texture in this section |
| content | The GUIContent to use in the field |
| texturePropertyName | The texture material property name |

### Returns

The rect that the texture field is using

### Description

Used to draw all the texture fields

Can be overrided to change how textures are drawn

---

## HandleAssignedTextures(string, int, MaterialProperty)

### Declaration

``` csharp
protected virtual int HandleAssignedTextures(string materialPrefix, int sectionIndex, MaterialProperty settingsProp)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialPrefix | The material property prefix for the material section |
| sectionIndex | The section that this texture is in |
| settingsProp | The settings material property for this material section |

### Returns

The compressed assigned textures

### Description

Handles assigned textures that the shader uses to determine whether to use textures or values

Can be overrided to change how the assigned textures are set

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

## DrawMaterialGUI(string, int, string)

### Declaration

``` csharp
protected virtual void DrawMaterialGUI(string materialPrefix, int sectionIndex, string headerText = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialPrefix | The material property prefix for the material section |
| sectionIndex | The material section index |
| headerText | The header label text for the section |

### Description

Draws a material section GUI

---

## DrawMaterialSettingsGUI(string, bool, bool, bool, bool, bool, int)

### Declaration

``` csharp
protected virtual void DrawMaterialSettingsGUI(string materialPrefix, bool showNoise = true, bool showVariation = true, bool showPT = true, bool showEmission = true, bool showSR = true, int extraWidth = 0)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialPrefix | The material property prefix for the material section |
| showNoise | Toggles if the noise settings are enabled |
| showVariation | Toggles if the variation setting is enabled |
| showPT | Toggles if the packed texture setting is enabled |
| showEmission | Toggles if the emission setting is enabled |
| showSR | Toggles if the smoothness/roughness toggle setting is enabled |
| extraWidth | Any extra width required for the whole section<br />Used to increase the required width for the labels to expand |

### Description

Draws the settings at the top of each material section

---

## DrawLeftMaterialSettingsGUI(int, string, int, int, bool, bool)

### Declaration

``` csharp
protected virtual int DrawLeftMaterialSettingsGUI(int compressedValues, string materialPrefix, int settingToggles, int minScaledTextWidth, bool showNoise = true, bool showVariation = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedValues | The compressed setting values to modify |
| materialPrefix | The material property prefix for the material section |
| settingToggles | The compressed settings toggles |
| minScaledTextWidth | The minimum width required for labels to expand |
| showNoise | Toggles if the noise settings are enabled |
| showVariation | Toggles if the variation setting is enabled |

### Returns

The modified compressed setting values

### Description

Draws the left section of the material settings

---

## DrawRightMaterialSettingsGUI(int, string, int, int, bool, bool, bool)

### Declaration

``` csharp
protected virtual int DrawRightMaterialSettingsGUI(int compressedValues, string materialPrefix, int settingToggles, int minScaledTextWidth, bool showPT = true, bool showEmission = true, bool showSR = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedValues | The compressed setting values to modify |
| materialPrefix | The material property prefix for the material section |
| settingToggles | The compressed settings toggles |
| minScaledTextWidth | The minimum width required for labels to expand |
| showPT | Toggles if the packed texture setting is enabled |
| showEmission | Toggles if the emission setting is enabled |
| showSR | Toggles if the smoothness/roughness toggle setting is enabled |

### Returns

The modified compressed setting values

### Description

Draws the right section of the material settings

---

## DrawMaterialMainProperties(string, int)

### Declaration

``` csharp
protected virtual void DrawMaterialMainProperties(string materialPrefix, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialPrefix | The material property prefix for the material section |
| sectionIndex | The material section index |

### Description

Draws the main properties in a material section

---

## DrawMaterialNoiseGUI(string)

### Declaration

``` csharp
protected virtual void DrawMaterialNoiseGUI(string materialPrefix)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialPrefix | The material property prefix for the material section |

### Description

Draws the noise properties in a material section

---

## DrawMaterialVariationProperties(string, int)

### Declaration

``` csharp
protected virtual void DrawMaterialVariationProperties(string materialPrefix, int sectionIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialPrefix | The material property prefix for the material section |
| sectionIndex | The material section index |

### Description

Draws the variation properties in a material section

---

## DrawBaseMaterialGUI(string)

### Declaration

``` csharp
protected virtual void DrawBaseMaterialGUI(string propertiesPrefix = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| propertiesPrefix | The material property prefix for the material |

### Description

Draws the base material GUI

---

## DrawDistanceBlendGUI(string)

### Declaration

``` csharp
protected virtual void DrawDistanceBlendGUI(string propertiesPrefix = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| propertiesPrefix | The material property prefix for the material |

### Description

Draws the distance blend GUI

---

## DrawMaterialBlendGUI(string)

### Declaration

``` csharp
protected virtual void DrawMaterialBlendGUI(string propertiesPrefix = "")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
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

