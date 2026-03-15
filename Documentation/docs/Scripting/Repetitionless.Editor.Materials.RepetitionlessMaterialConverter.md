# Editor.Materials.RepetitionlessMaterialConverter

## Description

`Unity Editor Only`

Used to convert lit materials to repetitionless materials


---

## ConvertMaterial(Material)

### Declaration

``` csharp
public static Material ConvertMaterial(Material material)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| material | The material to convert |

### Returns

The created material

### Description

Convert a material to a repetitionless material

Saves the material next to the original

---

## ConvertMaterials(Material[], bool)

### Declaration

``` csharp
public static void ConvertMaterials(Material[] materials, bool selectConverted = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materials | The materials to convert |
| selectConverted | If the materials will be selected in the project window after creation |

### Description

Convert multiple materials to repetitionless materials

Saves the materials next to the originals

---

