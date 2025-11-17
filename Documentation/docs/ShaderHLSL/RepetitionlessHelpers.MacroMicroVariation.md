# RepetitionlessHelpers/MacroMicroVariation

## Description

Gets variation for a material based on noise or a custom texture

---

## MacroMicroVariationTexture()

### Declaration 

``` csharp
float MacroMicroVariationTexture(
	float SmallScale,
	float MediumScale,
	float LargeScale,
	
	float VariationBrightness,
	UnityTexture2D Texture,
	SamplerState SS,
	
	float2 UV,
	float2 Tiling = float2(1, 1),
	float2 Offset = float2(0, 0)
)
```

### Parameters

| Parameter           | Description                                 |
| ------------------- | ------------------------------------------- |
| SmallScale          | Scale for the small texture sampling        |
| MediumScale         | Scale for the medium texture sampling       |
| LargeScale          | Scale for the large texture sampling        |
| VariationBrightness | Brightness added onto variation texure      |
| Texture             | Texture to be sampled                       |
| SS                  | Sampler state used for sampling the texture |
| UV                  | UV used for sampling the texture            |
| Tiling              | Texture Tiling                              |
| Offset              | Texture Offset                              |

### Returns

The variation multiplier based on the texture samples

### Description

Samples a given texture and turns it into a variation multiplier

---

## MacroMicroVariationPerlinNoise()

### Declaration 

``` csharp
float MacroMicroVariationPerlinNoise(
	float SmallScale,
	float MediumScale,
	float LargeScale,
	
	float VariationBrightness,
	
	float NoiseStrength,
	float2 UV,
	float NoiseScale = 1,
	float2 NoiseOffset = float2(0, 0)
)
```

### Parameters

| Parameter           | Description                            |
| ------------------- | -------------------------------------- |
| SmallScale          | Scale for the small texture sampling   |
| MediumScale         | Scale for the medium texture sampling  |
| LargeScale          | Scale for the large texture sampling   |
| VariationBrightness | Brightness added onto variation texure |
| Texture             | Texture to be sampled                  |
| NoiseStrength       | Strength of the noise                  |
| UV                  | UV used for sampling the noise         |
| NoiseScale          | Noise Scaling                          |
| NoiseOffset         | Noise Offset                           |

### Returns

The variation multiplier based on the noise samples

### Description

Samples perlin noise and turns it into a variation multiplier

---

## MacroMicroVariationSimplexNoise()

### Declaration 

``` csharp
float MacroMicroVariationSimplexNoise(
	float SmallScale,
	float MediumScale,
	float LargeScale,
	
	float VariationBrightness,
	
	float NoiseStrength,
	float2 UV,
	float NoiseScale = 1,
	float2 NoiseOffset = float2(0, 0)
)
```

### Parameters

| Parameter           | Description                            |
| ------------------- | -------------------------------------- |
| SmallScale          | Scale for the small texture sampling   |
| MediumScale         | Scale for the medium texture sampling  |
| LargeScale          | Scale for the large texture sampling   |
| VariationBrightness | Brightness added onto variation texure |
| Texture             | Texture to be sampled                  |
| NoiseStrength       | Strength of the noise                  |
| UV                  | UV used for sampling the noise         |
| NoiseScale          | Noise Scaling                          |
| NoiseOffset         | Noise Offset                           |

### Returns

The variation multiplier based on the noise samples

### Description

Samples simplex noise and turns it into a variation multiplier

---

## MacroMicroVariationTexture_float()

### Declaration 

``` csharp
void MacroMicroVariationTexture_float(
	float4 InputColor,
	
	float SmallScale,
	float MediumScale,
	float LargeScale,
	
	float VariationBrightness,
	float VariationOpacity,
	UnityTexture2D VariationTexture,
	
	SamplerState SS,
	float2 UV,
	float2 Tiling,
	float2 Offset,
	
	out float4 OutputColor
)
```

### Parameters

| Parameter           | Description                                                                |
| ------------------- | -------------------------------------------------------------------------- |
| InputColor          | Colour that is lerped between from this colour to variation texture colour |
| SmallScale          | Scale for the small texture sampling                                       |
| MediumScale         | Scale for the medium texture sampling                                      |
| LargeScale          | Scale for the large texture sampling                                       |
| VariationBrightness | Brightness added onto variation texure                                     |
| VariationOpacity    | Lerp factor from regular colour to variation texture                       |
| VariationTexture    | Texture to be overlayed onto the colour                                    |
| SS                  | Sampler state used for sampling the texture                                |
| UV                  | UV used for sampling the texture                                           |
| Tiling              | Texture Tiling                                                             |
| Offset              | Texture Offset                                                             |

### Returns

| Output      | Description                                           |
| ----------- | ----------------------------------------------------- |
| OutputColor | The variation multiplier based on the texture samples |

### Description

Outputs a variation colour based on an inputted texture

**Used in the MMV Texture sub graph**

---

## MacroMicroVariationPerlinNoise_float()

### Declaration 

``` csharp
void MacroMicroVariationPerlinNoise_float(
	float4 InputColor,
	
	float SmallScale,
	float MediumScale,
	float LargeScale,
	
	float VariationBrightness,
	float VariationOpacity,
	
	float NoiseStrength,
	float NoiseScale,
	float2 NoiseOffset,
	float2 UV,
	
	out float4 OutputColor
)
```

### Parameters

| Input               | Description                                                        |
| ------------------- | ------------------------------------------------------------------ |
| InputColor          | Colour that is lerped between from this colour to variation colour |
| SmallScale          | Scale for the small noise sampling                                 |
| MediumScale         | Scale for the medium noise sampling                                |
| LargeScale          | Scale for the large noise sampling                                 |
| VariationBrightness | Brightness added onto variation colour                             |
| VariationOpacity    | Lerp factor from regular colour to variation colour                |
| NoiseStrength       | Strength of the noise                                              |
| NoiseScale          | Noise Scaling                                                      |
| NoiseOffset         | Noise Offset                                                       |
| UV                  | UV used for sampling noise                                         |

### Returns

| Output      | Description                                           |
| ----------- | ----------------------------------------------------- |
| OutputColor | The variation multiplier based on the texture samples |

### Description

Outputs a variation colour based on perlin noise

**Used in the MMV Perlin Noise sub graph**

---

## MacroMicroVariationSimplexNoise_float()

### Declaration 

``` csharp
void MacroMicroVariationSimplexNoise_float(
	float4 InputColor,
	
	float SmallScale,
	float MediumScale,
	float LargeScale,
	
	float VariationBrightness,
	float VariationOpacity,
	
	float NoiseStrength,
	float NoiseScale,
	float2 NoiseOffset,
	float2 UV,
	
	out float4 OutputColor
)
```

### Parameters

| Input               | Description                                                        |
| ------------------- | ------------------------------------------------------------------ |
| InputColor          | Colour that is lerped between from this colour to variation colour |
| SmallScale          | Scale for the small noise sampling                                 |
| MediumScale         | Scale for the medium noise sampling                                |
| LargeScale          | Scale for the large noise sampling                                 |
| VariationBrightness | Brightness added onto variation colour                             |
| VariationOpacity    | Lerp factor from regular colour to variation colour                |
| NoiseStrength       | Strength of the noise                                              |
| NoiseScale          | Noise Scaling                                                      |
| NoiseOffset         | Noise Offset                                                       |
| UV                  | UV used for sampling noise                                         |

### Returns

| Output      | Description                                           |
| ----------- | ----------------------------------------------------- |
| OutputColor | The variation multiplier based on the texture samples |

### Description

Samples simplex noise and turns it into a variation multiplier

**Used in the MMV Simplex Noise sub graph**

---

