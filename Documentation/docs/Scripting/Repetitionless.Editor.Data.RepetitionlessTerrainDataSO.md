# Editor.Data.RepetitionlessTerrainDataSO

## Description

`Unity Editor Only`

Stores the terrain data for a Repetitionless material

## Variables

| Variable | Description |
|----------|-------------|
| TerrainLayers | The terrain layers linked to this material |
| AutoSyncLayers | Toggles if textures and settings are automatically saved and loaded to and from the terrain layers |

---

## Save()

### Declaration

``` csharp
public void Save()
```

### Description

Saves this object

---

## UpdateTerrainLayers(TerrainLayer[])

### Declaration

``` csharp
public void UpdateTerrainLayers(TerrainLayer[] layers)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| layers | The new terrain layers to use |

### Description

Updates the terrain layers, overwrites the currently set with the inputted layers

---

## ClearTerrainLayers()

### Declaration

``` csharp
public void ClearTerrainLayers()
```

### Description

Clears the terrain layers

---

## UpdateLayerMaterialData(TerrainLayer, bool)

### Declaration

``` csharp
public void UpdateLayerMaterialData(TerrainLayer terrainLayer, bool forceUpdate = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| terrainLayer | The terrain layer to update |
| forceUpdate | Ignores the AutoSyncLayers setting and updates the data anyway |

### Description

Updates the layer linked to the inputted terrain layer on the material

---

## RemoveUnusedLayerTextures()

### Declaration

``` csharp
public void RemoveUnusedLayerTextures()
```

### Description

Removes any unused terrain layers for a material

---

