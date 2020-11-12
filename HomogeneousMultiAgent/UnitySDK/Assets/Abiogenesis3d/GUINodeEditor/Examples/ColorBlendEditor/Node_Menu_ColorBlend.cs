using UnityEngine;
using GUINodeEditor;

public class Node_Menu_ColorBlend : Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Menu_ColorBlend ());
    }
}

public class NodeWindow_Menu_ColorBlend : NodeWindow_Menu {
    public override void OnGUI () {
        if (clickedWindow != null) {
            title = clickedWindow.node.GetType ().ToString ();
            GUILayout.Box ("Custom menu");
        }
        else {
            title = "Add:";
            GUILayout.Label ("types");
            if (GUILayout.Button ("Float")) nodeEditor.CreateNewWindow <Node_Float> ();
            if (GUILayout.Button ("Vector")) nodeEditor.CreateNewWindow <Node_Vector> ();
            if (GUILayout.Button ("Color")) nodeEditor.CreateNewWindow <Node_Color> ();
            GUILayout.Label ("operators");
            if (GUILayout.Button ("+ | - | * | ÷")) nodeEditor.CreateNewWindow <Node_BasicOperators> ();
            if (GUILayout.Button ("sin,cos,tg,ctg")) nodeEditor.CreateNewWindow <Node_Op_Trigonometry> ();
            if (GUILayout.Button ("Color Blend")) nodeEditor.CreateNewWindow <Node_ColorBlend> ();
            if (GUILayout.Button ("Abs")) nodeEditor.CreateNewWindow <Node_Op_Abs> ();
            GUILayout.Label ("values");
            if (GUILayout.Button ("Time")) nodeEditor.CreateNewWindow <Node_Time> ();

        }
    }
}
