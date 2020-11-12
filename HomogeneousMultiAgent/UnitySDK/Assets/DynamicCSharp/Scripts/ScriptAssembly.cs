using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DynamicCSharp
{
    /// <summary>
    /// A <see cref="ScriptAssembly"/> represents a managed assembly that has been loaded into a <see cref="ScriptDomain"/> at runtime.
    /// </summary>
    public sealed class ScriptAssembly
    {
        // Private
        private ScriptDomain domain = null;
        private Assembly rawAssembly = null;

        // Properties
        /// <summary>
        /// Get the <see cref="ScriptDomain"/> that this <see cref="ScriptAssembly"/> is currently loaded in.  
        /// </summary>
        public ScriptDomain Domain
        {
            get { return domain; }
        }

        /// <summary>
        /// Gets the main type for the assembly. This will always return the first defined type in the assembly which is especially useful for assemblies that only define a single type.
        /// </summary>
        public ScriptType MainType
        {
            get
            {
                // Find all defined types
                Type[] types = rawAssembly.GetTypes();

                // Make sure there is one or more types
                if (types.Length == 0)
                    throw new InvalidProgramException("The assembly does not contain a 'MainType'");

                // Get the type
                return new ScriptType(this, types[0]);
            }
        }

        /// <summary>
        /// Get the <see cref="Assembly"/> that this <see cref="ScriptAssembly"/> wraps.  
        /// </summary>
        public Assembly RawAssembly
        {
            get { return rawAssembly; }
        }

        // Constructor
        internal ScriptAssembly(ScriptDomain domain, Assembly rawAssembly)
        {
            this.domain = domain;
            this.rawAssembly = rawAssembly;
        }

        // Methods
        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type with the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type with the specified name is defined</returns>
        public bool HasType(string name)
        {
            // Try to find the type
            return FindType(name) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines one or more types that inherit from the specified type.
        /// The specified type may be a base class or interface type.
        /// </summary>
        /// <param name="baseType">The type to check for in the inheritace chain</param>
        /// <returns>True if there are one or more defined types that inherit from the specified type</returns>
        public bool HasSubtypeOf(Type baseType)
        {
            // Try to find the type
            return FindSubtypeOf(baseType) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type that inherits from the specified type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="baseType">The type to check for in the inheritance chain</param>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type that inherits from the specified type and has the specified name is defined</returns>
        public bool HasSubtypeOf(Type baseType, string name)
        {
            // Try to find type
            return FindSubtypeOf(baseType, name) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defined one or more types that inherit from the specified generic type.
        /// The specified generic type may be a base class or interface type.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>True if there are one or more defined types that inherit from the specified generic type</returns>
        public bool HasSubtypeOf<T>()
        {
            // Try to find the type
            return FindSubtypeOf<T>() != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type that inherits from the specified genric type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type that inherits from the specified type and has the specified name is defined</returns>
        public bool HasSubtypeOf<T>(string name)
        {
            // Try to find type
            return FindSubtypeOf<T>(name) != null;
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> with the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindType(string name)
        {
            // Try to find the type
            Type type = rawAssembly.GetType(name, false, DynamicCSharp.Settings.caseSensitiveNames);

            // Check for error
            if (type == null)
                return null;

            // Create the script type
            return new ScriptType(this, type);
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified base type.
        /// If there is more than one type that inherits from the specified base type, then the first matching type will be returned.
        /// If you want to find all types then use <see cref="FindAllSubtypesOf(Type)"/>. 
        /// </summary>
        /// <param name="baseType">The type to check for in the inheritance chain</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubtypeOf(Type baseType)
        {
            // Find all types in the assembly
            foreach(ScriptType type in FindAllTypes())
            {
                // Check for subtype
                if(type.IsSubtypeOf(baseType) == true)
                {
                    // Return first occurence
                    return type;
                }
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified base type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="baseType">The type to check for in the inheritance chain</param>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubtypeOf(Type baseType, string name)
        {
            // Find a type with the specified name
            ScriptType type = FindType(name);

            // Check for error
            if(type == null)
                return null;

            // Make sure the identifier type is a subclass
            if (type.IsSubtypeOf(baseType) == true)
                return type;

            return null;
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified generic type. 
        /// If there is more than one type that inherits from the specified generic type, then the first matching type will be returned.
        /// If you want to find all types then use <see cref="FindAllSubtypesOf{T}"/>.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubtypeOf<T>()
        {
            // Call through
            return FindSubtypeOf(typeof(T));
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified generic type and matches the specified name. 
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubtypeOf<T>(string name)
        {
            // Call through
            return FindSubtypeOf(typeof(T), name);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherits from the specified type.
        /// If there are no types that inherit from the specified type then the return value will be an empty array.
        /// </summary>
        /// <param name="baseType">The type to check for in the inheritance chain</param>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllSubtypesOf(Type baseType)
        {
            List<ScriptType> discovered = new List<ScriptType>();

            // Find all types
            foreach(Type type in rawAssembly.GetTypes())
            {
                // Check for non-public discovery
                if (DynamicCSharp.Settings.discoverNonPublicTypes == false)
                    if (type.IsPublic == false)
                        continue;

                // Create the script type
                ScriptType scriptType = new ScriptType(this, type);

                // Make sure the type is a Unity object
                if (scriptType.IsSubtypeOf(baseType) == true)
                {
                    // Add type
                    discovered.Add(scriptType);
                }
            }

            // Get the array
            return discovered.ToArray();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from the specified generic type.
        /// If there are no types that inherit from the specified type then the return value will be an empty array.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllSubtypesOf<T>()
        {
            // Call through
            return FindAllSubtypesOf(typeof(T));
        }
        
        /// <summary>
        /// Returns an array of all defined types in this <see cref="ScriptAssembly"/>. 
        /// </summary>
        /// <returns>An array of <see cref="ScriptType"/> representing all types defined in this <see cref="ScriptAssembly"/></returns>
        public ScriptType[] FindAllTypes()
        {
            List<ScriptType> discovered = new List<ScriptType>();

            // Find all types
            foreach (Type type in rawAssembly.GetTypes())
            {
                // Check for non-public discovery
                if(DynamicCSharp.Settings.discoverNonPublicTypes == false)
                    if (type.IsPublic == false)
                        continue;

                // Create the script type
                ScriptType scriptType = new ScriptType(this, type);

                // Add type
                discovered.Add(scriptType);
            }

            // Get the array
            return discovered.ToArray();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.Object"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.Object"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllUnityTypes()
        {
            // Find all types that inherit from object
            return FindAllSubtypesOf<UnityEngine.Object>();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.MonoBehaviour"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.MonoBehaviour"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllMonoBehaviourTypes()
        {
            // Find all types that inherit from mono behaviour
            return FindAllSubtypesOf<MonoBehaviour>();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.ScriptableObject"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.ScriptableObject"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllScriptableObjectTypes()
        {
            // Find all types that inherit from scriptable object
            return FindAllSubtypesOf<ScriptableObject>();
        }
    }
}