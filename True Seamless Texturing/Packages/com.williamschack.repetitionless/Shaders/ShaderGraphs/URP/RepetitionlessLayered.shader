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
        [NoScaleOffset] _AVTextures("AV Textures", 2DArray) = "white" {} // Albedo, Variation
        [NoScaleOffset] _NSOTextures("NSO Textures", 2DArray) = "white" {} // Normal, Smoothness/Roughness, Occlussion
        [NoScaleOffset] _EMTextures("EM Textures", 2DArray) = "white" {} // Emission, Metallic
        [NoScaleOffset] _BMTextures("BM Textures", 2DArray) = "white" {} // Blend Mask

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
    ENDHLSL

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "False"
            "TerrainCompatible" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0

            #pragma vertex RepetitionlessTerrainVert
            #pragma fragment RepetitionlessTerrainFrag

            // URP Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            // Unity defined
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            // Custom Keywords
            #pragma shader_feature_local _ _REPETITIONLESS_DISTANCE_BLEND
            #pragma shader_feature_local _ _REPETITIONLESS_MATERIAL_BLEND
            #pragma shader_feature_local _ _REPETITIONLESS_TRIPLANAR
            #pragma shader_feature_local _ _REPETITIONLESS_NOISE_TEXTURE
            #pragma shader_feature_local _ _REPETITIONLESS_VARIATION
            #pragma shader_feature_local _MAX_LAYERS_4 _MAX_LAYERS_8 _MAX_LAYERS_12 _MAX_LAYERS_16 _MAX_LAYERS_20 _MAX_LAYERS_24 _MAX_LAYERS_28 _MAX_LAYERS_32

            #pragma shader_feature_local_fragment _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ _ENVIRONMENTREFLECTIONS_OFF

            //#include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
            #include "RepetitionlessTerrainPasses.hlsl"

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

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "RepetitionlessTerrainPasses.hlsl"

            ENDHLSL
        }

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    CustomEditor "Repetitionless.Editor.Inspectors.RepetitionlessMaterialEditorTerrain"

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}