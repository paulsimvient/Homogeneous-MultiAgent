using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;

public class Node_Movement : Node {
    public PopAnywhereStack forwardBackward = new PopAnywhereStack ();
    public PopAnywhereStack leftRight = new PopAnywhereStack ();
    public PopAnywhereStack upDown = new PopAnywhereStack ();

    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Movement (), title: "Movement");

        AddInput (typeof(bool), "forward", 1f);
        AddInput (typeof(bool), "backward", -1f);
        AddInput (typeof(bool), "left", -1f);
        AddInput (typeof(bool), "right", 1f);
        AddInput (typeof(bool), "up", 1f);
        AddInput (typeof(bool), "down", -1f);

        AddOutput (typeof(Vector3), "result"); // x,y,z = +-1 || 0
    }

    public override void Update() {
        UpdateFromTriggers ();
        CraftRawVector ();
    }

    public void UpdateFromTriggers () {
        // forward, backward
        foreach (DockOutput d in inputs[0].targets) forwardBackward.HandleInsertRemove (d, d.node.isTriggered, inputs[0].value);
        foreach (DockOutput d in inputs[1].targets) forwardBackward.HandleInsertRemove (d, d.node.isTriggered, inputs[1].value);
        // left, right
        foreach (DockOutput d in inputs[2].targets) leftRight.HandleInsertRemove (d, d.node.isTriggered, inputs[2].value);
        foreach (DockOutput d in inputs[3].targets) leftRight.HandleInsertRemove (d, d.node.isTriggered, inputs[3].value);
        // up, down
        foreach (DockOutput d in inputs[4].targets) upDown.HandleInsertRemove (d, d.node.isTriggered, inputs[4].value);
        foreach (DockOutput d in inputs[5].targets) upDown.HandleInsertRemove (d, d.node.isTriggered, inputs[5].value);
    }

    public void CraftRawVector() {
        float x = (float)leftRight.Head (toReturnIfNull: 0f);
        float y = (float)upDown.Head (toReturnIfNull: 0f);
        float z = (float)forwardBackward.Head (toReturnIfNull: 0f);
        GetDockOutputByName ("result").value = new Vector3 (x, y, z);
    }

}
public class NodeWindow_Movement: NodeWindow {
    public override void OnGUI () {
        Node_Movement n = (Node_Movement)node;

        backgroundColor = Color.blue;

        foreach (DockInput dockInput in n.inputs) {
            GUILayout.BeginHorizontal ();
            DrawDock (dockInput);
            GUILayout.Label (dockInput.name);
            GUILayout.EndHorizontal ();
        }
        GUILayout.BeginHorizontal ();
        DockOutput dockOutput = n.GetDockOutputByName ("result");
        GUILayout.Box (dockOutput.value.ToString());
        DrawDock (dockOutput);
        GUILayout.EndHorizontal ();
    }
}
