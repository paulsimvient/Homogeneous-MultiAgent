using UnityEngine;
using UnityEditor;

namespace GSM
{
    public partial class GSMWindow
    {

        bool isRightScreenMinimized = false;
        private void DrawRightScreen()
        {
            if(isRightScreenMinimized)
            {
                DrawRightScreenMinimized();
                return;
            }

            if (machine == null)
                return;

            EditorGUI.DrawRect(RightSideWindowBounds, windowColorDefault);

            Rect contentRect = new Rect(RightSideWindowBounds.x + boxPadding,
                RightSideWindowBounds.y + boxPadding, sideWindowWidth - 2 * boxPadding - 15, EditorGUIUtility.singleLineHeight);

            //-----------------------------------------

            float spaceHeight = 12;
            string title = "Machine Settings";
            float titleHeight = titleStyle.CalcSize(new GUIContent(title)).y;
            Rect lineRect = new Rect(contentRect.x, contentRect.y, contentRect.width, titleHeight);

            EditorGUI.LabelField(lineRect, title, titleStyle);
            GSMUtilities.DrawSeparator(contentRect.x, lineRect.yMax + 4, lineRect.width, Color.gray);
            lineRect = new Rect(lineRect.x, lineRect.y + titleHeight + spaceHeight * 2, lineRect.width, EditorGUIUtility.singleLineHeight);

            lineRect = EditorGUI.PrefixLabel(lineRect, GSMUtilities.GetContent("Name|Name of the machine"));
            machine.machineName = EditorGUI.TextField(lineRect, machine.machineName);
            lineRect = lineRect.Move(0, 4);

            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Save Active State|If checked the machine will start with the state which was active the last time."));
            bool saveActiveState = EditorGUI.Toggle(lineRect, machine.saveActiveState);
            if(saveActiveState != machine.saveActiveState)
            {
                machine.saveActiveState = saveActiveState;
                if (!saveActiveState)
                    machine.ActiveState = null;
            }
            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Hide All Console Warnings|There will be no warnings in the console when starting the machine. Errors will still be shown"));
            machine.hideAllWarningsConsole = EditorGUI.Toggle(lineRect, machine.hideAllWarningsConsole);

            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Hide All Editor Warnings|There will be no warnings in the editor."));
            machine.hideAllWarningsEditor = EditorGUI.Toggle(lineRect, machine.hideAllWarningsEditor);

            lineRect = EditorGUI.PrefixLabel(new Rect(contentRect.x, lineRect.y + EditorGUIUtility.singleLineHeight, contentRect.width, contentRect.height),
                GSMUtilities.GetContent("Show Invocation Error|If checked there will be an error if calling an event does not work"));
            machine.errorOnFailedInvoke = EditorGUI.Toggle(lineRect, machine.errorOnFailedInvoke);

            //-----------------------------------------

            var miniButtonWidth = 25;
            var miniButtonRect = new Rect(
                RightSideWindowBounds.xMax - miniButtonWidth - boxPadding, 
                RightSideWindowBounds.yMax - EditorGUIUtility.singleLineHeight - boxPadding,
                miniButtonWidth, EditorGUIUtility.singleLineHeight);
            new CustomButton().Draw(miniButtonRect, windowColorDefault, stateColorDefault, 1, "-", titleStyle, () => isRightScreenMinimized = true);

        }

        private void DrawRightScreenMinimized()
        {
            var miniButtonWidth = 25;
            var miniButtonRect = new Rect(RightSideWindowBounds.xMax - miniButtonWidth - boxPadding, 
                RightSideWindowBounds.yMax - EditorGUIUtility.singleLineHeight - boxPadding,
                miniButtonWidth, EditorGUIUtility.singleLineHeight);
            new CustomButton().Draw(miniButtonRect, windowColorDefault, stateColorDefault, 1, "-", titleStyle, () => isRightScreenMinimized = false);
        }
    }
}
