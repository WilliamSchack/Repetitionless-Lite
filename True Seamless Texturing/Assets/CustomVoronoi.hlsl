#ifndef CUSTOMVORONOI_INCLUDED
#define CUSTOMVORONOI_INCLUDED

inline float2 randomVector (float2 UV, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    UV = frac(sin(mul(UV, m)) * 46839.32);
    return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
}

// Based on code by Inigo Quilez: https://iquilezles.org/articles/voronoilines/
void CustomVoronoi_float(float2 UV, float AngleOffset, float CellDensity, out float DistFromCenter, out float DistFromEdge, out float Cells)
{
    int2 cell = floor(UV * CellDensity);
    float2 posInCell = frac(UV * CellDensity);
    
    DistFromCenter = 8.0f;
    float2 closestOffset;
    float2 closestCell;
    
    for(int y1 = -1; y1 <= 1; ++y1)
    {
        for (int x1 = -1; x1 <= 1; ++x1)
        {
            int2 cellToCheck = int2(x1, y1);
            float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset);
            
            float distToPoint = dot(cellOffset, cellOffset);

            if (distToPoint < DistFromCenter)
            {
                DistFromCenter = distToPoint;
                closestOffset = cellOffset;
                closestCell = cellToCheck;
            }
        }
    }
    
    Cells = randomVector(cell + closestCell, AngleOffset).x;
    
    DistFromEdge = 8.0f;

    for(int y2 = -1; y2 <= 1; ++y2)
    {
        for(int x2 = -1; x2 <= 1; ++x2)
        {
            int2 cellToCheck = int2(x2, y2);
            float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset);
            
            float distToEdge = dot(0.5f * (closestOffset + cellOffset), normalize(cellOffset - closestOffset));

            DistFromEdge = min(DistFromEdge, distToEdge);
        }
    }
}

#endif