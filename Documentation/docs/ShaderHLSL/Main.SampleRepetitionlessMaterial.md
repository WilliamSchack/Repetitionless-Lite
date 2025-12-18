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
	
	int ArrayLayerIndex,
	UnityTexture2DArray AVTextures,
	UnityTexture2DArray NSOTextures,
	UnityTexture2DArray EMTextures,
	int AssignedAVTextures[3],
	int AssignedNSOTextures[3],
	int AssignedEMTextures[3],
	
	in RepetitionlessMaterialData MaterialData,
	
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
| LayerIndex          | The constant index of the layer in the texture arrays                                                                                                                                                                |
| AVTextures          | The texture array storing the AVTextures                                                                                                                                                                             |
| NSOTextures         | The texture array storing the NSOTextures                                                                                                                                                                            |
| EMTextures          | The texture array storing the EMTextures                                                                                                                                                                             |
| AssignedAVTextures  | The assigned AVTextures, each value in the array is 32 bools                                                                                                                                                         |
| AssignedNSOTextures | The assigned NSOTextures, each value in the array is 32 bools                                                                                                                                                        |
| AssignedEMTextures  | The assigned EMTextures, each value in the array is 32 bools                                                                                                                                                         |
| MaterialData        | The material data that will be used                                                                                                                                                                                  |

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