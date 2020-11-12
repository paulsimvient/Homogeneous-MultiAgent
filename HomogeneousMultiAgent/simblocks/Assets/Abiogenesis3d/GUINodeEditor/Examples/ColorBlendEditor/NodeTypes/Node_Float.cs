using UnityEngine;
using GUINodeEditor;

public class Node_Float : Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Float (), title: "Float");

        AddOutput (typeof(float));
    }
}

public class NodeWindow_Float : NodeWindow {
    NumberField numberField = new NumberField ();

    public override float GetWindowWidth () {
        return 75;
    }

    public override void OnGUI () {
        Node_Float n = (Node_Float)node;
        backgroundColor = Color.blue;

        GUILayout.BeginHorizontal ();
        {
            Dock dockOutput = n.outputs [0];
            dockOutput.value = numberField.Float ((float) dockOutput.value);
            DrawDock (dockOutput);
        }
        GUILayout.EndHorizontal ();
    }
}
