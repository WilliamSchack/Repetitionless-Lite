# Main/SampleRepetitionlessMaterial

## Description

Used to sample a repetitionless material

---

## GetRepetitionlessMaterialColor()

### Declaration

``` csharp
void GetRepetitionlessMaterialColor(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	int SurfaceType, int DebuggingIndex,
	
	in RepetitionlessMaterial Material,
	
	out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
)
```

### Parameters

| Parameter           | Description                                                                                                                                                                                                          |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SS                  | Sampler state used for sampling textures                                                                                                                                                                             |
| UV                  | The UV used for sampling textures and noise                                                                                                                                                                          |
| TangentNormalVector | The current tangent normal vector                                                                                                                                                                                    |
| SurfaceType         | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>2: Transparent                                                                                                               |
| DebuggingIndex      | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| Material            | The material that will be used                                                                                                                                                                                       |

### Returns

| Output           | Description                  |
| ---------------- | ---------------------------- |
| AlbedoColorOut   | The output albedo colour     |
| NormalVectorOut  | The output normal vector     |
| MetallicOut      | The output metallic colour   |
| SmoothnessOut    | The output smoothness colour |
| OcclussionOut    | The output occlussion colour |
| EmissionColorOut | The output emission colour   |

### Description

Samples a RepetitionlessMaterial

---

## GetRepetitionlessMaterialArrayColor()

### Declaration

``` csharp
void GetRepetitionlessMaterialArrayColor(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	int SurfaceType, int DebuggingIndex,
	
	in RepetitionlessMaterialArray Material,
	
	out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
)
```

### Parameters

| Parameter           | Description                                                                                                                                                                                                          |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SS                  | Sampler state used for sampling textures                                                                                                                                                                             |
| UV                  | The UV used for sampling textures and noise                                                                                                                                                                          |
| TangentNormalVector | The current tangent normal vector                                                                                                                                                                                    |
| SurfaceType         | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>2: Transparent                                                                                                               |
| DebuggingIndex      | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| Material            | The material that will be used                                                                                                                                                                                       |

### Returns

| Output           | Description                  |
| ---------------- | ---------------------------- |
| AlbedoColorOut   | The output albedo colour     |
| NormalVectorOut  | The output normal vector     |
| MetallicOut      | The output metallic colour   |
| SmoothnessOut    | The output smoothness colour |
| OcclussionOut    | The output occlussion colour |
| EmissionColorOut | The output emission colour   |

### Description

Samples a RepetitionlessMaterialArray

---

## GetRepetitionlessMaterialColorBase()

### Declaration

``` csharp
void GetRepetitionlessMaterialColorBase(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	int SurfaceType, int DebuggingIndex,
	
	bool UsingArrayMaterial,
	in RepetitionlessMaterial Material,
	in RepetitionlessMaterialArray ArrayMaterial,
	
	out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
)
```

### Parameters

| Parameter           | Description                                                                                                                                                                                                          |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SS                  | Sampler state used for sampling textures                                                                                                                                                                             |
| UV                  | The UV used for sampling textures and noise                                                                                                                                                                          |
| TangentNormalVector | The current tangent normal vector                                                                                                                                                                                    |
| SurfaceType         | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>2: Transparent                                                                                                               |
| DebuggingIndex      | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| UsingArrayMaterial  | If enabled, ArrayMaterial is used, otherwise Material is used. Changes how textures are sampled between textures and texture arrays                                                                                  |
| Material            | The material                                                                                                                                                                                                         |
| ArrayMaterial       | The array material                                                                                                                                                                                                   |

### Returns

| Output           | Description                  |
| ---------------- | ---------------------------- |
| AlbedoColorOut   | The output albedo colour     |
| NormalVectorOut  | The output normal vector     |
| MetallicOut      | The output metallic colour   |
| SmoothnessOut    | The output smoothness colour |
| OcclussionOut    | The output occlussion colour |
| EmissionColorOut | The output emission colour   |

### Description

Samples a RepetitionlessMaterial or RepetitionlessMaterialArray based on UsingArrayMaterial

There isnt really any good way to refactor this code to work with multiple types in hlsl (No inheritance or callbacks) so as a last resort I have just included both types of materials into the base and the wrappers tell it which one to use. Im not a fan of this hacky approach but I could not figure out any other way. If this is a point of bad performance and I cannot find another solution I will just resort to duplicate code.

**If anyone has a better solution please contact me I will change it asap**

---