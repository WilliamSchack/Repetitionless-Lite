Shader "Custom/ShaderTest"
{
    Properties
    {
        // Material Properties
        _SurfaceType ("SurfaceType", Float) = 0
        _DebuggingIndex ("DebuggingIndex", Float) = -1

        // Base Material
        _BaseSettings ("BaseSettings", Vector) = (47, 0, 0, 0)
        _BaseTilingOffset ("BaseTilingOffset", Vector) = (1, 1, 0, 0)
        [NoScaleOffset] _BaseAlbedo ("BaseAlbedo", 2D) = "white" { }
        [NoScaleOffset] _BaseMetallicMap ("BaseMetallicMap", 2D) = "black" { }
        [NoScaleOffset] _BaseSmoothnessMap ("BaseSmoothnessMap", 2D) = "black" { }
        [NoScaleOffset] _BaseRoughnessMap ("BaseRoughnessMap", 2D) = "white" { }
        [NoScaleOffset] [Normal] _BaseNormalMap ("BaseNormalMap", 2D) = "bump" { }
        [NoScaleOffset] _BaseOcclussionMap ("BaseOcclussionMap", 2D) = "white" { }
        [NoScaleOffset] _BaseEmissionMap ("BaseEmissionMap", 2D) = "black" { }
        _BaseAlbedoTint ("BaseAlbedoTint", Color) = (1, 1, 1, 1)
        [HDR] _BaseEmissionColor ("BaseEmissionColor", Color) = (0, 0, 0, 0)
        _BaseMaterialProperties1 ("BaseMaterialProperties1", Vector) = (0, 0.5, 0.5, 1)
        _BaseMaterialProperties2 ("BaseMaterialProperties2", Vector) = (1, 0.5, 0, 0)
        _BaseNoiseSettings ("BaseNoiseSettings", Vector) = (2, 5, 0, 0)
        _BaseNoiseMinMax ("BaseNoiseMinMax", Vector) = (0.8, 1.2, 0, 360)
        _BaseVariationMode ("BaseVariationMode", Float) = 2
        _BaseVariationSettings ("BaseVariationSettings", Vector) = (0.5, 2, 1, 0.5)
        _BaseVariationNoiseSettings ("BaseVariationNoiseSettings", Vector) = (0.4, 100, 0, 0)
        _BaseVariationBrightness ("BaseVariationBrightness", Range(0, 1)) = 0.3
        [NoScaleOffset] _BaseVariationTexture ("BaseVariationTexture", 2D) = "black" { }
        _BaseVariationTextureTO ("BaseVariationTextureTO", Vector) = (5, 5, 0, 0)

        // Distance Blend Settings
        [ToggleUI] _DistanceBlendEnabled ("DistanceBlendEnabled", Float) = 0
        _DistanceBlendMode ("DistanceBlendMode", Float) = 0
        _DistanceBlendMinMax ("DistanceBlendMinMax", Vector) = (100, 150, 0, 0)

        // Far Material
        _FarSettings ("FarSettings", Vector) = (47, 0, 0, 0)
        _FarTilingOffset ("FarTilingOffset", Vector) = (1, 1, 0, 0)
        [NoScaleOffset] _FarAlbedo ("FarAlbedo", 2D) = "white" { }
        [NoScaleOffset] _FarMetallicMap ("FarMetallicMap", 2D) = "black" { }
        [NoScaleOffset] _FarSmoothnessMap ("FarSmoothnessMap", 2D) = "black" { }
        [NoScaleOffset] _FarRoughnessMap ("FarRoughnessMap", 2D) = "white" { }
        [NoScaleOffset] [Normal] _FarNormalMap ("FarNormalMap", 2D) = "bump" { }
        [NoScaleOffset] _FarOcclussionMap ("FarOcclussionMap", 2D) = "white" { }
        [NoScaleOffset] _FarEmissionMap ("FarEmissionMap", 2D) = "black" { }
        _FarAlbedoTint ("FarAlbedoTint", Color) = (1, 1, 1, 1)
        [HDR] _FarEmissionColor ("FarEmissionColor", Color) = (0, 0, 0, 0)
        _FarMaterialProperties1 ("FarMaterialProperties1", Vector) = (0, 0.5, 0.5, 1)
        _FarMaterialProperties2 ("FarMaterialProperties2", Vector) = (1, 0.5, 0, 0)
        _FarNoiseSettings ("FarNoiseSettings", Vector) = (2, 5, 0, 0)
        _FarNoiseMinMax ("FarNoiseMinMax", Vector) = (0.8, 1.2, 0, 360)
        _FarVariationMode ("FarVariationMode", Float) = 2
        _FarVariationSettings ("FarVariationSettings", Vector) = (0.5, 2, 1, 0.5)
        _FarVariationBrightness ("FarVariationBrightness", Range(0, 1)) = 0.3
        _FarVariationNoiseSettings ("FarVariationNoiseSettings", Vector) = (0.4, 100, 0, 0)
        [NoScaleOffset] _FarVariationTexture ("FarVariationTexture", 2D) = "black" { }
        _FarVariationTextureTO ("FarVariationTextureTO", Vector) = (5, 5, 0, 0)

        // Material Blend Settings
        _MaterialBlendSettings ("MaterialBlendSettings", Float) = 6
        _BlendMaskType ("BlendMaskType", Float) = 0
        _BlendMaskDistanceTO ("BlendMaskDistanceTO", Vector) = (1, 1, 0, 0)
        _MaterialBlendProperties ("MaterialBlendProperties", Vector) = (1, 1, 0, 0)
        _MaterialBlendNoiseSettings ("MaterialBlendNoiseSettings", Vector) = (10, 0, 0, 0)
        [NoScaleOffset] _BlendMaskTexture ("BlendMaskTexture", 2D) = "black" { }
        _BlendMaskTextureTO ("BlendMaskTextureTO", Vector) = (1, 1, 0, 0)

        // Blend Material
        _BlendSettings ("BlendSettings", Vector) = (47, 0, 0, 0)
        _BlendTilingOffset ("BlendTilingOffset", Vector) = (1, 1, 0, 0)
        [NoScaleOffset] _BlendAlbedo ("BlendAlbedo", 2D) = "white" { }
        [NoScaleOffset] _BlendMetallicMap ("BlendMetallicMap", 2D) = "black" { }
        [NoScaleOffset] _BlendSmoothnessMap ("BlendSmoothnessMap", 2D) = "black" { }
        [NoScaleOffset] _BlendRoughnessMap ("BlendRoughnessMap", 2D) = "white" { }
        [NoScaleOffset] [Normal] _BlendNormalMap ("BlendNormalMap", 2D) = "bump" { }
        [NoScaleOffset] _BlendOcclussionMap ("BlendOcclussionMap", 2D) = "white" { }
        [NoScaleOffset] _BlendEmissionMap ("BlendEmissionMap", 2D) = "black" { }
        _BlendAlbedoTint ("BlendAlbedoTint", Color) = (1, 1, 1, 1)
        [HDR] _BlendEmissionColor ("BlendEmissionColor", Color) = (0, 0, 0, 0)
        _BlendMaterialProperties1 ("BlendMaterialProperties1", Vector) = (0, 0.5, 0.5, 1)
        _BlendMaterialProperties2 ("BlendMaterialProperties2", Vector) = (1, 0.5, 0, 0)
        _BlendNoiseSettings ("BlendNoiseSettings", Vector) = (2, 5, 0, 0)
        _BlendNoiseMinMax ("BlendNoiseMinMax", Vector) = (0.8, 1.2, 0, 360)
        _BlendVariationMode ("BlendVariationMode", Float) = 2
        _BlendVariationSettings ("BlendVariationSettings", Vector) = (0.5, 2, 1, 0.5)
        _BlendVariationBrightness ("BlendVariationBrightness", Range(0, 1)) = 0.3
        _BlendVariationNoiseSettings ("BlendVariationNoiseSettings", Vector) = (0.4, 100, 0, 0)
        [NoScaleOffset] _BlendVariationTexture ("BlendVariationTexture", 2D) = "black" { }
        _BlendVariationTextureTO ("BlendVariationTextureTO", Vector) = (5, 5, 0, 0)
    }
    
    HLSLINCLUDE

    // CHANGE INCLUDE TO RELATIVE PATH AFTER CLEANUP/COMPLETION
    #include "Assets/Repetitionless/Shaders/HLSL/SampleSeamlessMaterialMaster.hlsl"

    ENDHLSL

    // Built In Pipeline
    SubShader
    {
        Name "Built-In"
        Tags {
            "RenderType" = "Opaque"
        }
        LOD 200

        HLSLPROGRAM
            // CANT USE SURFACE IN URP
            // ALSO INCLUDES ALL THE VARIANTS THAT SHADER GRPAH DOES
            // JUST LEARN REGULAR SHADERS
            // AHHHHHHHHHHHHHHHHHHHHHHHHHHH

            #pragma surface surf Standard fullforwardshadows

            #include "Lighting.cginc"

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
                float3 worldNormal;
            };

            sampler2D _MainTexture;
            fixed4 _Color;

            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                fixed4 albedoOut = 0;
                float3 normalOut = 0;
                float metallicOut = 0;
                float smoothnessOut = 0;
                float occlussionOut = 0;
                fixed3 emissionOut = 0;
            }
        ENDHLSL
    }

    Fallback "Diffuse"
    CustomEditor "Repetitionless.Inspectors.SeamlessMaterialMasterGUI"
}
