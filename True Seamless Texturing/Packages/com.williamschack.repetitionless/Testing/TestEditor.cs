using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

using TextureArrayEssentials.GUIUtilities;

public class TestEditor : ShaderGUI
{
    private TextureArrayGUIDrawer _albedoVTexturesDrawer;
    private TextureArrayGUIDrawer _normalSOTexturesDrawer;
    private TextureArrayGUIDrawer _emissionMTexturesDrawer;

    bool _firstStart = true;

    private void OnEnable(MaterialProperty[] properties)
    {
        MaterialProperty albedoVTexturesProp = FindProperty("_AlbedoVTextures", properties);
        MaterialProperty normalSOTexturesProp = FindProperty("_NormalSOTextures", properties);
        MaterialProperty emissionMTexturesProp = FindProperty("_EmissionMTextures", properties);
        MaterialProperty assignedAlbedoVTexturesProp = FindProperty("_AssignedAlbedoVTextures", properties);
        MaterialProperty assignedNormalSOTexturesProp = FindProperty("_AssignedNormalSOTextures", properties);
        MaterialProperty assignedEmissionMTexturesProp = FindProperty("_AssignedEmissionMTextures", properties);

        _albedoVTexturesDrawer = new TextureArrayGUIDrawer(albedoVTexturesProp, assignedAlbedoVTexturesProp, 3);
        _normalSOTexturesDrawer = new TextureArrayGUIDrawer(normalSOTexturesProp, assignedNormalSOTexturesProp, 3);
        _emissionMTexturesDrawer = new TextureArrayGUIDrawer(emissionMTexturesProp, assignedEmissionMTexturesProp, 3);

        _albedoVTexturesDrawer.TextureFormat = TextureFormat.BC7;
        _normalSOTexturesDrawer.TextureFormat = TextureFormat.BC7;
        _normalSOTexturesDrawer.ArrayLinear = true;
        _emissionMTexturesDrawer.TextureFormat = TextureFormat.BC7;
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (_firstStart) {
            OnEnable(properties);
            _firstStart = false;
        }

        
    }
}
#endif