#ifndef TEXTUREUTILITIES_INCLUDED
#define TEXTUREUTILITIES_INCLUDED

float Remap(float In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

float2 RotateUVDegrees(float2 UV, float2 Center, float Rotation)
{
    Rotation = Rotation * (3.1415926f / 180.0f);
    UV -= Center;
    float s = sin(Rotation);
    float c = cos(Rotation);
    float2x2 rMatrix = float2x2(c, -s, s, c);
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2 - 1;
    UV.xy = mul(UV.xy, rMatrix);
    UV += Center;
    return UV;
}

float3 UnpackNormalMap(float4 PackedNormal, float Strength = 1.0)
{
    float3 normal = float3(0.5, 0.5, 1);

    // Check if the normal is assigned by checking if each xyz component of the PackedNormal is within a +-.002 range of the default normal
    // (An unassigned normal value is roughly rgba(0.498..., 0.498..., 1, 1))
    bool normalUnassigned = all(abs(PackedNormal.xyz - normal) < 0.002) && PackedNormal.w == 1;
    if (normalUnassigned)
        return normal;
    
    normal.xy = PackedNormal.wy * 2 - 1;
    normal.xy *= Strength;
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));

    return normal;
}

#endif