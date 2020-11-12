using UnityEngine;
using GUINodeEditor;

public class Node_Example: Node {
    public override void Init (Vector2 position) {
        // if you do not plan to change the title from OnGUI you can set it here
        Init (position, nodeWindow: new NodeWindow_Example (), title: "Example");

        // these will create input and output dock
        AddInput (typeof(string), "input_name");
        AddOutput (typeof(string), "output_name");
    }
}

public class NodeWindow_Example: NodeWindow {
    public override void OnGUI () {
        // cast node to the right type
        Node_Example n = (Node_Example)node;
        // change background color
        backgroundColor = Color.blue;

        // get dock references
        DockInput dockInput = n.GetDockInputByName ("input_name");
        DockOutput dockOutput = n.GetDockOutputByName ("output_name");

        // draw first row with dock input and its targets value
        GUILayout.BeginHorizontal ();
        DrawDock (dockInput);
        GUILayout.Label (n.GetFirstTargetValue<string> (dockInput));
        GUILayout.EndHorizontal ();

        // draw a second row with the output dock and text field that modify the output value
        GUILayout.BeginHorizontal ();
        dockOutput.value = GUILayout.TextField ((string) dockOutput.value);
        DrawDock (dockOutput);
        GUILayout.EndHorizontal ();
    }
}