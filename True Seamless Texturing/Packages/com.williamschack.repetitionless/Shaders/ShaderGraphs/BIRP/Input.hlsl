#ifndef REPETITIONLESSINPUT_INCLUDED
#define REPETITIONLESSINPUT_INCLUDED

// Compatibility with URP/HDRP defines
#define TEXTURE2D(tex) Texture2D tex
#define SAMPLER(tex) SamplerState tex
#define TEXTURE2D_ARRAY(tex) Texture2DArray tex
#define SAMPLE_TEXTURE2D(tex, ss, uv) tex.Sample(ss, uv)
#define SAMPLE_TEXTURE2D_ARRAY(tex, ss, uv, i) tex.Sample(ss, float3(uv, i))

// Material Properties
float _SurfaceTypeSetting;
float _UVSpace;
float _VertexColourBlendMode;
half  _DebuggingIndex;
float _LayersCount;
float4 _NoiseTexture_TexelSize;

// Textures
TEXTURE2D(_NoiseTexture);            SAMPLER(sampler_NoiseTexture);
TEXTURE2D(_PropertiesTexture);       SAMPLER(sampler_PropertiesTexture);
TEXTURE2D(_AssignedTexturesTexture); SAMPLER(sampler_AssignedTexturesTexture);
TEXTURE2D_ARRAY(_AVTextures);        SAMPLER(sampler_AVTextures);
TEXTURE2D_ARRAY(_NSOTextures);       SAMPLER(sampler_NSOTextures);
TEXTURE2D_ARRAY(_EMTextures);        SAMPLER(sampler_EMTextures);
TEXTURE2D_ARRAY(_BMTextures);        SAMPLER(sampler_BMTextures);

#ifdef REPETITIONLESS_LAYERED
TEXTURE2D(_TerrainHoles);            SAMPLER(sampler_TerrainHoles);
TEXTURE2D(_Control0);                SAMPLER(sampler_Control0);
TEXTURE2D(_Control1);                SAMPLER(sampler_Control1);
TEXTURE2D(_Control2);                SAMPLER(sampler_Control2);
TEXTURE2D(_Control3);                SAMPLER(sampler_Control3);
TEXTURE2D(_Control4);                SAMPLER(sampler_Control4);
TEXTURE2D(_Control5);                SAMPLER(sampler_Control5);
TEXTURE2D(_Control6);                SAMPLER(sampler_Control6);
TEXTURE2D(_Control7);                SAMPLER(sampler_Control7);
#endif
#endif