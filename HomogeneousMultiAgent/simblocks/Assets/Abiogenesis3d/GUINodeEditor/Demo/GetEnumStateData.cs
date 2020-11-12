using UnityEngine;
using GUINodeEditor;

public class GetEnumStateData: MonoBehaviour {
    public string nodeEditorName = "EnumStateEditor";
    public StackState stackState;

    GameObject nodeEditorGO;

    void Update () {
        if (nodeEditorGO == null)
            nodeEditorGO = GameObject.Find (nodeEditorName);
        NodeEditor nodeEditor = nodeEditorGO.GetComponent<NodeEditor> ();

        Node_EnumState nodeEnumState = (Node_EnumState)nodeEditor.nodeLogic.nodes
                .Find ((x) => x.GetType () == typeof(Node_EnumState));

        if (nodeEnumState != null)
            stackState = nodeEnumState.stackState;
    }

}
