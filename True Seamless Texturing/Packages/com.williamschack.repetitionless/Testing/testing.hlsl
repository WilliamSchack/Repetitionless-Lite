#ifndef TEST_INCLUDED
#define TEST_INCLUDED

void Test_float(UnityTexture2D tex, out half output)
{
    output = tex.Load(int3(0, 0, 0)).a;
}

#endif