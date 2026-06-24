# Runtime.Integrations.MapMagic.RepetitionlessMapMagic

## Description

Handles Repetitionless materials interfacing with a MapMagicObject, automatically updating terrain textures and syncing the terrain layers to the material

## Variables

| Variable | Description |
|----------|-------------|
| MapMagicObject | The MapMagicObject this component is referencing |
| MainMaterial | The main material set in the inspector |
| ApplyToDraftTerrains | If the material is applied to draft (Low-Detail) terrains |

---

## RemoveAllTilesMaterials()

### Declaration

``` csharp
public void RemoveAllTilesMaterials()
```

### Description

Removes the materials from every terrain

---

## UpdateMaterialTerrainTextures()

### Declaration

``` csharp
public void UpdateMaterialTerrainTextures()
```

### Description

Updates the terrain textures on the material instance of every terrain

---

## UpdateTerrainMaterials(Material, bool)

### Declaration

``` csharp
public void UpdateTerrainMaterials(Material material, bool assignMaterial = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material that will be instanced |
| assignMaterial | If the material instance should be assigned to the terrains |

### Description

Creates a new material instance and updates every terrain

---

## UpdateDraftTerrains()

### Declaration

``` csharp
public void UpdateDraftTerrains()
```

### Description

Enables or Disables draft terrains based on ApplyToDraftTerrains

---

## GetFirstTerrain()

### Declaration

``` csharp
public Terrain GetFirstTerrain()
```

### Returns

The first terrain in the MapMagic grid

### Description

Gets the first terrain in the MapMagic grid

---

## AssignNewMaterial(Material)

### Declaration

``` csharp
public void AssignNewMaterial(Material mat)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| mat | The material to use |

### Description

Updates the main material and assigns it to every terrain

---

## CheckAndUpdateMaterials()

### Declaration

``` csharp
public void CheckAndUpdateMaterials()
```

### Description

Checks if a terrain is using the repetitionless material and if not, it re-assigns it to every terrain

---

