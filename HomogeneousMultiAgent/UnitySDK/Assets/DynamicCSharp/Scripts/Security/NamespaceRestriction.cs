using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security
{
    /// <summary>
    /// Allows restrictions to be placed on referenced namespaces.
    /// Loaded assemblies cannot contain modules that reference restricted namespaces otherwise they will fail to load.
    /// </summary>
    [Serializable]
    public sealed class NamespaceRestriction : Restriction
    {
        // Private
        [SerializeField]
        private string namespaceName = string.Empty;

        // Properties
        /// <summary>
        /// Gets the name of the restricted namespace.
        /// </summary>
        public string RestrictedNamespace
        {
            get { return namespaceName; }
        }

        /// <summary>
        /// Gets the error mssage associated with this restriction.
        /// </summary>
        public override string Message
        {
            get { return string.Format("The namespace '{0}' is prohibited and cannot be referenced", namespaceName); }
        }

        public override RestrictionMode Mode
        {
            get { return DynamicCSharp.Settings.namespaceRestrictionMode; }
        }

        // Constructor
        /// <summary>
        /// Create a new <see cref="NamespaceRestriction"/> for the specified namespace. 
        /// </summary>
        /// <param name="restrictedName">The namespace to restrict. For example, 'System.IO'</param>
        public NamespaceRestriction(string restrictedName)
        {
            this.namespaceName = restrictedName;
        }

        // Methods
        /// <summary>
        /// Attempts to verify that the specified module does not contain an invalid namespace reference.
        /// </summary>
        /// <param name="module">The module to verify</param>
        /// <returns>True if the module passes verification or false if it fails</returns>
        public override bool Verify(ModuleDefinition module)
        {
            // Check for empty restriction - exit quickly
            if (string.IsNullOrEmpty(namespaceName) == true)
                return true;

            // Find all type references for the module
            IEnumerable<TypeReference> references = module.GetTypeReferences();

            // Check for illegal references
            foreach(TypeReference reference in references)
            {
                // Get the referenced namespace
                string name = reference.Namespace;

                // Check for matching names
                if(string.Compare(namespaceName, name) == 0)
                {
                    // The namespace is illegal
                    return false;
                }
            }

            // The namespace is not prohibited
            return true;
        }
    }
}