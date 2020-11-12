using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;
using System.Linq;
using Enum = System.Enum;

public class Node_Op_Abs: Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Op_Abs (), title: "Operator Abs");

        AddInput (typeof(float), "input");
        AddOutput (typeof(float), "result");
    }

    public override void Update () {
        DockInput input = GetDockInputByName ("input");
        DockOutput result = GetDockOutputByName ("result");
        
        float inputValue = GetFirstTargetValue <float> (input, 0f);
        result.value = Mathf.Abs (inputValue);
    }
}

public class NodeWindow_Op_Abs: NodeWindow {
    public override void OnGUI () {
        Node_Op_Abs n = (Node_Op_Abs)node;
        backgroundColor = Color.cyan;

        DockInput input = n.GetDockInputByName ("input");
        DockOutput result = n.GetDockOutputByName ("result");

        GUILayout.BeginHorizontal ();
        DrawDock (input);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("= ");
        GUILayout.Box (((float)result.value).ToString ("0.00"));
        DrawDock (result);
        GUILayout.EndHorizontal ();
    }
}
