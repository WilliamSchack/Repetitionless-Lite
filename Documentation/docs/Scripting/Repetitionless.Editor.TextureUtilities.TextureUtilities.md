# Editor.TextureUtilities.TextureUtilities

## Description

`Unity Editor Only`

Contains various helper functions for modifying textures

## Variables

| Variable | Description |
|----------|-------------|
| COMPRESSED_TEXTURE_FORMATS | List of all the compressed texture formats |

---

## ResizeTexture(Texture2D, int, int, FilterMode)

### Declaration

``` csharp
public static Texture2D ResizeTexture(Texture2D texture, int newWidth, int newHeight, FilterMode filterMode = FilterMode.Bilinear)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The texture that will be resized |
| newWidth | The width of the output texture |
| newHeight | The height of the output texture |
| filterMode | The filter mode used when resizing the texture |

### Returns

The texture resized to the input resolution

### Description

Scales the input texture to the desired resolution

Does not modify the original texture, returns a new resized one

---

## SetReadable(Texture2D, bool)

### Declaration

``` csharp
public static void SetReadable(Texture2D texture, bool logChanges = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The texture that will set to readable |
| logChanges | Debugs if the texture is set to readable if it is not already |

### Description

Re-Imports the input texture as readable if necessary

---

## SetReadable(Texture2D[], bool)

### Declaration

``` csharp
public static void SetReadable(Texture2D[] textures, bool logChanges = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| textures | The array of textures that will set to readable |
| logChanges | Debugs if the texture is set to readable if it is not already |

### Description

Re-Imports the input textures as readable if necessary

---

## TextureIsNormal(Texture2D)

### Declaration

``` csharp
public static bool TextureIsNormal(Texture2D texture)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The texture to check |

### Returns

If the texture is a normal map

### Description

Returns if the inputted texture is imported as a normal map

---

## SetTextureToNormal(Texture2D)

### Declaration

``` csharp
public static void SetTextureToNormal(Texture2D texture)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| texture | The texture to update |

### Description

Reimports a texture as a normal map

---

