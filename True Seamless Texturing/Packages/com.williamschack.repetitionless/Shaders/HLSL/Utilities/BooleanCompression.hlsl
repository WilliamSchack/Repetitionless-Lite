// ---------------------------------------------------
// | Script created by me taken from my other asset: |
// | Texture Array Essentials (https://u3d.as/3s4d)  |
// ---------------------------------------------------

#ifndef BOOLEANCOMPRESSION_INCLUDED
#define BOOLEANCOMPRESSION_INCLUDED

// Overwrites a value at a given index into the input CompressedValues
int AddCompressedValue(int CompressedValues, bool Value, int Index)
{
    if (Value) CompressedValues |=  (1 << Index);
    else       CompressedValues &= ~(1 << Index);
    return CompressedValues;
}

// Gets a compressed value at a given index from the input CompressedValues
bool GetCompressedValue(int CompressedValues, int Index)
{
    return (CompressedValues & (1 << Index)) != 0;
}

// Overwrites a value at a given index into the input CompressedValues
void AddCompressedValue_float(int CompressedValues, bool Value, int Index, out int Out)
{
    Out = AddCompressedValue(CompressedValues, Value, Index);
}

// Gets a compressed value at a given index from the input CompressedValues
void GetCompressedValue_float(int CompressedValues, int Index, out bool Out)
{
    Out = GetCompressedValue(CompressedValues, Index);
}

#endif