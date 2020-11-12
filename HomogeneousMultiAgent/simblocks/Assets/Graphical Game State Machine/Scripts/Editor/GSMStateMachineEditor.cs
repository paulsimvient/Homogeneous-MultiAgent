using UnityEditor;

namespace GSM
{
    [CustomEditor(typeof(GSMStateMachine))]
    public class GSMStateMachineEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Doubleclick a GSM-asset to edit");
            EditorGUILayout.LabelField("it inside the Game State Machine Editor");
        }
    }
}
