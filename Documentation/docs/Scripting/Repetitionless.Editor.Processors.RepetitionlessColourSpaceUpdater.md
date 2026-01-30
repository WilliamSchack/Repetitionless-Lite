# Editor.Processors.RepetitionlessColourSpaceUpdater

## Description

`Unity Editor Only`

Checks for when the colour space is changed then handles update any repetitionless materials that require repacking


---

## Initialize()

### Declaration

``` csharp
public static void Initialize()
```

### Description

Called on project open in PostProjectOpen

---

## RepackMaterialsIfColourSpaceChanged(List<Material>, bool)

### Declaration

``` csharp
public static void RepackMaterialsIfColourSpaceChanged(List<Material> materials, bool prompt = true)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| materials | The materials that will be checked and updated |
| prompt | If a dialog will show to confirm the change |

### Description

Repacks the textures on a set of repetitionless materials if they require it

---

