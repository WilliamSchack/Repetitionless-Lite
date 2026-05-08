Shader "Hidden/Repetitionless/TerrainBasemapGen"
{
    Properties
    {
        _SurfaceTypeSetting("SurfaceType", Int) = 0
        _UVSpace("UVSpace", Int) = 0
        _VertexColourBlendMode("VertexColourBlendMode", Int) = 0
        _DebuggingIndex("DebuggingIndex", Int) = -1
        _LayersCount("LayersCount", Float) = 1

        [HideInInspector] _TerrainHoles("TerrainHoles", 2D) = "white" {}
        [HideInInspector] _Control0("Control0", 2D) = "white" {}
        [HideInInspector] _Control1("Control1", 2D) = "black" {}
        [HideInInspector] _Control2("Control2", 2D) = "black" {}
        [HideInInspector] _Control3("Control3", 2D) = "black" {}
        [HideInInspector] _Control4("Control4", 2D) = "black" {}
        [HideInInspector] _Control5("Control5", 2D) = "black" {}
        [HideInInspector] _Control6("Control6", 2D) = "black" {}
        [HideInInspector] _Control7("Control7", 2D) = "black" {}

        [NoScaleOffset] _PropertiesTexture("PropertiesTexture", 2D) = "white" {}
        [NoScaleOffset] _AssignedTexturesTexture("AssignedTexturesTexture", 2D) = "white" {}
        [NoScaleOffset] _AVTextures("AVTextures", 2DArray) = "" {}
        [NoScaleOffset] _NSOTextures("NSOTextures", 2DArray) = "" {}
        [NoScaleOffset] _EMTextures("EMTextures", 2DArray) = "" {}
        [NoScaleOffset] _BMTextures("BMTextures", 2DArray) = "" {}
        [NoScaleOffset] _NoiseTexture("NoiseTexture", 2D) = "white" {}

        [HideInInspector] _MainTex("BaseMap (RGB) Smoothness (A)", 2D) = "grey" {}
        [HideInInspector] _MetallicTex("Metallic (R)", 2D) = "black" {}
    }

    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex BasemapVert
            #pragma fragment BasemapFrag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            #include "RepetitionlessTerrainPasses.hlsl"

            Varyings BasemapVert(Attributes v)
            {
                Varyings o = (Varyings)0;

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionWS    = posInputs.positionWS;
                o.clipPos       = posInputs.positionCS;
                o.uvMainAndLM.xy = v.texcoord;
                o.uvMainAndLM.zw = v.texcoord;
                o.normalWS      = float3(0, 1, 0);
                o.color         = float4(1, 1, 1, 1);

                return o;
            }

            half4 BasemapFrag(Varyings IN) : SV_Target
            {
                return 1;
            }
            ENDHLSL
        }
    }
}