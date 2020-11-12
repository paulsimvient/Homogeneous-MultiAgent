using UnityEngine;
using GUINodeEditor;

public class Node_EnumStateModifier : Node {
    public StackState modifierStackState;

    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_EnumStateModifier (), title: "Modifier");

        AddInput (typeof(bool), "trigger");
        AddOutput (typeof(void), "detector");

        modifierStackState = new StackState (StateEnums.GetInitialState ());

        foreach (string key in modifierStackState.state.Keys)
            AddOutput (modifierStackState.state[key].GetType(), key);
    }

    public override void Update() {
        UpdateIsTriggered ();
        UpdateTriggeredOutputs ();
    }

    void UpdateTriggeredOutputs () {
        foreach (DockOutput dockOutput in outputs) {
            foreach (DockInput dockInput in dockOutput.targets) {
                Node_EnumState enumStateNode = (Node_EnumState)dockInput.node;
                string key = dockInput.typeHolder.type.ToString ();
                enumStateNode.stackState.stacks [key]
                        .HandleInsertRemove (this, isTriggered, modifierStackState.state [key]);
            }
        }
    }

    public void UpdateIsTriggered () {
        isTriggered = false;
        foreach (DockInput dockInput in inputs) {
            foreach (DockOutput dockOutput in dockInput.targets) {
                if ((bool)dockOutput.value) {
                    isTriggered = true;
                    break;
                }
            }
        }
    }
}

public class NodeWindow_EnumStateModifier : NodeWindow {
    public override float GetWindowWidth() {return 140;}

    public override void OnGUI () {
        Node_EnumStateModifier n = (Node_EnumStateModifier)node;
        backgroundColor = Color.red;

        GUILayout.BeginHorizontal ();
        DrawDock (n.GetDockInputByName ("trigger"), isTitleRow: true);
        DrawDock (n.GetDockOutputByName ("detector"), isTitleRow: true);
        GUILayout.EndHorizontal ();

        foreach (DockOutput dockOutput in n.outputs) {
            // we alredy drew detector
            if (dockOutput.name == "detector")
                continue;

            // hide enums without targets
            if (dockOutput.targets.Count == 0) {
                dockOutput.dockWindow.rect = new Rect ();
                continue;
            }

            GUILayout.BeginHorizontal ();
            string key = dockOutput.typeHolder.type.ToString();
            n.modifierStackState.state [key] = popup.EnumPopup ((System.Enum)n.modifierStackState.state [key]);
            DrawTooltip (key.Substring (key.LastIndexOf('+') + 1));
            DrawDock (dockOutput);
            GUILayout.EndHorizontal ();

        }
    }
}
