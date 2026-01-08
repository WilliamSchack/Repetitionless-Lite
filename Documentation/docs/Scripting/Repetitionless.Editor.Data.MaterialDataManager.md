# Editor.Data.MaterialDataManager

## Description

`Unity Editor Only`

Manages the data folder for a repetitionless material

Stores the data in a folder accompanying the material

## Variables

| Variable | Description |
|----------|-------------|
| Material | The material this is handling data for |
| MaterialDataManager(Material) | MaterialDataManager Constructor |
| MaterialDataManager(Object) | Gets a data manager from the path of an asset inside the data folder<br />If the main material cannot be found, Material will be null |

---

## GenerateFolderName(string)

### Declaration

``` csharp
public static string GenerateFolderName(string prefix)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| prefix | The input folder name |

### Returns

The new folder name

### Description

Adds the folder suffix to the input name

---

## DataFolderParentPath()

### Declaration

``` csharp
public string DataFolderParentPath()
```

### Returns

The parent folder path

### Description

Gets the path of the data folder parent

---

## DataFolderName()

### Declaration

``` csharp
public string DataFolderName()
```

### Returns

The data folder name

### Description

Gets the data folder name

---

## DataFolderPath()

### Declaration

``` csharp
public string DataFolderPath()
```

### Returns

The data folder path

### Description

Gets the data folder path

---

## CreateAsset(Object, string, bool)

### Declaration

``` csharp
public void CreateAsset(Object asset, string fileName, bool overwrite = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| asset | The asset to create |
| fileName | The file name of the asset |
| overwrite | If it will overwrite an asset with the same name if it exists |

### Description

Creates an asset in the data folder

---

## LoadAsset<T>(string)

### Declaration

``` csharp
public T LoadAsset<T>(string fileName) where T : Object
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| fileName | The filename of the asset |

### Returns

The asset

### Description

Loads an asset from the data folder

---

## AssetExists(string)

### Declaration

``` csharp
public bool AssetExists(string fileName)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| fileName | The file name to check |

### Returns

If the asset exists

### Description

Checks if an asset exists in the data folder with the given name

---

## DeleteAsset(string)

### Declaration

``` csharp
public void DeleteAsset(string fileName)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| fileName | The asset file name |

### Description

Deletes an asset in the data folder with the given name if it exists

---

