using UnityEngine;
using System.Collections.Generic;
using GUINodeEditor;

public class SwitchNodeEditors : MonoBehaviour {
    int currentEditorInt = 0;
    List<string> editorNames = new List<string> ();

    void Awake () {
        foreach (Transform child in transform)
            if (child.GetComponent<RuntimeNodeEditor>() != null)
                editorNames.Add (child.name);
        EnableOnlySelected ();
    }

    void OnGUI () {
        GUILayout.BeginArea (new Rect (Screen.width - 140, 0, 130, 200));
        GUILayout.Box ("Switch editors");

        int tmp = GUILayout.SelectionGrid (currentEditorInt, editorNames.ToArray(), 1);

        if (GUILayout.Button ("None"))
            tmp = -1;

        if (tmp != currentEditorInt) {
            currentEditorInt = tmp;
            EnableOnlySelected ();
        }
        GUILayout.EndArea ();
    }

    void EnableOnlySelected () {
        for (int i = 0; i < editorNames.Count; ++i)
            GameObject.Find (editorNames [i]).GetComponent<RuntimeNodeEditor> ()
                .enabled = currentEditorInt == i;
    }
}
