## Data Folder

All the properties and textures are stored in a folder along side the material named<br />
`<Material Name>_RepetitionlessData`

This folder is automatically handled and you do not need to worry about it

*This folder will automatically be moved and deleted with the material but will not be copied so if you need to copy a material you will need to manually copy the folder aswell*

![image](Images/MaterialDetails/DataFolder.png)

## Texture Packing

All the textures are packed into texture arrays for less samples in the shader which are stored in the data folder along side the material. These can be modified by clicking the **Array Settings button** in the Material Properties section

Arrays are split into:

- AVTextures: Albedo (rgb), Variation (a)
- NSOTextures: Normal (rg), Smoothness (b), Occlussion (a)
- EMTextures: Emission (rgb), Metallic (a)
- BMTextures: Blend Mask (r)

*Only the required arrays are created*

***To view what each property does, visit the [Material Properties](material-properties.md) page***

## Colour Space

sRGB textures (Albedo/Colour) are packed with the set colour space in mind. They will need to be repacked when changing the colour space

There is a popup when changing the colour space if any textures need to be repacked that will automatically repack the textures for you.<br />
Make sure to select **Update Textures to \<Colour Space\>** for the textures to look correct

![image](Images/MaterialDetails/ColourSpacePopup.png)

