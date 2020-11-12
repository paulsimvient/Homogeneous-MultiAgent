using UnityEngine;
using GUINodeEditor;

public class GetColorData: MonoBehaviour {
    public string nodeEditorName = "ColorBlendEditor";
    public Color color;

    GameObject nodeEditorGO;

    void Update () {
        if (nodeEditorGO == null)
            nodeEditorGO = GameObject.Find (nodeEditorName);
        NodeEditor nodeEditor = nodeEditorGO.GetComponent<NodeEditor> ();

        Node_ColorBlend nodeColorBlend = (Node_ColorBlend)nodeEditor.nodeLogic.nodes
                .Find ((x) => x.GetType () == typeof(Node_ColorBlend));

        if (nodeColorBlend != null)
            color = (Color)nodeColorBlend.GetDockOutputByName ("result").value;
    }
}
