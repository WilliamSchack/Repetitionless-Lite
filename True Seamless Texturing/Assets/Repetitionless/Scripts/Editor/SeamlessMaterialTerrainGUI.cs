using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeamlessMaterial.Compression;


#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    public class SeamlessMaterialTerrainGUI : SeamlessMaterialGUI
    {
        public override void OnEnable(MaterialEditor materialEditor)
        {
            base.OnEnable(materialEditor);

            // Material Properties
            MaterialProperty settingTogglesProp = FindProperty($"_Layer1BaseSettings");

            // Get variables from settings prop
            int settingToggles = (int)settingTogglesProp.vectorValue.x;
            bool noiseEnabled = BooleanCompression.GetCompressedValue(settingToggles, 0);
            bool randomiseScaling = BooleanCompression.GetCompressedValue(settingToggles, 1);
            bool randomiseRotation = BooleanCompression.GetCompressedValue(settingToggles, 2);
            bool smoothnessEnabled = BooleanCompression.GetCompressedValue(settingToggles, 3);
            bool variationEnabled = BooleanCompression.GetCompressedValue(settingToggles, 4);
            bool packedTexture = BooleanCompression.GetCompressedValue(settingToggles, 5);
            bool emissionEnabled = BooleanCompression.GetCompressedValue(settingToggles, 6);

            Debug.Log("TERRAIN ENABLED:" + " || " + noiseEnabled + " || " + randomiseScaling + " || " + randomiseRotation + " || " + smoothnessEnabled + " || " + variationEnabled + " || " + packedTexture + " || " + emissionEnabled);

            int ahhh = (int)settingTogglesProp.vectorValue.y;
            Debug.Log("AHHH: " + ahhh);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            for (int i = 0; i < 4; i++) {
                DrawLayerGUI(i);
            }
        }

        private void DrawLayerGUI(int layerIndex)
        {
            
        }
    }
}
#endif