# TextureUtilities.Texture2DArrayUtilities

## Description

`Unity Editor Only`

Contains various helper functions for Texture2DArrays 

## Variables

| Variable | Description |
|----------|-------------|
| SUPPORTED_TEXTURE_FORMATS | List of all the supported texture formats for Texture2DArrays |

---

## CreateArray(Texture2D[], TextureFormat, bool, bool)

### Declaration

``` csharp
public static Texture2DArray CreateArray(Texture2D[] textures, TextureFormat textureFormat, bool transferMipmaps = true, bool linear = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| textures | The textures that will be in the array |
| textureFormat | The texture format of the output array |
| transferMipmaps | If mipmaps will be transferred from the textures to the array |
| linear | If the output array will be linear<br />Recommended in the Built-In Render Pipeline only when including normal maps<br /><strong>Not Recommended in URP/HDRP as it will result in brighter textures</strong> |

### Returns

A Texture2DArray with the input textures and settings

### Description

Creates a new Texture2DArray with the given textures taking into account unsupported formats

Resolution will be the resolution of the first input texture

Texture will be skipped if it is not the same resolution of the first

---

## CreateArrayAutoResize(Texture2D[], TextureFormat, bool, bool)

### Declaration

``` csharp
public static Texture2DArray CreateArrayAutoResize(Texture2D[] textures, TextureFormat textureFormat, bool transferMipmaps = true, bool linear = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| textures | The textures that will be in the array |
| textureFormat | The texture format of the output array |
| transferMipmaps | If mipmaps will be transferred from the textures to the array |
| linear | If the output array will be linear<br />Recommended in the Built-In Render Pipeline only when including normal maps<br /><strong>Not Recommended in URP/HDRP as it will result in brighter textures</strong> |

### Returns

A Texture2DArray with the input textures and settings

### Description

Creates a new Texture2DArray with the given textures taking into account unsupported formats

Resolution will be the resolution of the first input texture

Automatically resizes all textures to the resolution of the first

---

## CreateArrayUserInput(Texture2D[], TextureFormat, int[], bool, bool)

### Declaration

``` csharp
public static Texture2DArray CreateArrayUserInput(Texture2D[] textures, TextureFormat textureFormat, int[] autoResizeIndexes = null, bool transferMipmaps = true, bool linear = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| textures | The textures that will be in the array |
| textureFormat | The texture format of the output array |
| autoResizeIndexes | Automatically resizes all textures at these indexes if required instead of showing a prompt to the user<br />Example: If this is { 0, 2 }, the textures at index 0 and 2 will be automatically resized |
| transferMipmaps | If mipmaps will be transferred from the textures to the array |
| linear | If the output array will be linear<br />Recommended in the Built-In Render Pipeline only when including normal maps<br /><strong>Not Recommended in URP/HDRP as it will result in brighter textures</strong> |

### Returns

A Texture2DArray with the input textures and settings

### Description

Creates a new Texture2DArray with the given textures taking into account unsupported formats

Resolution will be the resolution of the first input texture

Checks for user input to decide whether to resize the texture or array if a texture is at a different resolution to the array

---

## UpdateTexture(Texture2DArray, Texture2D, int, bool)

### Declaration

``` csharp
public static Texture2DArray UpdateTexture(Texture2DArray array, Texture2D texture, int index, bool transferMipmaps = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array that will be updated |
| texture | The texture that will be inserted into the array |
| index | The index which the texture will overwrite |
| transferMipmaps | If mipmaps will be transferred from the texture to the array if possible |

### Returns

The modified array with the texture inserted

### Description

Overwrites the texture in the input array at a given index to the input texture

Uses GPU functions to speed up operations

Update will not be applied if the texture resolution is not the same as the array

---

## UpdateTextureAutoResize(Texture2DArray, Texture2D, int, bool)

### Declaration

``` csharp
public static Texture2DArray UpdateTextureAutoResize(Texture2DArray array, Texture2D texture, int index, bool transferMipmaps = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array that will be updated |
| texture | The texture that will be inserted into the array |
| index | The index which the texture will overwrite |
| transferMipmaps | If mipmaps will be transferred from the texture to the array if possible |

### Returns

The modified array with the texture inserted

### Description

Overwrites the texture in the input array at a given index to the input texture

Automatically resizes the input texture to the array size if it is a different resolution

---

## UpdateTextureUserInput(Texture2DArray, Texture2D, int, bool)

### Declaration

``` csharp
public static (Texture2DArray, bool) UpdateTextureUserInput(Texture2DArray array, Texture2D texture, int index, bool transferMipmaps = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array that will be updated |
| texture | The texture that will be inserted into the array |
| index | The index which the texture will overwrite |
| transferMipmaps | If mipmaps will be transferred from the texture to the array if possible |

### Returns

Item1: Modified Array, Item2: User Cancelled

### Description

Overwrites the texture in the input array at a given index to the input texture

Waits for the user to input the outcome when a texture is a different resolution to the array

---

## GetTexture(Texture2DArray, int)

### Declaration

``` csharp
public static Texture2D GetTexture(Texture2DArray array, int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array that will be searched |
| index | The index for which texture will be returned |

### Returns

The texture in the array at the specified index

### Description

Gets the texture in an input array at a given index

---

## GetTextures(Texture2DArray)

### Declaration

``` csharp
public static Texture2D[] GetTextures(Texture2DArray array)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array that will be searched |

### Returns

An array with all the textures from the given array

### Description

Retrieves all the textures in an input array

---

## ResizeArrayTextures(Texture2DArray, int, int, FilterMode)

### Declaration

``` csharp
public static Texture2DArray ResizeArrayTextures(Texture2DArray array, int newWidth, int newHeight, FilterMode resizeFilterMode = FilterMode.Bilinear)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| array | The array that will be searched |
| newWidth | The width of the output textures |
| newHeight | The height of the output textures |
| resizeFilterMode | The filter mode used when resizing the textures |

### Returns

The modified array with all the textures at the new resolution

### Description

Scales all textures in the array to the new specified resolution

Skips textures already at the new resolution

---

