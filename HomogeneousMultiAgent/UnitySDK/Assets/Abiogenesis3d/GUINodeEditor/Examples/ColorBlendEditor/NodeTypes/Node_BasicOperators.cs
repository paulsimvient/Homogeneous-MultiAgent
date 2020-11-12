using UnityEngine;
using GUINodeEditor;
using System.Collections.Generic;
using System.Linq;
using Enum = System.Enum;

public class Node_BasicOperators: Node {
    public enum BasicOperators {
        Add,
        Subtract,
        Multiply,
        Divide
    }
    public BasicOperators basicOperator = BasicOperators.Add;

    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_BasicOperators ());

        AddInput (typeof(float), "detector");
        AddOutput (typeof(float), "result");
    }

    public override void Update () {
        DockInput detectorInput = GetDockInputByName ("detector");
        DockOutput resultOutput = GetDockOutputByName ("result");

        // remove docks without targets
        foreach (DockInput dockInput in inputs.AsEnumerable().Reverse())
            if (dockInput.targets.Count == 0 && dockInput != detectorInput)
                inputs.Remove (dockInput);

        for (int i = 0; i < inputs.Count; ++i) {
            DockInput dockInput = inputs [i];

            // move connection from detector to new input
            if (dockInput == detectorInput) {
                if (dockInput.targets.Count > 0) {
                    DockOutput moveTargetDock = (DockOutput)dockInput.targets [0];
                    nodeWindow.nodeEditor.MoveConnection (
                        moveTargetDock, dockInput, AddInput (typeof(float), insertAtIndex: i));
                }
            }
            else if (dockInput.targets.Count > 1) {
                DockOutput moveTargetDock = (DockOutput)dockInput.targets [1];
                nodeWindow.nodeEditor.MoveConnection (
                    moveTargetDock, dockInput, AddInput (typeof(float), insertAtIndex: i));
            }
        }

        // only detector
        if (inputs.Count == 1) {
            resultOutput.value = 0f;
            return;
        }



        // head
        float result = (float)inputs[0].targets[0].value;

        // skip detector and head
        if (inputs.Count > 2) {
            foreach (DockInput dockInput in inputs.GetRange (1, inputs.Count - 1)) {
                if (dockInput == detectorInput)
                    continue;
                float f = (float)dockInput.targets[0].value;
                if (basicOperator == BasicOperators.Add) {result += f; continue;}
                if (basicOperator == BasicOperators.Subtract) {result -= f; continue;}
                if (basicOperator == BasicOperators.Multiply) {result *= f; continue;}
                if (basicOperator == BasicOperators.Divide) {result /= f; continue;}
            }
        }

        resultOutput.value = result;
    }

    public string GetOperatorSymbol () {
        if (basicOperator == BasicOperators.Add) {return "+";}
        if (basicOperator == BasicOperators.Subtract) {return "-";}
        if (basicOperator == BasicOperators.Multiply) {return "*";}
        if (basicOperator == BasicOperators.Divide) {return "/";}
        return "";
    }
}

public class NodeWindow_BasicOperators : NodeWindow {
    public override void OnGUI () {
        Node_BasicOperators n = (Node_BasicOperators)node;
        backgroundColor = Color.cyan;

        n.basicOperator = (Node_BasicOperators.BasicOperators) popup.EnumPopup ((Enum)n.basicOperator);
        title = "Operator " + n.GetOperatorSymbol ();

        DockInput detector = n.GetDockInputByName ("detector");
        DockOutput result = n.GetDockOutputByName ("result");

        foreach (DockInput dockInput in n.inputs) {
            // skip detector, draw it last in same row with result
            if (dockInput == detector)
                continue;

            GUILayout.BeginHorizontal ();
            DrawDock (dockInput);

            string operatorSymbol = "";
            // avoid head (first)
            if (dockInput != n.inputs[0])
                operatorSymbol = n.GetOperatorSymbol ();

            string targetString = n.GetFirstTargetValue<float> (dockInput, 0f).ToString();
            GUILayout.Label (operatorSymbol + " " + targetString);
            GUILayout.EndHorizontal ();
        }
        GUILayout.BeginHorizontal ();
        DrawDock (detector);
        GUILayout.FlexibleSpace ();
        GUILayout.Label ("= ");
        GUILayout.Box (((float)result.value).ToString ("0.00"));
        DrawDock (result);
        GUILayout.EndHorizontal ();
    }
}
