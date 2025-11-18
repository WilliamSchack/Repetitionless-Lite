# Main/SampleRepetitionlessLayer

## Description

Used to sample a repetitionless layer

---

## SampleRepetitionlessLayer()

### Declaration

``` csharp
void SampleRepetitionlessLayer(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	float3 WorldPosition, float3 CameraPosition,
	int SurfaceType, int DebuggingIndex,
	
	in RepetitionlessLayer Layer,
	
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
| WorldPosition       | The current world position                                                                                                                                                                                           |
| CameraPosition      | The current camera position                                                                                                                                                                                          |
| SurfaceType         | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>2: Transparent                                                                                                               |
| DebuggingIndex      | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| Layer               | The layer data that will be used                                                                                                                                                                                     |

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

Samples a RepetitionlessLayer, automatically blending between and handling each material in the layer 

---

## SampleRepetitionlessLayerTerrain()

### Declaration

``` csharp
void SampleRepetitionlessLayerTerrain(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	float3 WorldPosition, float3 CameraPosition,
	int SurfaceType, int DebuggingIndex,
	
	in RepetitionlessLayerTerrain Layer,
	
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
| SS                  | Sampler state used for sampling text                                                                                                                                                                                 |
| UV                  | The UV used for sampling textures and                                                                                                                                                                                |
| TangentNormalVector | The current tangent normal vector                                                                                                                                                                                    |
| WorldPosition       | The current world position                                                                                                                                                                                           |
| CameraPosition      | The current camera position                                                                                                                                                                                          |
| SurfaceType         | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>                                                                                                                             |
| DebuggingIndex      | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| Layer               | The layer data                                                                                                                                                                                                       |

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

Samples a RepetitionlessLayerTerrain, automatically blending between and handling each material in the layer

---

## SampleRepetitionlessLayerBase()

### Declaration

``` csharp
void SampleRepetitionlessLayerBase(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	float3 WorldPosition, float3 CameraPosition,
	int SurfaceType, int DebuggingIndex,
	
	bool UsingTerrainLayer,
	in RepetitionlessLayer Layer,
	in RepetitionlessLayerTerrain LayerTerrain,
	
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
| SS                  | Sampler state used for sampling text                                                                                                                                                                                 |
| UV                  | The UV used for sampling textures and                                                                                                                                                                                |
| TangentNormalVector | The current tangent normal vector                                                                                                                                                                                    |
| WorldPosition       | The current world position                                                                                                                                                                                           |
| CameraPosition      | The current camera position                                                                                                                                                                                          |
| SurfaceType         | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>                                                                                                                             |
| DebuggingIndex      | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| UsingTerrainLayer   | If enabled, LayerTerrain is used, otherwise Layer is used. Changes how textures are sampled between textures and texture arrays                                                                                      |
| Layer               | The layer data                                                                                                                                                                                                       |
| LayerTerrain        | The layer terrain data                                                                                                                                                                                               |

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

Samples a RepetitionlessLayer or RepetitionlessLayerTerrain based on usingTerrainLayer. Automatically blending between and handling each material in the layer

There isnt really any good way to refactor this code to work with multiple types in hlsl (No inheritance or callbacks) so as a last resort I have just included both types of materials into the base and the wrappers tell it which one to use. Im not a fan of this hacky approach but I could not figure out any other way. If this is a point of bad performance and I cannot find another solution I will just resort to duplicate code.

**If anyone has a better solution please contact me I will change it asap**

---