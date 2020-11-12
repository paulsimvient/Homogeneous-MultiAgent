using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;
using System.Linq;
using Enum = System.Enum;

public class Node_ColorBlend: Node {
    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_ColorBlend (), title: "Color Blend");


        AddInput (typeof(float), "amount");

        AddInput (typeof(Color), "first");
        AddInput (typeof(Color), "second");

        AddOutput (typeof(Color), "result");
    }

    public override void Update () {
        DockInput amount = GetDockInputByName ("amount");
        DockInput first = GetDockInputByName ("first");
        DockInput second = GetDockInputByName ("second");

        DockOutput result = GetDockOutputByName ("result");

        float amountValue = GetFirstTargetValue<float> (amount, amount.value);

        Color firstColor = GetFirstTargetValue<Color> (first, Color.white);
        Color secondColor = GetFirstTargetValue<Color> (second, Color.white);

        result.value = Blend (firstColor, secondColor, amountValue);
    }

    public Color Blend (Color c1, Color c2, float amount) {
        float r = (c1.r * (1 - amount)) + c2.r * amount;
        float g = (c1.g * (1 - amount)) + c2.g * amount;
        float b = (c1.b * (1 - amount)) + c2.b * amount;
        float a = (c1.a * (1 - amount)) + c2.a * amount;
        return new Color (r, g, b, a);
    }
}

public class NodeWindow_ColorBlend : NodeWindow {
    public override void OnGUI () {
        Node_ColorBlend n = (Node_ColorBlend)node;

        DockInput amount = n.GetDockInputByName ("amount");
        DockInput first = n.GetDockInputByName ("first");
        DockInput second = n.GetDockInputByName ("second");
        DockOutput result = n.GetDockOutputByName ("result");

        Color origColor = GUI.color;
        Color resultColor = (Color)result.value;
        Color firstColor = n.GetFirstTargetValue<Color> (first, Color.white);
        Color secondColor = n.GetFirstTargetValue<Color> (second, Color.white);

        // amount
        GUILayout.BeginHorizontal ();
        DrawDock (amount);
        amount.value = GUILayout.HorizontalSlider (n.GetFirstTargetValue<float> (amount, amount.value), 0f, 1f);
        GUILayout.EndHorizontal ();

        // first
        GUILayout.BeginHorizontal ();
        {
            DrawDock (first);
            GUILayout.Label ("");
            Rect texRect = GUILayoutUtility.GetLastRect ();
            GUI.color = SetOpacity (firstColor, 1);
            GUI.DrawTexture (texRect, Texture2D.whiteTexture);
            GUILayout.Label ("", GUILayout.Width (0));
        }
        GUILayout.EndHorizontal ();

        // second
        GUILayout.BeginHorizontal ();
        {
            DrawDock (second);
            GUILayout.Label ("");
            Rect texRect = GUILayoutUtility.GetLastRect ();
            GUI.color = SetOpacity (secondColor, 1);
            GUI.DrawTexture (texRect, Texture2D.whiteTexture);
            GUILayout.Label ("", GUILayout.Width (0));
        }
        GUILayout.EndHorizontal ();

        GUILayout.BeginHorizontal ();
        {
            GUILayout.Label ("", GUILayout.Width (0));
            GUILayout.Label ("");
            Rect textureRect = GUILayoutUtility.GetLastRect ();
            GUI.color = SetOpacity (resultColor, 1);
            GUI.DrawTexture (textureRect, Texture2D.whiteTexture);
            GUI.color = origColor;
            DrawDock (result);
        }
        GUILayout.EndHorizontal ();

        backgroundColor = SetOpacity (resultColor, Mathf.Clamp (resultColor.a, 0.25f, 1f));
        GUI.color = origColor;
    }
}
