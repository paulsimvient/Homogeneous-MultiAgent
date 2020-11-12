using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;

public class Node_Color : Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Color (), title: "Color");

        AddInput (typeof(float), "r", 255f);
        AddInput (typeof(float), "g", 255f);
        AddInput (typeof(float), "b", 255f);
        AddInput (typeof(float), "a", 255f);

        AddOutput (typeof(Color), "result");
    }

    public override void Update() {
        DockOutput dockOutput = outputs [0];
        int i = 0;
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);
        inputs[i].value = GetFirstTargetValue <float> (inputs[i], inputs[i++].value);

        dockOutput.value = new Color (
            (float) inputs[0].value / 255f,
            (float) inputs[1].value / 255f,
            (float) inputs[2].value / 255f,
            (float) inputs[3].value / 255f);
    }
}

public class NodeWindow_Color : NodeWindow {
    List<NumberField> numberFields;
    public NodeWindow_Color () {
        numberFields = new List<NumberField> ();
        numberFields.Add (new NumberField ());
        numberFields.Add (new NumberField ());
        numberFields.Add (new NumberField ());
        numberFields.Add (new NumberField ());
    }

    public override void OnGUI () {
        Node_Color n = (Node_Color)node;
        DockOutput result = n.GetDockOutputByName ("result");
        Color resultColor = (Color)result.value;
        backgroundColor = SetOpacity (resultColor, Mathf.Clamp (resultColor.a, 0.25f, 1f));

        for (int i = 0; i < n.inputs.Count; ++i) {
            GUILayout.BeginHorizontal ();
            DockInput dockInput = n.inputs [i];
            DrawDock (dockInput);
            GUILayout.Label (dockInput.name, GUILayout.Width (10));
            dockInput.value = numberFields[i].Float ((float) dockInput.value);
            dockInput.value = GUILayout.HorizontalSlider ((float)dockInput.value, 0, 255);
            
            GUILayout.EndHorizontal ();
        }
        GUILayout.BeginHorizontal ();

        GUILayout.Label ("");
        Rect textureRect = GUILayoutUtility.GetLastRect ();
        Color origColor = GUI.color;
        GUI.color = SetOpacity (resultColor, 1);

        GUI.DrawTexture (textureRect, Texture2D.whiteTexture);
        GUI.color = origColor;

        DrawDock (result);
        GUILayout.EndHorizontal ();
    }
}
