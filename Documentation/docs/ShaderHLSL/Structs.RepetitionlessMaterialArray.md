# Structs/RepetitionlessMaterialArray

## Description

Contains the textures and data for a repetitionless material using a texture array

Textures are only used if their respective settings are enabled

## Variables

| Variable              | Description                                                                                                                                           |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Textures              | The texture array with textures at the constant indexes:<br>0: Albedo<br>1: Metallic<br>2: Smoothness<br>3: Roughness<br>4: Occlussion<br>5: Emission |
| NormalMap             | The normal map                                                                                                                                        |
| ArrayAssignedTextures | The compressed assigned textures that contains which textures are assigned in the array                                                               |
| Data                  | The material data                                                                                                                                     |

---