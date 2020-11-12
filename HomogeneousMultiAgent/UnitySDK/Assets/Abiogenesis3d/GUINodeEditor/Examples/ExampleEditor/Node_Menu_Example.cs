using UnityEngine;
using GUINodeEditor;

public class Node_Menu_Example : Node {
    public override void Init (Vector2 position) {
        // init with custom nodeWindow, set node reference
        Init (position, nodeWindow: new NodeWindow_Menu_Example ());
    }
}
public class NodeWindow_Menu_Example: NodeWindow_Menu {
    public override void OnGUI () {
        // clicked on a window title
        if (clickedWindow != null) {
            title = clickedWindow.node.GetType ().ToString ();
            GUILayout.Box ("Specific node menu");
        }
        // clicked on background
        else {
            title = "Add:";
            // creates a new node window at menu location (closes the menu)
            if (GUILayout.Button ("Node Example")) nodeEditor.CreateNewWindow <Node_Example> ();
            if (GUILayout.Button ("Node Logo 128x128")) nodeEditor.CreateNewWindow <Node_Logo_128x128> ();

        }
    }
}