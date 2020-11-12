using UnityEngine;
using UnityEditor;


namespace GSM
{
    [CustomEditor(typeof(StateMachineProcessor))]
    public class StateMachineProcessorEditor : Editor
    {
        private SerializedProperty propertyMachine;

        void OnEnable()
        {
            propertyMachine = serializedObject.FindProperty("stateMachine");
        }


        string trigger = "";
        bool foldoutMachineSettings = false;
        bool foldoutInformation = false;
        bool foldoutPreviousStates = false;

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.ObjectField(propertyMachine, typeof(GSMStateMachine));
            EditorGUILayout.Space();

            var processor = target as StateMachineProcessor;
            var stateMachine = propertyMachine.objectReferenceValue as GSMStateMachine;
            if (stateMachine == null)
            {
                EditorGUILayout.LabelField("Select a state machine file to see more options", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic });
            } else
            {

                var wrapper = (target as StateMachineProcessor).Machine;


                stateMachine.startMachineOnAwake = EditorGUILayout.Toggle(
                    GSMUtilities.GetContent("Start On Awake|" +
                    "If checked the machine will start automatically when the containing StateMachineProcessor awakes"),
                    stateMachine.startMachineOnAwake);

                stateMachine.clearRuntimeCallbacksOnStop = EditorGUILayout.Toggle(
                    GSMUtilities.GetContent("Clear Runtime Callbacks|" +
                    "If checked, all runtime callbacks will automatically be removed when the machine stops"),
                    stateMachine.clearRuntimeCallbacksOnStop
                    ) ;
                EditorGUILayout.Space();

                foldoutMachineSettings = EditorGUILayout.Foldout(foldoutMachineSettings, GSMUtilities.GetContent("Machine Settings|The settings you can also modify in the State Machine Editor"));
                if (foldoutMachineSettings)
                {
                    EditorGUI.indentLevel++;
                    stateMachine.saveActiveState = EditorGUILayout.Toggle(
                        GSMUtilities.GetContent("Save Active State|" +
                        "If checked the machine will start with the state which was active the last time the machine was started"),
                        stateMachine.saveActiveState);

                    stateMachine.hideAllWarningsConsole = EditorGUILayout.Toggle(
                        GSMUtilities.GetContent("Hide Console Warnings|" +
                        "There will be no warnings in the console when starting the machine"),
                        stateMachine.hideAllWarningsConsole);

                    stateMachine.hideAllWarningsEditor = EditorGUILayout.Toggle(
                        GSMUtilities.GetContent("Hide Editor Warnings|" +
                        "There will be no warnings in the console when starting the machine"),
                        stateMachine.hideAllWarningsEditor);

                    stateMachine.hideAllWarningsEditor = EditorGUILayout.Toggle(
                        GSMUtilities.GetContent("Show Invocation Error|" +
                        "If checked, you will get an error if an event yould not be invoked."),
                        stateMachine.hideAllWarningsEditor);

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();
                var divider = EditorGUILayout.GetControlRect(false, 1);
                divider.height = 1;
                EditorGUI.DrawRect(divider, Color.gray);
                EditorGUILayout.Space();

                string status = stateMachine.isRunning ? "Running" : "Stopped";
                string statusTooltip = stateMachine.isRunning ? "" : "|Start machine to show informations and send triggers";
                var statusStyle = new GUIStyle(GUI.skin.label);
                statusStyle.fontStyle = stateMachine.isRunning ? FontStyle.Bold : FontStyle.Normal;
                statusStyle.normal.textColor = stateMachine.isRunning ? Color.green : Color.red;
                var statusContent = GSMUtilities.GetContent(status + statusTooltip);
                EditorGUILayout.LabelField(GSMUtilities.GetContent("Status|Shows if machine is running or stopped"), statusContent, statusStyle);

                EditorGUILayout.BeginHorizontal();
                if(EditorApplication.isPlaying && GUILayout.Button(stateMachine.isRunning ? "Stop" : "Start"))
                {
                    if (stateMachine.isRunning)
                        processor.StopMachine();
                    else processor.StartMachine();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();


                EditorGUI.BeginDisabledGroup(!stateMachine.isRunning);
                foldoutInformation = EditorGUILayout.Foldout(foldoutInformation,
                    GSMUtilities.GetContent("Informations|Shows Informations about current states while machine is running")) && stateMachine.isRunning;
                if (foldoutInformation)
                {

                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(GSMUtilities.GetContent("Active State|The state which is currently active"), new GUIContent(stateMachine.ActiveState.name));
                    EditorGUILayout.LabelField(new GUIContent("Active State ID"), new GUIContent(stateMachine.ActiveState.id+""));
                    EditorGUILayout.LabelField(GSMUtilities.GetContent("Activation Reason|Reason why the current active state was set active"), 
                        new GUIContent(wrapper.StateActivationReason.ToString()));
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField(GSMUtilities.GetContent("Available States|All States which can be reached by one edge"));
                    EditorGUI.indentLevel++;
                    var edges = stateMachine.GetOutgoingEdges(stateMachine.ActiveState);
                    if (edges.Count == 0)
                        EditorGUILayout.LabelField(new GUIContent("No Available States"));
                    else foreach (var edge in edges)
                    {
                        var target = stateMachine.GetState(edge.targetID);
                        EditorGUILayout.LabelField(new GUIContent("Trigger: " + edge.trigger + " \u2192 " + target.name));
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();

                    foldoutPreviousStates = EditorGUILayout.Foldout(foldoutPreviousStates, new GUIContent("Previous States"));
                    if(foldoutPreviousStates)
                    {
                        int prevSize = EditorGUILayout.IntSlider(GSMUtilities.GetContent("Amount|Amount of previous states which should be saved"), wrapper.PreviousStatesSize, 1, 1027);
                        if (prevSize != wrapper.PreviousStatesSize)
                            wrapper.PreviousStatesSize = prevSize;

                        var prev = wrapper.PreviousStates;

                        EditorGUI.indentLevel++;
                        if (prev.Count == 0)
                            EditorGUILayout.LabelField("No Previous States");
                        else foreach (var state in prev)
                        {
                            EditorGUILayout.LabelField("- "+state.Name);
                        }
                        EditorGUI.indentLevel--;
                    }


                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();



                    EditorGUILayout.Space();

                if(stateMachine.isRunning)
                {
                    EditorGUILayout.LabelField("Trigger to send:");
                    EditorGUILayout.BeginHorizontal();
                    trigger = EditorGUILayout.TextField(trigger);
                    if(GUILayout.Button("Send") || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
                    {
                        wrapper.SendTrigger(trigger);
                        trigger = "";
                        GUI.changed = true;
                    }
                    EditorGUILayout.EndHorizontal();

                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("When the machine is started you", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic });
                    EditorGUILayout.LabelField("will be able to send triggers here", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic });
                    trigger = null;
                    EditorGUI.EndDisabledGroup();
                }
            }
            if (serializedObject.ApplyModifiedProperties())
                Repaint();

            if (GUI.changed)
                Repaint();
        }
    }
}
