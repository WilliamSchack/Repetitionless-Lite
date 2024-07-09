using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    public class SeamlessMaterialTerrainGUI : SeamlessMaterialGUI
    {
        public override void OnEnable(MaterialEditor materialEditor)
        {
            _debugSettings = new string[] {
                "Voronoi Cells",
                "Edge Mask",
                "Distance Mask",
                "Variation Multiplier"
            };

            base.OnEnable(materialEditor);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            DrawLayerGUI(1);
            
            GUILayout.Space(SETTING_SPACING);

            // Footer Settings
            DrawDebugGUI();
        }

        private void DrawLayerGUI(int layer)
        {
            string layerPrefix = $"Layer{layer}";
        }
    }
}
#endif