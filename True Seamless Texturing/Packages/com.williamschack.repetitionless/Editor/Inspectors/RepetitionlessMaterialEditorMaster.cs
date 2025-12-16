#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    public class RepetitionlessMaterialEditorMaster : RepetitionlessMaterialEditorBase
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Base Material
            DrawBaseMaterialGUI(0);

            GUILayout.Space(SETTING_PADDING);

            // Distance Blend Material
            DrawDistanceBlendGUI(0);

            GUILayout.Space(SETTING_PADDING);

            // Material Blend
            DrawMaterialBlendGUI(0);

            GUILayout.Space(SETTING_PADDING);

            // Footer Settings
            DrawDebugGUI();
        }
    }
}
#endif