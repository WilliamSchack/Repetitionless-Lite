#ifndef REPETITIONLESSVORONOINOISE_INCLUDED
#define REPETITIONLESSVORONOINOISE_INCLUDED

// hash22 under MIT License Copyright (c)2014 David Hoskins
// https://www.shadertoy.com/view/4djSRW
inline float2 hash22(float2 p2)
{
    float3 p3 = frac(float3(p2.xyx) * float3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.xx + p3.yz) * p3.zy);
}

int2 WrapCell(int2 c, int tile)
{
    return (c % tile + tile) % tile;
}

// Based on code by Inigo Quilez: https://iquilezles.org/articles/voronoilines/
void VoronoiNoise(
    float2 UV,
    float AngleOffset,
    float CellDensity,
    out float DistFromCenter,
    out float DistFromEdge,
    out float Cells)
{
    int TileSize = (int)CellDensity;

    float2 uvScaled = UV * CellDensity;
    int2 cell = int2(floor(uvScaled));
    cell = WrapCell(cell, TileSize);

    float2 posInCell = frac(uvScaled);

    DistFromCenter = 8.0f;
    float2 closestOffset;
    int2 closestCell;

    [unroll]
    for(int y1 = -1; y1 <= 1; y1++) {
        [unroll]
        for(int x1 = -1; x1 <= 1; x1++) {

            int2 cellToCheck = int2(x1, y1);
            int2 wrappedCell = WrapCell(cell + cellToCheck, TileSize);

            float2 cellOffset =
                cellToCheck
                - posInCell
                + hash22(wrappedCell + AngleOffset);

            float distToPoint = dot(cellOffset, cellOffset);

            if (distToPoint < DistFromCenter) {
                DistFromCenter = distToPoint;
                closestOffset = cellOffset;
                closestCell = cellToCheck;
            }
        }
    }

    int2 wrappedClosest = WrapCell(cell + closestCell, TileSize);
    Cells = hash22(wrappedClosest + AngleOffset).x;

    DistFromEdge = 8.0f;

    [unroll]
    for(int y2 = -1; y2 <= 1; y2++) {
        [unroll]
        for(int x2 = -1; x2 <= 1; x2++) {

            int2 cellToCheck = int2(x2, y2);
            int2 wrappedCell = WrapCell(cell + cellToCheck, TileSize);

            float2 cellOffset =
                cellToCheck
                - posInCell
                + hash22(wrappedCell + AngleOffset);

            float distToEdge =
                dot(0.5f * (closestOffset + cellOffset),
                    normalize(cellOffset - closestOffset));

            DistFromEdge = min(DistFromEdge, distToEdge);
        }
    }
}


#endif