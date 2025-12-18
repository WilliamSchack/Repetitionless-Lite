# Variables.RepetitionlessLayerDataCompressed

## Description

The compressed layer data that will be passed to the shader fro a repetitionless material

## Members

| Member | Description |
|--------|-------------|
| BaseMaterialData | The compressed base material data |
| FarMaterialData | The compressed far material data |
| BlendMaterialData | The compressed blend material data |
| DistanceBlendSettings | x: DistanceBlendEnabledy: DistanceBlendModezw: DistanceBlendMinMax |
| BlendMaskDistanceTO | The blend mask distance tiling offset:xy: Tiling, zw: Offset |
| MaterialBlendSettings | x: Compressed Bools0 > MaterialBlendEnabled1 > BlendMaskAssigned2 > OverrideDistanceBlend3 > OverrideDistanceBlendTOy: BlendMaskTypez: BlendMaskOpacityw: BlendMaskStrength |
| MaterialBlendMaskTO | <strong>IF USING NOISE MASK:</strong>x: BlendMaskNoiseScalezw: BlendMaskNoiseOffset<strong>IF USING TEXTURE MASK:</strong>xyzw: BlendMaskTextureTO |

---

