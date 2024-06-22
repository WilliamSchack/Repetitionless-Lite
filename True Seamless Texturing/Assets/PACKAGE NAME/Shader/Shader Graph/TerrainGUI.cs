using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class TerrainGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        Material material = (Material)materialEditor.target;
        material.SetOverrideTag("SplatCount", "8");

    }
}
#endif