using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    using Compression;
    using Variables;

    public class SeamlessMaterialGUI : ShaderGUI
    {
        #region Variables
        // Custom Class
        internal class MaterialFoldoutState
        {
            public bool MainProperties = true;
            public bool NoiseProperties = true;
            public bool VariationProperties = true;
        }

        // Constants
        internal const int HEADER_PADDING = 4;
        internal const int SETTING_SPACING = 4;

        internal const int BACKGROUND_HEIGHT_DISABLED_SETTING = 29;
        internal const int BACKGROUND_HEIGHT_PROPERTIES = 116;

        internal const int BACKGROUND_HEIGHT_HEADERSETTINGS = 47;

        internal const int BACKGROUND_HEIGHT_COLLAPSED_FOLDOUT = 24;
        internal const int BACKGROUND_HEIGHT_MAIN_FOLDOUT_EMISSION_DISABLED = 178;
        internal const int BACKGROUND_HEIGHT_MAIN_FOLDOUT_EMISSION_ENABLED = 200;
        internal const int BACKGROUND_HEIGHT_NOISE_FOLDOUT = 112;
        internal const int BACKGROUND_HEIGHT_VARIATION_FOLDOUT = 222;

        internal const int SCALED_TEXT_PADDING = 10;

        // Material Helpers
        internal Material _material;
        internal MaterialEditor _editor;
        internal Dictionary<string, MaterialProperty> _cachedProperties = new Dictionary<string, MaterialProperty>();

        // Background Heights
        // Rough solution as it only works properly on second call of OnGUI but its better then estimating and fiddling around with the height
        // Drawing a box after calculating height of area would draw ontop of other fields, this will draw behind
        internal float _propertiesBackgroundHeight;

        // Foldout States, dynamically adds new materialPrefixes
        internal Dictionary<string, MaterialFoldoutState> _foldoutStates = new Dictionary<string, MaterialFoldoutState>();

        // ShaderGUI doesnt have an OnEnable function, using this instead
        private bool _firstSetup = true;
        #endregion

        #region Helpers
        internal MaterialProperty FindProperty(string name)
        {
            return _cachedProperties[name];
        }

        internal string GetScaledText(int minWidth, string largeText, string smallText)
        {
            // Using screen width so it is accurate in both layout and repaint events
            float areaWidth = Screen.width - GUIUtilities.BACKGROUND_SIDE_PADDING * 6 - GUIUtilities.SIDE_EMPTY_SPACE_WIDTH; // Multiplying side padding by 2 would make sense but only 6 is accurate for some reason idk
            return areaWidth <= minWidth + SCALED_TEXT_PADDING ? smallText : largeText;
        }

        private Dictionary<string, MaterialProperty> GetMaterialProperties(MaterialProperty[] properties)
        {
            Dictionary<string, MaterialProperty> cachedProperties = new Dictionary<string, MaterialProperty>();
            foreach (MaterialProperty property in properties) {
                cachedProperties.Add(property.name, property);
            }

            return cachedProperties;
        }
        #endregion

        #region GUI Calls
        public virtual void OnEnable(MaterialEditor materialEditor)
        {
            // Assign Material Helpers
            _material = (Material)materialEditor.target;
            _editor = materialEditor;

            _propertiesBackgroundHeight = BACKGROUND_HEIGHT_PROPERTIES;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Cache properties into dict, can change each call, faster than FindProperty (loops through all properties each call)
            _cachedProperties = GetMaterialProperties(properties);
            
            // OnEnable if first call
            if (_firstSetup) {
                OnEnable(materialEditor);
                _firstSetup = false;
            }
            
            // Make Vectors One Line
            EditorGUIUtility.wideMode = true;
            
            GUILayout.Space(HEADER_PADDING);

            // Material Properties
            DrawMaterialPropertiesGUI();
            
            GUILayout.Space(SETTING_SPACING);
        }
        #endregion

        #region Material GUI
        private void DrawMaterialPropertiesGUI()
        {
            // Start Background
            float backgroundStartingYPos = GUIUtilities.StartBackground(_propertiesBackgroundHeight);

            // Header
            GUIUtilities.DrawHeaderLabelLarge($"Material Properties");

            GUILayout.Space(4);

            // Surface Type
            MaterialProperty surfaceTypeProp = FindProperty("_SurfaceType");

            EditorGUI.BeginChangeCheck();
            SurfaceType surfaceType = (SurfaceType)surfaceTypeProp.floatValue;
            surfaceType = (SurfaceType)EditorGUI.EnumPopup(GUIUtilities.GetLineRect(), "Surface Type", surfaceType);

            if (EditorGUI.EndChangeCheck()) {
                surfaceTypeProp.floatValue = (int)surfaceType;

                switch (surfaceType) {
                    case SurfaceType.Opaque:
                        _material.renderQueue = (int)RenderQueue.Geometry;
                        _material.SetOverrideTag("RenderType", "Opaque");
                        _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 0.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        _material.SetInt("_BUILTIN_ZWrite", 1);
                        break;
                    case SurfaceType.Cutout:
                        _material.renderQueue = (int)RenderQueue.AlphaTest;
                        _material.SetOverrideTag("RenderType", "TransparentCutout");
                        _material.DisableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 0.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.One);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.Zero);
                        _material.SetInt("_BUILTIN_ZWrite", 1);
                        break;
                    case SurfaceType.Transparent:
                        _material.renderQueue = (int)RenderQueue.Transparent;
                        _material.SetOverrideTag("RenderType", "Transparent");
                        _material.EnableKeyword("_BUILTIN_SURFACE_TYPE_TRANSPARENT");

                        _material.SetFloat("_BUILTIN_Surface", 1.0f);
                        _material.SetInt("_BUILTIN_SrcBlend", (int)BlendMode.SrcAlpha);
                        _material.SetInt("_BUILTIN_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        _material.SetInt("_BUILTIN_ZWrite", 0);
                        break;
                }
            }

            // Advanced Options
            _editor.LightmapEmissionProperty();
            _editor.RenderQueueField();
            _editor.DoubleSidedGIField();

            // End Background
            float heightDiff = GUIUtilities.EndBackground(backgroundStartingYPos);
            if (heightDiff > 0)
                _propertiesBackgroundHeight = heightDiff;
        }
        #endregion
    }
}
#endif