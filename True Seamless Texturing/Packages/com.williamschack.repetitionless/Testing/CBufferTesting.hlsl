#ifndef CBUFFERTESTING_INCLUDED
#define CBUFFERTESTING_INCLUDED

half4 _AlbedoColour;
half4 _SM;
half4 _Emission;

void GetData_float(
    out half3 albedoColour,
    out half2 sm,
    out half3 emission
){
    albedoColour = _AlbedoColour;
    sm = 0;
    emission = 0;
}

#endif