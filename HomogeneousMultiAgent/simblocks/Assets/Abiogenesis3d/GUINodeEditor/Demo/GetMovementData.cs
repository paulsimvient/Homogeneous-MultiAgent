using UnityEngine;
using GUINodeEditor;

public class GetMovementData: MonoBehaviour {
    public string nodeEditorName = "MovementEditor";
    public Vector3 movementVector;

    GameObject nodeEditorGO;

    void Update () {
        if (nodeEditorGO == null)
            nodeEditorGO = GameObject.Find (nodeEditorName);
        NodeEditor nodeEditor = nodeEditorGO.GetComponent<NodeEditor> ();

        Node_Movement nodeMovement = (Node_Movement)nodeEditor.nodeLogic.nodes
                .Find ((x) => x.GetType () == typeof(Node_Movement));

        if (nodeMovement != null)
            movementVector = (Vector3)nodeMovement.GetDockOutputByName ("result").value;
    }
}
