using UnityEngine;
using GUINodeEditor;

public class Node_Menu_Movement : Node {
    public override void Init(Vector2 position) {
        Init (position, nodeWindow: new NodeWindow_Menu_Movement());
    }
}

public class NodeWindow_Menu_Movement : NodeWindow_Menu {
    public override void OnGUI () {
        if (clickedWindow != null) {
            title = clickedWindow.node.GetType ().ToString ();
            GUILayout.Box ("Custom menu");
        }
        else {
            title = "Add:";
            if (GUILayout.Button ("Movement")) nodeEditor.CreateNewWindow <Node_Movement> ();
            if (GUILayout.Button ("Trigger")) nodeEditor.CreateNewWindow <Node_Trigger> ();
        }
    }
}
