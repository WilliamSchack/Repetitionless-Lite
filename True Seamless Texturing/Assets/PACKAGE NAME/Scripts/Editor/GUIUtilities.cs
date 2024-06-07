using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SeamlessMaterial.Editor
{
    public static class GUIUtilities
    {
        // Constants
        public const int SIDE_EMPTY_SPACE_WIDTH = 35; // 33: Left Space + 2: Right Space

        public const int LINE_HEIGHT = 18;
        public const int LINE_SPACING = 2;

        public const int FOLDOUT_INDENT = 10;

        public const int BACKGROUND_SIDE_PADDING = 8;
        public const int BACKGROUND_TOP_PADDING = 4;
        public const int BACKGROUND_BOTTOM_PADDING = 4;
        public const int BACKGROUND_CORNER_RADIUS = 10;

        // Styles
        private static GUIStyle _boldHeaderLargeStyle = null;
        private static GUIStyle _majorToggleButtonStyle = null;
        private static GUIStyle _boldFoldoutStyle = null;

        public static GUIStyle BoldHeaderLargeStyle { get {
                if (_boldHeaderLargeStyle != null)
                    return _boldHeaderLargeStyle;

                _boldHeaderLargeStyle = new GUIStyle("label");
                _boldHeaderLargeStyle.fontStyle = FontStyle.Bold;
                _boldHeaderLargeStyle.fontSize = 16;
                _boldHeaderLargeStyle.alignment = TextAnchor.MiddleCenter;
                return _boldHeaderLargeStyle;
        } }

        public static GUIStyle MajorToggleButtonStyle { get {
                if (_majorToggleButtonStyle != null)
                    return _majorToggleButtonStyle;

                _majorToggleButtonStyle = new GUIStyle("button");
                _majorToggleButtonStyle.fontStyle = FontStyle.Bold;
                _majorToggleButtonStyle.fontSize = 18;
                _majorToggleButtonStyle.alignment = TextAnchor.MiddleCenter;
                return _majorToggleButtonStyle;
        } }

        public static GUIStyle BoldFoldoutStyle { get {
                if (_boldFoldoutStyle != null)
                    return _boldFoldoutStyle;

                _boldFoldoutStyle = EditorStyles.foldout;
                _boldFoldoutStyle.fontStyle = FontStyle.Bold;
                _boldFoldoutStyle.fontSize = 12;
                _boldFoldoutStyle.alignment = TextAnchor.MiddleLeft;
                return _boldFoldoutStyle;
        } }

        public static Rect GetLineRect(float? heightOverride = null)
        {
            if (heightOverride != null)
                return EditorGUILayout.GetControlRect(true, heightOverride.Value, EditorStyles.layerMaskField);
            else
                return EditorGUILayout.GetControlRect(true, LINE_HEIGHT + LINE_SPACING, EditorStyles.layerMaskField);
        }

        public static void DrawHeaderLabelLarge(string text)
        {
            GUIContent textGUIContent = new GUIContent(text);
            float height = BoldHeaderLargeStyle.CalcHeight(textGUIContent, Screen.width);
            Rect rect = GetLineRect(height);
            GUI.Label(rect, textGUIContent, BoldHeaderLargeStyle);
        }

        public static bool DrawMajorToggleButton(MaterialProperty property, string label)
        {
            EditorGUI.BeginChangeCheck();
            bool enabled = property.floatValue == 1 ? true : false;

            Color prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = enabled ? Color.green : Color.red;

            enabled = GUILayout.Toggle(enabled, label, MajorToggleButtonStyle);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = enabled ? 1.0f : 0.0f;

            GUI.backgroundColor = prevBackgroundColor;

            return enabled;
        }

        public static bool DrawMajorToggleButton(bool enabled, string label)
        {
            Color prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = enabled ? Color.green : Color.red;
            enabled = GUILayout.Toggle(enabled, label, MajorToggleButtonStyle);
            GUI.backgroundColor = prevBackgroundColor;

            return enabled;
        }

        public static float DrawTextureWithSlider(MaterialEditor editor, MaterialProperty textureProperty, bool sliderCondition, float sliderValue, GUIContent content)
        {
            Rect rect = GetLineRect();
            editor.TexturePropertyMiniThumbnail(rect, textureProperty, content.text, content.tooltip);
            if (sliderCondition) {
                sliderValue = EditorGUI.Slider(MaterialEditor.GetRectAfterLabelWidth(rect), sliderValue, 0, 1);
            }

            return sliderValue;
        }

        // More performant and fills whole area instead of leaving gap at the end. win win
        public static Vector2 DrawVector2Field(Vector2 value, GUIContent content)
        {
            float vector2FieldPadding = 4f;
            float floatFieldLabelPadding = 2f;

            // Get Line Rect
            Rect lineRect = GetLineRect();
            Rect fieldsRect = MaterialEditor.GetRectAfterLabelWidth(lineRect);
            float halfWidth = fieldsRect.width / 2;

            // Get Left Float Rect
            Rect leftRect = fieldsRect;
            leftRect.width = halfWidth - vector2FieldPadding;

            // Get Right Float Rect
            Rect rightRect = fieldsRect;
            rightRect.width = halfWidth - vector2FieldPadding;
            rightRect.x += halfWidth + vector2FieldPadding;

            GUI.Label(lineRect, content);

            // Set Label Width (Shortens regular label padding)
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent("X")).x + floatFieldLabelPadding;

            // Draw Float Fields
            float xVal = EditorGUI.FloatField(leftRect, new GUIContent("X"), value.x);
            float yVal = EditorGUI.FloatField(rightRect, new GUIContent("Y"), value.y);

            // Reset Label Width
            EditorGUIUtility.labelWidth = labelWidth;

            return new Vector2(xVal, yVal);
        }

        public static void DrawTilingOffset(MaterialProperty tilingOffsetProperty, string tilingLabel = "Scale", string offsetLabel = "Offset")
        {
            Vector4 tilingOffset = tilingOffsetProperty.vectorValue;
            bool tilingOffsetChanged = false;

            EditorGUI.BeginChangeCheck();
            Vector2 scale = new Vector2(tilingOffset.x, tilingOffset.y);
            scale = DrawVector2Field(scale, new GUIContent(tilingLabel));
            if (EditorGUI.EndChangeCheck()) {
                tilingOffset = new Vector4(scale.x, scale.y, tilingOffset.z, tilingOffset.w);
                tilingOffsetChanged = true;
            }

            EditorGUI.BeginChangeCheck();
            Vector2 offset = new Vector2(tilingOffset.z, tilingOffset.w);
            offset = DrawVector2Field(offset, new GUIContent(offsetLabel));
            if (EditorGUI.EndChangeCheck()) {
                tilingOffset = new Vector4(tilingOffset.x, tilingOffset.y, offset.x, offset.y);
                tilingOffsetChanged = true;
            }

            if (tilingOffsetChanged)
                tilingOffsetProperty.vectorValue = tilingOffset;
        }

        public static bool DrawFoldout(bool foldout, string label)
        {
            Rect rect = GetLineRect();
            rect.x += FOLDOUT_INDENT;
            rect.width -= FOLDOUT_INDENT;

            return EditorGUI.Foldout(rect, foldout, label, true, BoldFoldoutStyle);
        }

        public static float StartBackground(float backgroundHeight)
        {
            GUILayout.Space(BACKGROUND_TOP_PADDING);

            // Background, get height from previous OnGUI call
            Rect borderRect = GetLineRect(0);
            float startingYPosition = borderRect.position.y;
            borderRect.position += new Vector2(0, -BACKGROUND_TOP_PADDING);
            borderRect.height = backgroundHeight + BACKGROUND_TOP_PADDING + BACKGROUND_BOTTOM_PADDING;
            GUI.DrawTexture(borderRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0, 0, 0, 0.2f), 0, BACKGROUND_CORNER_RADIUS);

            // Start Padding
            GUILayout.BeginHorizontal();
            GUILayout.Space(BACKGROUND_SIDE_PADDING);
            GUILayout.BeginVertical();

            return startingYPosition;
        }

        public static float EndBackground(float startingYPos)
        {
            // End Padding
            GUILayout.EndVertical();
            GUILayout.Space(BACKGROUND_SIDE_PADDING);
            GUILayout.EndHorizontal();

            // Set background height for next OnGUI call
            Rect endRect = GetLineRect(0);
            float heightDifference = endRect.position.y - startingYPos;

            GUILayout.Space(BACKGROUND_BOTTOM_PADDING);

            return heightDifference;
        }
    }
}
#endif