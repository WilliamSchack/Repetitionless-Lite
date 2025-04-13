using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Repetitionless.Editor
{
    public class SeamlessMaterialMasterGUI : SeamlessMaterialGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Base Material
            DrawBaseMaterialGUI();

            GUILayout.Space(SETTING_SPACING);

            // Distance Blend Material
            DrawDistanceBlendGUI();

            GUILayout.Space(SETTING_SPACING);

            // Material Blend
            DrawMaterialBlendGUI();

            GUILayout.Space(SETTING_SPACING);

            // Footer Settings
            DrawDebugGUI();
        }
    }
}
#endif