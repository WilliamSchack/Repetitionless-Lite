using Repetitionless.Runtime.Variables;
using UnityEditor;
using UnityEngine;

namespace Repetitionless.Editor.Data
{
    public class RepetitionlessLayeredDataSO : ScriptableObject
    {
        [SerializeField] public EControlMode ControlMode = EControlMode.ControlTextures;

        [SerializeField] public Texture2D[] ControlTextures = new Texture2D[Constants.MAX_LAYERS_TERRAIN];

        /// <summary>
        /// Saves this object
        /// </summary>
        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}