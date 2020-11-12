using UnityEngine;
using UnityEditor;

namespace GSM
{
    public partial class GSMStateMachine
    {
        internal void DrawState(GSMState state)
        {
            bool isInspected = window.InspectedObject == state;
            bool isStartState = StartState == state;
            bool isActive = ActiveState == state;
            bool isTerminating = state.isTerminating;

            float minStateWidth = 80;
            float maxStateWidth = 250;

            float padding = 4;


            //var texture = isActive ? GSMWindow.stateTextureActive : (isInspected ? GSMWindow.stateTextureSelected : GSMWindow.stateTextureDefault);




            var style = new GUIStyle(window.defaultStyle);
            style.normal.textColor = window.stateFontColorDefault;

            var style2 = new GUIStyle(style);
            style2.normal.textColor = window.stateFontColorContent;

            #region Draw Functions Calculations
            string labelEnteredText = isTerminating ? "" : "OnEntered:";
            string labelActiveText = "OnSetActive:";
            string labelStayText = isTerminating ? "" : "OnStay:";
            string labelLeftText = isTerminating ? "" : "OnLeft:";

            string valueEnteredText = isTerminating ? "" : GSMUtilities.GenerateMethodName(state.onStateEntered);
            string valueActiveText = GSMUtilities.GenerateMethodName(state.onStateSetActive);
            string valueStayText = isTerminating ? "" : GSMUtilities.GenerateMethodName(state.onStateStay);
            string valueLeftText = isTerminating ? "" : GSMUtilities.GenerateMethodName(state.onStateLeft);


            Vector2 textSizeLabelEntered = style.CalcSize(new GUIContent(labelEnteredText));
            Vector2 textSizeLabelStay = style.CalcSize(new GUIContent(labelStayText));
            Vector2 textSizeLabelLeft = style.CalcSize(new GUIContent(labelLeftText));
            Vector2 textSizeLabelActive = style.CalcSize(new GUIContent(labelActiveText));
            Vector2 textSizeValueEntered = style.CalcSize(new GUIContent(valueEnteredText));
            Vector2 textSizeValueStay = style.CalcSize(new GUIContent(valueStayText));
            Vector2 textSizeValueLeft = style.CalcSize(new GUIContent(valueLeftText));
            Vector2 textSizeValueActive = style.CalcSize(new GUIContent(valueActiveText));
            float labelWidth = Mathf.Max(textSizeLabelEntered.x, textSizeLabelStay.x, textSizeLabelLeft.x, textSizeLabelActive.x) + padding;
            float valueWidth = Mathf.Min(Mathf.Max(textSizeValueEntered.x, textSizeValueStay.x, textSizeValueLeft.x, textSizeValueActive.x) + 2 * padding, maxStateWidth - labelWidth);
            #endregion


            GUIContent nameContent = new GUIContent(state.name);
            var nameSize = style.CalcSize(nameContent);

            Rect offsetRect = state.bounds.Move(offset);

            Rect nameRect = new Rect(offsetRect.x, offsetRect.y, Mathf.Max(nameSize.x, minStateWidth), nameSize.y + 2 * padding);


            Rect enterRect = new Rect(offsetRect.x + padding, nameRect.yMax + padding, labelWidth, EditorGUIUtility.singleLineHeight);
            Rect activeRect = isTerminating ? enterRect : new Rect(offsetRect.x + padding, enterRect.yMax + padding, labelWidth, EditorGUIUtility.singleLineHeight);
            Rect stayRect = new Rect(offsetRect.x + padding, activeRect.yMax + padding, labelWidth, EditorGUIUtility.singleLineHeight);
            Rect leftRect = new Rect(offsetRect.x + padding, stayRect.yMax + padding, labelWidth, EditorGUIUtility.singleLineHeight);

            Rect enterValueRect = new Rect(enterRect.xMax + padding, enterRect.y, valueWidth, enterRect.height);
            Rect activeValueRect = new Rect(activeRect.xMax + padding, activeRect.y, valueWidth, activeRect.height);
            Rect stayValueRect = new Rect(stayRect.xMax + padding, stayRect.y, valueWidth, stayRect.height);
            Rect leftValueRect = new Rect(leftRect.xMax + padding, leftRect.y, valueWidth, leftRect.height);


            float stateWidth = Mathf.Max(nameSize.x,
                minStateWidth,
                enterRect.width + enterValueRect.width + 2 * padding,
                stayRect.width + stayValueRect.width + 2 * padding,
                leftRect.width + leftValueRect.width + 2 * padding,
                activeRect.width + activeValueRect.width + 2 * padding);

            float stateHeight = (isTerminating ? activeRect.yMax : leftRect.yMax) - offsetRect.y + padding;
            Rect stateRect = new Rect(offsetRect.x, offsetRect.y, stateWidth, stateHeight);

            state.bounds = stateRect.Move(-offset);



            Rect actNameRect = new Rect(stateRect.center.x - nameSize.x * 0.5f, nameRect.y + padding, nameSize.x, nameSize.y);

            EditorGUI.DrawRect(offsetRect, window.stateColorDefault);
            GUI.Label(actNameRect, state.name, style);





            string warning = null;
            if (!state.hideWarningsInEditor && !hideAllWarningsEditor)
            {
                foreach (var other in states)
                {
                    if (other == state)
                        continue;
                    if (state.name == other.name)
                    {
                        new WarningBox("Make sure not to have to states with the same name.", window)
                            .Draw(new Vector2(actNameRect.xMax + padding, actNameRect.y), GSMWindow.mousePosition);
                        break;
                    }
                }




                if (GetIngoingEdges(state).Count == 0 && !isStartState)
                {
                    warning = "This state does not have an ingoing edge. ";
                }

                if (GetOutgoingEdges(state).Count == 0 && !isTerminating)
                {
                    warning += "This state does not have an outgoing edge";
                }

            }
                

            EditorGUI.DrawRect(new Rect(offsetRect.x, nameRect.yMax, offsetRect.width, 1), window.stateDividerColorDefault);

            if (!isTerminating)
            {
                EditorGUI.LabelField(enterRect, new GUIContent(labelEnteredText), style2);
                EditorGUI.LabelField(stayRect, new GUIContent(labelStayText), style2);
                EditorGUI.LabelField(leftRect, new GUIContent(labelLeftText), style2);
            }
            EditorGUI.LabelField(activeRect, new GUIContent(labelActiveText), style2);

            if (!isTerminating)
            {
                EditorGUI.LabelField(enterValueRect, new GUIContent(valueEnteredText), style2);
                EditorGUI.LabelField(stayValueRect, new GUIContent(valueStayText), style2);
                EditorGUI.LabelField(leftValueRect, new GUIContent(valueLeftText), style2);
            }
            EditorGUI.LabelField(activeValueRect, new GUIContent(valueActiveText), style2);


            if (isStartState)
            {
                var to = offsetRect.center - Vector2.right * offsetRect.width * 0.5f;
                var from = to - Vector2.right * 70;
                DrawArrow(from, to, (to - from).normalized, from, to, isInspected ? window.stateColorInspected : Color.white);
            }





            int lineWidth = 2;
            bool showActiveBorder = isActive && (isRunning || saveActiveState);


            if (showActiveBorder)
            {
                GSMUtilities.DrawUnfilledRect(offsetRect, lineWidth, window.stateColorActive);
            }

            if(isTerminating)
            {
                GSMUtilities.DrawUnfilledRect(offsetRect, lineWidth, window.stateColorTerminating);
            }

            if(isInspected && (showActiveBorder || isTerminating))
            {
                GSMUtilities.DrawUnfilledRect(offsetRect.Move(-lineWidth, -lineWidth).Resize(new Vector2(lineWidth, lineWidth) * 2), lineWidth, window.stateColorInspected);
            } else if(isInspected && !showActiveBorder && !isTerminating)
            {
                GSMUtilities.DrawUnfilledRect(offsetRect, lineWidth, window.stateColorInspected);
            }


            if (warning != null)
                new WarningBox(warning, window).Draw(new Vector2(offsetRect.xMax + padding, offsetRect.y), GSMWindow.mousePosition);

        }

    }
}
