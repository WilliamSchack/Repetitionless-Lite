// Cannot pass array from shader graph, use single value and index
void CompressValue_float(int CompressedValues, bool Value, int Index, out int Out)
{
    int compressedValues = CompressedValues | (Value ? (int) (1 * pow(2, Index)) : 0);
    Out = compressedValues;
}

void GetCompressedValue_float(int CompressedValues, int Index, out bool Out)
{
    int current = (int) (1 * pow(2, Index));
    Out = (CompressedValues & current) != 0;
}