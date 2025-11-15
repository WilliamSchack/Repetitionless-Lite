# GUIUtilities.GUIUtilities

## Description

`Unity Editor Only`

Many GUI functions to make inspector creation easier without MaterialProperties

## Variables

| Variable | Description |
|----------|-------------|
| LINE_HEIGHT | The height that one line takes in the inspector |
| LINE_SPACING | The height in-between two lines in the inspector |
| FOLDOUT_INDENT | The amount of left padding for foldouts |
| BoldHeaderLargeStyle | Style used for large headers |
| MajorToggleButtonStyle | Style for major toggle buttons |
| BoldFoldoutStyle | Style used for foldout headers |

---

## GetLineRect(float)

### Declaration

``` csharp
public static Rect GetLineRect(float heightOverride = 20)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| heightOverride | Default <b>20 (LINE_HEIGHT + LINE_SPACING)</b><br />Sets the height of the output rect |

### Returns

The rect of a single line

### Description

Gets the rect of a single line, updating the GUILayout accordingly

---

## DrawHeaderLabelLarge(string)

### Declaration

``` csharp
public static void DrawHeaderLabelLarge(string text)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| text | The text to show |

### Description

Draws a Large Header Label

---

## DrawMajorToggleButton(MaterialProperty, string)

### Declaration

``` csharp
public static bool DrawMajorToggleButton(MaterialProperty property, string label)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| property | The material property to be used and modified for the toggle state |
| label | The text to show inside the button |

### Returns

The button toggled state

### Description

Draws a large toggle button that uses and updates the inputted property

Displays red when disabled, green when enabled

---

## DrawMajorToggleButton(bool, string)

### Declaration

``` csharp
public static bool DrawMajorToggleButton(bool enabled, string label)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| enabled | Toggle state if the button is enabled |
| label | The text to show inside the button |

### Returns

The button toggled state

### Description

Draws a large toggle button using an input toggle state value

Displays red when disabled, green when enabled

---

## DrawTexture2DArray(Texture2DArray, GUIContent)

### Declaration

``` csharp
public static Texture2DArray DrawTexture2DArray(Texture2DArray array, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The input Texture2DArray |
| content | The GUIContent for the field |

### Returns

The Texture2DArray input into the inspector

### Description

Draws a Texture2DArray field

---

## DrawTexture2DArray(Rect, Texture2DArray, GUIContent)

### Declaration

``` csharp
public static Texture2DArray DrawTexture2DArray(Rect rect, Texture2DArray array, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| array | The input Texture2DArray |
| content | The GUIContent for the field |

### Returns

The Texture2DArray input into the inspector

### Description

Draws a Texture2DArray field

---

## DrawTexture(Texture2D, GUIContent)

### Declaration

``` csharp
public static Texture2D DrawTexture(Texture2D texture, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The input texture |
| content | The GUIContent for the field |

### Returns

The texture input into the inspector

### Description

Draws a single texture field

---

## DrawTexture(Rect, Texture2D, GUIContent)

### Declaration

``` csharp
public static Texture2D DrawTexture(Rect rect, Texture2D texture, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| texture | The input texture |
| content | The GUIContent for the field |

### Returns

The texture input into the inspector

### Description

Draws a single texture field

---

## DrawTextureWithInt(Texture2D, int, GUIContent)

### Declaration

``` csharp
public static (Texture2D, int) DrawTextureWithInt(Texture2D texture, int intValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The input texture |
| intValue | The input integer value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Int Value

### Description

Draws a single texture field with an accompanied integer value

---

## DrawTextureWithInt(Rect, Texture2D, int, GUIContent)

### Declaration

``` csharp
public static (Texture2D, int) DrawTextureWithInt(Rect rect, Texture2D texture, int intValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| texture | The input texture |
| intValue | The input integer value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Int Value

### Description

Draws a single texture field with an accompanied integer value

---

## DrawTextureWithIntSlider(Texture2D, int, int, int, GUIContent)

### Declaration

``` csharp
public static (Texture2D, int) DrawTextureWithIntSlider(Texture2D texture, int sliderValue, int sliderMin, int sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The input texture |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a single texture field with an accompanied integer slider

---

## DrawTextureWithIntSlider(Rect, Texture2D, int, int, int, GUIContent)

### Declaration

``` csharp
public static (Texture2D, int) DrawTextureWithIntSlider(Rect rect, Texture2D texture, int sliderValue, int sliderMin, int sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| texture | The input texture |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a single texture field with an accompanied integer slider

---

## DrawTextureWithFloat(Texture2D, float, GUIContent)

### Declaration

``` csharp
public static (Texture2D, float) DrawTextureWithFloat(Texture2D texture, float floatValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The input texture |
| floatValue | The input float value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Float Value

### Description

Draws a single texture field with an accompanied float value

---

## DrawTextureWithFloat(Rect, Texture2D, float, GUIContent)

### Declaration

``` csharp
public static (Texture2D, float) DrawTextureWithFloat(Rect rect, Texture2D texture, float floatValue, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| texture | The input texture |
| floatValue | The input float value |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Float Value

### Description

Draws a single texture field with an accompanied float value

---

## DrawTextureWithSlider(Texture2D, float, float, float, GUIContent)

### Declaration

``` csharp
public static (Texture2D, float) DrawTextureWithSlider(Texture2D texture, float sliderValue, float sliderMin, float sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The input texture |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a single texture field with an accompanied slider

---

## DrawTextureWithSlider(Rect, Texture2D, float, float, float, GUIContent)

### Declaration

``` csharp
public static (Texture2D, float) DrawTextureWithSlider(Rect rect, Texture2D texture, float sliderValue, float sliderMin, float sliderMax, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| texture | The input texture |
| sliderValue | The input slider value |
| sliderMin | The minimum value that the slider will allow |
| sliderMax | The maximum value that the slider will allow |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Slider Value

### Description

Draws a single texture field with an accompanied slider

---

## DrawTextureWithColor(Texture2D, Color, bool, GUIContent)

### Declaration

``` csharp
public static (Texture2D, Color) DrawTextureWithColor(Texture2D texture, Color colorValue, bool hdr, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The input texture |
| colorValue | The input color value |
| hdr | If the color is HDR |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Color Value

### Description

Draws a single texture field with an accompanied color field

---

## DrawTextureWithColor(Rect, Texture2D, Color, bool, GUIContent)

### Declaration

``` csharp
public static (Texture2D, Color) DrawTextureWithColor(Rect rect, Texture2D texture, Color colorValue, bool hdr, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| texture | The input texture |
| colorValue | The input color value |
| hdr | If the color is HDR |
| content | The GUIContent for the field |

### Returns

Item1: Texture, Item2: Color Value

### Description

Draws a single texture field with an accompanied color field

---

## DrawTextures(Texture2D[], GUIContent[])

### Declaration

``` csharp
public static Texture2D[] DrawTextures(Texture2D[] textures, GUIContent[] content = null)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| textures | The input textures that will be drawn in order |
| content | The GUIContent for each field in orderIf unassigned each texture will be named "Texture 1", "Texture 2", ... |

### Returns

Array of textures input into the inspector

### Description

Draws multiple texture fields in sequence from the given texture array

---

## DrawVector2Field(Vector2, GUIContent)

### Declaration

``` csharp
public static Vector2 DrawVector2Field(Vector2 value, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| value | The input Vector2 value |
| content | The GUIContent for the field |

### Returns

The Vector2 input into the inspector

### Description

Draws a Vector2 field removing the space at the end which EditorGUI.Vector2Field draws

---

## DrawVector2Field(Rect, Vector2, GUIContent)

### Declaration

``` csharp
public static Vector2 DrawVector2Field(Rect rect, Vector2 value, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| value | The input Vector2 value |
| content | The GUIContent for the field |

### Returns

The Vector2 input into the inspector

### Description

Draws a Vector2 field removing the space at the end which EditorGUI.Vector2Field draws

---

## DrawVector2IntField(Vector2Int, GUIContent)

### Declaration

``` csharp
public static Vector2Int DrawVector2IntField(Vector2Int value, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| value | The input Vector2Int value |
| content | The GUIContent for the field |

### Returns

The Vector2Int input into the inspector

### Description

Draws a Vector2Int field removing the space at the end which EditorGUI.Vector2IntField draws

---

## DrawVector2IntField(Rect, Vector2Int, GUIContent)

### Declaration

``` csharp
public static Vector2Int DrawVector2IntField(Rect rect, Vector2Int value, GUIContent content)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| rect | The space that the field will use |
| value | The input Vector2Int value |
| content | The GUIContent for the field |

### Returns

The Vector2Int input into the inspector

### Description

Draws a Vector2Int field removing the space at the end which EditorGUI.Vector2IntField draws

---

## DrawTilingOffset(MaterialProperty, string, string)

### Declaration

``` csharp
public static void DrawTilingOffset(MaterialProperty tilingOffsetProperty, string tilingLabel = "Scale", string offsetLabel = "Offset")
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| tilingOffsetProperty | A float4 property that will be used and updated for the tiling and offset values |
| tilingLabel | The label to show for the tiling field |
| offsetLabel | The label to show for the offset field |

### Description

Draws tiling offset fields using and updating values in a float4 materialProperty

Uses the DrawVector2Field function to remove empty space

---

## DrawFoldout(bool, string)

### Declaration

``` csharp
public static bool DrawFoldout(bool foldout, string label)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| foldout | If the foldout state is enabled |
| label | The label on the foldout |

### Returns

The foldout state

### Description

Draws a foldout and updates the gui layout

---

## BeginBackgroundVertical()

### Declaration

``` csharp
public static void BeginBackgroundVertical()
```

### Description

Begins a background in the same style as a HelpBox using GUILayout.BeginVertical

---

## EndBackgroundVertical()

### Declaration

``` csharp
public static void EndBackgroundVertical()
```

### Description

Ends the vertical background, same as calling GUILayout.EndVertical

---

## BeginBackgroundHorizontal()

### Declaration

``` csharp
public static void BeginBackgroundHorizontal()
```

### Description

Begins a background in the same style as a HelpBox using GUILayout.BeginHorizontal

---

## EndBackgroundHorizontal()

### Declaration

``` csharp
public static void EndBackgroundHorizontal()
```

### Description

Ends the horizontal background, same as calling GUILayout.EndHorizontal

---

