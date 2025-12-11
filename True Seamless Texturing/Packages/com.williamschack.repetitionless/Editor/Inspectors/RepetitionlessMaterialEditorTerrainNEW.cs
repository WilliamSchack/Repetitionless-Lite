#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Inspectors
{
    public class RepetitionlessMaterialEditorTerrainNEW : RepetitionlessMaterialEditorBaseNEW
    {
        protected override int _maxLayers => 32;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            // TESTING
            for (int i = 0; i < 2; i++) {
                // Base Material
                DrawBaseMaterialGUI(i);

                GUILayout.Space(SETTING_PADDING);

                // Distance Blend Material
                DrawDistanceBlendGUI(i);

                GUILayout.Space(SETTING_PADDING);

                // Material Blend
                DrawMaterialBlendGUI(i);

                GUILayout.Space(SETTING_PADDING);
            }

            // Footer Settings
            DrawDebugGUI();
        }
    }
}
#endif