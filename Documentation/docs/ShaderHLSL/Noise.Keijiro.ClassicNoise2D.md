# Noise/Keijiro/ClassicNoise2D

## Description

**Taken from keijiro's NoiseShader repo**<br />[`https://github.com/keijiro/NoiseShader`](https://github.com/keijiro/NoiseShader)

Used for perlin noise

---

## ClassicNoise()

### Declaration 

``` csharp
float ClassicNoise(float2 p)
```

### Parameters

| Parameter | Description                    |
| --------- | ------------------------------ |
| p         | Coordinate to sample the noise |

### Returns

The noise value

### Description

Classic Perlin noise

---

## PeriodicNoise()

### Declaration 

``` csharp
float PeriodicNoise(float2 p, float2 rep)
```

### Parameters

| Parameter | Description                    |
| --------- | ------------------------------ |
| p         | Coordinate to sample the noise |
| rep       | Repeat interval                |

### Returns

The noise value

### Description

Classic Perlin noise, periodic variant

---