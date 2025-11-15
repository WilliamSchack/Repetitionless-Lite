#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    public class SeamlessMaterialMasterGUI : SeamlessMaterialGUIBase
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // Base Material
            DrawBaseMaterialGUI();

            GUILayout.Space(SETTING_PADDING);

            // Distance Blend Material
            DrawDistanceBlendGUI();

            GUILayout.Space(SETTING_PADDING);

            // Material Blend
            DrawMaterialBlendGUI();

            GUILayout.Space(SETTING_PADDING);

            // Footer Settings
            DrawDebugGUI();
        }
    }
}
#endif