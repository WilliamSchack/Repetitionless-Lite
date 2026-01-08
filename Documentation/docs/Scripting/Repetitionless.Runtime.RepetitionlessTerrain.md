# Runtime.RepetitionlessTerrain

## Description

Handles Repetitionless materials interfacing with a terrain, automatically updating terrain textures and syncing the terrain layers to the material

## Variables

| Variable | Description |
|----------|-------------|
| MainMaterial | The main material set in the inspector |
| MaterialInstance | The instance of the material the terrain is using |
| AutoSaveTextures | If modifying the terrain layers will automatically update the material |
| Terrain | The terrain being used |
| ParentTerrain | The parent terrain that is handling this terrains material |
| OnTerrainLayersChanged | Callback when the terrain layers are changed |
| OnMaterialAssigned | Callback when the material is changed |
| OnMaterialTexturesUpdated | Callback when the material textures are updated |

---

## UpdateTerrainMaterial(Material, bool)

### Declaration

``` csharp
public void UpdateTerrainMaterial(Material material, bool assignMaterial = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material that will be instanced |
| assignMaterial | If the material instance should be assigned to the terrain |

### Description

Creates a new material instance and updates the terrain

---

## AssignMaterialInstance()

### Declaration

``` csharp
public void AssignMaterialInstance()
```

### Description

Assigns the currently used material instance to the terrain

---

## UpdateMaterialTerrainTextures()

### Declaration

``` csharp
public void UpdateMaterialTerrainTextures()
```

### Description

Updates the terrain textures on the material instance

---

## UpdateLayersCount()

### Declaration

``` csharp
public void UpdateLayersCount()
```

### Description

Updates the layer cound on the material instance

---

## UpdateControlTextures()

### Declaration

``` csharp
public void UpdateControlTextures()
```

### Description

Updates the control textures on the material instance

---

## UpdateHolesTexture()

### Declaration

``` csharp
public void UpdateHolesTexture()
```

### Description

Updates the holes texture on the material instance

---

## UpdateParentCallback(RepetitionlessTerrain)

### Declaration

``` csharp
public void UpdateParentCallback(RepetitionlessTerrain newParent)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| newParent | The new parent being subscribed to |

### Description

Updates the parent callbacks

Assumes this is called from the editor before the current parent is updated

---

## ParentMaterialChanged(Material)

### Declaration

``` csharp
public void ParentMaterialChanged(Material material)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The new material to use |

### Description

Updates the terrain material and its textures

Called by the parent when its material has changed

---

## ParentMaterialTexturesUpdated()

### Declaration

``` csharp
public void ParentMaterialTexturesUpdated()
```

### Description

Updates the material terrain textures

Called by the parent when its material has changed

---

## SetupNewTerrainNeighbour(Terrain)

### Declaration

``` csharp
public void SetupNewTerrainNeighbour(Terrain newNeighbour)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| newNeighbour | The new terrain to setup |

### Description

Sets up a terrain neighbour with a RepetitionlessTerrain component with this terrain as its parent

Called via the editor when a terrain is created beside this one

---

