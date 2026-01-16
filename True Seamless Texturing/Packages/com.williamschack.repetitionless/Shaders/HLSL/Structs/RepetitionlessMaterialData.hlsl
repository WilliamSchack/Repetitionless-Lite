#ifndef REPETITIONLESSMATERIALDATA_INCLUDED
#define REPETITIONLESSMATERIALDATA_INCLUDED

#define REPETITIONLESS_MATERIAL_VARIABLE_COUNT 9

struct RepetitionlessMaterialData
{
    half4 Settings1;
    half4 Settings2;
    half4 Settings3;
    half4 Settings4;
    half4 Settings5;

    half3 AlbedoTint;
    half3 EmissionColor;

    half4 TilingOffset;
    half4 VariationTO;
};

#endif