using Microsoft.CodeAnalysis;
using RoslynCSharp.Compiler;
using System;
using System.Collections.Generic;
using System.Reflection;
using Trivial.CodeSecurity;
using UnityEngine;

namespace RoslynCSharp
{
    /// <summary>
    /// A <see cref="ScriptAssembly"/> represents a managed assembly that has been loaded into a <see cref="ScriptDomain"/> at runtime.
    /// </summary>
    public sealed class ScriptAssembly : IMetadataReferenceProvider
    {
        // Private
        private static List<ScriptType> matchedTypes = new List<ScriptType>();

        private ScriptDomain domain = null;
        private Assembly rawAssembly = null;
        private CodeSecurityEngine securityEngine = null;
        private CodeSecurityReport securityReport = null;
        private CompilationResult compileResult = null;
        private Dictionary<string, ScriptType> scriptTypes = new Dictionary<string, ScriptType>();        
        private DateTime runtimeCompiledTime = DateTime.MinValue;
        private bool isRuntimeCompiled = false;
        private bool isSecurityValidated = false;

        // Properties
        /// <summary>
        /// Get the name of the wrapped assembly.
        /// </summary>
        public string Name
        {
            get { return rawAssembly.GetName().Name; }
        }

        /// <summary>
        /// Get the version of the wrapped assembly.
        /// </summary>
        public Version Version
        {
            get { return rawAssembly.GetName().Version; }
        }

        /// <summary>
        /// Get the full name of the wrapped assembly.
        /// </summary>
        public string FullName
        {
            get { return rawAssembly.FullName; }
        }

        public MetadataReference Reference
        {
            get
            {
                // Use the compile result
                if (compileResult != null)
                    return compileResult.Reference;

                return null;
            }
        }

        /// <summary>
        /// Get the DateTime when this assembly was runtime compiled.
        /// </summary>
        public DateTime RuntimeCompiledTime
        {
            get { return runtimeCompiledTime; }
        }

        /// <summary>
        /// Returns true if this assembly was compiled at runtime by the Roslyn compiler service.
        /// </summary>
        public bool IsRuntimeCompiled
        {
            get { return isRuntimeCompiled; }
        }

        public CompilationResult CompileResult
        {
            get { return compileResult; }
        }

        /// <summary>
        /// Returns true if this assembly has passed security verification.
        /// </summary>
        public bool IsSecurityValidated
        {
            get { return isSecurityValidated; }
        }

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
                // Check for main type
                if (scriptTypes.Count == 0)
                    return null;

                ScriptType firstType = null;

                // Enumerate to first type
                foreach(ScriptType type in scriptTypes.Values)
                {
                    firstType = type;
                    break;
                }

                // Get first type
                return firstType;
            }
        }

        /// <summary>
        /// Get the <see cref="Assembly"/> that this <see cref="ScriptAssembly"/> wraps.  
        /// </summary>
        public Assembly RawAssembly
        {
            get { return rawAssembly; }
        }

        /// <summary>
        /// Get the security checker that validated this assembly.
        /// This allows information about illegal type references to be retrieved when security verification fails.
        /// </summary>
        public CodeSecurityEngine SecurityEngine
        {
            get { return securityEngine; }
        }

        // Constructor
        internal ScriptAssembly(ScriptDomain domain, Assembly rawAssembly, CodeSecurityEngine securityEngine, CompilationResult compileResult = null)
        {
            this.domain = domain;
            this.rawAssembly = rawAssembly;
            this.securityEngine = securityEngine;
            this.compileResult = compileResult;

            // Create cached types
            foreach (Type type in rawAssembly.GetTypes())
                scriptTypes.Add(type.FullName, new ScriptType(this, type));
        }

        // Methods
        /// <summary>
        /// Run security verification on this assembly using the specified security restrictions.
        /// </summary>
        /// <param name="restrictions">The restrictions used to verify the assembly</param>
        /// <returns>True if the assembly passes security verification or false if it fails</returns>
        public bool SecurityCheckAssembly(CodeSecurityRestrictions restrictions)
        {
            // Check for already checked
            if (securityEngine == null)
                return isSecurityValidated;

            // Run code valdiation
            isSecurityValidated = securityEngine.SecurityCheckAssembly(restrictions);

            // Release security engine
            securityEngine = null;

            return isSecurityValidated;
        }

        /// <summary>
        /// Run security verification on this assembly using the specified security restrictions and output a security report
        /// </summary>
        /// <param name="restrictions">The restrictions used to verify the assembly</param>
        /// <param name="report">The security report generated by the assembly checker</param>
        /// <returns>True if the assembly passes security verification or false if it fails</returns>
        public bool SecurityCheckAssembly(CodeSecurityRestrictions restrictions, out CodeSecurityReport report)
        {
            // Check for already checked
            if (securityEngine == null)
            {
                report = securityReport;
                return isSecurityValidated;
            }

            // Run code validation
            isSecurityValidated = securityEngine.SecurityCheckAssembly(restrictions, out report);

            // Release security engine and store report
            securityEngine = null;
            securityReport = report;            

            return isSecurityValidated;
        }

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
        /// <param name="subType">The type to check for in the inheritace chain</param>
        /// <returns>True if there are one or more defined types that inherit from the specified type</returns>
        public bool HasSubTypeOf(Type subType)
        {
            // Try to find the type
            return FindSubTypeOf(subType) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type that inherits from the specified type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type that inherits from the specified type and has the specified name is defined</returns>
        public bool HasSubTypeOf(Type subType, string name)
        {
            // Try to find type
            return FindSubTypeOf(subType, name) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defined one or more types that inherit from the specified generic type.
        /// The specified generic type may be a base class or interface type.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>True if there are one or more defined types that inherit from the specified generic type</returns>
        public bool HasSubTypeOf<T>()
        {
            // Try to find the type
            return FindSubTypeOf<T>() != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type that inherits from the specified genric type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type that inherits from the specified type and has the specified name is defined</returns>
        public bool HasSubTypeOf<T>(string name)
        {
            // Try to find type
            return FindSubTypeOf<T>(name) != null;
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
            Type type = rawAssembly.GetType(name, false, false);

            // Check for error
            if (type == null)
                return null;

            // Get the cached script type
            return scriptTypes[type.FullName];
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified base type.
        /// If there is more than one type that inherits from the specified base type, then the first matching type will be returned.
        /// If you want to find all types then use <see cref="FindAllSubTypesOf(Type, bool)"/>. 
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubTypeOf(Type subType, bool includeNonPublic = true)
        {
            // Find all types in the assembly
            foreach(ScriptType type in scriptTypes.Values)
            {
                // Check for non-public discoverability
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Check for subtype
                if(type.IsSubTypeOf(subType) == true)
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
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubTypeOf(Type subType, string name)
        {
            // Find a type with the specified name
            ScriptType type = FindType(name);

            // Check for error
            if(type == null)
                return null;

            // Make sure the identifier type is a subclass
            if (type.IsSubTypeOf(subType) == true)
                return type;

            return null;
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified generic type. 
        /// If there is more than one type that inherits from the specified generic type, then the first matching type will be returned.
        /// If you want to find all types then use <see cref="FindAllSubTypesOf{T}(bool)"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubTypeOf<T>(bool includeNonPublic = true)
        {
            // Call through
            return FindSubTypeOf(typeof(T), includeNonPublic);
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified generic type and matches the specified name. 
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubTypeOf<T>(string name)
        {
            // Call through
            return FindSubTypeOf(typeof(T), name);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherits from the specified type.
        /// If there are no types that inherit from the specified type then the return value will be an empty array.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllSubTypesOf(Type subType, bool includeNonPublic = true)
        {
            // Use shared list
            matchedTypes.Clear();

            // Find all types
            foreach(ScriptType type in scriptTypes.Values)
            {
                // Check for non-public discovery
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Make sure the type is a Unity object
                if (type.IsSubTypeOf(subType) == true)
                {
                    // Add type
                    matchedTypes.Add(type);
                }
            }

            // Get the array
            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from the specified generic type.
        /// If there are no types that inherit from the specified type then the return value will be an empty array.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllSubTypesOf<T>(bool includeNonPublic = true)
        {
            // Call through
            return FindAllSubTypesOf(typeof(T), includeNonPublic);
        }
        
        /// <summary>
        /// Returns an array of all defined types in this <see cref="ScriptAssembly"/>. 
        /// </summary>
        /// <returns>An array of <see cref="ScriptType"/> representing all types defined in this <see cref="ScriptAssembly"/></returns>
        public ScriptType[] FindAllTypes(bool includeNonPublic = true)
        {
            // Use shared array
            matchedTypes.Clear();
            matchedTypes.AddRange(scriptTypes.Values);

            // Get as array
            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.Object"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.Object"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllUnityTypes(bool includeNonPublic = true)
        {
            // Find all types that inherit from object
            return FindAllSubTypesOf<UnityEngine.Object>(includeNonPublic);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.MonoBehaviour"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.MonoBehaviour"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllMonoBehaviourTypes(bool includeNonPublic = true)
        {
            // Find all types that inherit from mono behaviour
            return FindAllSubTypesOf<MonoBehaviour>(includeNonPublic);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.ScriptableObject"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.ScriptableObject"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllScriptableObjectTypes(bool includeNonPublic = true)
        {
            // Find all types that inherit from scriptable object
            return FindAllSubTypesOf<ScriptableObject>(includeNonPublic);
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherits from the specified type.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllSubTypesOf(Type subType, bool includeNonPublic = true)
        {
            foreach(ScriptType type in scriptTypes.Values)
            {
                // Check for visible
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Check for sub type
                if (type.IsSubTypeOf(subType) == true)
                    yield return type;
            }
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from the specified generic type.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllSubTypesOf<T>(bool includeNonPublic = true)
        {
            return EnumerateAllSubTypesOf(typeof(T), includeNonPublic);
        }

        /// <summary>
        /// Enumerate all defined types in this <see cref="ScriptAssembly"/>. 
        /// </summary>
        /// <returns>Enumerable of all results</returns>
        public IEnumerable<ScriptType> EnumerateAllTypes(bool includeNonPublic = true)
        {
            foreach (ScriptType type in scriptTypes.Values)
            {
                // Check for visible
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Return type
                yield return type;
            }
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.Object"/>.  
        /// </summary>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllUnityTypes(bool includeNonPublic = true)
        {
            return EnumerateAllSubTypesOf<UnityEngine.Object>(includeNonPublic);
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.MonoBehaviour"/>.  
        /// </summary>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllMonoBehaviourTypes(bool includeNonPublic = true)
        {
            return EnumerateAllSubTypesOf<MonoBehaviour>(includeNonPublic);
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.ScriptableObject"/>.  
        /// </summary>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllScriptableObjectTypes(bool includeNonPublic = true)
        {
            return EnumerateAllSubTypesOf<ScriptableObject>(includeNonPublic);
        }

        internal void MarkAsRuntimeCompiled()
        {
            isRuntimeCompiled = true;
            runtimeCompiledTime = DateTime.Now;
        }
    }
}