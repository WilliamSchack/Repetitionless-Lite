# Editor.Data.RepetitionlessLayeredDataSO

## Description

`Unity Editor Only`

Stores the properties for a layered Repetitionless material

## Variables

| Variable | Description |
|----------|-------------|
| LayerMode | The layer mode |
| MaxLayers | The max amount of layers allowed to be rendered |
| ControlTextures | The control textures, storing 4 channels/textures per control texture |
| PackedControlTextures | Stores references to the packed control textures |
| HolesTexture | The holes texture for when the layer mode is set to ControlTextures |

---

## Save()

### Declaration

``` csharp
public void Save()
```

### Description

Saves this object

---

## Init()

### Declaration

``` csharp
public void Init()
```

### Description

Resets the texture data fields and packs the initial textures

---

## SetupControlTextures()

### Declaration

``` csharp
public void SetupControlTextures()
```

### Description

Resets the control textures data

---

## SetupControlTextures(int)

### Declaration

``` csharp
public void SetupControlTextures(int controlIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| controlIndex | The index to setup |

### Description

Initialises the control textures array for a specific control texture

---

## SetupControlChannelTexture(int, int)

### Declaration

``` csharp
public void SetupControlChannelTexture(int controlIndex, int channelIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| controlIndex | The index to setup |
| channelIndex | The channel to setup |

### Description

Initialises a texture channel for a control texture

---

## SetupControlTexture(int)

### Declaration

``` csharp
public void SetupControlTexture(int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer index to setup |

### Description

Initialises a control texture based on a layer index

---

## GetControlTextureData(int)

### Declaration

``` csharp
public ref TexturePacker.TextureData GetControlTextureData(int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer index used to get the control index |

### Returns

A reference to the TextureData

### Description

Gets a reference to the control texture data from a layer index

---

## SetupHolesTexture()

### Declaration

``` csharp
public void SetupHolesTexture()
```

### Description

Resets the holes texture data

---

## PackControlTextures()

### Declaration

``` csharp
public void PackControlTextures()
```

### Description

Packs all the control textures

---

## GetControlIndexFromLayerIndex(int)

### Declaration

``` csharp
public int GetControlIndexFromLayerIndex(int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer index used to get the control index |

### Returns

The control index

### Description

Gets a control texture index from a layer index

---

## PackControlTexture(int)

### Declaration

``` csharp
public void PackControlTexture(int controlIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| controlIndex | The control texture index to pack |

### Description

Packs a control texture based on the textures set in ControlTextures

---

## AssignControlTextures()

### Declaration

``` csharp
public void AssignControlTextures()
```

### Description

Assigns all the control textures to the material

---

## AssignControlTexture(int)

### Declaration

``` csharp
public void AssignControlTexture(int index)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| index | The index to assign |

### Description

Assigns a control texture to the material

---

## UpdateLayersCount()

### Declaration

``` csharp
public void UpdateLayersCount()
```

### Description

Updates the layer count in the material based on the textures assigned

---

## UpdateMaxLayers(int)

### Declaration

``` csharp
public void UpdateMaxLayers(int layerCount)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerCount | The layer count to transfer to EMaxLayers |

### Description

Updates the max layers keyword based on an input layer count

---

