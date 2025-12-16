using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Samples
{
#if UNITY_EDITOR
    using Watchers;
#endif
    // Used to check if the colour space matches the one in the sample scene
    // Updates it if they are mismatched

    [ExecuteInEditMode]
    public class ColourSpaceChecker : MonoBehaviour
    {
        [SerializeField] private Material _repetitionlessMaterial;

        void OnEnable()
        {
#if UNITY_EDITOR
            // Update the material colour space if it is different
            RepetitionlessColourSpaceUpdater.RepackMaterialsIfColourSpaceChanged(new List<Material> { _repetitionlessMaterial }, false);
#endif
        }
    }
}