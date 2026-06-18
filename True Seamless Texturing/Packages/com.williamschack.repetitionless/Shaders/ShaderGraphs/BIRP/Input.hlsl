#ifndef REPETITIONLESSINPUT_INCLUDED
#define REPETITIONLESSINPUT_INCLUDED

// Material Properties
float _SurfaceTypeSetting;
float _UVSpace;
float _VertexColourBlendMode;
half  _DebuggingIndex;
float _LayersCount;
float4 _NoiseTexture_TexelSize;

// Textures
sampler2D _NoiseTexture;
sampler2D _PropertiesTexture;
sampler2D _AssignedTexturesTexture;
UNITY_DECLARE_TEX2DARRAY(_AVTextures);
UNITY_DECLARE_TEX2DARRAY(_NSOTextures);
UNITY_DECLARE_TEX2DARRAY(_EMTextures);
UNITY_DECLARE_TEX2DARRAY(_BMTextures);

#ifdef REPETITIONLESS_LAYERED
sampler2D _TerrainHoles;
sampler2D _Control0;
sampler2D _Control1;
sampler2D _Control2;
sampler2D _Control3;
sampler2D _Control4;
sampler2D _Control5;
sampler2D _Control6;
sampler2D _Control7;
#endif
#endif