using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace DynamicCSharp
{
    /// <summary>
    /// Represents a type that may or may not derive from MonoBehaviour.
    /// A <see cref="ScriptType"/> is a wrapper for <see cref="Type"/> that contains methods for Unity specific operations.
    /// The type may also be used to create instances of objects.
    /// </summary>
    public sealed class ScriptType
    {
        // Private
        private Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>();
        private Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();

        private Type rawType = null;
        private ScriptAssembly assembly = null;

        // Properties
        /// <summary>
        /// Get the <see cref="Type"/> that this <see cref="ScriptType"/> wraps.   
        /// </summary>
        public Type RawType
        {
            get { return rawType; }
        }

        /// <summary>
        /// Get the <see cref="ScriptAssembly"/> that this <see cref="ScriptType"/> is defined in.  
        /// </summary>
        public ScriptAssembly Assembly
        {
            get { return assembly; }
        }

        /// <summary>
        /// Returns true if this type inherits from <see cref="UnityEngine.Object"/>.
        /// See also <see cref="IsMonoBehaviour"/>.
        /// </summary>
        public bool IsUnityObject
        {
            get { return IsSubtypeOf<UnityEngine.Object>(); }
        }

        /// <summary>
        /// Returns true if this type inherits from <see cref="MonoBehaviour"/>.
        /// </summary>
        public bool IsMonoBehaviour
        {
            get { return IsSubtypeOf<MonoBehaviour>(); }
        }

        /// <summary>
        /// Returns true if this type inherits from <see cref="ScriptableObject"/> 
        /// </summary>
        public bool IsScriptableObject
        {
            get { return IsSubtypeOf<ScriptableObject>(); }
        }

        // Constructor
        /// <summary>
        /// Create a <see cref="ScriptType"/> from a <see cref="Type"/>.  
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to create the <see cref="ScriptType"/> from</param>
        public ScriptType(Type type)
        {
            this.assembly = null;
            this.rawType = type;
        }

        internal ScriptType(ScriptAssembly assembly, Type type)
        {
            this.assembly = assembly;
            this.rawType = type;
        }

        // Methods
        #region CreateInstance
        /// <summary>
        /// Creates an instance of this type.
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <returns>An instance of <see cref="ScriptProxy"/></returns>
        public ScriptProxy CreateInstance(GameObject parent = null)
        {
            if (IsMonoBehaviour == true)
            {
                // Create a component instance
                return CreateBehaviourInstance(parent);
            }
            else if (IsScriptableObject == true)
            {
                // Create a scriptable object instance
                return CreateScriptableInstance();
            }

            // Create a C# instance
            return CreateCSharpInstance();
        }

        /// <summary>
        /// Creates an instance of this type.
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <param name="parameters">The parameter list for the desired constructor. only used when the type does not inherit from <see cref="UnityEngine.Object"/></param>
        /// <returns>An instance of <see cref="ScriptProxy"/></returns>
        public ScriptProxy CreateInstance(GameObject parent = null, params object[] parameters)
        {
            if (IsMonoBehaviour == true)
            {
                // Create a component instance
                return CreateBehaviourInstance(parent);
            }
            else if (IsScriptableObject == true)
            {
                // Create a scriptable object instance
                return CreateScriptableInstance();
            }

            // Create a C# instance
            return CreateCSharpInstance(parameters);
        }

        /// <summary>
        /// Creates a raw instance of this type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <returns>A raw instance that can be cast to the desired type</returns>
        public object CreateRawInstance(GameObject parent = null)
        {
            // Call through
            ScriptProxy proxy = CreateInstance(parent);

            // Check for error
            if (proxy == null)
                return null;

            // Get the instance
            return proxy.Instance;
        }

        /// <summary>
        /// Creates a raw instance of this type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <param name="parameters">The parameter list for the desired constructor. only used when the type does not inherit from <see cref="UnityEngine.Object"/></param>
        /// <returns>A raw instance that can be cast to the desired type</returns>
        public object CreateRawInstance(GameObject parent = null, params object[] parameters)
        {
            // Call through
            ScriptProxy proxy = CreateInstance(parent, parameters);

            // Check for error
            if (proxy == null)
                return null;

            // Get the instance
            return proxy.Instance;
        }

        /// <summary>
        /// Creates an instance of this type and returns the result as the specified generic type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <typeparam name="T">The generic type to return the instance as</typeparam>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <returns>A raw instance as the specified generic type</returns>
        public T CreateRawInstance<T>(GameObject parent = null) where T : class
        {
            // Call through
            ScriptProxy proxy = CreateInstance(parent);

            // Check for error
            if (proxy == null)
                return null;
            
            // Get the instance
            return proxy.GetInstanceAs<T>(false);
        }

        /// <summary>
        /// Creates an instance of this type and returns the result as the specified generic type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <typeparam name="T">The generic type to return the instance as</typeparam>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <param name="parameters">The parameter list for the desired constructor. only used when the type does not inherit from <see cref="UnityEngine.Object"/></param>
        /// <returns>A raw instance as the specified generic type</returns>
        public T CreateRawInstance<T>(GameObject parent = null, params object[] parameters) where T : class
        {
            // Call through
            ScriptProxy proxy = CreateInstance(parent);

            // Check the error
            if (proxy == null)
                return null;

            // Get the instance
            return proxy.GetInstanceAs<T>(false);
        }


        #region MainCreateInstance
        private ScriptProxy CreateBehaviourInstance(GameObject parent)
        {
            // Check for null parent
            if (parent == null)
                throw new ArgumentNullException("parent");

            // Try to add component
            MonoBehaviour instance = parent.AddComponent(rawType) as MonoBehaviour;

            // Check for valid instance
            if (instance != null)
            {
                // Create an object proxy
                return new ScriptProxy(this, instance);
            }

            // Error
            return null;
        }

        private ScriptProxy CreateScriptableInstance()
        {
            // Allow unity to create the instance - Note we dont need to use the parent object so it can be null
            ScriptableObject instance = ScriptableObject.CreateInstance(rawType);

            // Check for valid instance
            if (instance != null)
            {
                // Create an object proxy
                return new ScriptProxy(this, instance);
            }

            // Error
            return null;
        }

        private ScriptProxy CreateCSharpInstance(params object[] args)
        {
            // Try to create the type
            object instance = null;

            try
            {
                // Try to create an instance with the default or parameter constructor
                instance = Activator.CreateInstance(rawType, DynamicCSharp.Settings.GetMemberBindings(), null, args, null);
            }
            catch(MissingMethodException)
            {
                // Check for arguments
                if (args.Length > 0)
                    return null;

                // Create an instance without calling constructor
                instance = FormatterServices.GetUninitializedObject(rawType);
            }

            // Check for valid instance
            if (instance != null)
            {
                // Create the proxy for the C# instance
                return new ScriptProxy(this, instance);
            }

            // Error
            return null;
        }
        #endregion

        #endregion

        /// <summary>
        /// Returns true if this type inherits from the specified type.
        /// </summary>
        /// <param name="baseClass">The base type</param>
        /// <returns>True if this type inherits from the specified type</returns>
        public bool IsSubtypeOf(Type baseClass)
        {
            // Check for subclass
            return baseClass.IsAssignableFrom(rawType);
        }

        /// <summary>
        /// Returns true if this type inherits from the specified type.
        /// </summary>
        /// <typeparam name="T">The base type</typeparam>
        /// <returns>True if this type inherits from the specified type</returns>
        public bool IsSubtypeOf<T>()
        {
            // Call through
            return IsSubtypeOf(typeof(T));
        }

        /// <summary>
        /// Finds a field with the specified name from the cache if possible.
        /// If the field is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// This method may or may not locate private members depending upon the value of <see cref="DynamicCSharp.discoverNonPublicMembers"/>.
        /// </summary>
        /// <param name="name">The name of the field to find</param>
        /// <returns>The <see cref="FieldInfo"/> for the specified field</returns>
        public FieldInfo FindCachedField(string name)
        {
            // Check cache
            if (fieldCache.ContainsKey(name) == true)
                return fieldCache[name];

            // Get field with correct flags
            FieldInfo field = rawType.GetField(name, DynamicCSharp.Settings.GetMemberBindings());

            // Check for null
            if (field == null)
                return null;

            // Cache the field
            fieldCache.Add(name, field);

            return field;
        }

        /// <summary>
        /// Finds a property with the specified name from the cache if possible.
        /// If the property is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// This method may or may not locate private members depending upon the value of <see cref="DynamicCSharp.discoverNonPublicMembers"/>.
        /// </summary>
        /// <param name="name">The name of the property to find</param>
        /// <returns>The <see cref="PropertyInfo"/> for the specified property</returns>
        public PropertyInfo FindCachedProperty(string name)
        {
            // Check cache
            if (propertyCache.ContainsKey(name) == true)
                return propertyCache[name];

            // Get property with correct flags
            PropertyInfo property = rawType.GetProperty(name, DynamicCSharp.Settings.GetMemberBindings());

            // Check for null
            if (property == null)
                return null;

            // Cache the property
            propertyCache.Add(name, property);

            return property;
        }

        /// <summary>
        /// Finds a method with the specified name from the cache if possible.
        /// If the method is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// This method may or may not locate private members depending upon the value of <see cref="DynamicCSharp.discoverNonPublicMembers"/>.
        /// </summary>
        /// <param name="name">The name of the method to find</param>
        /// <returns>The <see cref="MethodInfo"/> for the specified method</returns>
        public MethodInfo FindCachedMethod(string name)
        {
            // Check cache
            if (methodCache.ContainsKey(name) == true)
                return methodCache[name];

            // Get method with correct flags
            MethodInfo method = rawType.GetMethod(name, DynamicCSharp.Settings.GetMemberBindings());

            // Check for null
            if (method == null)
                return null;

            // Cache the method
            methodCache.Add(name, method);

            return method;
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="methodName">The name of the static method to call</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        /// <exception cref="TargetException">The target method is not static</exception>
        public object CallStatic(string methodName)
        {
            // Find the method
            MethodInfo method = FindCachedMethod(methodName);

            // Check for error
            if (method == null)
                throw new TargetException(string.Format("Type '{0}' does not define a static method called '{1}'", this, methodName));

            // Check for static
            if (method.IsStatic == false)
                throw new TargetException(string.Format("The target method '{0}' is not marked as static and must be called on an object", methodName));

            // Call the method
            return method.Invoke(null, null);
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="methodName">The name of the static method to call</param>
        /// <param name="arguments">The arguemnts passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        /// <exception cref="TargetException">The target method is not static</exception>
        public object CallStatic(string methodName, params object[] arguments)
        {
            // Find the method
            MethodInfo method = FindCachedMethod(methodName);

            // Check for error
            if (method == null)
                throw new TargetException(string.Format("Type '{0}' does not define a static method called '{1}'", this, methodName));

            // Check for static
            if (method.IsStatic == false)
                throw new TargetException(string.Format("The target method '{0}' is not marked as static and must be called on an object", methodName));

            // Call the method
            return method.Invoke(null, arguments);
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// Any exceptions throw as a result of locating or calling the method will be caught silently
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="method">The name of the static method to call</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public object SafeCallStatic(string method)
        {
            try
            {
                // Call the method and catch any exceptions
                return CallStatic(method);
            }
            catch
            {
                // Exception - Fail silently
                return null;
            }
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// Any exceptions throw as a result of locating or calling the method will be caught silently
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="method">The name of the static method to call</param>
        /// <param name="arguments">The arguments passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public object SafeCallStatic(string method, params object[] arguments)
        {
            try
            {
                // Call the method and catch any exceptions
                return CallStatic(method, arguments);
            }
            catch
            {
                // Exception - Fail silently
                return null;
            }
        }

        /// <summary>
        /// Override ToString implementation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ScriptType({0})", rawType.Name);
        }
    }
}