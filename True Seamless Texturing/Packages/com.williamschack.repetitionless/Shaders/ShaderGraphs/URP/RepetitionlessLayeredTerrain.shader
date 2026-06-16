Shader "Repetitionless/URP/RepetitionlessLayered"
{
    Properties
    {
        _SurfaceTypeSetting("Surface Type", Int) = 0
        _UVSpace("UV Space", Int) = 0
        _VertexColourBlendMode("Vertex Colour Blend Mode", Int) = 0
        _DebuggingIndex("Debugging Index", Int) = -1
        _LayersCount("Layers Count", Int) = 1

        [NoScaleOffset] _PropertiesTexture("Properties Texture", 2D) = "white" {}
        [NoScaleOffset] _AssignedTexturesTexture("Assigned Textures Texture", 2D) = "white" {}
        [NoScaleOffset] _AVTextures("AV Textures", 2DArray) = "white" {}   // Albedo, Variation
        [NoScaleOffset] _NSOTextures("NSO Textures", 2DArray) = "white" {} // Normal, Smoothness/Roughness, Occlussion
        [NoScaleOffset] _EMTextures("EM Textures", 2DArray) = "white" {}   // Emission, Metallic
        [NoScaleOffset] _BMTextures("BM Textures", 2DArray) = "white" {}   // Blend Mask
        [NoScaleOffset] _NoiseTexture("Noise Texture", 2D) = "white" {}

        _TerrainHoles("Terran Holes", 2D) = "white" {}
        _Control0("Control 0", 2D) = "white" {}
        _Control1("Control 1", 2D) = "black" {}
        _Control2("Control 2", 2D) = "black" {}
        _Control3("Control 3", 2D) = "black" {}
        _Control4("Control 4", 2D) = "black" {}
        _Control5("Control 5", 2D) = "black" {}
        _Control6("Control 6", 2D) = "black" {}
        _Control7("Control 7", 2D) = "black" {}
    }

    HLSLINCLUDE
    #pragma multi_compile_fragment __ _ALPHATEST_ON

    #ifndef REPETITIONLESS_LAYERED
    #define REPETITIONLESS_LAYERED
    #endif
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "False"
            "Queue" = "Geometry-100"
            "TerrainCompatible" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex Vert
            #pragma fragment Frag

            // Common
            #include_with_pragmas "Pragmas/TerrainKeywords.hlsl"
            #include_with_pragmas "Pragmas/TerrainCommon.hlsl"

            // Keywords
#ifdef UNITY_PLATFORM_META_QUEST
            #pragma multi_compile _ META_QUEST_ORTHO_PROJ
            #pragma multi_compile _ META_QUEST_NO_SPOTLIGHTS_LIGHT_LOOP
#endif

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            #include "Input.hlsl"
            #include "TerrainPasses.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex ShadowPassVert
            #pragma fragment ShadowPassFrag
            
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Input.hlsl"
            #include "TerrainPasses.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "GBuffer"
            Tags { "LightMode" = "UniversalGBuffer" }

            HLSLPROGRAM
            #pragma target 4.5

            #pragma exclude_renderers gles3 glcore

            #pragma vertex Vert
            #pragma fragment Frag

            // Common
            #include_with_pragmas "Pragmas/TerrainKeywords.hlsl"
            #include_with_pragmas "Pragmas/TerrainCommon.hlsl"

            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GBufferOutputFormat.hlsl"
            #include "Input.hlsl"
            #include "TerrainPasses.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 2.0

            #pragma vertex DepthOnlyVert
            #pragma fragment DepthOnlyFrag

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #include "Input.hlsl"
            #include "TerrainPasses.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            HLSLPROGRAM
            #pragma vertex TerrainVertexMeta
            #pragma fragment TerrainFragmentMeta

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #pragma shader_feature EDITOR_VISUALIZATION

            #include "Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitMetaPass.hlsl"

            ENDHLSL
        }

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    CustomEditor "Repetitionless.Editor.Inspectors.RepetitionlessMaterialEditorTerrain"

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}