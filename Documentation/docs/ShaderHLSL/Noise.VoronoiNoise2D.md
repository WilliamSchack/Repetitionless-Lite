# Noise/VoronoiNoise2D

## Description

**Based on code by Inigo Quilez**<br />[`https://iquilezles.org/articles/voronoilines/`](https://iquilezles.org/articles/voronoilines/)

Used for voronoi noise

---

## VoronoiNoise()

### Declaration 

``` csharp
void VoronoiNoise(float2 UV, float AngleOffset, float CellDensity, out float DistFromCenter, out float DistFromEdge, out float Cells)
```

### Parameters

| Parameter   | Description                    |
| ----------- | ------------------------------ |
| UV          | Coordinate to sample the noise |
| AngleOffset | Varies rotation of the noise   |
| CellDensity | Changes amount of cells        |

### Returns

| Output         | Description                      |
| -------------- | -------------------------------- |
| DistFromCenter | The distance to the closest cell |
| DistFromEdge   | The distance to the closest edge |
| Cells          | The current noise value          |

### Description

Voronoi noise

---