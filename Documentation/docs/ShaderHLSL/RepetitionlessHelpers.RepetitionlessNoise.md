# RepetitionlessHelpers/RepetitionlessNoise

## Description

Takes and modifies UVs, returning those and noise masks and data that is used to remove repetition from materials

---

## GetRepetitionlessNoiseUVs()

### Declaration 

``` csharp
void GetRepetitionlessNoiseUVs(
	float2 UV,
	
	float NoiseAngleOffset,
	float NoiseScale,
	bool RandomiseNoiseScaling,
	float2 NoiseScalingMinMax,
	
	bool RandomiseRotation,
	float2 RandomiseRotationMinMax,
	
	out float VoronoiCells,
	out float EdgeMask,
	out float2 EdgeUV,
	out float2 TransformedUV
)
```

### Parameters

| Parameter               | Description                                                                         |
| ----------------------- | ----------------------------------------------------------------------------------- |
| UV                      | UV used for sampling the noise                                                      |
| NoiseAngleOffset        | Voronoi noise angle offset                                                          |
| NoiseScale              | Voronoi noise scale                                                                 |
| RandomiseNoiseScaling   | If the noise scaling is randomised. NoiseScalingMinMax is ignored if disabled       |
| NoiseScalingMinMax      | The range to randomly scale the noise                                               |
| RandomiseRotation       | If the noise rotation is randomised. RandomiseRotationMinMax is ignored if disabled |
| RandomiseRotationMinMax | The range in degrees to randomly rotate the noise                                   |

### Returns

| Output        | Description                          |
| ------------- | ------------------------------------ |
| VoronoiCells  | The current voronoi cell noise value |
| EdgeMask      | The edge mask of each cell           |
| EdgeUV        | The UV to sample edges               |
| TransformedUV | The UV to sample cells               |

### Description

Gets UVs based on voronoi noise

---

## AddRepetitionlessNoise_float()

### Declaration 

``` csharp
void AddRepetitionlessNoise_float(
	UnityTexture2D InputTexture,
	SamplerState SS,
	float2 UV,
	
	float NoiseAngleOffset,
	float NoiseScale,
	bool RandomiseNoiseScaling,
	float2 NoiseScalingMinMax,
	
	bool RandomiseRotation,
	float2 RandomiseRotationMinMax,
	
	out float4 OutputColor
)
```

### Parameters

| Parameter               | Description                                                                         |
| ----------------------- | ----------------------------------------------------------------------------------- |
| Texture                 | The texture to sample                                                               |
| SS                      | Sampler state used for sampling the texture                                         |
| UV                      | UV used for sampling the texture                                                    |
| NoiseAngleOffset        | Voronoi noise angle offset                                                          |
| NoiseScale              | Voronoi noise scale                                                                 |
| RandomiseNoiseScaling   | If the noise scaling is randomised. NoiseScalingMinMax is ignored if disabled       |
| NoiseScalingMinMax      | The range to randomly scale the noise                                               |
| RandomiseRotation       | If the noise rotation is randomised. RandomiseRotationMinMax is ignored if disabled |
| RandomiseRotationMinMax | The range in degrees to randomly rotate the noise                                   |

### Returns

| Output      | Description       |
| ----------- | ----------------- |
| OutputColor | The output colour |

### Description

Samples the given texture using modified UVs based on voronoi noise.

Samples the voronoi cells base and edge colour if required and lerps them together

**Used in the Repetitionless Noise sub graph**

---
