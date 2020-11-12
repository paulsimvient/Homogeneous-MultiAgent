using UnityEngine;
using UnityEditor;

namespace GSM
{
    public class CustomButton
    {
        public void Draw(Rect rect, Color background, Color borderColor, int border, string text, GUIStyle textStyle, ClickCallback click)
        {
            Draw(rect, background, borderColor, border, (r) => {
                //Text hier
                var size = textStyle.CalcSize(new GUIContent(text));

                EditorGUI.LabelField(new Rect(rect.center - size * 0.5f, size), new GUIContent(text), textStyle);

            }, click);
        }

        public void Draw(Rect rect, Color background, Color borderColor, int border, DrawDelegate draw, ClickCallback click)
        {
            EditorGUI.DrawRect(rect, borderColor);
            EditorGUI.DrawRect(new Rect(rect.x + border, rect.y + border, rect.width - 2 * border, rect.height - 2 * border), background);

            draw?.Invoke(rect);
            if(Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (click != null)
                {
                    click.Invoke();
                    GUI.changed = true;
                    Event.current.Use();
                }
            }
        }

        public delegate void DrawDelegate(Rect rect);
        public delegate void ClickCallback();
    }
}
