# Editor.Data.RepetitionlessTextureDataSO

## Description

`Unity Editor Only`

Stores the textures for a RepetitionlessMaterial

Handles drawing and updating the texture arrays

## Variables

| Variable | Description |
|----------|-------------|
| TEXTURE_PROP_NAME | The material property name of the array assigned textures texture |
| LayersTextureData | The data for the textures |
| PackedColourSpace | Which colour space the textures were previously packed in<br />Used when switching colour spaces to determine which textures need to be repacked |
| AVTexturesDrawer | The AVTextures drawer<br />Not serialized, needs to be setup with SetupTextureDrawers for each session using this |
| NSOTexturesDrawer | The NSOTextures drawer<br />Not serialized, needs to be setup with SetupTextureDrawers for each session using this |
| EMTexturesDrawer | The EMTextures drawer<br />Not serialized, needs to be setup with SetupTextureDrawers for each session using this |
| BMTexturesDrawer | The BMTextures drawer<br />Not serialized, needs to be setup with SetupTextureDrawers for each session using this |
| OnDataChanged | Callback for when any of the textures are changed |

---

## Init(int)

### Declaration

``` csharp
public void Init(int layersCount)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layersCount | The max amount of terrain layers that will be used |

### Description

Initializes this with a new set of texture data

---

## SetupLayer(int)

### Declaration

``` csharp
public void SetupLayer(int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer index to setup |

### Description

Initializes a repetitionless layer with new texture data

---

## SetupMaterial(ref MaterialTextureData)

### Declaration

``` csharp
public void SetupMaterial(ref RepetitionlessTextureDataSO.MaterialTextureData data)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| data | A reference to the material texture data being initialized |

### Description

Initializes a repetitionless material with new texture data

---

## Save()

### Declaration

``` csharp
public void Save()
```

### Description

Saves this object

---

## UpdateAssignedTexturesTexture()

### Declaration

``` csharp
public void UpdateAssignedTexturesTexture()
```

### Description

Updates the array assigned textures texture

---

## UpdateAssignedTexturesTexture(MaterialProperty)

### Declaration

``` csharp
public void UpdateAssignedTexturesTexture(MaterialProperty property)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| property | The assigned textures property that will be updated |

### Description

Updates the array assigned textures texture

---

## SetupTextureDrawers()

### Declaration

``` csharp
public void SetupTextureDrawers()
```

### Description

Initializes the texture drawers

These are not serialized and this must be called for each session using them

---

## GetMaterialTextureData(int, int)

### Declaration

``` csharp
public ref RepetitionlessTextureDataSO.MaterialTextureData GetMaterialTextureData(int layerIndex, int materialIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to retrieve data from |
| materialIndex | The material to retrieve data from:<br />0: Base material, 1: Far material, 2: Blend material |

### Returns



### Description

Gets a reference to the material texture data saved in this object

---

## GetTextureData(ref MaterialTextureData, int)

### Declaration

``` csharp
public ref TexturePacker.TextureData[] GetTextureData(ref RepetitionlessTextureDataSO.MaterialTextureData materialData, int texturesIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materialData | The material data to get the texture data from |
| texturesIndex | The index of the textures that will be used:<br />0: AV, 1: NSO, 2: EM, 3: BM |

### Returns

A reference to the texture data

### Description

Gets a reference to the texture data saved in a material data

---

## GetTextureData(int, int, int)

### Declaration

``` csharp
public ref TexturePacker.TextureData[] GetTextureData(int layerIndex, int materialIndex, int texturesIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to retrieve data from |
| materialIndex | The index of the material:<br />0: Base, 1: Far, 2: Blend |
| texturesIndex | The index of the textures that will be used:<br />0: AV, 1: NSO, 2: EM, 3: BM |

### Returns

A reference to the texture data

### Description

Gets a reference to the texture data saved in a material data

---

## UpdatePackedTexture(int, int, bool)

### Declaration

``` csharp
public void UpdatePackedTexture(int layerIndex, int materialIndex, bool enabled)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer to update |
| materialIndex | The index of the material:<br />0: Base, 1: Far, 2: Blend |
| enabled | The new state of the packed texture |

### Description

Updates the packed texture data and the associated arrays

---

