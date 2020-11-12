using UnityEngine;
using UnityEditor;

namespace GSM
{
    public partial class GSMStateMachine
    {
        public void DrawEdge(GSMEdge edge)
        {
            bool isInspected = window.InspectedObject == edge;
            bool isInCycle = HasComplementEdge(edge);



            var from = GetState(edge.originID);
            var to = GetState(edge.targetID);


            bool fromTerminating = from.isTerminating;
            bool duplicateTrigger = false;
            foreach (var otherEdge in GetOutgoingEdges(from))
            {
                if (otherEdge == edge)
                    continue;
                if(otherEdge.trigger == edge.trigger)
                {
                    duplicateTrigger = true;
                    break;
                }
            }


            var fromBounds = from.bounds.Move(offset);
            var toBounds = to.bounds.Move(offset);

            var diff = (toBounds.center - fromBounds.center).normalized;

            var toRadius = GetRadius(diff, toBounds, true, edge.IsSelfLoop);
            var fromRadius = GetRadius(diff, fromBounds, false, edge.IsSelfLoop);
       

            /*#region Start position radius
            var fromAngle = Vector2.Angle(diff, Vector2.right);
            var fromThreshold = Mathf.Atan((fromBounds.height * 0.5f) / (fromBounds.width * 0.5f)) * Mathf.Rad2Deg;
            fromRadius = fromAngle > fromThreshold && fromAngle + fromThreshold < 180 ?
                fromBounds.height * 0.5f / Mathf.Sin(Mathf.Deg2Rad * fromAngle) :
                fromBounds.width * 0.5f / Mathf.Cos(Mathf.Deg2Rad * fromAngle);

            if (fromAngle + fromThreshold < 180)
            {
                fromRadius = -fromRadius;
            }
            #endregion*/


            //position to point to
            var peakPos = edge.IsSelfLoop ? toBounds.center + Vector2.right * (toBounds.width * 0.5f - toBounds.height * 0.5f) - Vector2.up * toBounds.height * 0.5f : toBounds.center - diff * toRadius;

            //poition to start
            var startPos = edge.IsSelfLoop ? toBounds.center + Vector2.right * toBounds.width * 0.5f : fromBounds.center - diff * fromRadius;

            //point between start and end
            var midPoint = (startPos + peakPos) * 0.5f;
            var diffy = diff.y == 0 ? 0.0001f : diff.y; //Kann überlauf verursachen. Wenn kanten weg dann zahl größer
            var ortho = isInCycle ? new Vector2(1, -diff.x / diffy).normalized : Vector2.zero;


            var edgeStretchPerc = 0.07f;
            var maxStretch = 30;
            var edgeStretch = Mathf.Min(maxStretch, (toBounds.center - fromBounds.center).magnitude * edgeStretchPerc);
            var selfLoopStretch = 127;

            var tangentLeft = edge.IsSelfLoop ? startPos + Vector2.right * selfLoopStretch : midPoint + edgeStretch * (peakPos.y >= startPos.y ? ortho : -ortho);
            var tangentRight = edge.IsSelfLoop ? peakPos - Vector2.up * selfLoopStretch : tangentLeft;
            var handlePos = edge.IsSelfLoop ? toBounds.center + new Vector2(toBounds.width * 0.5f + 20, -toBounds.height * 0.5f - 20): tangentLeft;

            var arrowColor = Color.white;
            if (isInspected)
                arrowColor = window.stateColorInspected;
            if (duplicateTrigger)
                arrowColor = window.textColorError;
            //if (isActive)
            //    arrowColor = window.stateColorActive;
            DrawArrow(
                startPos, 
                peakPos, 
                edge.IsSelfLoop ? Vector2.up : (peakPos - tangentLeft).normalized, 
                tangentLeft, 
                tangentRight, 
                arrowColor);

            if (Handles.Button(handlePos, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                window.SetInspectedObject(edge);
            }

            if(fromTerminating)
            {
                new WarningBox("A terminating state does not need outgoing edges", window)
                    .Draw(startPos - Vector2.one * WarningBox.boxSize * 0.5f + diff * WarningBox.boxSize, GSMWindow.mousePosition);
            }

            GUIStyle style = new GUIStyle(window.machineTextStyle);
            style.normal.textColor = isInspected ? window.stateColorInspected : style.normal.textColor;
            if (duplicateTrigger)
                style.normal.textColor = window.textColorError;

            string methodName = GSMUtilities.GenerateMethodName(edge.onEdgePassed);

            var tw = style.CalcSize(new GUIContent(edge.trigger));
            var tw2 = style.CalcSize(new GUIContent(methodName));
            var rt = new Rect(handlePos.x - tw.x * 0.5f, handlePos.y - 8 - tw.y, tw.x, tw.y);
            var rt2 = new Rect(handlePos.x - tw2.x * 0.5f, handlePos.y - 8 + tw2.y, tw2.x, tw2.y);

            float angle = Vector2.Angle(Vector2.right, (startPos - peakPos));
            angle = angle >= 90 ? angle - 180 : angle;
            angle = fromBounds.y <= toBounds.y ? -angle : angle;
            if(edge.IsSelfLoop)
            {
                angle = 45;
            }

            GUIUtility.RotateAroundPivot(angle, handlePos);
            EditorGUI.LabelField(rt, new GUIContent(edge.trigger), style);
            EditorGUI.LabelField(rt2, new GUIContent(methodName), style);
            GUIUtility.RotateAroundPivot(-angle, handlePos);


        }


        private void DrawArrow(Vector2 from, Vector2 to, Vector2 arrowDir, Vector2 fromTangent, Vector2 toTangent, Color color)
        {
            Handles.color = color;
            Handles.DrawBezier(
                from,
                to,
                fromTangent,
                toTangent,
                color,
                null,
                2f);

            var arrowAngle = 45;
            var wingLength = 15;

            var a = Quaternion.Euler(0, 0, arrowAngle * 0.5f) * arrowDir;
            var b = Quaternion.Euler(0, 0, -arrowAngle * 0.5f) * arrowDir;
            var leftWingPos = to - new Vector2(a.x, a.y) * wingLength;
            var rightWingPos = to - new Vector2(b.x, b.y) * wingLength;
            Handles.DrawAAConvexPolygon(to, leftWingPos, rightWingPos);
        }


        private float GetRadius(Vector2 diff, Rect bounds, bool to, bool selfloop = false)
        {

            if (selfloop)
            {
                //if it is a selfloop and we want the target radius, it is always on top of the state
                return to ? bounds.height * 0.5f : bounds.width * 0.5f;
            }
            var angle = Vector2.Angle(diff, Vector2.right);
            var threshold = Mathf.Atan((bounds.height * 0.5f) / (bounds.width * 0.5f)) * Mathf.Rad2Deg;
            var radius = angle > threshold && angle + threshold < 180 ?
                bounds.height * 0.5f / Mathf.Sin(Mathf.Deg2Rad * angle) :
                bounds.width * 0.5f / Mathf.Cos(Mathf.Deg2Rad * angle);

            if (to && angle + threshold > 180 || !to && angle + threshold < 180)
            {
                radius = -radius;
            }
            return radius;
        }
    }
}
