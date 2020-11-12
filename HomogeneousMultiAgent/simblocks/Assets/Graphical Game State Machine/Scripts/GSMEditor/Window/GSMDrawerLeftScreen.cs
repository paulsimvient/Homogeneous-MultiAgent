using UnityEngine;
using UnityEditor;
using System;

namespace GSM
{
    public partial class GSMWindow
    {

        private Vector2 leftScrollPos = Vector2.zero;
        bool isLeftScreenMinimized = false;

        /// <summary>
        /// Draws left window used for inspector
        /// </summary>
        private void DrawLeftScreen()
        {
            if (machine == null)
                return;

            if (isLeftScreenMinimized)
            {
                DrawLeftScreenMinimized();
                return;
            }


            try
            {
                EditorGUI.DrawRect(LeftSideWindowBounds, windowColorDefault);
                leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, false, false, GUILayout.Width(LeftSideWindowBounds.width + 1), GUILayout.Height(LeftSideWindowBounds.height));
                float drawnHeight = 0;

                Rect contentRect = new Rect(LeftSideWindowBounds.x + boxPadding,
                    LeftSideWindowBounds.y + boxPadding, sideWindowWidth - 2 * boxPadding - 15, EditorGUIUtility.singleLineHeight);

                if (InspectedObject != null)
                {
                    if (InspectedObject is GSMState)
                        drawnHeight = OnStateInspected(InspectedObject as GSMState, contentRect);
                    else if (InspectedObject is GSMEdge)
                        drawnHeight = OnEdgeInspected(InspectedObject as GSMEdge, contentRect);
                }
                else
                {
                    GUILayout.Label(new GUIContent("Click a state or an edge to edit"));
                }


                GUILayout.Box(GUIContent.none, GUIStyle.none, GUILayout.Height(drawnHeight + contentRect.y));
                GUILayout.EndScrollView();
            }
            catch (ArgumentException) { }



            var miniButtonWidth = 25;
            var miniButtonRect = new Rect(LeftSideWindowBounds.x + boxPadding,
                RightSideWindowBounds.yMax - EditorGUIUtility.singleLineHeight - boxPadding,
                miniButtonWidth, EditorGUIUtility.singleLineHeight);
            new CustomButton().Draw(miniButtonRect, windowColorDefault, stateColorDefault, 1, "-", titleStyle, () =>
                {
                    isLeftScreenMinimized = true;
                });

        }




        #region Inspector drawing


        bool foldoutOutgoingEdges = false;
        bool foldoutIngoingEdges = false;
        float OnStateInspected(GSMState state, Rect contentRect)
        {


            float spaceHeight = 16;
            int nameFieldMaxLength = 32;
            var oEdges = machine.GetOutgoingEdges(state);
            var iEdges = machine.GetIngoingEdges(state);



            ////////////////////
            string title = state.name == "" ? "State " + state.id : state.name;
            float titleHeight = titleStyle.CalcSize(new GUIContent(title)).y;
            Rect lineRect = new Rect(contentRect.x, contentRect.y, contentRect.width, titleHeight);

            EditorGUI.LabelField(lineRect, title, titleStyle);
            GSMUtilities.DrawSeparator(contentRect.x, lineRect.yMax + 4, lineRect.width, Color.gray);
            lineRect = lineRect.Move(0, titleHeight + spaceHeight);

            lineRect = EditorGUI.PrefixLabel(lineRect, GSMUtilities.GetContent("State ID|Automatically generated state ID"), defaultStyleOutgrayed);
            EditorGUI.LabelField(lineRect, new GUIContent("" + state.id), defaultStyleOutgrayed);

            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Name|Name of the state"));
            var name = EditorGUI.TextField(lineRect, state.name);
            name = name.Substring(0, Mathf.Min(name.Length, nameFieldMaxLength));
            if(name == "")
            {
                name = "State " + state.id;
            }
            state.name = name;

            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Start State"));
            bool isStartState = machine.StartState == state;
            isStartState = EditorGUI.Toggle(lineRect, isStartState);
            if (isStartState)
            {
                machine.SetStartState(state);
            }



            EditorGUI.BeginDisabledGroup(!machine.saveActiveState && !machine.isRunning || state.isTerminating);
            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Active State|Check \"Save Active State\" in machine settings to enable this box"));
            bool isActiveState = machine.ActiveState == state;
            isActiveState = EditorGUI.Toggle(lineRect, isActiveState);
            if (isActiveState)
            {
                machine.ActiveState = state;
                machine.ActiveState.onStateSetActive.Invoke(machine.errorOnFailedInvoke);
            }
            else if (machine.ActiveState == state)
                machine.ActiveState = null;

            EditorGUI.EndDisabledGroup();




            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Is Terminating|If a state is terminating, the machine will be stopped after setting the state active"));
            bool isTerminating = EditorGUI.Toggle(lineRect, state.isTerminating);
            if(isTerminating != state.isTerminating)
            {
                state.isTerminating = isTerminating;
                hasUnsavedChanges = true;
            }

            if (isTerminating && isActiveState)
            {
                machine.ActiveState = null;
                hasUnsavedChanges = true;
            }




            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Hide Warnings Editor|If checked there will be no warnings shown on states"));
            bool suppressWarningsEditor = EditorGUI.Toggle(lineRect, state.hideWarningsInEditor);
            if (suppressWarningsEditor != state.hideWarningsInEditor)
            {
                state.hideWarningsInEditor = suppressWarningsEditor;
                hasUnsavedChanges = true;
            }




            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Hide Warnings Console|If checked there will be no warnings shown in console"));
            bool suppressWarningsConsole = EditorGUI.Toggle(lineRect, state.hideWarningsInConsole);
            if (suppressWarningsConsole != state.hideWarningsInConsole)
            {
                state.hideWarningsInConsole = suppressWarningsConsole;
                hasUnsavedChanges = true;
            }



            EditorGUI.BeginDisabledGroup(state.isTerminating);
            string[] updateTypes = new string[] { "Update()", "FixedUpdate()", "LateUpdate()" };
            int[] updateTypeValues = new int[] { GSMState.UpdateTypeUpdate, GSMState.UpdateTypeFixedUpdate, GSMState.UpdateTypeLateUpdate };
            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Update Type|Determine at which update type the OnStateStay() event should be called"));
            int updateType = EditorGUI.IntPopup(lineRect, state.updateType, updateTypes, updateTypeValues);
            if (updateType != state.updateType)
            {
                state.updateType = updateType;
                hasUnsavedChanges = true;
            }
            EditorGUI.EndDisabledGroup();





            string[] orderTypes = new string[] { "File before Runtime", "Runtime Before File", "Only Runtime", "Only File" };
            int[] orderTypesValues = new int[] { GSMStateMachine.FileBeforeRuntime, GSMStateMachine.RuntimeBeforeFile, GSMStateMachine.OnlyRuntime, GSMStateMachine.OnlyFile };
            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Callback Order|Determine the order to invoke callbacks of your events for this state"));
            int orderType = EditorGUI.IntPopup(lineRect, state.callbackInvokationOrder, orderTypes, orderTypesValues);
            if (orderType != state.callbackInvokationOrder)
            {
                state.callbackInvokationOrder = orderType;
                hasUnsavedChanges = true;
            }






            ////////////////// HEADER EDGES /////////////////
            lineRect = new Rect(contentRect.x, lineRect.yMax + 2 * spaceHeight, contentRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lineRect, new GUIContent("Edges"), headerStyle);
            GSMUtilities.DrawSeparator(contentRect.x, lineRect.yMax, lineRect.width, Color.gray);
            lineRect = lineRect.Move(0, lineRect.height + 8);

            #region Outgoing Edges
            foldoutOutgoingEdges = EditorGUI.Foldout(lineRect, foldoutOutgoingEdges, GSMUtilities.GetContent("Outgoint Edges (" + oEdges.Count + ")|Click the arrow to show and hide outgoing edges"));
            if (foldoutOutgoingEdges)
            {
                lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight + 8);
                if (oEdges.Count == 0)
                {
                    EditorGUI.LabelField(lineRect, GSMUtilities.GetContent("No outgoing edges."));
                }
                else
                {
                    lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight);
                    foreach (var edge in oEdges)
                    {
                        lineRect = DrawEdgeInspector(lineRect, edge);
                    }
                }

            }
            #endregion

            #region IngoingEdges
            lineRect = new Rect(contentRect.x, lineRect.yMax + spaceHeight, contentRect.width, EditorGUIUtility.singleLineHeight);
            foldoutIngoingEdges = EditorGUI.Foldout(lineRect, foldoutIngoingEdges, GSMUtilities.GetContent("Ingoing Edges (" + iEdges.Count + ")|Click the arrow to show and hide ingoing edges"));
            if (foldoutIngoingEdges)
            {
                lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight + 8);
                if (iEdges.Count == 0)
                {
                    EditorGUI.LabelField(lineRect, GSMUtilities.GetContent("No outgoing edges."));
                }
                else
                {
                    lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight);
                    foreach (var edge in iEdges)
                    {
                        lineRect = DrawEdgeInspector(lineRect, edge);
                    }
                }

            }
            #endregion



            ///////////////////// HEADER EVENTS /////////////////
            lineRect = new Rect(contentRect.x, lineRect.yMax + 2 * spaceHeight, contentRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(lineRect, new GUIContent("Events"), headerStyle);
            GSMUtilities.DrawSeparator(contentRect.x, lineRect.yMax, lineRect.width, Color.gray);
            lineRect = lineRect.Move(0, lineRect.height + 8);


            #region Events
            if(!state.isTerminating)
                lineRect = DrawUnityEvent(lineRect, state.onStateEntered, "On State Entered()", "This gets called when the state was entered. Does not get called when set as start state").Move(0, 12);

            lineRect = DrawUnityEvent(lineRect, state.onStateSetActive, "On State Set Active()", "This gets called when the state is set active. Right after OnStateEntered()." +
                "Also getting called when saved active state is started").Move(0, 12);

            if (!state.isTerminating)
                lineRect = DrawUnityEvent(lineRect, state.onStateStay, "On State Stay()", "This gets called in each Update()-call while this state is active").Move(0, 12);

            if (!state.isTerminating)
                lineRect = DrawUnityEvent(lineRect, state.onStateLeft, "On State Left()", "This gets called when the state is left").Move(0, 16);

            #endregion

            if (GUI.Button(lineRect, "Delete State"))
            {
                machine.DeleteState(state);
                SetInspectedObject(null);
            }
            lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight);



            return lineRect.yMax - contentRect.y+ EditorGUIUtility.singleLineHeight;
        }


        float OnEdgeInspected(GSMEdge edge, Rect contentRect)
        {
            var origin = machine.GetState(edge.originID);
            var target = machine.GetState(edge.targetID);

            var spaceHeight = 8f;

            string title = "Edge";
            float titleHeight = titleStyle.CalcSize(new GUIContent(title)).y;
            Rect lineRect = new Rect(contentRect.x, contentRect.y, contentRect.width, titleHeight);

            EditorGUI.LabelField(lineRect, title, titleStyle);
            GSMUtilities.DrawSeparator(contentRect.x, lineRect.yMax + 4, lineRect.width, Color.gray);
            lineRect = lineRect.Move(0, titleHeight + spaceHeight * 2);

            EditorGUI.LabelField(lineRect, origin.name + " \u2192 " + target.name, headerStyle);
            lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight + spaceHeight * 3);

            lineRect = EditorGUI.PrefixLabel(lineRect, GSMUtilities.GetContent("Trigger|When this trigger is set the edge will be passed")).Move(0, 4);
            edge.trigger = EditorGUI.TextField(lineRect, edge.trigger);



            string[] orderTypes = new string[] { "File before Runtime", "Runtime Before File", "Only Runtime", "Only File" };
            int[] orderTypesValues = new int[] { GSMStateMachine.FileBeforeRuntime, GSMStateMachine.RuntimeBeforeFile, GSMStateMachine.OnlyRuntime, GSMStateMachine.OnlyFile };
            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Callback Order|Determine the order to invoke callbacks of your events for this state"));
            int orderType = EditorGUI.IntPopup(lineRect, edge.callbackInvokationOrder, orderTypes, orderTypesValues);
            if (orderType != edge.callbackInvokationOrder)
            {
                edge.callbackInvokationOrder = orderType;
                hasUnsavedChanges = true;
            }



            lineRect = new Rect(contentRect.x, lineRect.yMax + spaceHeight * 2, contentRect.width, EditorGUIUtility.singleLineHeight);

            ///////////////// ORIGIN //////////////////
            lineRect = EditorGUI.PrefixLabel(lineRect, new GUIContent("Origin"), defaultStyle);
            if (GUI.Button(lineRect, new GUIContent(origin.name)))
            {
                SetInspectedObject(origin);
            }

            lineRect = new Rect(contentRect.x, lineRect.yMax + spaceHeight, contentRect.width, EditorGUIUtility.singleLineHeight);

            /////////////////// TARGET //////////////////
            lineRect = EditorGUI.PrefixLabel(lineRect, new GUIContent("Target"), defaultStyle);
            if (GUI.Button(lineRect, new GUIContent(target.name)))
            {
                SetInspectedObject(target);
            }



            lineRect = new Rect(contentRect.x, lineRect.yMax + spaceHeight * 3, contentRect.width, EditorGUIUtility.singleLineHeight);
            lineRect = DrawUnityEvent(lineRect, edge.onEdgePassed, "On Edge Passed()", "Getting called when this edge is used to reach another state").Move(0, 16);

            if (GUI.Button(lineRect, "Delete Edge"))
            {
                machine.DeleteEdge(edge);
                SetInspectedObject(null);
            }
            lineRect = lineRect.Move(0, EditorGUIUtility.singleLineHeight);

            return lineRect.yMax + contentRect.y;
        }
        #endregion


        private void DrawLeftScreenMinimized()
        {
            var miniButtonWidth = 25;
            var miniButtonRect = new Rect(LeftSideWindowBounds.x + boxPadding,
                RightSideWindowBounds.yMax - EditorGUIUtility.singleLineHeight - boxPadding,
                miniButtonWidth, EditorGUIUtility.singleLineHeight);
            new CustomButton().Draw(miniButtonRect, windowColorDefault, stateColorDefault, 1, "-", titleStyle, () => isLeftScreenMinimized = false);
        }


    }
}
