# Main/SampleRepetitionlessTerrain

## Description

Used to sample a repetitionless terrain

---

## SampleRepetitionlessLayer()

### Declaration

``` csharp
void SampleRepetitionlessLayer_float(
	SamplerState SS, float2 UV, float3 TangentNormalVector,
	float3 WorldPosition, float3 CameraPosition,
	int SurfaceType, int DebuggingIndex,
	
	int LayersCount,
	UnityTexture2D PropertiesTexture,
	UnityTexture2D AssignedTexturesTexture,

	UnityTexture2DArray AVTextures,
	UnityTexture2DArray NSOTextures,
	UnityTexture2DArray EMTextures,
	UnityTexture2DArray BMTextures,
	
    out float4 AlbedoColorOut,
    out float3 NormalVectorOut,
    out float MetallicOut,
    out float SmoothnessOut,
    out float OcclussionOut,
    out float3 EmissionColorOut
)
```

### Parameters

| Parameter               | Description                                                                                                                                                                                                          |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SS                      | Sampler state used for sampling textures                                                                                                                                                                             |
| UV                      | The UV used for sampling textures and noise                                                                                                                                                                          |
| TangentNormalVector     | The current tangent normal vector                                                                                                                                                                                    |
| WorldPosition           | The current world position                                                                                                                                                                                           |
| CameraPosition          | The current camera position                                                                                                                                                                                          |
| SurfaceType             | The surface type of the material as shown in the inspector<br>0: Opaque<br>1: Cutout<br>2: Transparent                                                                                                               |
| DebuggingIndex          | The selected debugging type as shown in the inspector<br>*Anything outside the below range is disabled*<br>0: Voronoi Cells<br>1: Edge Mask<br>2: Distance Mask<br>3: Blend Material Mask<br>4: Variation Multiplier |
| LayersCount             | The amount of layers that will be sampled                                                                                                                                                                            |
| PropertiesTexture       | The texture storing the material properties                                                                                                                                                                          |
| AssignedTexturesTexture | The texture storing all the arrays assigned textures                                                                                                                                                                 |
| AVTextures              | The texture array storing the AVTextures                                                                                                                                                                             |
| NSOTextures             | The texture array storing the NSOTextures                                                                                                                                                                            |
| EMTextures              | The texture array storing the EMTextures                                                                                                                                                                             |
| BMTextures              | The texture array storing the BMTextures                                                                                                                                                                             |

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

Samples a multiple RepetitionlessLayers using the control textures from the terrain and clipping using the holes texture from the terrain.

---