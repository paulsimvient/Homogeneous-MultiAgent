using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security
{
    /// <summary>
    /// Allows restrictions to be placed on referenced assemblies.
    /// Loaded assemblies cannot contain references to restricted assemblies otherwise they will fail to load.
    /// </summary>
    [Serializable]
    public sealed class ReferenceRestriction : Restriction
    {
        // Private
        [SerializeField]
        private string referenceName = string.Empty;

        // Properties
        /// <summary>
        /// Gets the name of the restricted reference.
        /// </summary>
        public string RestrictedName
        {
            get { return referenceName; }
        }

        /// <summary>
        /// Gets the error message associated with this restriction.
        /// </summary>
        public override string Message
        {
            get { return string.Format("The references assembly '{0}' is prohibited and cannot be referenced", referenceName); }
        }

        public override RestrictionMode Mode
        {
            get { return DynamicCSharp.Settings.assemblyRestrictionMode; }
        }

        // Constructor
        /// <summary>
        /// Create a new <see cref="ReferenceRestriction"/> for the specified reference name. 
        /// </summary>
        /// <param name="referenceName">The assembly reference name to restrict</param>
        public ReferenceRestriction(string referenceName)
        {
            this.referenceName = referenceName;
        }

        // Methods
        /// <summary>
        /// Attempts to verify that the specified module does not contain an invalid assembly reference.
        /// </summary>
        /// <param name="module">The module to verify</param>
        /// <returns>True if the module passes verification or false if it fails</returns>
        public override bool Verify(ModuleDefinition module)
        {
            // Check for empty restriction - exit quickly
            if (string.IsNullOrEmpty(referenceName) == true)
                return true;

            // Find all referenced assemblies
            IEnumerable<AssemblyNameReference> references = module.AssemblyReferences;            

            // Process each reference
            foreach (AssemblyNameReference reference in references)
            {
                // Compare values
                if (string.Compare(referenceName, reference.Name + ".dll") == 0)
                {
                    // The strings should not match
                    return false;
                }
            }

            // No matches were found
            return true;
        }
    }
}
