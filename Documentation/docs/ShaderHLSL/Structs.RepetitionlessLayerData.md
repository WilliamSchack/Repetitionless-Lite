# Structs/RepetitionlessLayerData

## Description

Contains the data for a repetitionless layer

## Variables

| Variable                   | Description                                                                                                                                            |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| DistanceBlendEnabled       | If distance blend is enabled                                                                                                                           |
| DistanceBlendingMode       | The distance blending mode index<br>0: Tiling and offset<br>1: Seperate material                                                                       |
| DistanceBlendMinMax        | The world distance range from the camera for the distance blending                                                                                     |
| MaterialBlendSettings      | Compressed material blend settings<br>0: Material blend enabled<br>1: Override distance blending<br>2: Override Distance Blending Tiling and Offset    |
| BlendMaskType              | The blend mask type<br>0: Perlin noise<br>1: Simplex noise<br>2: Custom Texture                                                                        |
| BlendMaskDistanceTO        | The distance blend tiling and offset<br>xy: Distance blend tiling<br>zw: Distance blend offset                                                         |
| MaterialBlendProperties    | Material blend mask properties<br>x: Blend mask opacity<br>y: Blend mask strength                                                                      |
| MaterialBlendNoiseSettings | Material blend noise settings<br>*Used if BlendMaskType is set to any noise*<br>x: Blend mask noise scale<br>yz: Blend mask noise offset               |
| BlendMaskTexture           | The blend mask texture<br>*Used if BlendMaskType is set to Custom Texture*                                                                             |
| BlendMaskTextureTO         | The blend mask tiling and offset<br>*Used if BlendMaskType is set to Custom Texture*<br>xy: Blend mask texture tiling<br>zw: Blend mask texture offset |

---