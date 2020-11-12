using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Delegate = System.Delegate;

/// Save/Load GUI handler, set the fields externally.
public class SaveLoadGUI {
    /// The load dialogue, when set to true it will show dropdown of fileNames.
    bool _isLoadDialogue = true;

    /// <summary>
    /// Displayed in the dropdown when _isLoadDialogue is set to true.
    /// When fileName is clicked, OnLoad is invoked with that fileName.
    /// </summary>
    public List<string> fileNames = new List<string> ();

    public delegate void OnSave (string fileName);
    /// callback when Save button is pressed
    public OnSave onSave;

    public delegate void OnLoad (string fileName);
    /// callback when Load button is pressed
    public OnSave onLoad;

    /// scroll position of the area, if items exceed Screen.height
    public Vector2 scrollPosition = Vector2.zero;

    /// String that the user can change
    public string saveLoadName = "";

    /// Set this to the name that is currently loaded to highlight it
    public string currentName = "";

    bool didInit;

    public void OnGUI () {
        if (! didInit) {
            saveLoadName = currentName;
            didInit = true;
        }

        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();

        // change saveLoadName with text area
        GUIStyle s = new GUIStyle (GUI.skin.textField);
        s.alignment = TextAnchor.UpperCenter;
        // name the control
        GUI.SetNextControlName ("save_load_input_control");
        saveLoadName = GUILayout.TextField (saveLoadName, s, GUILayout.Width(120));

        if (GUILayout.Button ("Save", GUILayout.Width (50)) && saveLoadName != "")
        if (onSave != null)
            onSave (saveLoadName);
        GUILayout.EndHorizontal ();

        // load
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
        if (GUILayout.Button ("Load " +(_isLoadDialogue ? "↑" : "↓"), GUILayout.Width(50)))
            _isLoadDialogue = !_isLoadDialogue;
        GUILayout.EndHorizontal ();


        Vector2 tmpScrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (_isLoadDialogue) {
            scrollPosition = tmpScrollPosition;

            foreach (string name in fileNames) {
                Color origColor = GUI.color;

                if (name == currentName)
                    GUI.color = Color.cyan;

                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button(name, GUI.skin.box))
                    onLoad (name);

                GUI.color = origColor;
                GUILayout.EndHorizontal ();
            }
        }
        GUILayout.EndScrollView();
    }
}
