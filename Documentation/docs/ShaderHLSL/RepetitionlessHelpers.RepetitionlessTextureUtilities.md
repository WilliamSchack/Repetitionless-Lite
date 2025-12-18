# RepetitionlessHelpers/RepetitionlessTextureUtilities

## Description

Samples textures at the center and edges if required of modified UVs based on voronoi noise. UVs are expected to come from [RepetitionlessNoise](RepetitionlessHelpers.RepetitionlessNoise.md)

---

## SampleRepetitionlessTexture()

### Declaration 

``` csharp
float4 SampleRepetitionlessTexture(
	UnityTexture2D Texture,
	SamplerState SS,
	
	float EdgeMask,
	float2 EdgeUV,
	float2 TransformedUV,
	bool SampleEdge,
	
	bool NormalMap = false,
	float NormalStrength = 1.0
)
```

### Parameters

| Parameter      | Description                                                |
| -------------- | ---------------------------------------------------------- |
| Texture        | The texture to sample                                      |
| SS             | Sampler state used for sampling the texture                |
| EdgeUV         | The UV to sample edges                                     |
| TransformedUV  | The UV to sample cells                                     |
| SampleEdge     | If the edge is being sampled                               |
| NormalMap      | If this texture is a normal map                            |
| NormalStrength | The normal strength<br>*Only used if NormalMap is enabled* |

### Returns

The output colour

### Description

Samples the base and edge colour if required and lerps them together using a regular texture

---

## SampleRepetitionlessArrayTexture()

### Declaration 

``` csharp
float4 SampleRepetitionlessArrayTexture(
	UnityTexture2DArray TextureArray,
	int AssignedTextures[3],
	int ConstantIndex,
	SamplerState SS,
	
	float EdgeMask,
	float2 EdgeUV,
	float2 TransformedUV,
	bool SampleEdge
)
```

### Parameters

| Parameter        | Description                                                                                                                                                        |
| ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| TextureArray     | The texture to sample                                                                                                                                              |
| AssignedTextures | The compressed assigned textures that contains which textures are assigned in the array, each value in the array is 32 bools                                       |
| ConstantIndex    | The constant index of the texture in the array<br>*(Doesnt care about how many textures are in the array, think of it as a constant within the max texture count)* |
| SS               | Sampler state used for sampling the texture                                                                                                                        |
| EdgeUV           | The UV to sample edges                                                                                                                                             |
| TransformedUV    | The UV to sample cells                                                                                                                                             |
| SampleEdge       | If the edge is being sampled                                                                                                                                       |
| NormalMap        | If this texture is a normal map                                                                                                                                    |
| NormalStrength   | The normal strength<br>*Only used if NormalMap is enabled*                                                                                                         |

### Returns

The output colour

### Description

Samples the base and edge colour if required and lerps them together using a texture array

Normal maps do not work properly in an array so they are not allowed

---