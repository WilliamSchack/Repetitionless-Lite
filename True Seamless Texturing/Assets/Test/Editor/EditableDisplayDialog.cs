#if UNITY_EDITOR
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SeamlessMaterial.Editor
{
    public class EditableDisplayDialog : EditorWindow
    {
        private const int WINDOW_WIDTH = 380;
        private const int WINDOW_HEIGHT = 130;

        private const int OUTER_PADDING = 10;

        private Color _defaultBackgroundColour;
        private Color _focusedBackgroundColour = new Color(0.8f, 0.8f, 1.0f);

        private GUIStyle _labelStyle;

        private static Texture2D _texture = null;
        private static string _header;
        private static string _message;
        private static string[] _buttonTexts = new string[3];

        private static int _returnValue = -1;

        private int _currentFocus = 0;

        /// <summary>
        /// Returns 0, 1, 2 for ok, alt, cancel responses respectively
        /// </summary>
        public static async Task<int> Show(Texture2D texture, string title, string header, string message, string ok, string alt, string cancel)
        {
            _texture = texture;
            _header = header;
            _message = message;
            _buttonTexts = new string[3] {
                ok, alt, cancel
            };

            _returnValue = -1;

            EditableDisplayDialog window = GetWindow<EditableDisplayDialog>(true, title);
            window.Show(); // Would use ShowModalUtility but gives continuous warnings about PropertiesGUI() ?, manually handle focus instead

            // Wait for user to click a button
            while (_returnValue == -1) {
                await Task.Delay(100);
            }

            return _returnValue;
        }

        private void OnLostFocus()
        {
            Focus();
        }

        private void CreateGUI()
        {
            _labelStyle = EditorStyles.label;
            _labelStyle.wordWrap = true;
        
            _defaultBackgroundColour = GUI.backgroundColor;
        
            minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            maxSize = minSize;
        }
        
        private void OnGUI()
        {
            // Enable Keyboard Tabbing
            if (Event.current.type == EventType.KeyDown) {
                KeyCode key = Event.current.keyCode;
                if(key == KeyCode.Tab) {
                    _currentFocus = (_currentFocus + 1) % 3;
                    GUI.FocusControl(_currentFocus.ToString());
                }
            }
        
            GUILayout.Space(OUTER_PADDING);
            GUILayout.BeginHorizontal();
            GUILayout.Space(OUTER_PADDING);
        
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
        
            // Draw Image
            Rect textureRect = GUILayoutUtility.GetRect(105, 105);
            if(_texture != null) EditorGUI.DrawPreviewTexture(textureRect, _texture);
            else EditorGUI.DrawRect(textureRect, Color.red); // Incase texture no assigned
        
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        
            GUILayout.Space(5);
            GUILayout.BeginVertical();
        
            // Draw header and message
            if(_header != "") GUILayout.Label(_header, EditorStyles.boldLabel);
            GUILayout.Label(_message, _labelStyle);
            GUILayout.FlexibleSpace();
        
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        
            // Draw Buttons
            for (int i = 0; i < 3; i++) {
                GUI.SetNextControlName(i.ToString());
        
                GUI.backgroundColor = _currentFocus == i ? _focusedBackgroundColour : _defaultBackgroundColour;
                if(GUILayout.Button(new GUIContent(_buttonTexts[i]))) {
                    _returnValue = i;
                    Close();
                }
                GUI.backgroundColor = _defaultBackgroundColour;
            }
        
        
            GUILayout.Space(OUTER_PADDING);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        
            GUILayout.Space(OUTER_PADDING);
        
            GUI.FocusControl(_currentFocus.ToString());
        }
    }
}
#endif