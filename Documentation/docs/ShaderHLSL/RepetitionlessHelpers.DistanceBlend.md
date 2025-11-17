# RepetitionlessHelpers/DistanceBlend

## Description

Helper for changing and blending values between and past a distance range

Used for sampling different textures or colours based on distance to the camera

---

## CalculateFarDistance()

### Declaration 

``` csharp
float CalculateFarDistance(float3 WorldPosition, float3 CameraPosition, float2 DistanceBlendMinMax)
```

### Parameters

| Parameter           | Description                                                                |
| ------------------- | -------------------------------------------------------------------------- |
| WorldPosition       | The current world position                                                 |
| CameraPosition      | The current camera position                                                |
| DistanceBlendMinMax | Lerps from the 0 to 1 using the world distance from the camera from X to Y |

### Returns

The normalised distance between the inputted min max based on the camera position
### Description

Calculates a normalised distance between the inputted min max based on the camera position

---

## DistanceBlendTilingOffset_float()

### Declaration 

``` csharp
void DistanceBlendTilingOffset_float(
	float3 WorldPosition, float3 CameraPosition,
	float2 DistanceBlendMinMax,
	
	SamplerState SS, float2 UV,
	float4 BaseTilingOffset, float4 FarTilingOffset,
	UnityTexture2D Texture,
	
	out float4 ColourOut
)
```

### Parameters

| Parameter           | Description                                                                                                             |
| ------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| WorldPosition       | The current world position                                                                                              |
| CameraPosition      | The current camera position                                                                                             |
| DistanceBlendMinMax | Lerps from the 0 to 1 using the world distance from the camera from X to Y                                              |
| SS                  | Sampler state used for sampling textures                                                                                |
| UV                  | UV used for sampling textures                                                                                           |
| BaseTilingOffset    | The tiling and offset that starts from the camera to DistanceBlendMinMax.x                                              |
| FarTilingOffset     | The tiling and offset that starts blending in at DistanceBlendMinMax.x, and 100% of the colour at DistanceBlendMinMax.y |
| Texture             | The texture that will be sampled                                                                                        |

### Returns

| Output    | Description       |
| --------- | ----------------- |
| ColourOut | The output colour |

### Description

Blends between a texture with differing tiling offset based on the camera position

**Used in the Distance Blend Tiling Offset sub graph**

---

## DistanceBlendColour_float(()

### Declaration 

``` csharp
void DistanceBlendColour_float(
	float3 WorldPosition, float3 CameraPosition,
	float2 DistanceBlendMinMax,
	
	float4 BaseColour, float4 FarColour,
	
	out float4 ColourOut
)
```

### Parameters

| Parameter           | Description                                                                                                  |
| ------------------- | ------------------------------------------------------------------------------------------------------------ |
| WorldPosition       | The current world position                                                                                   |
| CameraPosition      | The current camera position                                                                                  |
| DistanceBlendMinMax | Lerps from the 0 to 1 using the world distance from the camera from X to Y                                   |
| BaseTilingOffset    | The colour that starts from the camera to DistanceBlendMinMax.x                                              |
| FarTilingOffset     | The colour that starts blending in at DistanceBlendMinMax.x, and 100% of the colour at DistanceBlendMinMax.y |

### Returns

| Output    | Description       |
| --------- | ----------------- |
| ColourOut | The output colour |

### Description

Blends between two inputted colours based on the camera position

**Used in the Distance Blend Colour, Distance Blend Texture sub graphs**

---
