using UnityEngine; 
using GUINodeEditor;
using System.Collections.Generic;

public class Node_Logo_128x128: Node {
    public bool toggle = true;
    public string text1 = "GUI";
    public string text2 = "Node Editor";
    public float slider1 = 0;
    public float slider2 = 0.33f;
    public float slider3 = 1;

    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Logo_128x128 (), title: " ");

        AddInput (typeof(float));

        AddOutput (typeof(float));
        AddOutput (typeof(float));
    }
}

public class NodeWindow_Logo_128x128: NodeWindow {
    public override float GetWindowWidth () {
        return 120;
    }

    public override void OnGUI () {
        Node_Logo_128x128 n = (Node_Logo_128x128)node;

        backgroundColor = Color.black;

        GUILayout.BeginHorizontal ();
        Color origColor = GUI.color;
        DrawDock (n.inputs[0]);

        GUI.color = Color.green;
        n.toggle = GUILayout.Toggle (n.toggle, "", GUILayout.Width (15));
        GUI.color = origColor;
        n.text1 = GUILayout.TextField (n.text1, GUI.skin.box);
        DrawDock (n.outputs[0]);
        GUILayout.EndHorizontal ();
        GUILayout.BeginHorizontal ();
        n.text2 = GUILayout.TextField (n.text2, GUI.skin.box);
        DrawDock (n.outputs[1]);
        GUILayout.EndHorizontal ();
        n.slider1 = GUILayout.HorizontalSlider (n.slider1, 0, 1);
        n.slider2 = GUILayout.HorizontalSlider (n.slider2, 0, 1);
        n.slider3 = GUILayout.HorizontalSlider (n.slider3, 0, 1);
    }
}
