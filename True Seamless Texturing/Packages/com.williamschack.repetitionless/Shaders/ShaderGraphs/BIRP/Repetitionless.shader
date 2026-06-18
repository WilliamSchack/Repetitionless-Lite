Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _SurfaceTypeSetting("Surface Type", Int) = 0
        _UVSpace("UV Space", Int) = 0
        _VertexColourBlendMode("Vertex Colour Blend Mode", Int) = 0
        _DebuggingIndex("Debugging Index", Int) = -1

        [NoScaleOffset] _PropertiesTexture("Properties Texture", 2D) = "white" {}
        [NoScaleOffset] _AssignedTexturesTexture("Assigned Textures Texture", 2D) = "white" {}
        [NoScaleOffset] _AVTextures("AV Textures", 2DArray) = "white" {}   // Albedo, Variation
        [NoScaleOffset] _NSOTextures("NSO Textures", 2DArray) = "white" {} // Normal, Smoothness/Roughness, Occlussion
        [NoScaleOffset] _EMTextures("EM Textures", 2DArray) = "white" {}   // Emission, Metallic
        [NoScaleOffset] _BMTextures("BM Textures", 2DArray) = "white" {}   // Blend Mask
        [NoScaleOffset] _NoiseTexture("Noise Texture", 2D) = "white" {}

        // Blending
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }

    HLSLINCLUDE
    #ifndef REPETITIONLESS_BIRP
    #define REPETITIONLESS_BIRP
    #endif
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend[_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            #include "HLSLSupport.cginc"
            #include "UnityShaderVariables.cginc"

            #pragma target 3.0

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

#if UNITY_VERSION >= 202220
            #pragma multi_compile _ LOD_FADE_CROSSFADE
#endif

            #include "Input.hlsl"
            #include "Passes.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "Repetitionless.Editor.Inspectors.RepetitionlessMaterialEditorMaster"
    FallBack "Diffuse"
}