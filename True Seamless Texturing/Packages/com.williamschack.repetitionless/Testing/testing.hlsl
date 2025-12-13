#ifndef TEST_INCLUDED
#define TEST_INCLUDED

void Test_float(UnityTexture2D tex, out float4 output)
{
    uint test = tex.Load(int3(0, 0, 0)).x * 65535;
    output = test == 37449 ? float4(1, 0, 0, 1) : 0;
}

float4 SampleSplat1_float(float2 uv) {
    return SAMPLE_TEXTURE2D(_Control1, sampler_Control1, uv);
}

#endif