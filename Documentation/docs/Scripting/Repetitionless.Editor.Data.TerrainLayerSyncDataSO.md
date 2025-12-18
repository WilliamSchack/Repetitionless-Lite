# Editor.Data.TerrainLayerSyncDataSO

## Description

`Unity Editor Only`

Syncs the terrain layers from the terrains to the materials

Only one of this asset should exist

## Variables

| Variable | Description |
|----------|-------------|
| MaterialToTerrainLayer | Material to terrain layers dictionary |
| TerrainLayerToMaterial | Terrain layer to materials dictionary |

---

## Load()

### Declaration

``` csharp
public static TerrainLayerSyncDataSO Load()
```

### Returns

The sync data object

### Description

Loads the sync data instance

---

## Save()

### Declaration

``` csharp
public void Save()
```

### Description

Saves this object

---

## UpdateGlobalMaterialLayers(Material, TerrainLayer[])

### Declaration

``` csharp
public void UpdateGlobalMaterialLayers(Material mat, TerrainLayer[] layers)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| mat | The material to update |
| layers | The new terrain layers |

### Description

Updates the synced material and terrain layers, overriding the layers with the new ones for this material

---

## RemoveMaterial(Material)

### Declaration

``` csharp
public void RemoveMaterial(Material mat)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| mat | The material to remove |

### Description

Removes a material from the lists

---

## UpdateLayerMaterialsData(TerrainLayer, Material)

### Declaration

``` csharp
public void UpdateLayerMaterialsData(TerrainLayer terrainLayer, Material material = null)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| terrainLayer | The terrain layer to use |
| material | Used to only update a specific material |

### Description

Updates the material properties and texture data for a specific terrain layer

---

## RemoveUnusedLayerTextures(Material)

### Declaration

``` csharp
public void RemoveUnusedLayerTextures(Material material)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material to update |

### Description

Removes any unused terrain layers for a material

---

