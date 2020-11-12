using UnityEngine;
using UnityEditor;

namespace GSM
{
    public partial class GSMWindow
    {

        private Rect DrawEdgeInspector(Rect rect, GSMEdge edge)
        {
            var target = machine.GetState(edge.targetID);
            var origin = machine.GetState(edge.originID);
            string title = origin.name + " \u2192 " + target.name;

            float padding = 4;
            float indent = 16;
            Rect titleRect = new Rect(rect.x + indent + padding, rect.y + padding, rect.width - indent - 2*padding, EditorGUIUtility.singleLineHeight);
            Rect triggerRect = new Rect(titleRect.x, titleRect.yMax + 2 * padding, titleRect.width, titleRect.height);
            Rect triggerLabelRect = new Rect(triggerRect.x, triggerRect.y, triggerRect.width * 0.3f, triggerRect.height);
            Rect triggerValueRect = new Rect(triggerLabelRect.xMax, triggerRect.y, triggerRect.width - triggerLabelRect.width, triggerLabelRect.height);
            Rect leftButtonRect = new Rect(triggerRect.x, triggerRect.yMax + padding, triggerRect.width * 0.5f, triggerRect.height + padding);
            Rect rightButtonRect = new Rect(leftButtonRect.xMax + padding * 0.5f, leftButtonRect.y, leftButtonRect.width - padding * 0.5f, leftButtonRect.height);
            Rect boxRect = new Rect(rect.x + indent, rect.y, rect.width - indent, rightButtonRect.yMax - rect.y + padding);


            EditorGUI.DrawRect(boxRect, eventColor);
            EditorGUI.LabelField(titleRect, GSMUtilities.GetContent(title + "|" + "Edge going from "+target.name + " to " + origin.name+"."));
            GSMUtilities.DrawSeparator(boxRect.x, titleRect.yMax, boxRect.width, new Color(0.4f, 0.4f, 0.4f));
            EditorGUI.LabelField(triggerLabelRect, GSMUtilities.GetContent("Trigger|Sending this string using SendTrigger(string) will use this edge."));
            edge.trigger = EditorGUI.TextField(triggerValueRect, edge.trigger);
            if(GUI.Button(leftButtonRect, new GUIContent("Select Edge"))) {
                SetInspectedObject(edge);
            }

            if (GUI.Button(rightButtonRect, new GUIContent("Select Target"))) {
                SetInspectedObject(target);
            }

            return rect.Move(0, boxRect.height + padding);
        }

    }
}
