#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Repetitionless.Samples
{
    public class TerrainSceneSettings : MonoBehaviour
    {
        [SerializeField] private Terrain[] _terrains;

        [SerializeField] private Material _repetitionlessMaterial;
        [SerializeField] private Material _litMaterial;

        internal bool TerrainUsingRepetitionless = true;

        internal void ToggleTerrainRepetitionless()
        {
            Material currentMaterial = TerrainUsingRepetitionless ? _litMaterial : _repetitionlessMaterial;
            for (uint i = 0; i < _terrains.Length; i++) {
                _terrains[i].materialTemplate = currentMaterial;
            }

            TerrainUsingRepetitionless = !TerrainUsingRepetitionless;

            // Repaint scene and game view
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        internal void SelectRepetitionlessMaterial()
        {
            Selection.activeObject = _repetitionlessMaterial;
        }
    }
}
#endif