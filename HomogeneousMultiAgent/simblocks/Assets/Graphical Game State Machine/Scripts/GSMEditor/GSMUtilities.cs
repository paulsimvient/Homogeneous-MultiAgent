using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GSM
{
    public static class GSMUtilities
    {



        public static GUIContent GetContent(string content)
        {
            GUIContent gc = new GUIContent();
            if(!content.Contains("|"))
            {
                gc.text = content;
            } else
            {
                string[] s = content.Split('|');
                gc.text = s[0];
                gc.tooltip = s[1];
            }
            return gc;
        }

        public static void DrawSeparator(float xStart, float yStart, float width, Color color)
        {
            EditorGUI.DrawRect(new Rect(xStart, yStart, width, 1), color);
        } 


        public static Rect MoveResize(this Rect rect, Rect other)
        {
            return new Rect(rect.x + other.x, rect.y + other.y, rect.width, rect.height);
        }


        public static Rect Move(this Rect rect, Vector2 offset)
        {
            return new Rect(rect.x + offset.x, rect.y + offset.y, rect.width, rect.height);
        }

        public static Rect Resize(this Rect rect, Vector2 size)
        {
            return new Rect(rect.x, rect.y, rect.width + size.x, rect.height + size.y);
        }

        public static Rect Move(this Rect rect, float x, float y)
        {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }


        internal const string PATH_SEPARATOR = "|-->|";

        public static string GeneratePathByObject(UnityEngine.Object uobj)
        {
            GameObject obj = uobj as GameObject;

            string name = "";
            if (obj == null)
                return name;

            if (uobj.name.Contains(PATH_SEPARATOR))
            {
                Debug.LogWarning("No object's or one of its parent's name which is used for the state machine should containt \"" + PATH_SEPARATOR + "\".");
                return name;
            }


            Transform t = obj.transform;
            name = t.name;
            t = t.parent;

            while (t != null) {
                name = t.name +PATH_SEPARATOR + name;
                t = t.parent;
            }

            return name;
        }

        public static GameObject FindObjectByPath(string path)
        {
            if (path == "")
                return null;

            if (path == null)
                return null;

            List<string> split = new List<string>();
            split.AddRange(path.Split(new string[] { PATH_SEPARATOR }, StringSplitOptions.None));
            string searched = split[split.Count - 1];


            Scene s = SceneManager.GetActiveScene();
            var objs = new List<GameObject>();
            s.GetRootGameObjects(objs);

            foreach (var obj in objs)
            {
                GameObject foundObj = FindObjectByPath(split, obj, searched);
                if (foundObj != null)
                    return foundObj;
            }
            return null;
        }

        private static GameObject FindObjectByPath(List<string> path, GameObject root, string searched)
        {

            if (root.name.Contains(searched))
                return root;

            if (path.Count == 0)
                return null;


            path.RemoveAt(0);
            foreach (Transform child in root.transform)
            {
                GameObject found = FindObjectByPath(path, child.gameObject, searched);
                if (found != null)
                    return found;
            }
            return null;
        }


        #region Reflection 



        internal static Type[] FindComponentTypesOfObject(UnityEngine.Object draggedObject)
        {
            GameObject draggedGameObject = draggedObject as GameObject;
            var components = new Type[0];
            if (draggedGameObject != null)
            {
                var comps = draggedGameObject.GetComponents<Component>();
                components = new Type[comps.Length];
                for (int i = 0; i < components.Length; i++)
                {
                    if (comps[i] == null)
                        continue;
                    components[i] = comps[i].GetType();
                }
            }
            return components;
        }


        internal static string[] MethodsToNames(List<MethodInfo> methods)
        {
            var names = new string[methods.Count + 1];
            names[0] = "<none>";
            for (int i = 0; i < names.Length - 1; i++)
            {
                names[i + 1] = methods[i].Name;
            }
            return names;
        }


        internal static List<MethodInfo> FindMethodsOfType(Type type)
        {
            MethodInfo[] methods = type.GetMethods();
            List<MethodInfo> ret = new List<MethodInfo>();
            foreach (var method in methods)
            {
                if (method.GetParameters().Length == 0 &&
                    !method.IsAbstract &&
                    !method.IsConstructor &&
                    !method.IsGenericMethod &&
                    method.ReturnType == typeof(void))
                    ret.Add(method);
            }
            return ret;
        }

        internal static List<MethodInfo> FindMethodsOfType(Type type, bool parametersAllowed)
        {
            if (!parametersAllowed)
                return FindMethodsOfType(type);

            if (type == null)
                return null;

            MethodInfo[] methods = type.GetMethods();
            List<MethodInfo> ret = new List<MethodInfo>();

            foreach (var method in methods)
            {
                if (method.IsAbstract || method.IsConstructor || method.IsGenericMethod || method.IsSpecialName)
                    continue;

                if (IsValidMethod(method))
                    ret.Add(method);                               
            }
            return ret;
        }
        #endregion

        public static bool IsValidMethod(MethodInfo method)
        {
            var allowedTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(int),
                typeof(float),
                typeof(bool)
            };

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 0 || parameters.Length == 1 && allowedTypes.Contains(parameters[0].ParameterType);
        }


        public static void DrawUnfilledRect(Rect rect, int width, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x - width, rect.y - width,
                 rect.width + 2 * width, width), color); //oben

            EditorGUI.DrawRect(new Rect(rect.x - width, rect.yMax,
                rect.width + 2 * width, width), color); //unten

            EditorGUI.DrawRect(new Rect(rect.x - width, rect.y - width,
                width, rect.height + 2 * width), color); //rechts

            EditorGUI.DrawRect(new Rect(rect.xMax, rect.y - width,
                width, rect.height + 2 * width), color); //links
        }

        public static string GenerateMethodName(GSMEvent evt)
        {

            int callbackCount = evt.callbacks.Count;
            if (callbackCount == 0)
                return "-";
            var firstCallback = evt.FindFirstUsefulCallback();
            if (firstCallback == null)
                return "-";

            string more = callbackCount > 1 ? " +" + (callbackCount - 1) : "";
            string parameter = firstCallback.Parameter;
            string methodName = firstCallback.methodName + "(" + parameter + ")" + more;
            return methodName;
        }
    }
}
