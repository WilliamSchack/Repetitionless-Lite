# Editor.Materials.RepetitionlessMaterialCreator

## Description

`Unity Editor Only`

Used to create new repetitionless materials


---

## CreateMaterial(ERenderPipeline, string, string, bool)

### Declaration

``` csharp
public static MaterialDataObjects CreateMaterial(ERenderPipeline pipeline, string folderPath, string fileName = "RepetitionlessMaterial.mat", bool ping = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| pipeline | The materials render pipeline |
| folderPath | The folder to save the material to |
| fileName | The file name for the material<br />Must include the extension .mat |
| ping | If the material will be selected and pinged in the project window after creation |

### Returns

The data objects for the created material

### Description

Creates a repetitionless material

---

## CreateMaterial(string, string, bool)

### Declaration

``` csharp
public static MaterialDataObjects CreateMaterial(string folderPath, string fileName = "RepetitionlessMaterial.mat", bool ping = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| folderPath | The folder to save the material to |
| fileName | The file name for the material<br />Must include the extension .mat |
| ping | If the material will be selected and pinged in the project window after creation |

### Returns

The data objects for the created material

### Description

Creates a repetitionless material

Uses the currently selected render pipeline

---

## CreateMaterialAtCurrentFolder(bool)

### Declaration

``` csharp
public static MaterialDataObjects CreateMaterialAtCurrentFolder(bool ping = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| ping | If the material will be selected and pinged in the project window after creation |

### Returns

The data objects for the created material

### Description

Creates a repetitionless material

Uses the currently selected render pipeline

Saves to the currently opened folder in the project window

---

## CreateTerrainMaterial(ERenderPipeline, string, string, bool)

### Declaration

``` csharp
public static MaterialDataObjects CreateTerrainMaterial(ERenderPipeline pipeline, string folderPath, string fileName = "RepetitionlessTerrainMaterial.mat", bool ping = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| pipeline | The materials render pipeline |
| folderPath | The folder to save the material to |
| fileName | The file name for the material<br />Must include the extension .mat |
| ping | If the material will be selected and pinged in the project window after creation |

### Returns

The data objects for the created material

### Description

Creates a repetitionless terrain material

---

## CreateTerrainMaterial(string, string, bool)

### Declaration

``` csharp
public static MaterialDataObjects CreateTerrainMaterial(string folderPath, string fileName = "RepetitionlessTerrainMaterial.mat", bool ping = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| folderPath | The folder to save the material to |
| fileName | The file name for the material<br />Must include the extension .mat |
| ping | If the material will be selected and pinged in the project window after creation |

### Returns

The data objects for the created material

### Description

Creates a repetitionless terrain material

Uses the currently selected render pipeline

---

## CreateTerrainMaterialAtCurrentFolder(bool)

### Declaration

``` csharp
public static MaterialDataObjects CreateTerrainMaterialAtCurrentFolder(bool ping = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| ping | If the material will be selected and pinged in the project window after creation |

### Returns

The data objects for the created material

### Description

Creates a repetitionless terrain material

Uses the currently selected render pipeline

Saves to the currently opened folder in the project window

---

## SetupMaterial(Material, int, Action<RepetitionlessMaterialDataSO>)

### Declaration

``` csharp
public static MaterialDataObjects SetupMaterial(Material mat, int maxLayers, Action<RepetitionlessMaterialDataSO> onPropertiesCreatedCallback = null)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| mat | The material to setup |
| maxLayers | The max amount of layers that can be used |
| onPropertiesCreatedCallback | Callback for when the material properties are created |

### Returns

The data objects for the material

### Description

Loads or Creates the data objects for a repetitionless material

---

