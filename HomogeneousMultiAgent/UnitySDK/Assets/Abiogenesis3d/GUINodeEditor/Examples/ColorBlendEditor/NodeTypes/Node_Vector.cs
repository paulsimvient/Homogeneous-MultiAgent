using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;

public class Node_Vector : Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Vector (), title: "Vector3");

        AddInput (typeof(float), "x");
        AddInput (typeof(float), "y");
        AddInput (typeof(float), "z");

        AddOutput (typeof(Vector3));
    }

    public override void Update() {
        DockOutput dockOutput = outputs [0];
        int i = 0;
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);

        dockOutput.value = new Vector3 (
            (float) inputs[0].value,
            (float) inputs[1].value,
            (float) inputs[2].value);
    }
}

public class NodeWindow_Vector : NodeWindow {
    List<NumberField> numberFields;
    public NodeWindow_Vector () {
        numberFields = new List<NumberField> ();
        numberFields.Add (new NumberField ());
        numberFields.Add (new NumberField ());
        numberFields.Add (new NumberField ());
    }

    public override void OnGUI () {
        Node_Vector n = (Node_Vector)node;
        backgroundColor = Color.green;

        for (int i = 0; i < n.inputs.Count; ++i) {
            GUILayout.BeginHorizontal ();
            DockInput dockInput = n.inputs [i];
            DrawDock (dockInput);
            GUILayout.Label (dockInput.name);
            dockInput.value = numberFields[i].Float ((float) dockInput.value);
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
        }

        GUILayout.BeginHorizontal ();
        DockOutput dockOutput = n.outputs [0];
        GUILayout.Label (dockOutput.value.ToString());
        DrawDock (dockOutput);
        GUILayout.EndHorizontal ();
    }
}
