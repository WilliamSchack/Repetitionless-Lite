using UnityEngine;

namespace SeamlessMaterial.Compression
{
    public static class BooleanCompression
    {
        public static int CompressValues(params bool[] values)
        {
            int compressedValues = (values[0] ? 1 : 0);

            int current = 2;
            for (int i = 1; i < values.Length; i++) {
                bool value = values[i];
                compressedValues |= (value ? current : 0);

                current *= 2;
            }

            return compressedValues;
        }

        public static bool[] GetCompressedValues(int compressedValues, int valueCount)
        {
            bool[] values = new bool[valueCount];

            int current = 1; 
            for (int i = 0; i < valueCount; i++) {
                values[i] = (compressedValues & current) != 0;
                current *= 2;
            }

            return values;
        }

        public static bool GetCompressedValue(int compressedValues, int index)
        {
            int current = (int)(1 * Mathf.Pow(2, index));
            return (compressedValues & current) != 0;
        }
    }
}