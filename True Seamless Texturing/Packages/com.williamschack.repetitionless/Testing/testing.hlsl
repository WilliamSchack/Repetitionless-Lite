#ifndef TEST_INCLUDED
#define TEST_INCLUDED

void Test_float(UnityTexture2D tex, out half output)
{
    output = tex.Load(int3(0, 0, 0)).a;
}

TEXTURE2D(_Control1);
SAMPLER(sampler_Control1);

float4 SampleSplat1_float(float2 uv) {
    return SAMPLE_TEXTURE2D(_Control1, sampler_Control1, uv);
}

#endif