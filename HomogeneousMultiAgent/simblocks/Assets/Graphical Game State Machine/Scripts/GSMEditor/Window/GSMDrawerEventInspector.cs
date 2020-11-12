using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace GSM
{
    public partial class GSMWindow
    {
        private static GSMCallback selectedCallback;
        private static GSMEvent selectedEvent;

        private static GSMCallback draggedCallback;
        private static GSMEvent draggedEvent;
        private static int draggedIndex = -1;

        internal Rect DrawUnityEvent(Rect rect, GSMEvent evt, string name, string description)
        {
            bool foldout = Foldout(evt);
            int boxMargin = 4;
            int contentSpacing = 4;
            float titleHeight = EditorGUIUtility.singleLineHeight;
            int buttonWidth = 18;

            //Containing Title. On Left, Right and Top boxMargin away form rect
            Rect titleRect = new Rect(rect.x + boxMargin, rect.y + boxMargin, rect.width - 2 * boxMargin, titleHeight + boxMargin);
            Rect subButtonRect = new Rect(titleRect.xMax - buttonWidth, titleRect.y, buttonWidth, titleHeight);
            Rect addButtonRect = new Rect(subButtonRect.x - buttonWidth - boxMargin, titleRect.y, buttonWidth, titleHeight);
            Rect foldoutRect = new Rect(titleRect.x, titleRect.y, addButtonRect.x - titleRect.x, titleRect.height);
            Rect callbackRect = new Rect(titleRect.x, titleRect.yMax + contentSpacing, titleRect.width, EditorGUIUtility.singleLineHeight * 3 + 4 * boxMargin);


            float boxHeight = titleRect.height + boxMargin + 
                (foldout ? evt.callbacks.Count * (callbackRect.height + boxMargin) + 
                    (evt.callbacks.Count > 0 ? boxMargin : 0): 0);
            Rect boxRect = new Rect(rect.x, rect.y, rect.width, boxHeight);


            //////////////////// HEADER LINE //////////////////////
            EditorGUI.DrawRect(boxRect, eventColor);
            Foldout(evt, EditorGUI.Foldout(foldoutRect, foldout,GSMUtilities.GetContent(name + "|" + description)));
            if(foldout && evt.callbacks.Count > 0)
                GSMUtilities.DrawSeparator(rect.x, titleRect.yMax, boxRect.width, new Color(0.4f, 0.4f, 0.4f));


            /////////////////// ADD AND SUB BUTTON //////////////////////
            if(GUI.Button(subButtonRect, GSMUtilities.GetContent("-|Remove selected callback")))
            {
                if(selectedEvent != null && selectedCallback != null && selectedEvent == evt)
                {
                    evt.callbacks.Remove(selectedCallback);
                    if(draggedIndex > 0)
                    {
                        selectedCallback = selectedEvent.callbacks[draggedIndex - 1];
                    }
                    hasUnsavedChanges = true;
                }
                Foldout(evt, true);
            }
            if (GUI.Button(addButtonRect, GSMUtilities.GetContent("+|Add a new callback")))
            {
                Foldout(evt, true);
                var c = selectedCallback != null && selectedEvent == evt ? selectedCallback.Clone() : new GSMCallback();
                evt.callbacks.Add(c);
                selectedCallback = c;
                selectedEvent = evt;
                hasUnsavedChanges = true;
            }


            ////////////////////////////// CALLBACKS /////////////////////////////

            int index = 0;
            if(foldout)
                foreach (var callback in evt.callbacks)
                {
                    callbackRect = DrawUnityEventCallback(callbackRect, evt, callback, index++, out bool breakAndRepaint);
                    if(breakAndRepaint)
                    {
                        GUI.changed = true;
                        break;
                    }
                }

            ///////////////////////////
            return new Rect(rect.x, rect.y + boxRect.height, rect.width, rect.height);
        }



        private class MethodInfoComparer : IComparer<MethodInfo>
        {
            public int Compare(MethodInfo x, MethodInfo y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        private Rect DrawUnityEventCallback(Rect rect, GSMEvent evt, GSMCallback callback, int callbackIndex, out bool breakAndRepaint)
        {
            breakAndRepaint = false;
            int margin = 4;
            float labelPerc = 0.3f;
            bool isSelected = callback == selectedCallback;

            Rect objectRect = new Rect(rect.x + margin, rect.y + margin, rect.width - 2 * margin, EditorGUIUtility.singleLineHeight);
            Rect objectLabelRect = new Rect(objectRect.x, objectRect.y, objectRect.width * labelPerc, objectRect.height);
            Rect objectValueRect = new Rect(objectRect.x + objectLabelRect.width, objectRect.y, objectRect.width - objectLabelRect.width, objectLabelRect.height);

            Rect componentRect = new Rect(objectRect.x, objectRect.yMax + margin, objectRect.width, objectRect.height);
            Rect componentLabelRect = new Rect(componentRect.x, componentRect.y, componentRect.width * labelPerc, componentRect.height);
            Rect componentValueRect = new Rect(componentRect.x + componentLabelRect.width, componentRect.y, componentRect.width - componentLabelRect.width, componentLabelRect.height);

            Rect parameterRect = new Rect(componentRect.x, componentRect.yMax + margin, componentRect.width, componentRect.height);
            Rect parameterLabelRect = new Rect(parameterRect.x, parameterRect.y, parameterRect.width * labelPerc, parameterRect.height);
            Rect parameterValueRect = new Rect(parameterRect.x + parameterLabelRect.width, parameterRect.y, parameterRect.width - parameterLabelRect.width, parameterLabelRect.height);

            var newRect = new Rect(rect.x, parameterRect.yMax + 2 * margin, rect.width, rect.height);


            Object draggedObject = callback.SourceObject;

            EditorGUI.DrawRect(rect, isSelected ? callbackColorSelected : callbackColorDefault);
            EditorGUI.LabelField(objectLabelRect, GSMUtilities.GetContent("Object|Drag and drop an object from your scene here"));
            EditorGUI.LabelField(componentLabelRect, GSMUtilities.GetContent("Method"));
            EditorGUI.LabelField(parameterLabelRect, GSMUtilities.GetContent("Parameter"));

            #region event
            Event crt = Event.current;
            if(rect.Contains(crt.mousePosition))
            {
                if(selectedCallback != null && draggedEvent == evt)
                {
                    GSMUtilities.DrawUnfilledRect(rect, 2, callbackColorSwap);
                }


                if (crt.type == EventType.MouseDown)
                {
                    if (crt.button == 0)
                    {
                        selectedCallback = callback;
                        selectedEvent = evt;

                        draggedCallback = callback;
                        draggedEvent = evt;
                        draggedIndex = callbackIndex;
                        GUI.changed = true;
                    }
                    if (crt.button == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Move Up"), false, () => {
                            evt.SwapCallbacks(callbackIndex, callbackIndex - 1);
                            GUI.changed = true;
                        });
                        menu.AddItem(new GUIContent("Move Down"), false, () => {
                            evt.SwapCallbacks(callbackIndex, callbackIndex + 1);
                            GUI.changed = true;
                        });
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Reset"), false, () => {
                            callback.methodName = "";
                            callback.objectName = "";
                            callback.componentName = "";
                            callback.parameterBool = false;
                            callback.parameterFloat = 0;
                            callback.parameterInt = 0;
                            callback.parameterString = "";
                            callback.parameterType = "";
                        });
                        menu.AddItem(new GUIContent("Delete"), false, () => {
                            evt.callbacks.RemoveAt(callbackIndex);
                            GUI.changed = true;
                        });
                        menu.ShowAsContext();
                    }
                } else if (crt.type == EventType.MouseUp)
                {
                    if (draggedCallback != null && draggedEvent != null && draggedIndex != -1)
                    {
                        //perform swap 
                        if (selectedEvent == evt)
                        {
                            //Same event. Swap callbacks
                            evt.SwapCallbacks(draggedIndex, callbackIndex);
                            breakAndRepaint = true;
                        }
                    }
                    draggedCallback = null;
                    draggedEvent = null;
                    draggedIndex = -1;
                    GUI.changed = true;
                } else if(selectedCallback != null && crt.type == EventType.MouseDrag)
                {
                    GUI.changed = true;
                }
            }
            
            #endregion

            ////////////// SHOW OBJECT FIELD ///////////////
            draggedObject = EditorGUI.ObjectField(objectValueRect, draggedObject, typeof(GameObject), true);

            if (draggedObject != callback.SourceObject)
            {
                hasUnsavedChanges = true;
            }

            if (draggedObject != null)
                callback.SourceObject = draggedObject;



            //////////////// SHOW COMPONENT FIELD ///////////////
            if (draggedObject == null)
            {
                EditorGUI.LabelField(componentValueRect, GSMUtilities.GetContent(
                    callback.componentName != "" && callback.componentName != "<none>" ?
                    "Unknown object|The object you selected seems to be missing in the current scene." : "-"));

            }
            else
            {
                System.Type[] components = GSMUtilities.FindComponentTypesOfObject(draggedObject);
                List<(string, string)> paths = new List<(string, string)>();
                foreach (var c in components)
                {
                    List<MethodInfo> mts = GSMUtilities.FindMethodsOfType(c, true);
                    if (mts == null)
                    {
                        break;
                    }
                    mts.Sort(new MethodInfoComparer());
                    foreach (var m in mts)
                    {
                        var pt = m.GetParameters().Length == 0 ? "" : m.GetParameters()[0].ParameterType.Name;
                        paths.Add((c.FullName + "/" + m.Name, pt));
                    }
                }


                int componentIndex = 0;
                int ctr = 0;
                foreach (var p in paths)
                {
                    var pa = p.Item1;
                    if (pa == callback.componentName + "/" + callback.methodName)
                    {
                        componentIndex = ctr;
                        break;
                    }
                    ctr++;
                }
                var componentPaths = new string[paths.Count];
                for (int i = 0; i < paths.Count; i++)
                {
                    componentPaths[i] = paths[i].Item1;
                }
                componentIndex = EditorGUI.Popup(componentValueRect, componentIndex, componentPaths);
                string path = componentPaths[componentIndex];
                string parameterType = paths[componentIndex].Item2;
                var split = path.Split('/');
                var componentName = split[0];
                var methodName = split[1];

                if (callback.componentName != componentName)
                    callback.componentName = componentName;
                if (callback.methodName != methodName)
                    callback.methodName = methodName;
                if (callback.parameterType != parameterType)
                    callback.parameterType = parameterType;


                if (parameterType == "String")
                {



                    var parameterString = EditorGUI.TextField(parameterValueRect, callback.parameterString);
                    if (callback.parameterString != parameterString)
                    {
                        callback.parameterString = parameterString;
                        hasUnsavedChanges = true;
                    }



                }
                else if (parameterType == "Boolean")
                {


                    var parameterBool = EditorGUI.Toggle(parameterValueRect, callback.parameterBool);
                    if (callback.parameterBool != parameterBool)
                    {
                        callback.parameterBool = parameterBool;
                        hasUnsavedChanges = true;
                    }

                }
                else if (parameterType == "Single") //float
                {
                    var parameterFloat = EditorGUI.FloatField(parameterValueRect, callback.parameterFloat);
                    if (callback.parameterFloat != parameterFloat)
                    {
                        callback.parameterFloat = parameterFloat;
                        hasUnsavedChanges = true;
                    }
                }
                else if (parameterType == "Int32")
                {
                    var parameterInt = EditorGUI.IntField(parameterValueRect, callback.parameterInt);
                    if (callback.parameterInt != parameterInt)
                    {
                        callback.parameterInt = parameterInt;
                        hasUnsavedChanges = true;
                    }
                }
                else
                {
                    EditorGUI.LabelField(parameterValueRect, "No parameters");
                }




            }
            return newRect;
        }


        private readonly Dictionary<GSMEvent, bool> foldouts = new Dictionary<GSMEvent, bool>();
        private bool Foldout(GSMEvent evt)
        {
            if (!foldouts.ContainsKey(evt))
                return true;
            else return foldouts[evt];
        }

        private void Foldout(GSMEvent evt, bool foldout)
        {
            foldouts[evt] = foldout;
        }
    }
}