# Editor.Data.RepetitionlessDataPacker

## Description

`Unity Editor Only`

Compresses repetitionless material data


---

## Vector4ToColour(Vector4)

### Declaration

``` csharp
public static Color Vector4ToColour(Vector4 vector)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| vector | The vector to convert |

### Returns

The colour

### Description

Converts a vector4 to a colour

---

## Vector3ToColour(Vector3)

### Declaration

``` csharp
public static Color Vector3ToColour(Vector3 vector)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| vector | The vector to convert |

### Returns

The colour

### Description

Converts a vector3 to a colour

---

## UpdateCompressedMaterialData(ref RepetitionlessMaterialDataCompressed, RepetitionlessMaterialData, Vector4, bool)

### Declaration

``` csharp
public static int UpdateCompressedMaterialData(ref RepetitionlessMaterialDataCompressed compressedData, RepetitionlessMaterialData newData, Vector4 globalTilingOffset, bool updateAll = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedData | Reference to the compressed data that will be updated |
| newData | The new data to compress |
| globalTilingOffset | The global tiling offset for the material |
| updateAll | If all the values will be updated if changed<br />If disabled only the first changed value will update |

### Returns

Returns the changed variable index in the struct<br />
If updateAll is enabled it will return the default -1

### Description

Updates changed values in the compressed material data based on the given material data

---

## UpdateCompressedLayerData(ref RepetitionlessLayerDataCompressed, RepetitionlessLayerData, bool)

### Declaration

``` csharp
public static int UpdateCompressedLayerData(ref RepetitionlessLayerDataCompressed compressedData, RepetitionlessLayerData newData, bool updateAll = false)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedData | Reference to the compressed data that will be updated |
| newData | The new data to compress |
| updateAll | If all the values will be updated if changed<br />If disabled only the first changed value will update |

### Returns

Returns the changed variable index in the struct, incrementing for each material<br />
If updateAll is enabled it will return the default -1

### Description

Updates changed values in the compressed layer data based on the given layer data

---

## GetMaterialFieldColour(RepetitionlessMaterialDataCompressed, int)

### Declaration

``` csharp
public static Color? GetMaterialFieldColour(RepetitionlessMaterialDataCompressed compressedData, int compressedFieldIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedData | The compressed data to get the value from |
| compressedFieldIndex | The index of the compressed field to get:<br />0: Settings1<br />1: Settings2<br />2: Settings3<br />3: Settings4<br />4: Settings5<br />5: AlbedoTint<br />6: EmissionColour<br />7: TilingOffset<br />8: VariationTO |

### Returns

The nullable field colour, will be null if compressedFieldIndex was outside the range of values

### Description

Gets a material field colour based on the given index

---

## GetLayerFieldColour(RepetitionlessLayerDataCompressed, int)

### Declaration

``` csharp
public static Color? GetLayerFieldColour(RepetitionlessLayerDataCompressed compressedData, int compressedFieldIndex)
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| compressedData | The compressed data to get the value from |
| compressedFieldIndex | The index of the compressed field to get:<br />0-8: Base Material<br />9-17: Far Material<br />18-26: Blend Material<br />27: DistanceBlendSettings<br />28: BlendMaskDistanceTO<br />29: MaterialBlendSettings<br />30: MaterialBlendMaskTO |

### Returns

The nullable field colour, will be null if compressedFieldIndex was outside the range of values

### Description

Gets a layer field colour based on the given index

---

