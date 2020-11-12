using System;
using System.Reflection;
using UnityEngine;

namespace GSM
{
    [Serializable]
    public class GSMCallback
    {
        [SerializeField] internal string objectName = "";
        [SerializeField] internal string componentName = "";
        [SerializeField] internal string methodName = "";
        [SerializeField] internal string parameterType = "";
        [SerializeField] internal int parameterInt = 0;
        [SerializeField] internal string parameterString = "";
        [SerializeField] internal float parameterFloat = 0;
        [SerializeField] internal bool parameterBool = false;


        internal UnityEngine.Object FindObjectReferences()
        {
            sourceObject = GSMUtilities.FindObjectByPath(objectName);
            return sourceObject;
        }

        internal GSMCallback Clone()
        {
            GSMCallback newOne = new GSMCallback
            {
                objectName = objectName,
                componentName = componentName,
                methodName = methodName,
                parameterType = parameterType,
                parameterInt = parameterInt,
                parameterString = parameterString,
                parameterFloat = parameterFloat,
                parameterBool = parameterBool
            };
            return newOne;
        }

        internal string Parameter
        {
            get
            {
                switch (parameterType)
                {
                    case "":
                        return "";
                    case "Int32":
                        return parameterInt + "";
                    case "Single":
                        return parameterFloat + "";
                    case "String":
                        return parameterString;
                    case "Boolean":
                        return parameterBool + "";
                    default:
                        return "";
                }
            }
        }

        private object[] ParameterArray
        {
            get
            {
                switch (parameterType)
                {
                    case "":
                        return new object[0];
                    case "Int32":
                        return new object[1] { parameterInt };
                    case "Single":
                        return new object[1] { parameterFloat };
                    case "String":
                        return new object[1] { parameterString };
                    case "Boolean":
                        return new object[1] { parameterBool };
                    default:
                        return new object[0];
                }
            }
        }

        private Type[] ParameterTypeArray
        {
            get
            {
                switch (parameterType)
                {
                    case "":
                        return new Type[0];
                    case "Int32":
                        return new Type[1] { typeof(int) };
                    case "Single":
                        return new Type[1] { typeof(float) };
                    case "String":
                        return new Type[1] { typeof(string) };
                    case "Boolean":
                        return new Type[1] { typeof(bool) };
                    default:
                        return new Type[0];
                }
            }
        }

        public UnityEngine.Object SourceObject
        {
            get { return sourceObject ?? FindObjectReferences(); }
            set { objectName = GSMUtilities.GeneratePathByObject(value); sourceObject = value; }
        }

        private UnityEngine.Object sourceObject;

        public bool Invoke(bool error)
        {
            GameObject o = SourceObject as GameObject;
            if (o == null)
            {
                if (error)
                    throw new NullReferenceException("Could not find object \"" + objectName + "\" in current scene. (Did you rename it?)");
                return false;
            }
            //Debug.Log("Testing: "+o.name);


            Type t = Type.GetType(componentName, error);
            if (t == null)
            {
                if (error)
                    throw new NullReferenceException("Could not find script called \""+componentName+"\". (Did you delete or rename the script?)");
                return false;
            }

            Component c = o.GetComponent(t);
            if (c == null)
            {
                if (error)
                    throw new MissingComponentException("Could not find \"" + componentName + "\" on object \""+objectName+"\".");
                return false;
            }


            MethodInfo method = t.GetMethod(methodName, ParameterTypeArray);
            if (method == null)
            {
                if (error)
                    throw new MissingMethodException("Could not find method \"" + methodName + "\" on script \"" + componentName + "\". (Did you delete or rename the method?)");
                return false;
            }


            method.Invoke(c, ParameterArray);
            return true;
        }

    }
}
