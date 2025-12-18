# Editor.Data.RepetitionlessMaterialDataSO

## Description

`Unity Editor Only`

Stores the material properties for a Repetitionless material

Creates and manages a texture storing these properties that will be passed to the shader

## Variables

| Variable | Description |
|----------|-------------|
| PROPERTIES_TEXTURE_PROP_NAME | The properties texture material property |
| Data | The data for the material<br />Do not update this in the scriptable object, do it in the material inspector |

---

## Init(int)

### Declaration

``` csharp
public void Init(int layerCount)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerCount | The max amount of terrain layers that will be used |

### Description

Initializes this with a new set of data

---

## Save()

### Declaration

``` csharp
public void Save()
```

### Description

Saves this object

---

## SetDataManager(MaterialDataManager)

### Declaration

``` csharp
public void SetDataManager(MaterialDataManager dataManager)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| dataManager | The data manager to use |

### Description

Sets the data manager

This is not serialized and this must be called for each session using this

---

## UpdateMaterialTexture(Material, int)

### Declaration

``` csharp
public void UpdateMaterialTexture(Material material, int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material that will have its texture property updated |
| layerIndex | The layer that will be updated |

### Description

Updates the properties texture with the data saved in this object

---

## UpdateMaterialTexture(Material, string, int)

### Declaration

``` csharp
public void UpdateMaterialTexture(Material material, string texturePropertyName, int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material that will have its texture property updated |
| texturePropertyName | The name of the properties texture property |
| layerIndex | The layer that will be updated |

### Description

Updates the properties texture with the data saved in this object

---

## UpdateMaterialTexture(MaterialProperty, int)

### Declaration

``` csharp
public void UpdateMaterialTexture(MaterialProperty property, int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| property | The properties texture property that will be updated |
| layerIndex | The layer that will be updated |

### Description

Updates the properties texture with the data saved in this object

---

## GetMaterialData(int, int)

### Declaration

``` csharp
public RepetitionlessMaterialData GetMaterialData(int layerIndex, int materialIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layerIndex | The layer that the material is in |
| materialIndex | The index of the material:<br />0: Base, 1: Far, 2: Blend |

### Returns

The properties for a specific material

### Description

Gets the properties for a specific material

---

## UpdateAssignedTextures(Material, RepetitionlessTextureDataSO, int, int)

### Declaration

``` csharp
public void UpdateAssignedTextures(Material material, RepetitionlessTextureDataSO textureData, int materialIndex, int layerIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material that will have its texture property updated  |
| textureData | The texture data that assigned textures will be read from |
| materialIndex | The index of the material:<br />0: Base, 1: Far, 2: Blend |
| layerIndex | The layer that the material is in |

### Description

Updates the assigned textures and the property texture based on a texture data

---

