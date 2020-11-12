using System;
using UnityEngine;
using EditorDesignerUI;
using EditorDesignerUI.Controls;

namespace DynamicCSharp.Editor
{
    public sealed class EditListBox : ListBox
    {
        // Events
        public Action<object> OnAddClicked;

        public Action<object> OnRemoveClicked;

        // Methods
        public override void OnRender()
        {
            base.OnRender();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent("+", "Add a new item to the list"), GUILayout.Width(35)) == true)
                {
                    // Trigger add event
                    if (OnAddClicked != null)
                        OnAddClicked(this);                }

                if(GUILayout.Button(new GUIContent("-", "Remove the selected item in the list"), GUILayout.Width(35)) == true)
                {
                    // Trigger remove event
                    if (OnRemoveClicked != null)
                        OnRemoveClicked(this);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
