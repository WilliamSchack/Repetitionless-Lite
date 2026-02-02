Shader "CustomRenderTexture/Simple"
{
    Properties
    {
        _AngleOffset ("Angle Offset", float) = 0
        _CellDensity ("Cell Density", float) = 64
    }

    SubShader
    {
    Lighting Off
    Blend One Zero

    Pass
    {
        HLSLPROGRAM
        #include "UnityCustomRenderTexture.cginc"
        #include "SeamlessVoronoi.hlsl"
        #pragma vertex CustomRenderTextureVertexShader
        #pragma fragment frag
        #pragma target 3.0

        float _AngleOffset;
        float _CellDensity;

        float2 frag(v2f_customrendertexture IN) : COLOR
        {
            float dfc = 0;
            float dfe = 0;
            float cells = 0;
            VoronoiNoise(IN.localTexcoord.xy, _AngleOffset, _CellDensity, dfc, dfe, cells);

            float edgeMask = lerp(0.23, -1.5, dfe) * 5;
            edgeMask = clamp(edgeMask, 0, 1);

            float2 Colour = 0;
            Colour.r = cells;
            Colour.g = edgeMask;

            return Colour;
        }
        ENDHLSL
        }
    }
}
