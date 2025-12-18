# TextureArrayEssentials/TextureArrayUtilities

## Description

Helpers for sampling texture arrays

---

## GetIndexInArray()

### Declaration 

``` csharp
int GetIndexInArray(int TexturesAssignedCompressed[BOOLEAN_COMPRESSION_MAX_CHUNKS], int Index)
```

### Parameters

| Parameter                  | Description                                                                                                                                                        |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| TexturesAssignedCompressed | The compressed assigned textures that contains which textures are assigned in the array, each value in the array is 32 bools                                       |
| Index                      | The constant index of the texture in the array<br>*(Doesnt care about how many textures are in the array, think of it as a constant within the max texture count)* |

### Returns

The actual index in the array from the constant index

### Description

Gets the actual index in a texture array based off the assigned textures and constant index

---

## GetIndexInArray_float()

### Declaration 

``` csharp
void GetIndexInArray_float(int TexturesAssignedCompressed, int Index, out int Out)
```

### Parameters

| Parameter                  | Description                                                                                                                                                        |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| TexturesAssignedCompressed | The compressed assigned textures that contains which textures are assigned in the array                                                                            |
| Index                      | The constant index of the texture in the array<br>*(Doesnt care about how many textures are in the array, think of it as a constant within the max texture count)* |

### Returns

| Output | Description                                           |
| ------ | ----------------------------------------------------- |
| Out    | The actual index in the array from the constant index |

### Description

Gets the actual index in a texture array based off the assigned textures and constant index

---

## SampleArrayAtConstantIndex()

### Declaration 

``` csharp
float4 SampleArrayAtConstantIndex(
	UnityTexture2DArray TextureArray,
	int TexturesAssignedCompressed[BOOLEAN_COMPRESSION_MAX_CHUNKS],
	int Index,
	float2 UV,
	float4 UnassignedColor,
	SamplerState SS
)
```

### Parameters

| Parameter                  | Description                                                                                                                                                        |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| TextureArray               | The texture array to sample                                                                                                                                        |
| TexturesAssignedCompressed | The compressed assigned textures that contains which textures are assigned in the array, each value in the array is 32 bools                                       |
| Index                      | The constant index of the texture in the array<br>*(Doesnt care about how many textures are in the array, think of it as a constant within the max texture count)* |
| UV                         | UV used for sampling the texture                                                                                                                                   |
| UnassignedColor            | The output colour if no texture is found at the inputted Index                                                                                                     |
| SS                         | Sampler state used for sampling the texture                                                                                                                        |

### Returns

| Output | Description       |
| ------ | ----------------- |
| Out    | The output colour |

### Description

Samples a texture array based on a constant index

---

## SampleArrayAtConstantIndex_float()

### Declaration 

``` csharp
void SampleArrayAtConstantIndex_float(
	UnityTexture2DArray TextureArray,
	int TexturesAssignedCompressed,
	int Index,
	float2 UV,
	float4 UnassignedColor,
	SamplerState SS,
	out float4 Out
)
```

### Parameters

| Parameter                  | Description                                                                                                                                                        |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| TextureArray               | The texture array to sample                                                                                                                                        |
| TexturesAssignedCompressed | The compressed assigned textures that contains which textures are assigned in the array                                                                            |
| Index                      | The constant index of the texture in the array<br>*(Doesnt care about how many textures are in the array, think of it as a constant within the max texture count)* |
| UV                         | UV used for sampling the texture                                                                                                                                   |
| UnassignedColor            | The output colour if no texture is found at the inputted Index                                                                                                     |
| SS                         | Sampler state used for sampling the texture                                                                                                                        |

### Returns

| Output | Description       |
| ------ | ----------------- |
| Out    | The output colour |

### Description

Samples a texture array based on a constant index

---