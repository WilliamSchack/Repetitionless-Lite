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
    #include "Assets/Repetitionless/Shaders/HLSL/Structs/RepetitionlessStructs.hlsl"
    #include "Assets/Repetitionless/Shaders/HLSL/SampleSeamlessMaterialMaster_CODE.hlsl" // CHANGE CODE NAMES TO PROPER AFTERWARDS

    // Properties
    CBUFFER_START(UnityPerMaterial)
        // Material Properties
        int _SurfaceType;
        int _DebuggingIndex;

        // Base Material
        float2 _BaseSettings;
        float4 _BaseTilingOffset;
        sampler2D _BaseAlbedo;
        sampler2D _BaseMetallicMap;
        sampler2D _BaseSmoothnessMap;
        sampler2D _BaseRoughnessMap;
        sampler2D _BaseNormalMap;
        sampler2D _BaseOcclussionMap;
        sampler2D _BaseEmissionMap;
        float4 _BaseAlbedoTint;
        float3 _BaseEmissionColor;
        float4 _BaseMaterialProperties1;
        float2 _BaseMaterialProperties2;
        float2 _BaseNoiseSettings;
        float4 _BaseNoiseMinMax;
        float _BaseVariationMode;
        float4 _BaseVariationSettings;
        float4 _BaseVariationNoiseSettings;
        float _BaseVariationBrightness;
        sampler2D _BaseVariationTexture;
        float4 _BaseVariationTextureTO;

        // Distance Blend Settings
        bool _DistanceBlendEnabled;
        int _DistanceBlendMode;
        float2 _DistanceBlendMinMax;

        // Far Material
        float2 _FarSettings;
        float4 _FarTilingOffset;
        sampler2D _FarAlbedo;
        sampler2D _FarMetallicMap;
        sampler2D _FarSmoothnessMap;
        sampler2D _FarRoughnessMap;
        sampler2D _FarNormalMap;
        sampler2D _FarOcclussionMap;
        sampler2D _FarEmissionMap;
        float4 _FarAlbedoTint;
        float3 _FarEmissionColor;
        float4 _FarMaterialProperties1;
        float2 _FarMaterialProperties2;
        float2 _FarNoiseSettings;
        float4 _FarNoiseMinMax;
        float _FarVariationMode;
        float4 _FarVariationSettings;
        float4 _FarVariationNoiseSettings;
        float _FarVariationBrightness;
        sampler2D _FarVariationTexture;
        float4 _FarVariationTextureTO;

        // Material Blend Settings
        float _MaterialBlendSettings;
        int _BlendMaskType;
        float4 _BlendMaskDistanceTO;
        float2 _MaterialBlendProperties;
        float3 _MaterialBlendNoiseSettings;
        sampler2D _BlendMaskTexture;
        float4 _BlendMaskTextureTO;

        // Blend Material
        float2 _BlendSettings;
        float4 _BlendTilingOffset;
        sampler2D _BlendAlbedo;
        sampler2D _BlendMetallicMap;
        sampler2D _BlendSmoothnessMap;
        sampler2D _BlendRoughnessMap;
        sampler2D _BlendNormalMap;
        sampler2D _BlendOcclussionMap;
        sampler2D _BlendEmissionMap;
        float4 _BlendAlbedoTint;
        float3 _BlendEmissionColor;
        float4 _BlendMaterialProperties1;
        float2 _BlendMaterialProperties2;
        float2 _BlendNoiseSettings;
        float4 _BlendNoiseMinMax;
        float _BlendVariationMode;
        float4 _BlendVariationSettings;
        float4 _BlendVariationNoiseSettings;
        float _BlendVariationBrightness;
        sampler2D _BlendVariationTexture;
        float4 _BlendVariationTextureTO;
    CBUFFER_END

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
            #pragma surface surf Standard fullforwardshadows

            #include "UnityCG.cginc"
            #include "HLSLSupport.cginc"
            #include "Lighting.cginc"

            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
                float3 worldNormal;
                INTERNAL_DATA
            };

            void surf (Input IN, inout SurfaceOutputStandard o)
            {
                // Create Materials
                RepetitionlessMaterial baseMaterial = {
                    _BaseSettings,
                    _BaseTilingOffset,
                    _BaseAlbedo,
                    _BaseMetallicMap,
                    _BaseSmoothnessMap,
                    _BaseRoughnessMap,
                    _BaseNormalMap,
                    _BaseOcclussionMap,
                    _BaseEmissionMap,
                    _BaseAlbedoTint,
                    _BaseEmissionColor,
                    _BaseMaterialProperties1,
                    _BaseMaterialProperties2,
                    _BaseNoiseSettings,
                    _BaseNoiseMinMax,
                    _BaseVariationMode,
                    _BaseVariationSettings,
                    _BaseVariationNoiseSettings,
                    _BaseVariationBrightness,
                    _BaseVariationTexture,
                    _BaseVariationTextureTO
                };

                RepetitionlessMaterial farMaterial = {
                    _FarSettings,
                    _FarTilingOffset,
                    _FarAlbedo,
                    _FarMetallicMap,
                    _FarSmoothnessMap,
                    _FarRoughnessMap,
                    _FarNormalMap,
                    _FarOcclussionMap,
                    _FarEmissionMap,
                    _FarAlbedoTint,
                    _FarEmissionColor,
                    _FarMaterialProperties1,
                    _FarMaterialProperties2,
                    _FarNoiseSettings,
                    _FarNoiseMinMax,
                    _FarVariationMode,
                    _FarVariationSettings,
                    _FarVariationNoiseSettings,
                    _FarVariationBrightness,
                    _FarVariationTexture,
                    _FarVariationTextureTO
                };

                RepetitionlessMaterial blendMaterial = {
                    _BlendSettings,
                    _BlendTilingOffset,
                    _BlendAlbedo,
                    _BlendMetallicMap,
                    _BlendSmoothnessMap,
                    _BlendRoughnessMap,
                    _BlendNormalMap,
                    _BlendOcclussionMap,
                    _BlendEmissionMap,
                    _BlendAlbedoTint,
                    _BlendEmissionColor,
                    _BlendMaterialProperties1,
                    _BlendMaterialProperties2,
                    _BlendNoiseSettings,
                    _BlendNoiseMinMax,
                    _BlendVariationMode,
                    _BlendVariationSettings,
                    _BlendVariationNoiseSettings,
                    _BlendVariationBrightness,
                    _BlendVariationTexture,
                    _BlendVariationTextureTO
                };

                // Create layer
                RepetitionlessLayer layer = {
                    baseMaterial,
                    farMaterial,
                    blendMaterial
                };

                // Sample materials
                fixed4 albedoOut = 0;
                float3 normalOut = 0;
                float metallicOut = 0;
                float smoothnessOut = 0;
                float occlussionOut = 0;
                fixed3 emissionOut = 0;

                SampleSeamlessMaterialMaster(
                    IN.uv_MainTex, IN.worldNormal,
                    IN.worldPos, _WorldSpaceCameraPos,
                    _SurfaceType, _DebuggingIndex,
                    baseMaterial, farMaterial, blendMaterial,
                    _DistanceBlendEnabled, _DistanceBlendMode, _DistanceBlendMinMax,
                    _MaterialBlendSettings, _BlendMaskType, _BlendMaskDistanceTO,
                    _MaterialBlendProperties, _MaterialBlendNoiseSettings,
                    _BlendMaskTexture, _BlendMaskTextureTO,
                    albedoOut, normalOut, metallicOut, smoothnessOut, occlussionOut, emissionOut
                );

                o.Albedo = albedoOut;
                o.Normal = normalOut;
            }
        ENDHLSL
    }

    Fallback "Diffuse"
    CustomEditor "Repetitionless.Inspectors.SeamlessMaterialMasterGUI"
}
