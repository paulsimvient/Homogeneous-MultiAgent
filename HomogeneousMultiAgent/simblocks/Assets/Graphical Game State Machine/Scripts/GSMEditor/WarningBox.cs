using UnityEditor;
using UnityEngine;

namespace GSM
{
    public class WarningBox
    {
        private readonly string message;
        private readonly GSMWindow window;

        public const int boxSize = 16;

        public WarningBox(string message, GSMWindow window)
        {
            this.message = message;
            this.window = window;
        }

        public void Draw(Vector2 position, Vector2 mousePosition)
        {
            GUIStyle style1 = new GUIStyle(window.defaultStyle)
            {
                fontStyle = FontStyle.Bold
            };
            GUIStyle style2 = new GUIStyle(window.defaultStyle);
            style2.normal.textColor = window.textColorWarning;

            GUIContent icon = new GUIContent(" ! ");
            var rect = new Rect(position, Vector2.one * boxSize);
            EditorGUI.DrawRect(rect, window.textColorWarning);
            EditorGUI.LabelField(rect, icon, style1);

            if(rect.Contains(mousePosition))
            {
                GUIContent text = new GUIContent(message);
                var textRect = new Rect(mousePosition + Vector2.up * boxSize, style2.CalcSize(text));
                EditorGUI.LabelField(textRect, text, style2);
                GUI.changed = true;
            }
        }

    }
}
