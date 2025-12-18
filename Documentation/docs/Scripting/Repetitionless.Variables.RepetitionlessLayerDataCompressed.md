# Variables.RepetitionlessLayerDataCompressed

## Description

The compressed layer data that will be passed to the shader fro a repetitionless material

## Members

| Member | Description |
|--------|-------------|
| BaseMaterialData | The compressed base material data |
| FarMaterialData | The compressed far material data |
| BlendMaterialData | The compressed blend material data |
| DistanceBlendSettings | x: DistanceBlendEnabled<br />y: DistanceBlendMode<br />zw: DistanceBlendMinMax |
| BlendMaskDistanceTO | The blend mask distance tiling offset:<br />xy: Tiling, zw: Offset |
| MaterialBlendSettings | x: Compressed Bools<br />0 > MaterialBlendEnabled<br />1 > BlendMaskAssigned<br />2 > OverrideDistanceBlend<br />3 > OverrideDistanceBlendTO<br />y: BlendMaskType<br />z: BlendMaskOpacity<br />w: BlendMaskStrength |
| MaterialBlendMaskTO | <strong>IF USING NOISE MASK:</strong><br />x: BlendMaskNoiseScale<br />zw: BlendMaskNoiseOffset<br /><strong>IF USING TEXTURE MASK:</strong><br />xyzw: BlendMaskTextureTO |

---

