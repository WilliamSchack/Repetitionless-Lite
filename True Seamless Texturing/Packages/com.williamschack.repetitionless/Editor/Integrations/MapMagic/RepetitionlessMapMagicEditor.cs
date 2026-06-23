#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Repetitionless.Runtime.Integrations.MapMagic;

namespace Repetitionless.Editor.Integrations.MapMagic
{
    [CustomEditor(typeof(RepetitionlessMapMagic))]
    public class RepetitionlessMapMagicEditor : UnityEditor.Editor
    {
        private RepetitionlessMapMagic _main;

        private void OnEnable()
        {
            _main = (RepetitionlessMapMagic)serializedObject.targetObject;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // There is no callback for mapmagic terrain setting changes
            // Check on inspector instead (assuming the MapMagicObject is on this object)
            _main.CheckAndUpdateMaterials();
        }
    }
}
#endif