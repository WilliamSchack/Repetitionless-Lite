#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CBufferTestingEditor : ShaderGUI
{
    private Color colour;

    private Material _material;

    private int _structSize;
    private ComputeBuffer _buffer;

    private bool _firstStart = true;

    private Color FromVector3(Vector3 vector)
    {
        return new Color(vector.x, vector.y, vector.z);
    } 

    private Vector3 ToVector3(Color colour)
    {
        return new Vector3(colour.r, colour.g, colour.b);
    }

    private void OnEnable(MaterialProperty[] properties)
    {
        MaterialProperty testProp = FindProperty("_testProp", properties);
        _material = (Material)testProp.targets[0];
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (_firstStart) {
            OnEnable(properties);
            _firstStart = false;
        }

        EditorGUI.BeginChangeCheck();
        colour = EditorGUILayout.ColorField("colour", colour);
        if (EditorGUI.EndChangeCheck()) {
            _material.SetColor("_AlbedoColour", colour);
        }

        //_materialData.SM.x = EditorGUILayout.Slider("smoothness", _materialData.SM.x, 0, 1);
        //_materialData.SM.y = EditorGUILayout.Slider("metallic", _materialData.SM.y, 0, 1);
        //_materialData.Data.Emission = ToVector3(EditorGUILayout.ColorField("emission", FromVector3(_materialData.Data.Emission)));
    }
}
#endif