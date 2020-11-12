using UnityEngine;
using GUINodeEditor;

public class Node_Menu_EnumState : Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Menu_EnumState());
    }
}

public class NodeWindow_Menu_EnumState : NodeWindow_Menu {
    public override void OnGUI () {
        if (clickedWindow != null) {
            title = clickedWindow.node.GetType ().ToString ();
            GUILayout.Box ("Custom menu");
        }
        else {
            title = "Add:";
            if (GUILayout.Button ("State")) nodeEditor.CreateNewWindow <Node_EnumState> ();
            if (GUILayout.Button ("Modifier")) nodeEditor.CreateNewWindow <Node_EnumStateModifier> ();
            if (GUILayout.Button ("Trigger")) nodeEditor.CreateNewWindow <Node_Trigger> ();
        }
    }
}
