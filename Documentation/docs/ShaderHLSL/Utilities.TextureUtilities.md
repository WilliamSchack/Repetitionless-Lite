# Utilities/TextureUtilities

## Description

Helpers for textures

---

## Remap()

### Declaration

``` csharp
float Remap(float In, float2 InMinMax, float2 OutMinMax)
```

### Parameters

| Parameter | Description                                         |
| --------- | --------------------------------------------------- |
| In        | The input value to remap                            |
| InMinMax  | The current min max range the input value is inside |
| OutMinMax | The new min max range to remap the input value      |

### Returns

Remapped value from InMinMax to OutMinMax

### Description

Remaps the in value from InMinMax to OutMinMax

---

## RotateUVDegrees()

### Declaration

``` csharp
float2 RotateUVDegrees(float2 UV, float2 Center, float Rotation)
```

### Parameters

| Parameter | Description                       |
| --------- | --------------------------------- |
| UV        | The UVs to rotate                 |
| Center    | The center point to rotate around |
| Rotation  | The rotation angle in degrees     |

### Returns

Rotated UVs

### Description

Rotates the inputted UVs around the given Center by the given Rotation in degrees

---

## UnpackNormalMap()

### Declaration

``` csharp
float3 UnpackNormalMap(float4 PackedNormal, float Strength = 1.0)
```

### Parameters

| Parameter    | Description                         |
| ------------ | ----------------------------------- |
| PackedNormal | The packed normal map to unpack     |
| Strength     | Strength to apply to the normal map |

### Returns

The unpacked normal

### Description

Unpacks the inputted normal and applies strength

---