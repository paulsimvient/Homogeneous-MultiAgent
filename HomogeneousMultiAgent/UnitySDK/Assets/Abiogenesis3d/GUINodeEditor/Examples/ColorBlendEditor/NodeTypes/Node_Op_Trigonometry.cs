using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;
using System.Linq;
using Enum = System.Enum;

public class Node_Op_Trigonometry: Node {
    public enum Op_Trigonometry {
        Sin,
        Cos,
        Tg,
        Ctg
    }
    public Op_Trigonometry opTrigonometry = Op_Trigonometry.Sin;

    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Op_Trigonometry ());

        AddInput (typeof(float), "input");
        AddOutput (typeof(float), "result");
    }

    public override void Update () {
        DockInput input = GetDockInputByName ("input");
        DockOutput result = GetDockOutputByName ("result");
        
        float inputValue = GetFirstTargetValue <float> (input, 0f);

        switch (opTrigonometry) {
        case Op_Trigonometry.Sin: result.value = Mathf.Sin (inputValue); break;
        case Op_Trigonometry.Cos: result.value = Mathf.Cos (inputValue); break;
        case Op_Trigonometry.Tg: result.value = Mathf.Tan (inputValue); break;
        case Op_Trigonometry.Ctg: result.value = 1f/ Mathf.Tan (inputValue); break;
        }
    }

    public string GetOperatorSymbol () {
        string symbol = opTrigonometry.ToString ();
        return symbol.Substring (symbol.LastIndexOf ("+") + 1);
    }
}

public class NodeWindow_Op_Trigonometry : NodeWindow {
    public override void OnGUI () {
        Node_Op_Trigonometry n = (Node_Op_Trigonometry)node;
        backgroundColor = Color.cyan;

        n.opTrigonometry = (Node_Op_Trigonometry.Op_Trigonometry) popup.EnumPopup ((Enum)n.opTrigonometry);
        title = "Operator " + n.GetOperatorSymbol ();

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
