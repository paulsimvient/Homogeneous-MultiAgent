using UnityEngine;
using GUINodeEditor;

public class Node_Trigger: Node {
    public Trigger trigger;
    public bool isReceivingKeyInput;

    public override void Init(Vector2 position) {
        Init (position, new NodeWindow_Trigger(), title: "Trigger");
        trigger = new Trigger ();
        AddOutput (typeof(bool), "trigger");
    }

    public override void Update () {
        UpdateTrigger ();
    }

    public void UpdateTrigger () {
        trigger.UpdateTrigger ();
        isTriggered = trigger.isTriggered;
        GetDockOutputByName ("trigger").value = isTriggered;
    }
}

public class NodeWindow_Trigger : NodeWindow {
    public override void OnGUI () {
        Node_Trigger n = (Node_Trigger)node;
        backgroundColor = Color.green;
        Event e = Event.current;

        DrawDock (n.GetDockOutputByName ("trigger"), isTitleRow: true);

        GUILayout.BeginHorizontal ();
        string displayStr = n.trigger.source == Trigger.Source.Key ?
            n.trigger.key.ToString() : "Mouse: " + n.trigger.button;

        if (GUILayout.Button (n.isReceivingKeyInput ? "press|click" : displayStr)) {
            if (n.isReceivingKeyInput) {
                n.trigger.source = Trigger.Source.Mouse;
                n.trigger.button = e.button;
                n.isReceivingKeyInput = false;
            }
            else n.isReceivingKeyInput = true;
        }
        if (n.isReceivingKeyInput) {
            if (e.isKey) {
                n.trigger.source = Trigger.Source.Key;
                n.trigger.key = e.keyCode;
                n.isReceivingKeyInput = false;
            }
        }

        if (GUILayout.Button ("x", GUILayout.Width(20))) {
            if (n.isReceivingKeyInput)
                n.isReceivingKeyInput = false;
            else {
                n.trigger.key = KeyCode.None;
                n.trigger.source = Trigger.Source.Key;
            }
        }
        GUILayout.EndHorizontal ();

        n.trigger.style = (Trigger.Style) popup.EnumPopup (n.trigger.style);

    }
}
