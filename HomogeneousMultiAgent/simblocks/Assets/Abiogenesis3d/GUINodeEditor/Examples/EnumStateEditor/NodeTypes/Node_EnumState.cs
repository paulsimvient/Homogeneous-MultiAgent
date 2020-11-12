using UnityEngine;
using Type = System.Type;
using GUINodeEditor;

public class Node_EnumState: Node {
    public StackState stackState;

    public override void Init (Vector2 position) {
        Init (position, new NodeWindow_EnumState (), title: "State");

        stackState = new StackState (StateEnums.GetInitialState ());

        foreach (string key in stackState.state.Keys)
            AddInput (stackState.state[key].GetType(), key);
    }

    public override void Update() {
        stackState.UpdateState ();
    }

}

public class NodeWindow_EnumState: NodeWindow {
    public override float GetWindowWidth () {return 160;}

    public override void OnGUI () {
        Node_EnumState n = (Node_EnumState)node;
        backgroundColor = Color.yellow;

        foreach (DockInput dockInput in n.inputs) {
            GUILayout.BeginHorizontal ();
            DrawDock (dockInput);
            string enumTypeStr = dockInput.typeHolder.type.ToString();
            // render popup
            n.stackState.ChangeSubState (enumTypeStr, popup.EnumPopup ((System.Enum)n.stackState.state [enumTypeStr]));
            DrawTooltip (enumTypeStr.Substring (enumTypeStr.LastIndexOf('+') + 1));
            GUILayout.EndHorizontal ();
        }
    }
}
