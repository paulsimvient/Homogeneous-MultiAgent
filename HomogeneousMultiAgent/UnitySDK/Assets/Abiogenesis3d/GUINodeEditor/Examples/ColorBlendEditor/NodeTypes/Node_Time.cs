using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;
using Convert = System.Convert;
using Action = System.Action;

public class Node_Time : Node {
    public override void Init(Vector2 position) {
        base.Init (position, new NodeWindow_Time (), title: "Time");

        AddOutput (typeof(float), "time");
    }

    public override void Update () {
        outputs[0].value = Time.time;
    }
}

public class NodeWindow_Time : NodeWindow {

    public override void OnGUI () {
        Node_Time n = (Node_Time)node;
        backgroundColor = Color.grey;

        GUILayout.BeginHorizontal ();
        Dock dockOutput = n.GetDockOutputByName ("time");
        GUILayout.Box (((float)dockOutput.value).ToString ("0.00"));
        DrawDock (dockOutput);
        GUILayout.EndHorizontal ();
    }
}
