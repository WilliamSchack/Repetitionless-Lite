# RepetitionlessHelpers/GetArrayAssignedTextures

## Description

Loads the array assigned textures from a texture

---

## GetArrayAssignedTextures()

### Declaration 

``` csharp
float MacroMicroVariationTexture(
	UnityTexture2D tex,
	
	out int AssignedAVTextures[3],
	out int AssignedNSOTextures[3],
	out int AssignedEMTextures[3],
	out int AssignedBMTextures
)
```

### Parameters

| Parameter           | Description                                                   |
| ------------------- | ------------------------------------------------------------- |
| tex                 | The texture to read                                           |

### Returns

| Output              | Description                                                   |
| ------------------- | ------------------------------------------------------------- |
| AssignedAVTextures  | The assigned AVTextures, each value in the array is 32 bools  |
| AssignedNSOTextures | The assigned NSOTextures, each value in the array is 32 bools |
| AssignedEMTextures  | The assigned EMTextures, each value in the array is 32 bools  |
| AssignedBMTextures  | The assigned BMTextures (0-31)                                |

### Description

Loads the array assigned textures from a texture

---