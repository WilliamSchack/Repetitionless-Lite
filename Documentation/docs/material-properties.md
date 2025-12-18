![image](Images/MaterialProperties/MaterialProperties.png)

Has all the general properties for the material

| Property                         | Description                                                                                                                                                                                                                                                                         |
| -------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Surface Type                     | The surface type of the material<br>0: Opaque<br>1: Cutout<br>2: Transparent                                                                                                                                                                                                        |
| UV Space                         | Sets the UV Space<br>0: Local<br>1: World<br>*Note that with world space UVs you will require larger Tiling*                                                                                                                                                                        |
| Triplanar                        | Enables triplanar sampling and forces world UVs<br>*Note that the first time this is set for a material it will take some time to finish as it will recompile the shader, it will be near instant for subsequent toggles*                                                           |
| Global Illuminattion             | Controls if the global illumination is baked or realtime<br>0: Realtime<br>1: Baked<br>2: None                                                                                                                                                                                      |
| Render Queue                     | Changes when the material is drawn                                                                                                                                                                                                                                                  |
| Double Sided Global Illumination | If the lightmapper accounts for both sides of the geometry when calculating Global Illumination                                                                                                                                                                                     |
| Array Settings                   | Shows a dropdown with buttons to modify each of the texture arrays that the textures are stored in<br>AVTextures: Albedo (rgb), Variation (a)<br>NSOTextures: Normal (rg), Smoothness (b), Occlussion (a)<br>EMTextures: Emission (rgb), Metallic (a)<br>BMTextures: Blend Mask (r) |
| Ping Data Folder                 | Pings the data folder                                                                                                                                                                                                                                                               |

## Toolbar

![image](Images/MaterialProperties/MaterialToolbar.png)

Each material has a toolbar with various settings

Each settings text will shrink to the first letters of each word if the inspector width is too small for the whole words to fit

| Setting         | Description                                                                                                                                                                 |
| --------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Noise           | Adds random scaling & rotation based on voronoi noise                                                                                                                       |
| Random Scaling  | Adds random scaling to each voronoi cell<br>*Only shown if Noise is enabled*                                                                                                |
| Random Rotation | Adds random rotation to each voronoi cell<br>*Only shown if Noise is enabled*                                                                                               |
| Variation       | Adds random variation on top of the albedo color<br>**Using a custom texture can cause visible tiling**                                                                     |
| Packed Texture  | If you are using a packed texture of multiple regular ones (Better for performance)<br>R: Metallic<br>G: Occlussion<br>A: Smoothness/Roughness                              |
| Emission        | If Emission is enabled                                                                                                                                                      |
| Smooth/Rough    | Toggles between the smoothness and roughness texture<br>**If you are using a packed texture it will sample it with a smoothness or roughness texture based on this toggle** |

## Material Settings

![image](Images/MaterialProperties/MaterialSettings.png)

Contains all the settings for a material. Settings can be enabled and disabled through the toolbar

This is shown for each enabled material (Base material, distance blend material, blend material)

### MainProperties

| Property             | Description                                                                                                                                                                                                                    |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Albedo               | Albedo (RGB)                                                                                                                                                                                                                   |
| NormalMap            | Normal map (RG)                                                                                                                                                                                                                |
| Metallic             | Metallic (R)                                                                                                                                                                                                                   |
| Smoothness/Roughness | Smoothness/Roughness (R)<br>*Changes between smoothness and roughness based on toolbar setting*                                                                                                                                |
| Occlussion           | Occlussion (R)                                                                                                                                                                                                                 |
| Emission             | Emission (RGB)                                                                                                                                                                                                                 |
| Packed Texture       | The mask map for the current material<br>R: Metallic<br>G: Occlussion<br>A: Smoothness/Roughness<br>- *Only shown if Packed Texture is enabled*<br>- *Alpha toggles between smoothness and roughness based on toolbar setting* |
| Occlussion Strength  | The occlussion strength of the packed texture occlussion<br>*Only shown if Packed Texture is enabled*                                                                                                                          |
| Scale                | The texture tiling                                                                                                                                                                                                             |
| Offset               | The texture offset                                                                                                                                                                                                             |
### Noise Properties

These are only shown if Noise is enabled in the toolbar

| Property                | Description                                                                                                                                                            |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Noise Angle Offset      | The angle offset of the voronoi noise                                                                                                                                  |
| Noise Scale             | The scale of the voronoi noise                                                                                                                                         |
| Noise Scaling Min Max   | Range that each voronoi cell is randomly scaled by<br>x: Min Scale<br>y: Max Scale<br>*Only shown if Random Scaling is enabled in the toolbar*                         |
| Random Rotation Min Max | Range that each voronoi cell is randomly rotated by<br>x: Min Rotation Degrees<br>y: Max Rotation Degrees<br>*Only shown if Random Rotation is enabled in the toolbar* |

### Variation Properties

These are only shown if Variation is enabled in the toolbar

| Property          | Description                                                                                                                                                                  |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Variation Mode    | The variation mode used<br>Using a custom texture can cause visible tiling<br>0: Perlin noise<br>1: Simplex noise<br>2: Custom texture                                       |
| Opacity           | Transparency of the variation                                                                                                                                                |
| Brightness        | Intensity of the variation                                                                                                                                                   |
| Small Scale       | Scale of the small variation sample                                                                                                                                          |
| Medium Scale      | Scale of the medium variation sample                                                                                                                                         |
| Large Scale       | Scale of the large variation sample                                                                                                                                          |
| Noise Strength    | Strength of the noise<br>*Only shown if Variation Mode is set to any noise*                                                                                                  |
| Noise Scale       | The noise tiling<br>*Only shown if Variation Mode is set to any noise*                                                                                                       |
| Noise Offset      | The noise offset<br>*Only shown if Variation Mode is set to any noise*                                                                                                       |
| Variation Texture | Texture that is drawn onto other materials, can cause visible tiling<br>Variation (R), other channels are ignored<br>*Only shown if Variation Mode is set to Custom Texture* |
| Variation Scale   | The variation tiling<br>*Only shown if Variation Mode is set to Custom Texture*                                                                                              |
| Variation Offset  | The variation offset<br>*Only shown if Variation Mode is set to Custom Texture*                                                                                              |

## Distance Blending

![image](Images/MaterialProperties/DistanceBlendingSettingsTO.png)

This is the material that will fade into the distance based on Distance Blend Min Max. Can be toggled by clicking the Distance Blending button

If the Blend Mode is set to Material, a separate Material Settings will be shown below for the far material

| Property               | Description                                                                                                                                                              |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Blend Mode             | Tiling Offset: Resamples materials with different Tiling and Offset<br>Material: Samples a seperate far material                                                         |
| Distance Blend Min Max | Blend distance which the material will be sampled. Materials will be blended with regular material at min, and far material at max<br>x: Min Distance<br>y: Max Distance |
| Scale                  | The far tiling<br>*Only shown if Blend mode is set to Tiling Offset*                                                                                                     |
| Offset                 | The far offset<br>*Only shown if Blend mode is set to Tiling Offset*                                                                                                     |

## Material Blending

![image](Images/MaterialProperties/MaterialBlendingSettings.png)

This is a separate material that will overlay the base and far material if set based on a mask. Can be toggled by clicking the Material Blending button

### Mask Settings

| Property      | Description                                                                                                                                                                                                     |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Mask Type     | The mask type used<br>0: Perlin noise<br>1: Simplex noise<br>2: Custom texture                                                                                                                                  |
| Mask Opacity  | Opacity of the mask and in response the blend material                                                                                                                                                          |
| Mask Strength | The higher the value, the sharper the edges and vice versa                                                                                                                                                      |
| Noise Scale   | The noise tiling<br>*Only shown if Mask Type is set to any noise*                                                                                                                                               |
| Noise Offset  | The noise offset<br>*Only shown if Mask Type is set to any noise*                                                                                                                                               |
| Blend Mask    | Texture that is sampled as the mask for the blend material. Color from black-white represents opacity (0-1)<br>Blend Mask (R), other channels are ignored<br>*Only shown if Mask Type is set to Custom Texture* |
| Scale         | The blend mask tiling<br>*Only shown if Mask Type is set to Custom Texture*                                                                                                                                     |
| Offset        | The blend mask offset<br>*Only shown if Mask Type is set to Custom Texture*                                                                                                                                     |

### Distance Blending Settings

These are only shown if distance blending is enabled

| Property                   | Description                                                                           |
| -------------------------- | ------------------------------------------------------------------------------------- |
| Override Distance Blending | Draws the blend material on top of the far material                                   |
| Override Tiling & Offset   | Uses defined Tiling & Offset rather than distance blend Tiling & Offset               |
| Scale                      | The blend mask distance scale<br>*Only shown if Override Tiling & Offset is enabled*  |
| Offset                     | The blend mask distance offset<br>*Only shown if Override Tiling & Offset is enabled* |

## Debug

![image](Images/MaterialProperties/DebugSettings.png)

This will show the values of the selected option. Can be toggled by clicking the Debug button

| Option               | Description                                 |
| -------------------- | ------------------------------------------- |
| Voronoi Cells        | Outputs the value of each voronoi cell      |
| Edge Mask            | Outputs the edge mask for the voronoi cells |
| Distance Mask        | Outputs the distance mask                   |
| Blend Material Mask  | Outputs the blend material mask             |
| Variation Multiplier | Outputs the variation multiplier            |
